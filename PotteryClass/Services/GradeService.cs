using System.Text.Json;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class GradeService(
    ISubmissionRepository submissionRepo,
    ISubmissionAssessmentRepository assessmentRepository,
    IAssignmentRepository assignmentRepo,
    ICriterionRepository criterionRepository,
    IAssignmentTeamRepository assignmentTeamRepository,
    ICourseRepository courseRepo,
    IGradeCalculationService gradeCalculationService,
    ICurrentUser currentUser)
    : IGradeService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private async Task<(Data.Entities.Assignment Assignment, Data.Entities.Course Course)> GetAssignmentAndCourseAsync(Guid assignmentId)
    {
        var assignment = await assignmentRepo.GetByIdAsync(assignmentId)
                         ?? throw new NotFoundException("Задание не найдено");

        var course = await courseRepo.GetByIdAsync(assignment.CourseId)
                     ?? throw new NotFoundException("Курс не найден");

        return (assignment, course);
    }

    private async Task<(Data.Entities.Submission Submission, Data.Entities.Assignment Assignment, Data.Entities.Course Course)>
        GetSubmissionAssignmentAndCourseAsync(Guid submissionId)
    {
        var submission = await submissionRepo.GetByIdAsync(submissionId)
                         ?? throw new NotFoundException("Решение не найдено");

        var assignment = await assignmentRepo.GetByIdAsync(submission.AssignmentId)
                         ?? throw new NotFoundException("Задание не найдено");

        var course = await courseRepo.GetByIdAsync(assignment.CourseId)
                     ?? throw new NotFoundException("Курс не найден");

        return (submission, assignment, course);
    }

    private void EnsureTeacherOrAdmin(Data.Entities.Course course)
    {
        var teacherId = currentUser.GetUserId();
        var role = currentUser.GetRole();
        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (role != UserRole.Admin && !isTeacher)
            throw new ForbiddenException("Только преподаватель курса или администратор может выполнять это действие");
    }

    private void EnsureStudentOrAdmin(Data.Entities.Course course)
    {
        var studentId = currentUser.GetUserId();
        var role = currentUser.GetRole();

        if (role == UserRole.Admin)
            return;

        var student = course.Students.FirstOrDefault(s => s.UserId == studentId);
        if (student == null || student.IsBlocked)
            throw new ForbiddenException("Нет доступа");
    }

    private static SubmissionGradeDto MapGrade(Data.Entities.Submission submission)
    {
        return new SubmissionGradeDto
        {
            SubmissionId = submission.Id,
            AssignmentId = submission.AssignmentId,
            StudentId = submission.StudentId,
            Grade = ResolveGrade(submission),
            TeacherComment = submission.TeacherComment,
            GradedByTeacherId = submission.GradedByTeacherId,
            GradedAtUtc = submission.GradedAtUtc
        };
    }

    private static int? ResolveGrade(Data.Entities.Submission? submission)
    {
        if (submission == null)
            return null;

        return submission.Assessment is null
            ? submission.Grade
            : decimal.ToInt32(decimal.Round(submission.Assessment.FinalGrade, 0, MidpointRounding.AwayFromZero));
    }

    private static AssignmentGradingRulesDto DeserializeGradingRules(string? gradingRules)
    {
        if (string.IsNullOrWhiteSpace(gradingRules))
            return new AssignmentGradingRulesDto();

        try
        {
            return JsonSerializer.Deserialize<AssignmentGradingRulesDto>(gradingRules, JsonOptions)
                   ?? new AssignmentGradingRulesDto();
        }
        catch (JsonException)
        {
            throw new BadRequestException("Invalid assignment grading rules");
        }
    }

    private static CriterionDto MapCriterion(Criterion criterion)
    {
        using var document = JsonDocument.Parse(criterion.Settings);

        return new CriterionDto
        {
            Id = criterion.Id,
            CriterionGroupId = criterion.CriterionGroupId,
            Name = criterion.Name,
            Description = criterion.Description,
            Type = criterion.Type,
            Category = criterion.Category,
            Settings = document.RootElement.Clone(),
            MaxScore = criterion.MaxScore,
            SortOrder = criterion.SortOrder,
            CreatedAtUtc = criterion.CreatedAtUtc
        };
    }

    private static SubmissionAssessmentDto MapAssessment(SubmissionAssessment assessment)
    {
        using var valuesDocument = JsonDocument.Parse(assessment.CriterionValues);
        using var detailsDocument = JsonDocument.Parse(assessment.CalculationDetails);

        return new SubmissionAssessmentDto
        {
            Id = assessment.Id,
            SubmissionId = assessment.SubmissionId,
            AssignmentId = assessment.AssignmentId,
            StudentId = assessment.StudentId,
            CheckedByUserId = assessment.CheckedByUserId,
            CriterionValues = valuesDocument.RootElement.Clone(),
            MainPoints = assessment.MainPoints,
            BonusPoints = assessment.BonusPoints,
            PenaltyPoints = assessment.PenaltyPoints,
            Multiplier = assessment.Multiplier,
            FinalGrade = assessment.FinalGrade,
            CalculationDetails = detailsDocument.RootElement.Clone(),
            CheckedAtUtc = assessment.CheckedAtUtc,
            Comment = assessment.Comment
        };
    }

    private static SubmissionAssessmentCriterionGroupDto MapAssessmentGroup(IGrouping<CriterionGroup, Criterion> group)
    {
        return new SubmissionAssessmentCriterionGroupDto
        {
            Id = group.Key.Id,
            Name = group.Key.Name,
            Description = group.Key.Description,
            SortOrder = group.Key.SortOrder,
            Criteria = group
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CreatedAtUtc)
                .Select(MapCriterion)
                .ToList()
        };
    }

    private static SubmissionDto MapSubmission(Data.Entities.Submission submission)
    {
        return new SubmissionDto
        {
            Id = submission.Id,
            AssignmentId = submission.AssignmentId,
            StudentId = submission.StudentId,
            FirstName = submission.Student?.FirstName,
            LastName = submission.Student?.LastName,
            MiddleName = submission.Student?.MiddleName,
            Created = submission.Created,
            Grade = ResolveGrade(submission),
            CalculatedGrade = submission.Assessment?.FinalGrade,
            TeacherComment = submission.TeacherComment,
            GradedByTeacherId = submission.GradedByTeacherId,
            GradedAtUtc = submission.GradedAtUtc,
            Status = submission.Status,
            Files = submission.Files.Select(f => new SubmissionFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                Url = f.Url,
                MimeType = f.MimeType,
                Size = f.Size,
                Type = f.Type
            }).ToList()
        };
    }

    private static AssignmentTeamGradeDto MapTeamGrade(
        Data.Entities.AssignmentTeam team,
        IReadOnlyDictionary<Guid, Data.Entities.Submission> latestSubmissionsByStudentId,
        IReadOnlyCollection<Data.Entities.Submission> allTeamSubmissions)
    {
        var members = team.Members
            .OrderBy(x => x.User.LastName)
            .ThenBy(x => x.User.FirstName)
            .Select(member =>
            {
                latestSubmissionsByStudentId.TryGetValue(member.UserId, out var submission);
                var grade = ResolveGrade(submission);

                return new TeamGradeMemberDto
                {
                    StudentId = member.UserId,
                    FirstName = member.User.FirstName,
                    LastName = member.User.LastName,
                    MiddleName = member.User.MiddleName,
                    SubmissionId = submission?.Id,
                    Grade = grade,
                    CalculatedGrade = submission?.Assessment?.FinalGrade,
                    TeacherComment = submission?.TeacherComment
                };
            })
            .ToList();

        decimal? teamGrade = null;
        if (members.Count > 0)
        {
            var total = members.Sum(x => (decimal)(x.Grade ?? 0));
            teamGrade = Math.Round(total / members.Count, 2);
        }

        Data.Entities.Submission? finalSubmission = null;
        if (team.FinalSubmissionId.HasValue)
            finalSubmission = allTeamSubmissions.FirstOrDefault(x => x.Id == team.FinalSubmissionId.Value);

        return new AssignmentTeamGradeDto
        {
            TeamId = team.Id,
            AssignmentId = team.AssignmentId,
            TeamName = team.Name,
            FinalSubmissionId = team.FinalSubmissionId,
            TeamGrade = teamGrade,
            FinalSubmission = finalSubmission is null ? null : MapSubmission(finalSubmission),
            Members = members
        };
    }

    public async Task<SubmissionGradeDto> SetGradeAsync(Guid submissionId, SetSubmissionGradeRequest dto)
    {
        var submission = await submissionRepo.GetByIdAsync(submissionId);

        if (submission == null)
        {
            throw new NotFoundException("Решение не найдено");
        }

        var assignment = await assignmentRepo.GetByIdAsync(submission.AssignmentId);

        if (assignment == null)
        {
            throw new NotFoundException("Задание не найдено");
        }

        var course = await courseRepo.GetByIdAsync(assignment.CourseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var teacherId = currentUser.GetUserId();
        var role = currentUser.GetRole();

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (role != UserRole.Admin && !isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса или администратор может ставить оценки");
        }

        submission.Grade = dto.Value;
        submission.TeacherComment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();
        submission.GradedByTeacherId = teacherId;
        submission.GradedAtUtc = DateTime.UtcNow;

        if (submission.Assessment != null)
        {
            assessmentRepository.Delete(submission.Assessment);
            submission.Assessment = null;
        }

        await submissionRepo.SaveChangesAsync();

        return MapGrade(submission);
    }

    public async Task<SubmissionAssessmentFormDto> GetAssessmentFormAsync(Guid submissionId)
    {
        var (submission, assignment, course) = await GetSubmissionAssignmentAndCourseAsync(submissionId);
        EnsureTeacherOrAdmin(course);

        var criteria = await criterionRepository.GetByAssignmentIdAsync(assignment.Id);
        var savedAssessment = await assessmentRepository.GetBySubmissionIdAsync(submissionId);

        return new SubmissionAssessmentFormDto
        {
            SubmissionId = submission.Id,
            AssignmentId = assignment.Id,
            StudentId = submission.StudentId,
            Rules = DeserializeGradingRules(assignment.GradingRules),
            Groups = criteria
                .Where(x => x.CriterionGroup != null)
                .GroupBy(x => x.CriterionGroup)
                .OrderBy(x => x.Key.SortOrder)
                .ThenBy(x => x.Key.CreatedAtUtc)
                .Select(MapAssessmentGroup)
                .ToList(),
            SavedAssessment = savedAssessment is null ? null : MapAssessment(savedAssessment)
        };
    }

    public async Task<SubmissionAssessmentDto> GetAssessmentAsync(Guid submissionId)
    {
        var (_, _, course) = await GetSubmissionAssignmentAndCourseAsync(submissionId);
        EnsureTeacherOrAdmin(course);

        var assessment = await assessmentRepository.GetBySubmissionIdAsync(submissionId)
                         ?? throw new NotFoundException("Проверка решения не найдена");

        return MapAssessment(assessment);
    }

    public async Task<SubmissionAssessmentDto> SaveAssessmentAsync(Guid submissionId, SaveSubmissionAssessmentRequest dto)
    {
        var (submission, assignment, course) = await GetSubmissionAssignmentAndCourseAsync(submissionId);
        EnsureTeacherOrAdmin(course);

        var criteria = await criterionRepository.GetByAssignmentIdAsync(assignment.Id);
        var calculation = gradeCalculationService.Calculate(new GradeCalculationRequest
        {
            Rules = DeserializeGradingRules(assignment.GradingRules),
            Criteria = criteria.Select(MapCriterion).ToList(),
            Values = dto.Values,
            Penalties = dto.Penalties
        });

        var checkedByUserId = currentUser.GetUserId();
        var checkedAtUtc = DateTime.UtcNow;
        var comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();
        var criterionValues = JsonSerializer.Serialize(dto.Values, JsonOptions);
        var calculationDetails = JsonSerializer.Serialize(calculation, JsonOptions);

        var assessment = await assessmentRepository.GetBySubmissionIdAsync(submissionId);

        if (assessment == null)
        {
            assessment = new SubmissionAssessment
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                AssignmentId = submission.AssignmentId,
                StudentId = submission.StudentId
            };

            await assessmentRepository.AddAsync(assessment);
        }

        assessment.CheckedByUserId = checkedByUserId;
        assessment.CriterionValues = criterionValues;
        assessment.MainPoints = calculation.MainPoints;
        assessment.BonusPoints = calculation.BonusPoints;
        assessment.PenaltyPoints = calculation.PenaltyPoints;
        assessment.Multiplier = calculation.Multiplier;
        assessment.FinalGrade = calculation.FinalGrade;
        assessment.CalculationDetails = calculationDetails;
        assessment.CheckedAtUtc = checkedAtUtc;
        assessment.Comment = comment;

        submission.Grade = decimal.ToInt32(decimal.Round(calculation.FinalGrade, 0, MidpointRounding.AwayFromZero));
        submission.TeacherComment = comment;
        submission.GradedByTeacherId = checkedByUserId;
        submission.GradedAtUtc = checkedAtUtc;

        await assessmentRepository.SaveChangesAsync();

        return MapAssessment(assessment);
    }

    public async Task DeleteGradeAsync(Guid submissionId)
    {
        var submission = await submissionRepo.GetByIdAsync(submissionId);

        if (submission == null)
        {
            throw new NotFoundException("Решение не найдено");
        }

        var assignment = await assignmentRepo.GetByIdAsync(submission.AssignmentId);

        if (assignment == null)
        {
            throw new NotFoundException("Задание не найдено");
        }

        var course = await courseRepo.GetByIdAsync(assignment.CourseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var teacherId = currentUser.GetUserId();
        var role = currentUser.GetRole();

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (role != UserRole.Admin && !isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса или администратор может удалять оценки");
        }

        submission.Grade = null;
        submission.TeacherComment = null;
        submission.GradedByTeacherId = null;
        submission.GradedAtUtc = null;

        if (submission.Assessment != null)
        {
            assessmentRepository.Delete(submission.Assessment);
            submission.Assessment = null;
        }

        await submissionRepo.SaveChangesAsync();
    }

    public async Task<List<CourseStudentGradeDto>> GetCourseGradesAsync(Guid courseId)
    {
        var course = await courseRepo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var teacherId = currentUser.GetUserId();
        var role = currentUser.GetRole();

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (role != UserRole.Admin && !isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса или администратор может смотреть успеваемость");
        }

        return await submissionRepo.GetCourseGradesAsync(courseId);
    }

    public async Task<List<MyCourseGradeDto>> GetMyCourseGradesAsync(Guid courseId)
    {
        var course = await courseRepo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var studentId = currentUser.GetUserId();
        var role = currentUser.GetRole();

        if (role == UserRole.Admin)
        {
            return await submissionRepo.GetStudentCourseGradesAsync(courseId, studentId);
        }

        var student = course.Students.FirstOrDefault(s => s.UserId == studentId);

        if (student == null)
        {
            throw new ForbiddenException("Только студент курса может смотреть свои оценки");
        }

        if (student.IsBlocked)
        {
            throw new ForbiddenException("Вы заблокированы на курсе");
        }

        return await submissionRepo.GetStudentCourseGradesAsync(courseId, studentId);
    }

    public async Task<List<AssignmentTeamGradeDto>> GetAssignmentTeamGradesAsync(Guid assignmentId)
    {
        var (_, course) = await GetAssignmentAndCourseAsync(assignmentId);
        EnsureTeacherOrAdmin(course);

        var teams = await assignmentTeamRepository.GetByAssignmentAsync(assignmentId);
        var studentIds = teams.SelectMany(x => x.Members).Select(x => x.UserId).Distinct().ToList();
        var submissions = await submissionRepo.GetByAssignmentAndStudentsAsync(assignmentId, studentIds);
        var latestSubmissions = submissions
            .GroupBy(x => x.StudentId)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(s => s.Created).First());

        return teams.Select(team => MapTeamGrade(team, latestSubmissions, submissions)).ToList();
    }

    public async Task<AssignmentTeamGradeDto> GetMyTeamGradeAsync(Guid assignmentId)
    {
        var (_, course) = await GetAssignmentAndCourseAsync(assignmentId);
        EnsureStudentOrAdmin(course);

        var studentId = currentUser.GetUserId();
        var team = await assignmentTeamRepository.GetStudentTeamAsync(assignmentId, studentId)
                   ?? throw new NotFoundException("Команда пользователя для задания не найдена");

        var memberIds = team.Members.Select(x => x.UserId).Distinct().ToList();
        var submissions = await submissionRepo.GetByAssignmentAndStudentsAsync(assignmentId, memberIds);
        var latestSubmissions = submissions
            .GroupBy(x => x.StudentId)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(s => s.Created).First());

        return MapTeamGrade(team, latestSubmissions, submissions);
    }
}
