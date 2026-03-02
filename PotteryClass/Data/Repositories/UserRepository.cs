using Microsoft.EntityFrameworkCore;
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
}