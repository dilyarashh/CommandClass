using System.Text.Json;
using PotteryClass.Data.DTOs;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class GradeCalculationService : IGradeCalculationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public GradeCalculationResultDto Calculate(GradeCalculationRequest request)
    {
        var values = request.Values.ToDictionary(x => x.CriterionId, x => x.Value);
        var result = new GradeCalculationResultDto();

        foreach (var criterion in request.Criteria.OrderBy(x => x.SortOrder))
        {
            var type = Normalize(criterion.Type);
            var category = NormalizeCategory(criterion.Category);
            values.TryGetValue(criterion.Id, out var value);

            var detail = CalculateCriterion(criterion, type, category, value);
            result.Criteria.Add(detail);

            if (!detail.IsApplied)
                continue;

            switch (category)
            {
                case CriterionCategoryDto.Main:
                    result.MainPoints += detail.Score;
                    break;
                case CriterionCategoryDto.Bonus:
                    result.BonusPoints += detail.Score;
                    break;
                case CriterionCategoryDto.Penalty:
                    var penalty = Math.Abs(detail.Score);
                    result.PenaltyPoints += penalty;
                    result.AppliedPenalties.Add(new AppliedPenaltyDetailDto
                    {
                        Source = criterion.Name,
                        Value = penalty
                    });
                    break;
                case CriterionCategoryDto.Multiplier:
                    var multiplier = detail.Multiplier ?? 1m;
                    result.Multiplier *= multiplier;
                    result.AppliedMultipliers.Add(new AppliedMultiplierDetailDto
                    {
                        Source = criterion.Name,
                        Value = multiplier
                    });
                    break;
                default:
                    throw new BadRequestException("Некорректная категория критерия");
            }

            if (detail.Multiplier.HasValue && category != CriterionCategoryDto.Multiplier)
            {
                result.Multiplier *= detail.Multiplier.Value;
                result.AppliedMultipliers.Add(new AppliedMultiplierDetailDto
                {
                    Source = criterion.Name,
                    Value = detail.Multiplier.Value
                });
            }
        }

        ApplyRulePenalties(request, result);
        ApplyPeerReviewPenalty(request.PeerReviewPenalty, result);
        ApplyMainThreshold(request.Rules, request.Criteria, result);

        var mode = Normalize(request.Rules.Mode);
        var baseGrade = mode switch
        {
            AssignmentGradingModeDto.SumPoints => result.MainPoints + result.BonusPoints - result.PenaltyPoints,
            AssignmentGradingModeDto.BaseWithMultipliers => request.Rules.BaseGrade
                ?? throw new BadRequestException("Base grade is required for base_with_multipliers mode"),
            _ => throw new BadRequestException("Unsupported grading mode")
        };

        if (!result.MainCriteriaThresholdPassed &&
            NormalizeThresholdBehavior(request.Rules.MainCriteriaThreshold.Behavior) == MainCriteriaThresholdBehaviorDto.SetToZero)
        {
            baseGrade = 0m;
        }

        result.FinalGrade = Math.Max(0m, decimal.Round(baseGrade * result.Multiplier, 2, MidpointRounding.AwayFromZero));

        return result;
    }

    private static CriterionCalculationDetailDto CalculateCriterion(
        CriterionDto criterion,
        string type,
        string category,
        JsonElement value)
    {
        var detail = new CriterionCalculationDetailDto
        {
            CriterionId = criterion.Id,
            Name = criterion.Name,
            Type = type,
            Category = category
        };

        switch (type)
        {
            case CriterionTypeDto.Score:
            {
                var settings = DeserializeSettings<ScoreCriterionSettingsDto>(criterion.Settings);
                var selectedValue = ReadInt(value, criterion.Id);
                EnsureRange(selectedValue, settings.MinValue, settings.MaxValue, criterion.Id);

                detail.Score = ResolveRangeScore(settings, selectedValue);
                detail.Multiplier = settings.Multiplier;
                detail.IsApplied = true;
                return detail;
            }
            case CriterionTypeDto.PassFail:
            {
                var settings = DeserializeSettings<PassFailCriterionSettingsDto>(criterion.Settings);
                var selectedValue = ReadString(value, criterion.Id);

                detail.Score = ResolveMappedScore(settings.ScoreMappings, selectedValue, criterion.Id);
                detail.Multiplier = settings.Multiplier;
                detail.IsApplied = true;
                return detail;
            }
            case CriterionTypeDto.Option:
            {
                var settings = DeserializeSettings<OptionCriterionSettingsDto>(criterion.Settings);
                var selectedValue = ReadString(value, criterion.Id);

                detail.Score = ResolveMappedScore(settings.ScoreMappings, selectedValue, criterion.Id);
                detail.Multiplier = settings.Multiplier;
                detail.IsApplied = true;
                return detail;
            }
            case CriterionTypeDto.Multiplier:
            {
                var settings = DeserializeSettings<MultiplierCriterionSettingsDto>(criterion.Settings);
                detail.Multiplier = settings.Coefficient;
                detail.IsApplied = ShouldApplyMultiplier(value);
                return detail;
            }
            default:
                throw new BadRequestException("Некорректный тип критерия");
        }
    }

    private static void ApplyRulePenalties(GradeCalculationRequest request, GradeCalculationResultDto result)
    {
        ApplyRulePenalty("deadline", request.Penalties.Deadline, request.Rules.Penalties.Deadline, result);
        ApplyRulePenalty("progress", request.Penalties.Progress, request.Rules.Penalties.Progress, result);
        ApplyRulePenalty("required_criteria", request.Penalties.RequiredCriteria, request.Rules.Penalties.RequiredCriteria, result);
    }

    private static void ApplyRulePenalty(
        string source,
        bool shouldApply,
        PenaltyRuleDto rule,
        GradeCalculationResultDto result)
    {
        if (!shouldApply || !rule.Enabled || !rule.Percentage.HasValue)
            return;

        var multiplier = Math.Max(0m, 1m - rule.Percentage.Value / 100m);
        result.Multiplier *= multiplier;
        result.AppliedPenalties.Add(new AppliedPenaltyDetailDto
        {
            Source = source,
            Value = rule.Percentage.Value,
            Kind = "percentage"
        });
        result.AppliedMultipliers.Add(new AppliedMultiplierDetailDto
        {
            Source = source,
            Value = multiplier
        });
    }

    private static void ApplyPeerReviewPenalty(
        PeerReviewPenaltyInputDto penalty,
        GradeCalculationResultDto result)
    {
        if (!penalty.ShouldApply || penalty.Percent <= 0)
            return;

        var multiplier = Math.Max(0m, 1m - penalty.Percent / 100m);
        result.Multiplier *= multiplier;
        result.AppliedPenalties.Add(new AppliedPenaltyDetailDto
        {
            Source = "peer_review",
            Value = penalty.Percent,
            Kind = "percentage",
            Percent = penalty.Percent,
            RequiredReviewsCount = penalty.RequiredReviewsCount,
            CompletedReviewsCount = penalty.CompletedReviewsCount
        });
        result.AppliedMultipliers.Add(new AppliedMultiplierDetailDto
        {
            Source = "peer_review",
            Value = multiplier
        });
    }

    private static void ApplyMainThreshold(
        AssignmentGradingRulesDto rules,
        IReadOnlyCollection<CriterionDto> criteria,
        GradeCalculationResultDto result)
    {
        if (!rules.MainCriteriaThreshold.Enabled || !rules.MainCriteriaThreshold.Threshold.HasValue)
            return;

        var mainMaxScore = criteria
            .Where(x => NormalizeCategory(x.Category) == CriterionCategoryDto.Main)
            .Sum(x => (decimal)x.MaxScore);

        var mainPercent = mainMaxScore <= 0 ? 0 : result.MainPoints / mainMaxScore * 100m;
        result.MainCriteriaThresholdPassed = mainPercent >= rules.MainCriteriaThreshold.Threshold.Value;

        if (!result.MainCriteriaThresholdPassed &&
            NormalizeThresholdBehavior(rules.MainCriteriaThreshold.Behavior) == MainCriteriaThresholdBehaviorDto.MarkAsFailed)
        {
            result.IsFailed = true;
        }
    }

    private static decimal ResolveRangeScore(ScoreCriterionSettingsDto settings, int selectedValue)
    {
        if (settings.Ranges is null || settings.Ranges.Count == 0)
            return selectedValue;

        var range = settings.Ranges.FirstOrDefault(x => selectedValue >= x.From && selectedValue <= x.To)
            ?? throw new BadRequestException("Значение критерия не попадает в настроенные диапазоны");

        return range.Score;
    }

    private static decimal ResolveMappedScore(
        IReadOnlyCollection<CriterionScoreMappingDto> scoreMappings,
        string selectedValue,
        Guid criterionId)
    {
        var mapping = scoreMappings.FirstOrDefault(x =>
            string.Equals(x.Value.Trim(), selectedValue, StringComparison.OrdinalIgnoreCase));

        if (mapping == null)
            throw new BadRequestException($"Некорректное значение критерия {criterionId}");

        return mapping.Score;
    }

    private static T DeserializeSettings<T>(JsonElement settings)
    {
        try
        {
            var result = settings.Deserialize<T>(JsonOptions);
            return result ?? throw new BadRequestException("Некорректные настройки критерия");
        }
        catch (JsonException)
        {
            throw new BadRequestException("Некорректные настройки критерия");
        }
    }

    private static int ReadInt(JsonElement value, Guid criterionId)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var result))
            throw new BadRequestException($"Некорректное значение критерия {criterionId}");

        return result;
    }

    private static string ReadString(JsonElement value, Guid criterionId)
    {
        if (value.ValueKind != JsonValueKind.String)
            throw new BadRequestException($"Некорректное значение критерия {criterionId}");

        var result = value.GetString();
        if (string.IsNullOrWhiteSpace(result))
            throw new BadRequestException($"Некорректное значение критерия {criterionId}");

        return result.Trim();
    }

    private static bool ShouldApplyMultiplier(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Undefined => false,
            JsonValueKind.Null => false,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new BadRequestException("Некорректное значение критерия-множителя")
        };
    }

    private static void EnsureRange(int value, int min, int max, Guid criterionId)
    {
        if (value < min || value > max)
            throw new BadRequestException($"Значение критерия {criterionId} вне допустимого диапазона");
    }

    private static string Normalize(string value)
        => value.Trim().ToLowerInvariant();

    private static string NormalizeCategory(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? CriterionCategoryDto.Main
            : value.Trim().ToLowerInvariant();

    private static string? NormalizeThresholdBehavior(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToLowerInvariant();
}
