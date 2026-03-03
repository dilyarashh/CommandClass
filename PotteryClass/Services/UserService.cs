using FluentValidation;
using Microsoft.AspNetCore.Identity;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;
using ValidationException = PotteryClass.Infrastructure.Errors.Exceptions.ValidationException;

namespace PotteryClass.Services;

public class UserService(
    IUserRepository userRepository,
    IValidator<RegistrationRequest> userValidator,
    ICurrentUser currentUser)
    : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IValidator<RegistrationRequest> _userValidator = userValidator;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<User> CreateUserAsync(RegistrationRequest dto)
    {
        var validationResult = await _userValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            throw new ValidationException(errors);
        }
        
        var passwordHasher = new PasswordHasher<User>();
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            Email = dto.Email,
            Role = UserRole.Student,
            Created = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);
        await _userRepository.AddAsync(user);

        return user;
    }
    
    public async Task<UserDto> GetCurrentUserAsync()
    {
        var userId = _currentUser.GetUserId();

        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new NotFoundException("Пользователь не найден");

        return Map(user);
    }
    
    public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequest dto)
    {
        var userId = _currentUser.GetUserId();

        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new NotFoundException("Пользователь не найден");

        if (dto.FirstName is not null)
            user.FirstName = dto.FirstName;

        if (dto.LastName is not null)
            user.LastName = dto.LastName;

        if (dto.MiddleName is not null)
            user.MiddleName = dto.MiddleName;

        if (dto.Email is not null)
            user.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);
        }

        await _userRepository.UpdateAsync(user);
        
        return Map(user);
    }
    
    public async Task DeleteCurrentUserAsync()
    {
        var userId = _currentUser.GetUserId();

        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new NotFoundException("Пользователь не найден");

        await _userRepository.DeleteAsync(user);
    }
    
    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id)
                   ?? throw new NotFoundException("Пользователь не найден");

        return Map(user);
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(UsersQuery query)
    {
        var result = await _userRepository.GetAllAsync(query);

        return new PagedResult<UserDto>
        {
            TotalCount = result.TotalCount,
            Items = result.Items.Select(Map).ToList()
        };
    }

    private static UserDto Map(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        MiddleName = user.MiddleName,
        Email = user.Email,
        Role = user.Role
    };
}