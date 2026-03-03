using PotteryClass.Data.DTOs;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;

namespace PotteryClass.Services;

public class CourseService(ICourseRepository repo, ICurrentUser currentUser, ICourseCodeGenerator codeGen) : ICourseService
{
    public Task<CourseDto> CreateCourseAsync(CreateCourseRequest dto) => throw new NotImplementedException();
}