using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Infrastructure.Data;

public class ExpenseRepository : IExpenseRepository
{
    private readonly MoneyTrackDbContext _context;

    public ExpenseRepository(MoneyTrackDbContext context)
    {
        _context = context;
    }

    public async Task<List<Expense>> GetByUser(int userId)
    {
        return await _context.Expenses
            .Include(e => e.Items)
                .ThenInclude(i => i.Category)
            .Include(e => e.Location)
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<Expense?> GetById(int id)
    {
        return await _context.Expenses
            .Include(e => e.Items)
                .ThenInclude(i => i.Category)
            .Include(e => e.Location)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Expense> Add(Expense expense)
    {
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task Update(Expense expense)
    {
        _context.Expenses.Update(expense);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Expense expense)
    {
        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
    }

    private static DateTime ToUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
            return value;

        if (value.Kind == DateTimeKind.Local)
            return value.ToUniversalTime();

        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public async Task<List<Expense>> GetFiltered(ExpenseFilterDto filter)
    {
        var query = _context.Expenses
            .Include(e => e.Items)
                .ThenInclude(i => i.Category)
            .Include(e => e.Location)
            .AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(e => e.UserId == filter.UserId.Value);

        if (filter.NoteId.HasValue)
            query = query.Where(e => e.Id == filter.NoteId.Value);

        if (filter.From.HasValue)
        {
            var from = ToUtc(filter.From.Value.Date);
            query = query.Where(e => e.Date >= from);
        }

        if (filter.To.HasValue)
        {
            var to = ToUtc(filter.To.Value.Date.AddDays(1).AddTicks(-1));
            query = query.Where(e => e.Date <= to);
        }

        if (filter.LocationId.HasValue)
            query = query.Where(e => e.LocationId == filter.LocationId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(e =>
                e.Items.Any(i => i.CategoryId == filter.CategoryId.Value)
            );

        if (!string.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(e =>
                e.Items.Any(i =>
                    i.Description.ToLower().Contains(filter.Description.ToLower())
                )
            );

        if (filter.Min.HasValue)
            query = query.Where(e =>
                e.Items.Sum(i => i.Amount) >= filter.Min.Value
            );

        if (filter.Max.HasValue)
            query = query.Where(e =>
                e.Items.Sum(i => i.Amount) <= filter.Max.Value
            );

        return await query
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }
}