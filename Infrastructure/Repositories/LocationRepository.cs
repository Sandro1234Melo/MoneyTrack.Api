using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Application.Dtos.Locations;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Infrastructure.Data;

public class LocationRepository : ILocationRepository
{
    private readonly MoneyTrackDbContext _context;

    public LocationRepository(MoneyTrackDbContext context)
    {
        _context = context;
    }

    public async Task<List<Location>> GetAll()
    {
        return await _context.Locations
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<List<Location>> GetByUser(int userId)
    {
        return await _context.Locations
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<List<LocationResponseDto>> GetByUserWithStats(int userId)
    {
        var locations = await _context.Locations
            .AsNoTracking()
            .Include(l => l.User)
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.Name)
            .ToListAsync();

        var expenses = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Items)
            .Where(e => e.UserId == userId && e.LocationId.HasValue)
            .ToListAsync();

        return locations.Select(location =>
        {
            var locationExpenses = expenses
                .Where(e => e.LocationId == location.Id)
                .ToList();

            var totalPurchases = locationExpenses.Count;
            var totalSpent = locationExpenses
                .SelectMany(e => e.Items)
                .Sum(i => i.Amount);

            return new LocationResponseDto
            {
                Id = location.Id,
                Name = location.Name,
                UserName = location.User?.FullName,
                TotalPurchases = totalPurchases,
                TotalSpent = totalSpent,
                AveragePerPurchase = totalPurchases > 0 ? Math.Round(totalSpent / totalPurchases, 2) : 0,
                LastPurchaseDate = locationExpenses
                    .OrderByDescending(e => e.Date)
                    .Select(e => (DateTime?)e.Date)
                    .FirstOrDefault()
            };
        })
        .OrderByDescending(l => l.TotalSpent)
        .ThenBy(l => l.Name)
        .ToList();
    }

    public async Task<Location?> GetById(int id)
    {
        return await _context.Locations.FindAsync(id);
    }

    public async Task<Location> Add(Location location)
    {
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    public async Task Update(Location location)
    {
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Location location)
    {
        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int userId, string name)
    {
        var normalizedName = name.Trim().ToLower();

        return await _context.Locations
            .AnyAsync(l => l.UserId == userId && l.Name.ToLower() == normalizedName);
    }
}
