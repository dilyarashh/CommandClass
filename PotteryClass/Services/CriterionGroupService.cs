using FluentValidation;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;
using ValidationException = PotteryClass.Infrastructure.Errors.Exceptions.ValidationException;

namespace PotteryClass.Services;

public class CriterionGroupService(
    ICriterionGroupRepository repository,
    ICourseTeacherRepository teacherRepository,
    ICurrentUser currentUser,
    IValidator<CreateCriterionGroupRequest> createValidator,
    IValidator<UpdateCriterionGroupRequest> updateValidator)
    : ICriterionGroupService
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

    private static CriterionGroupDto Map(CriterionGroup criterionGroup)
    {
        return new CriterionGroupDto
        {
            Id = criterionGroup.Id,
            AssignmentId = criterionGroup.AssignmentId,
            Name = criterionGroup.Name,
            Description = criterionGroup.Description,
            SortOrder = criterionGroup.SortOrder,
            CreatedAtUtc = criterionGroup.CreatedAtUtc
        };
    }

    public async Task<CriterionGroupDto> CreateAsync(Guid assignmentId, CreateCriterionGroupRequest request)
    {
        await ValidateAndThrowAsync(createValidator, request);

        var assignment = await repository.GetAssignmentAsync(assignmentId);

        if (assignment == null)
            throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        var criterionGroup = new CriterionGroup
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder,
            CreatedAtUtc = DateTime.UtcNow
        };

        await repository.AddAsync(criterionGroup);
        await repository.SaveChangesAsync();

        return Map(criterionGroup);
    }

    public async Task<List<CriterionGroupDto>> GetByAssignmentAsync(Guid assignmentId)
    {
        var assignment = await repository.GetAssignmentAsync(assignmentId);

        if (assignment == null)
            throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        var criterionGroups = await repository.GetByAssignmentIdAsync(assignmentId);

        return criterionGroups.Select(Map).ToList();
    }

    public async Task<CriterionGroupDto> UpdateAsync(Guid criterionGroupId, UpdateCriterionGroupRequest request)
    {
        await ValidateAndThrowAsync(updateValidator, request);

        var criterionGroup = await repository.GetByIdAsync(criterionGroupId);

        if (criterionGroup == null)
            throw new NotFoundException("Группа критериев не найдена");

        var assignment = await repository.GetAssignmentAsync(criterionGroup.AssignmentId);

        if (assignment == null)
            throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        if (request.Name != null)
            criterionGroup.Name = request.Name.Trim();

        if (request.Description != null)
            criterionGroup.Description = request.Description.Trim();

        if (request.SortOrder.HasValue)
            criterionGroup.SortOrder = request.SortOrder.Value;

        await repository.SaveChangesAsync();

        return Map(criterionGroup);
    }

    public async Task DeleteAsync(Guid criterionGroupId)
    {
        var criterionGroup = await repository.GetByIdAsync(criterionGroupId);

        if (criterionGroup == null)
            throw new NotFoundException("Группа критериев не найдена");

        var assignment = await repository.GetAssignmentAsync(criterionGroup.AssignmentId);

        if (assignment == null)
            throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        repository.Delete(criterionGroup);
        await repository.SaveChangesAsync();
    }
}
