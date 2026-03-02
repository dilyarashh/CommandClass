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
public class AuthController(AppDbContext db, AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user == null)
        {
            throw new BadRequestException("Пользователь с такой почтой не зарегистрирован");
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        
        if (result != PasswordVerificationResult.Success)
        {
            throw new BadRequestException("Неверный пароль");
        }
        
        var token = authService.GenerateToken(user);
        return Ok(new { token });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("Пользователь не был авторизован");
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