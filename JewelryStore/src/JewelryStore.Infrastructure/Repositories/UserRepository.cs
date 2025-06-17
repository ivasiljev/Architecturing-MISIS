using Microsoft.EntityFrameworkCore;
using JewelryStore.Core.Entities;
using JewelryStore.Core.Interfaces;
using JewelryStore.Infrastructure.Data;

namespace JewelryStore.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly JewelryStoreContext _context;

    public UserRepository(JewelryStoreContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string username, string email)
    {
        return await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
    }
}