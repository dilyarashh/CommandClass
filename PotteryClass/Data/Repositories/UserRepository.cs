using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
    
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await db.Users.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }
    
    public async Task<PagedResult<User>> GetAllAsync(UsersQuery query)
    {
        var users = db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            users = users.Where(x =>
                x.FirstName.Contains(query.Search) ||
                x.LastName.Contains(query.Search) ||
                x.Email.Contains(query.Search));
        }

        if (query.Role.HasValue)
        {
            users = users.Where(x => x.Role == query.Role.Value);
        }

        users = query.SortBy?.ToLower() switch
        {
            "firstname" => query.Desc
                ? users.OrderByDescending(x => x.FirstName)
                : users.OrderBy(x => x.FirstName),

            "lastname" => query.Desc
                ? users.OrderByDescending(x => x.LastName)
                : users.OrderBy(x => x.LastName),

            "email" => query.Desc
                ? users.OrderByDescending(x => x.Email)
                : users.OrderBy(x => x.Email),

            _ => query.Desc
                ? users.OrderByDescending(x => x.Created)
                : users.OrderBy(x => x.Created)
        };

        var total = await users.CountAsync();

        var items = await users
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = total
        };
    }
}