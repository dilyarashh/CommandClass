using Microsoft.AspNetCore.Identity;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace PotteryClass.Data;

public static class DbSeeder
{
    public static async Task SeedInitialDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var passwordHasher = new PasswordHasher<User>();

        const string adminEmail = "admin@pottery.local";
        const string teacherEmail = "teacher@pottery.local";
        const string studentEmail = "student@pottery.local";

        var admin = await context.Users.FirstOrDefaultAsync(x => x.Email == adminEmail);
        if (admin == null)
        {
            admin = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "System",
                LastName = "Admin",
                MiddleName = "Course",
                Email = adminEmail,
                Role = UserRole.Admin
            };

            admin.PasswordHash = passwordHasher.HashPassword(admin, "12345678");
            context.Users.Add(admin);
        }

        var teacher = await context.Users.FirstOrDefaultAsync(x => x.Email == teacherEmail);
        if (teacher == null)
        {
            teacher = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Anna",
                LastName = "Teacher",
                MiddleName = "Ivanovna",
                Email = teacherEmail,
                Role = UserRole.Teacher
            };

            teacher.PasswordHash = passwordHasher.HashPassword(teacher, "12345678");
            context.Users.Add(teacher);
        }

        var student = await context.Users.FirstOrDefaultAsync(x => x.Email == studentEmail);
        if (student == null)
        {
            student = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Ivan",
                LastName = "Student",
                MiddleName = "Petrovich",
                Email = studentEmail,
                Role = UserRole.Student
            };

            student.PasswordHash = passwordHasher.HashPassword(student, "12345678");
            context.Users.Add(student);
        }

        var extraStudents = new List<User>();

        for (int i = 1; i <= 10; i++)
        {
            var email = $"student{i}@pottery.local";

            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = $"Student{i}",
                    LastName = "Seeder",
                    MiddleName = "Test",
                    Email = email,
                    Role = UserRole.Student
                };

                user.PasswordHash = passwordHasher.HashPassword(user, "12345678");
                context.Users.Add(user);
            }

            extraStudents.Add(user);
        }

        var extraTeachers = new List<User>();

        for (int i = 1; i <= 4; i++)
        {
            var email = $"teacher{i}@pottery.local";

            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = $"Teacher{i}",
                    LastName = "Seeder",
                    MiddleName = "Test",
                    Email = email,
                    Role = UserRole.Teacher
                };

                user.PasswordHash = passwordHasher.HashPassword(user, "12345678");
                context.Users.Add(user);
            }

            extraTeachers.Add(user);
        }

        await context.SaveChangesAsync();

        const string course1Code = "POTTERY-101";
        const string course2Code = "CERAMIC-201";

        var course1 = await context.Courses.FirstOrDefaultAsync(x => x.Code == course1Code);
        if (course1 == null)
        {
            course1 = new Course
            {
                Id = Guid.NewGuid(),
                Name = "Основы гончарного дела",
                Description = "Базовый курс по лепке и работе на гончарном круге",
                Code = course1Code,
                IsActive = true,
                CreatedByUserId = admin.Id,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-20)
            };

            context.Courses.Add(course1);
        }

        var course2 = await context.Courses.FirstOrDefaultAsync(x => x.Code == course2Code);
        if (course2 == null)
        {
            course2 = new Course
            {
                Id = Guid.NewGuid(),
                Name = "Продвинутая керамика",
                Description = "Курс по сложным техникам глазурования и обжига",
                Code = course2Code,
                IsActive = true,
                CreatedByUserId = admin.Id,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-10)
            };

            context.Courses.Add(course2);
        }

        await context.SaveChangesAsync();

        await AddTeacherIfNotExists(context, course1.Id, admin.Id, DateTime.UtcNow.AddDays(-20));
        await AddTeacherIfNotExists(context, course2.Id, admin.Id, DateTime.UtcNow.AddDays(-10));

        await AddTeacherIfNotExists(context, course1.Id, teacher.Id, DateTime.UtcNow.AddDays(-19));
        await AddTeacherIfNotExists(context, course2.Id, teacher.Id, DateTime.UtcNow.AddDays(-9));

        await AddTeacherIfNotExists(context, course1.Id, extraTeachers[0].Id, DateTime.UtcNow.AddDays(-18));
        await AddTeacherIfNotExists(context, course1.Id, extraTeachers[1].Id, DateTime.UtcNow.AddDays(-17));

        await AddTeacherIfNotExists(context, course2.Id, extraTeachers[2].Id, DateTime.UtcNow.AddDays(-7));
        await AddTeacherIfNotExists(context, course2.Id, extraTeachers[3].Id, DateTime.UtcNow.AddDays(-6));

        await AddStudentIfNotExists(context, course1.Id, student.Id, DateTime.UtcNow.AddDays(-18));
        await AddStudentIfNotExists(context, course2.Id, student.Id, DateTime.UtcNow.AddDays(-8));

        await AddStudentIfNotExists(context, course1.Id, extraStudents[0].Id, DateTime.UtcNow.AddDays(-18));
        await AddStudentIfNotExists(context, course1.Id, extraStudents[1].Id, DateTime.UtcNow.AddDays(-18));
        await AddStudentIfNotExists(context, course1.Id, extraStudents[2].Id, DateTime.UtcNow.AddDays(-17));
        await AddStudentIfNotExists(context, course1.Id, extraStudents[3].Id, DateTime.UtcNow.AddDays(-17));
        await AddStudentIfNotExists(context, course1.Id, extraStudents[4].Id, DateTime.UtcNow.AddDays(-16));

        await AddStudentIfNotExists(context, course2.Id, extraStudents[3].Id, DateTime.UtcNow.AddDays(-8));
        await AddStudentIfNotExists(context, course2.Id, extraStudents[4].Id, DateTime.UtcNow.AddDays(-8));
        await AddStudentIfNotExists(context, course2.Id, extraStudents[5].Id, DateTime.UtcNow.AddDays(-7));
        await AddStudentIfNotExists(context, course2.Id, extraStudents[6].Id, DateTime.UtcNow.AddDays(-7));
        await AddStudentIfNotExists(context, course2.Id, extraStudents[7].Id, DateTime.UtcNow.AddDays(-6));
        await AddStudentIfNotExists(context, course2.Id, extraStudents[8].Id, DateTime.UtcNow.AddDays(-6));
        await AddStudentIfNotExists(context, course2.Id, extraStudents[9].Id, DateTime.UtcNow.AddDays(-5));

        await context.SaveChangesAsync();

        var blockedStudent = await context.CourseStudents
            .FirstOrDefaultAsync(x => x.UserId == extraStudents[2].Id && x.CourseId == course1.Id);

        if (blockedStudent != null)
        {
            blockedStudent.IsBlocked = true;
        }

        await context.SaveChangesAsync();

        var assignment1 = await GetOrCreateAssignmentAsync(
            context,
            course1.Id,
            teacher.Id,
            "Подготовить глину",
            "Изучите виды глины и подготовьте материал для первой практики.",
            false,
            DateTime.UtcNow.AddDays(7),
            DateTime.UtcNow.AddDays(-5));

        var assignment2 = await GetOrCreateAssignmentAsync(
            context,
            course1.Id,
            admin.Id,
            "Сделать простую чашку",
            "Слепите простую чашку и загрузите фото результата.",
            true,
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(-3));

        var assignment3 = await GetOrCreateAssignmentAsync(
            context,
            course2.Id,
            teacher.Id,
            "Глазурование изделия",
            "Подберите глазурь и опишите, какой эффект хотите получить после обжига.",
            true,
            DateTime.UtcNow.AddDays(14),
            DateTime.UtcNow.AddDays(-2));

        var assignment4 = await GetOrCreateAssignmentAsync(
            context,
            course2.Id,
            admin.Id,
            "Форма высокой вазы",
            "Создайте эскиз высокой вазы и продумайте пропорции изделия.",
            false,
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(-1));

        await context.SaveChangesAsync();
    }

    private static async Task AddTeacherIfNotExists(AppDbContext context, Guid courseId, Guid userId, DateTime createdAtUtc)
    {
        var exists = await context.CourseTeachers
            .AnyAsync(x => x.CourseId == courseId && x.UserId == userId);

        if (!exists)
        {
            context.CourseTeachers.Add(new CourseTeacher
            {
                CourseId = courseId,
                UserId = userId,
                CreatedAtUtc = createdAtUtc
            });
        }
    }

    private static async Task AddStudentIfNotExists(AppDbContext context, Guid courseId, Guid userId, DateTime createdAtUtc)
    {
        var exists = await context.CourseStudents
            .AnyAsync(x => x.CourseId == courseId && x.UserId == userId);

        if (!exists)
        {
            context.CourseStudents.Add(new CourseStudent
            {
                CourseId = courseId,
                UserId = userId,
                IsBlocked = false,
                CreatedAtUtc = createdAtUtc
            });
        }
    }

    private static async Task<Assignment> GetOrCreateAssignmentAsync(AppDbContext context, Guid courseId, Guid createdById, string title,
        string text, bool requiresSubmission, DateTime? deadline, DateTime created)
    {
        var existing = await context.Assignments.FirstOrDefaultAsync(x =>
            x.CourseId == courseId &&
            x.Title == title);

        if (existing != null)
            return existing;

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            CreatedById = createdById,
            Title = title,
            Text = text,
            RequiresSubmission = requiresSubmission,
            Deadline = deadline,
            Created = created
        };

        context.Assignments.Add(assignment);
        return assignment;
    }
}