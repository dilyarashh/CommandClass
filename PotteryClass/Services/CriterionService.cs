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
    IValidator<UpdateCriterionRequest> updateValidator)
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
            throw new ForbiddenException("Нет доступа");
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
        return new CriterionDto
        {
            Id = criterion.Id,
            CriterionGroupId = criterion.CriterionGroupId,
            Name = criterion.Name,
            Description = criterion.Description,
            Type = criterion.Type,
            Settings = criterion.Settings,
            MaxScore = criterion.MaxScore,
            SortOrder = criterion.SortOrder,
            CreatedAtUtc = criterion.CreatedAtUtc
        };
    }

    private static string NormalizeType(string type)
        => type.Trim().ToLowerInvariant();

    private static int ReadRequiredInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var value))
            throw new BadRequestException("Некорректные настройки критерия");

        return value;
    }

    private static string ValidateSettings(string type, string? settings, int maxScore)
    {
        if (maxScore <= 0)
            throw new BadRequestException("Некорректные настройки критерия");

        switch (type)
        {
            case CriterionTypeDto.Score:
            {
                if (string.IsNullOrWhiteSpace(settings))
                    throw new BadRequestException("Некорректные настройки критерия");

                using var document = JsonDocument.Parse(settings);
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                    throw new BadRequestException("Некорректные настройки критерия");

                var minValue = ReadRequiredInt(root, "minValue");
                var maxValue = ReadRequiredInt(root, "maxValue");

                if (minValue < 0 || maxValue < minValue || maxValue != maxScore)
                    throw new BadRequestException("Некорректные настройки критерия");

                return root.GetRawText();
            }
            case CriterionTypeDto.PassFail:
            {
                if (string.IsNullOrWhiteSpace(settings))
                    return "{}";

                using var document = JsonDocument.Parse(settings);
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object || root.EnumerateObject().Any())
                    throw new BadRequestException("Некорректные настройки критерия");

                return "{}";
            }
            default:
                throw new BadRequestException("Некорректный тип критерия");
        }
    }

    public async Task<CriterionDto> CreateAsync(Guid criterionGroupId, CreateCriterionRequest request)
    {
        await ValidateAndThrowAsync(createValidator, request);

        var criterionGroup = await repository.GetCriterionGroupAsync(criterionGroupId);

        if (criterionGroup == null)
            throw new NotFoundException("Группа критериев не найдена");

        await EnsureTeacherOrAdmin(criterionGroup.Assignment.CourseId);

        var type = NormalizeType(request.Type);
        var settings = ValidateSettings(type, request.Settings, request.MaxScore);

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
            throw new NotFoundException("Группа критериев не найдена");

        await EnsureTeacherOrAdmin(criterionGroup.Assignment.CourseId);

        var criteria = await repository.GetByCriterionGroupIdAsync(criterionGroupId);

        return criteria.Select(Map).ToList();
    }

    public async Task<CriterionDto> UpdateAsync(Guid criterionId, UpdateCriterionRequest request)
    {
        await ValidateAndThrowAsync(updateValidator, request);

        var criterion = await repository.GetByIdAsync(criterionId);

        if (criterion == null)
            throw new NotFoundException("Критерий не найден");

        await EnsureTeacherOrAdmin(criterion.CriterionGroup.Assignment.CourseId);

        var nextType = request.Type is null ? criterion.Type : NormalizeType(request.Type);
        var nextSettings = request.Settings ?? criterion.Settings;
        var nextMaxScore = request.MaxScore ?? criterion.MaxScore;

        var validatedSettings = ValidateSettings(nextType, nextSettings, nextMaxScore);

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
            throw new NotFoundException("Критерий не найден");

        await EnsureTeacherOrAdmin(criterion.CriterionGroup.Assignment.CourseId);

        repository.Delete(criterion);
        await repository.SaveChangesAsync();
    }
}
