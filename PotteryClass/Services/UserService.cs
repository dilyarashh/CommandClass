using FluentValidation;
using Microsoft.AspNetCore.Identity;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using ValidationException = PotteryClass.Infrastructure.Errors.Exceptions.ValidationException;

namespace PotteryClass.Services;

public class UserService(
    IUserRepository userRepository,
    IValidator<RegistrationRequest> userValidator)
    : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IValidator<RegistrationRequest> _userValidator = userValidator;

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
}