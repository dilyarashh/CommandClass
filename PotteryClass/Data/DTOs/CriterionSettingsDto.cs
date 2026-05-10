namespace PotteryClass.Data.DTOs;

/// <summary>
/// Вариант значения критерия.
/// </summary>
public class CriterionOptionDto
{
    public string Value { get; set; } = null!;
    public string? Label { get; set; }
}

/// <summary>
/// Диапазон значений с соответствующим количеством баллов.
/// </summary>
public class CriterionRangeDto
{
    public int From { get; set; }
    public int To { get; set; }
    public int Score { get; set; }
}

/// <summary>
/// Соответствие значения и количества баллов.
/// </summary>
public class CriterionScoreMappingDto
{
    public string Value { get; set; } = null!;
    public int Score { get; set; }
}

/// <summary>
/// Настройки критерия типа score.
/// Пример: { "minValue": 0, "maxValue": 10, "multiplier": 1.2, "ranges": [{ "from": 0, "to": 3, "score": 2 }] }
/// </summary>
public class ScoreCriterionSettingsDto
{
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public decimal? Multiplier { get; set; }
    public List<CriterionRangeDto>? Ranges { get; set; }
}

/// <summary>
/// Настройки критерия типа pass_fail.
/// Пример: { "multiplier": 1, "options": [{ "value": "pass", "label": "Pass" }, { "value": "fail", "label": "Fail" }], "scoreMappings": [{ "value": "pass", "score": 5 }, { "value": "fail", "score": 0 }] }
/// </summary>
public class PassFailCriterionSettingsDto
{
    public decimal? Multiplier { get; set; }
    public List<CriterionOptionDto> Options { get; set; } = new();
    public List<CriterionScoreMappingDto> ScoreMappings { get; set; } = new();
}

/// <summary>
/// Настройки критерия типа option.
/// Пример: { "multiplier": 0.5, "options": [{ "value": "a", "label": "A" }, { "value": "b", "label": "B" }], "scoreMappings": [{ "value": "a", "score": 10 }, { "value": "b", "score": 6 }] }
/// </summary>
public class OptionCriterionSettingsDto
{
    public decimal? Multiplier { get; set; }
    public List<CriterionOptionDto> Options { get; set; } = new();
    public List<CriterionScoreMappingDto> ScoreMappings { get; set; } = new();
}
