using System.Text.Json;
using FluentValidation;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;
using ValidationException = PotteryClass.Infrastructure.Errors.Exceptions.ValidationException;

namespace PotteryClass.Services;

public class CriterionService(
    ICriterionRepository repository,
    ICourseTeacherRepository teacherRepository,
    ICurrentUser currentUser,
    IValidator<CreateCriterionRequest> createValidator,
    IValidator<UpdateCriterionRequest> updateValidator,
    IValidator<ScoreCriterionSettingsDto> scoreSettingsValidator,
    IValidator<PassFailCriterionSettingsDto> passFailSettingsValidator,
    IValidator<OptionCriterionSettingsDto> optionSettingsValidator)
    : ICriterionService
{
    private async Task EnsureTeacherOrAdmin(Guid courseId)
    {
        var role = currentUser.GetRole();

        if (role == UserRole.Admin)
            return;

        var userId = currentUser.GetUserId();
        var isTeacher = await teacherRepository.IsTeacherAsync(courseId, userId);

        if (!isTeacher)
            throw new ForbiddenException("РќРµС‚ РґРѕСЃС‚СѓРїР°");
    }

    private static async Task ValidateAndThrowAsync<T>(IValidator<T> validator, T dto)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (validationResult.IsValid)
            return;

        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        throw new ValidationException(errors);
    }

    private static CriterionDto Map(Criterion criterion)
    {
        using var document = JsonDocument.Parse(criterion.Settings);

        return new CriterionDto
        {
            Id = criterion.Id,
            CriterionGroupId = criterion.CriterionGroupId,
            Name = criterion.Name,
            Description = criterion.Description,
            Type = criterion.Type,
            Settings = document.RootElement.Clone(),
            MaxScore = criterion.MaxScore,
            SortOrder = criterion.SortOrder,
            CreatedAtUtc = criterion.CreatedAtUtc
        };
    }

    private static string NormalizeType(string type)
        => type.Trim().ToLowerInvariant();

    private static T DeserializeSettings<T>(JsonElement? settings)
    {
        if (!settings.HasValue || settings.Value.ValueKind == JsonValueKind.Null)
            throw new BadRequestException("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Рµ РЅР°СЃС‚СЂРѕР№РєРё РєСЂРёС‚РµСЂРёСЏ");

        try
        {
            var result = settings.Value.Deserialize<T>();

            if (result == null)
                throw new BadRequestException("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Рµ РЅР°СЃС‚СЂРѕР№РєРё РєСЂРёС‚РµСЂРёСЏ");

            return result;
        }
        catch (JsonException)
        {
            throw new BadRequestException("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Рµ РЅР°СЃС‚СЂРѕР№РєРё РєСЂРёС‚РµСЂРёСЏ");
        }
    }

    private static async Task ValidateSettingsDtoAsync<T>(IValidator<T> validator, T dto)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (validationResult.IsValid)
            return;

        var message = validationResult.Errors
            .Select(x => x.ErrorMessage)
            .FirstOrDefault() ?? "РќРµРєРѕСЂСЂРµРєС‚РЅС‹Рµ РЅР°СЃС‚СЂРѕР№РєРё РєСЂРёС‚РµСЂРёСЏ";

        throw new BadRequestException(message);
    }

    private static void ValidateUniqueOptionValues(List<CriterionOptionDto> options)
    {
        if (options.Select(x => x.Value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Count)
            throw new BadRequestException("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Рµ РІР°СЂРёР°РЅС‚С‹");
    }

    private static void ValidateScoreMappings(
        IReadOnlyCollection<CriterionOptionDto> options,
        IReadOnlyCollection<CriterionScoreMappingDto> scoreMappings,
        int maxScore)
    {
        if (scoreMappings.Select(x => x.Value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != scoreMappings.Count)
            throw new BadRequestException("Invalid score mapping");

        var optionValues = options
            .Select(x => x.Value.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (scoreMappings.Any(x => !optionValues.Contains(x.Value.Trim())))
            throw new BadRequestException("Invalid score mapping");

        if (scoreMappings.Any(x => x.Score < 0 || x.Score > maxScore))
            throw new BadRequestException("Invalid score mapping");

        if (scoreMappings.Count != optionValues.Count)
            throw new BadRequestException("Invalid score mapping");
    }

    private static void ValidateRanges(IReadOnlyList<CriterionRangeDto> ranges, int minValue, int maxValue, int maxScore)
    {
        if (ranges.Count == 0)
            return;

        foreach (var range in ranges)
        {
            if (range.From < minValue || range.To > maxValue || range.To < range.From)
                throw new BadRequestException("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Рµ РґРёР°РїР°Р·РѕРЅС‹");

            if (range.Score < 0 || range.Score > maxScore)
                throw new BadRequestException("Invalid score mapping");
        }

        var orderedRanges = ranges.OrderBy(x => x.From).ThenBy(x => x.To).ToList();

        for (var i = 1; i < orderedRanges.Count; i++)
        {
            if (orderedRanges[i].From <= orderedRanges[i - 1].To)
                throw new BadRequestException("РџРµСЂРµСЃРµРєР°СЋС‰РёРµСЃСЏ РґРёР°РїР°Р·РѕРЅС‹");
        }
    }

    private async Task<string> ValidateSettingsAsync(string type, JsonElement? settings, int maxScore)
    {
        if (maxScore <= 0)
            throw new BadRequestException("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Рµ РЅР°СЃС‚СЂРѕР№РєРё РєСЂРёС‚РµСЂРёСЏ");

        switch (type)
        {
            case CriterionTypeDto.Score:
            {
                var dto = DeserializeSettings<ScoreCriterionSettingsDto>(settings);
                await ValidateSettingsDtoAsync(scoreSettingsValidator, dto);

                if (dto.MaxValue != maxScore)
                    throw new BadRequestException("Invalid score mapping");

                ValidateRanges(dto.Ranges ?? [], dto.MinValue, dto.MaxValue, maxScore);

                return JsonSerializer.Serialize(dto);
            }
            case CriterionTypeDto.PassFail:
            {
                var dto = DeserializeSettings<PassFailCriterionSettingsDto>(settings);
                await ValidateSettingsDtoAsync(passFailSettingsValidator, dto);
                ValidateUniqueOptionValues(dto.Options);
                ValidateScoreMappings(dto.Options, dto.ScoreMappings, maxScore);

                return JsonSerializer.Serialize(dto);
            }
            case CriterionTypeDto.Option:
            {
                var dto = DeserializeSettings<OptionCriterionSettingsDto>(settings);
                await ValidateSettingsDtoAsync(optionSettingsValidator, dto);
                ValidateUniqueOptionValues(dto.Options);
                ValidateScoreMappings(dto.Options, dto.ScoreMappings, maxScore);

                return JsonSerializer.Serialize(dto);
            }
            default:
                throw new BadRequestException("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Р№ С‚РёРї РєСЂРёС‚РµСЂРёСЏ");
        }
    }

    public async Task<CriterionDto> CreateAsync(Guid criterionGroupId, CreateCriterionRequest request)
    {
        await ValidateAndThrowAsync(createValidator, request);

        var criterionGroup = await repository.GetCriterionGroupAsync(criterionGroupId);

        if (criterionGroup == null)
            throw new NotFoundException("Р“СЂСѓРїРїР° РєСЂРёС‚РµСЂРёРµРІ РЅРµ РЅР°Р№РґРµРЅР°");

        await EnsureTeacherOrAdmin(criterionGroup.Assignment.CourseId);

        var type = NormalizeType(request.Type);
        var settings = await ValidateSettingsAsync(type, request.Settings, request.MaxScore);

        var criterion = new Criterion
        {
            Id = Guid.NewGuid(),
            CriterionGroupId = criterionGroupId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Type = type,
            Settings = settings,
            MaxScore = request.MaxScore,
            SortOrder = request.SortOrder,
            CreatedAtUtc = DateTime.UtcNow
        };

        await repository.AddAsync(criterion);
        await repository.SaveChangesAsync();

        return Map(criterion);
    }

    public async Task<List<CriterionDto>> GetByCriterionGroupAsync(Guid criterionGroupId)
    {
        var criterionGroup = await repository.GetCriterionGroupAsync(criterionGroupId);

        if (criterionGroup == null)
            throw new NotFoundException("Р“СЂСѓРїРїР° РєСЂРёС‚РµСЂРёРµРІ РЅРµ РЅР°Р№РґРµРЅР°");

        await EnsureTeacherOrAdmin(criterionGroup.Assignment.CourseId);

        var criteria = await repository.GetByCriterionGroupIdAsync(criterionGroupId);

        return criteria.Select(Map).ToList();
    }

    public async Task<CriterionDto> UpdateAsync(Guid criterionId, UpdateCriterionRequest request)
    {
        await ValidateAndThrowAsync(updateValidator, request);

        var criterion = await repository.GetByIdAsync(criterionId);

        if (criterion == null)
            throw new NotFoundException("РљСЂРёС‚РµСЂРёР№ РЅРµ РЅР°Р№РґРµРЅ");

        await EnsureTeacherOrAdmin(criterion.CriterionGroup.Assignment.CourseId);

        var nextType = request.Type is null ? criterion.Type : NormalizeType(request.Type);
        var nextMaxScore = request.MaxScore ?? criterion.MaxScore;

        JsonElement nextSettings;
        if (request.Settings.HasValue)
        {
            nextSettings = request.Settings.Value;
        }
        else
        {
            using var existingSettingsDocument = JsonDocument.Parse(criterion.Settings);
            nextSettings = existingSettingsDocument.RootElement.Clone();
        }

        var validatedSettings = await ValidateSettingsAsync(nextType, nextSettings, nextMaxScore);

        if (request.Name != null)
            criterion.Name = request.Name.Trim();

        if (request.Description != null)
            criterion.Description = request.Description.Trim();

        if (request.SortOrder.HasValue)
            criterion.SortOrder = request.SortOrder.Value;

        criterion.Type = nextType;
        criterion.Settings = validatedSettings;
        criterion.MaxScore = nextMaxScore;

        await repository.SaveChangesAsync();

        return Map(criterion);
    }

    public async Task DeleteAsync(Guid criterionId)
    {
        var criterion = await repository.GetByIdAsync(criterionId);

        if (criterion == null)
            throw new NotFoundException("РљСЂРёС‚РµСЂРёР№ РЅРµ РЅР°Р№РґРµРЅ");

        await EnsureTeacherOrAdmin(criterion.CriterionGroup.Assignment.CourseId);

        repository.Delete(criterion);
        await repository.SaveChangesAsync();
    }
}
