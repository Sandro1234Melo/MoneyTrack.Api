using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Infrastructure.Data;

public class CategoryRepository : ICategoryRepository
{
    private readonly MoneyTrackDbContext _context;

    public CategoryRepository(MoneyTrackDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetByUserId(int userId)
    {
        return await _context.Categories
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<Category?> GetById(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> Exists(int userId, string name)
    {
        return await _context.Categories
            .AnyAsync(c => c.UserId == userId && c.Name == name);
    }

    public async Task<Category> Add(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task Update(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}