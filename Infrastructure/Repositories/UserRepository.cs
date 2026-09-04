using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly MoneyTrackDbContext _context;

    public UserRepository(MoneyTrackDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task<User?> GetByEmail(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task<User?> GetById(int? id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> Add(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task Update(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
