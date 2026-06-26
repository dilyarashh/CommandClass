using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PotteryClass.Data;
using PotteryClass.Data.Entities;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;
using LoginRequest = PotteryClass.Data.DTOs.LoginRequest;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext db,
    AuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadRequestException("Email and password are required");
        }

        var user = await db.Users
            .Where(x => x.Email == request.Email)
            .Select(x => new
            {
                x.Id,
                x.Email,
                x.PasswordHash,
                x.Role
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        var hasher = new PasswordHasher<User>();
        var userForPasswordCheck = new User
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            FirstName = string.Empty,
            LastName = string.Empty,
            MiddleName = string.Empty
        };

        PasswordVerificationResult result;

        try
        {
            result = hasher.VerifyHashedPassword(userForPasswordCheck, user.PasswordHash, request.Password);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to verify password hash for user {UserId}", user.Id);
            throw new UnauthorizedException("Invalid email or password");
        }

        if (result != PasswordVerificationResult.Success)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        var token = authService.GenerateToken(userForPasswordCheck);
        return Ok(new { token });
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("User is not authorized");
        }

        await db.BlackTokens.AddAsync(new BlackToken
        {
            Token = token,
            ExpiredAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return NoContent();
    }
}
