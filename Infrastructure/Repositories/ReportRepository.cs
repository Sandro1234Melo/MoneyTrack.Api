using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Infrastructure.Data;

public class ReportRepository : IReportRepository
{
    private readonly MoneyTrackDbContext _context;

    public ReportRepository(MoneyTrackDbContext context)
    {
        _context = context;
    }

    private static DateTime AsUtcDate(DateTime value)
    {
        // O Npgsql exige DateTime com Kind=Utc quando a coluna no PostgreSQL e timestamptz.
        // Datas vindas da query string chegam como Unspecified, por isso normalizamos aqui.
        var dateOnly = value.Date;
        return DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
    }

    private static (DateTime From, DateTime ToExclusive) NormalizeRange(DateTime? from, DateTime? to)
    {
        var today = DateTime.UtcNow.Date;
        var finalFrom = AsUtcDate(from ?? today.AddDays(-29));
        var finalTo = AsUtcDate(to ?? today).AddDays(1);

        if (finalTo <= finalFrom)
            finalTo = finalFrom.AddDays(1);

        return (finalFrom, finalTo);
    }

    public async Task<object> GetDashboardSummary(int userId, DateTime? from, DateTime? to)
    {
        var range = NormalizeRange(from, to);
        var fromDate = range.From;
        var toExclusive = range.ToExclusive;
        var daysInFilter = Math.Max(1, (toExclusive.Date - fromDate.Date).Days);

        var currentExpensesQuery = _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= fromDate && e.Date < toExclusive);

        var totalExpense = await currentExpensesQuery
            .SelectMany(e => e.Items)
            .SumAsync(i => (decimal?)i.Amount) ?? 0m;

        var totalPurchases = await currentExpensesQuery.CountAsync();

        var distinctLocations = await currentExpensesQuery
            .Where(e => e.LocationId.HasValue)
            .Select(e => e.LocationId!.Value)
            .Distinct()
            .CountAsync();

        var distinctCategories = await currentExpensesQuery
            .SelectMany(e => e.Items)
            .Select(i => i.CategoryId)
            .Distinct()
            .CountAsync();

        var previousFrom = fromDate.AddDays(-daysInFilter);
        var previousToExclusive = fromDate;

        var previousTotal = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= previousFrom && e.Date < previousToExclusive)
            .SelectMany(e => e.Items)
            .SumAsync(i => (decimal?)i.Amount) ?? 0m;

        decimal totalTrendPercent = previousTotal > 0
            ? Math.Round(((totalExpense - previousTotal) / previousTotal) * 100m, 2)
            : totalExpense > 0 ? 100m : 0m;

        var categoryRowsRaw = await currentExpensesQuery
            .SelectMany(e => e.Items)
            .GroupBy(i => new { i.CategoryId, i.Category.Name })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Total = g.Sum(i => i.Amount),
                ItemsCount = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        var categoryExpenses = categoryRowsRaw
            .Select(x => new
            {
                x.CategoryId,
                x.CategoryName,
                Name = x.CategoryName,
                Value = x.Total,
                x.Total,
                x.ItemsCount,
                Percentage = totalExpense > 0 ? Math.Round((x.Total / totalExpense) * 100m, 2) : 0m
            })
            .ToList();

        var locationRowsRaw = await currentExpensesQuery
            .GroupBy(e => new { e.LocationId, LocationName = e.Location != null ? e.Location.Name : "Sem local" })
            .Select(g => new
            {
                LocationId = g.Key.LocationId,
                LocationName = g.Key.LocationName,
                Total = g.SelectMany(e => e.Items).Sum(i => i.Amount),
                PurchasesCount = g.Count(),
                ItemsCount = g.SelectMany(e => e.Items).Count()
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        var locationExpenses = locationRowsRaw
            .Select(x => new
            {
                x.LocationId,
                x.LocationName,
                Name = x.LocationName,
                Value = x.Total,
                x.Total,
                x.PurchasesCount,
                x.ItemsCount,
                Percentage = totalExpense > 0 ? Math.Round((x.Total / totalExpense) * 100m, 2) : 0m
            })
            .ToList();

        var dailyRaw = await currentExpensesQuery
            .GroupBy(e => e.Date.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.SelectMany(e => e.Items).Sum(i => i.Amount),
                PurchasesCount = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var dailyExpenses = dailyRaw
            .Select(x => new
            {
                Date = x.Date.ToString("yyyy-MM-dd"),
                Label = x.Date.ToString("dd/MM"),
                x.Total,
                x.PurchasesCount
            })
            .ToList();

        var historyStart = fromDate.AddDays(-180);
        var historicalDailyTotals = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= historyStart && e.Date < fromDate)
            .GroupBy(e => e.Date.Date)
            .Select(g => g.SelectMany(e => e.Items).Sum(i => i.Amount))
            .ToListAsync();

        var baseDailyAverage = historicalDailyTotals.Count > 0
            ? historicalDailyTotals.Average()
            : daysInFilter > 0 ? totalExpense / daysInFilter : 0m;

        var nextMonthDays = DateTime.DaysInMonth(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month);
        var forecastNextWeek = Math.Round(baseDailyAverage * 7m, 2);
        var forecastNextMonth = Math.Round(baseDailyAverage * nextMonthDays, 2);

        return new
        {
            From = fromDate.ToString("yyyy-MM-dd"),
            To = toExclusive.AddDays(-1).ToString("yyyy-MM-dd"),
            Days = daysInFilter,
            TotalExpense = totalExpense,
            MonthlyExpense = totalExpense,
            TotalPurchases = totalPurchases,
            AveragePerPurchase = totalPurchases > 0 ? Math.Round(totalExpense / totalPurchases, 2) : 0m,
            AveragePerLocation = distinctLocations > 0 ? Math.Round(totalExpense / distinctLocations, 2) : 0m,
            AveragePerCategory = distinctCategories > 0 ? Math.Round(totalExpense / distinctCategories, 2) : 0m,
            PreviousPeriodTotal = previousTotal,
            TotalTrendPercent = totalTrendPercent,
            ForecastNextWeek = forecastNextWeek,
            ForecastNextMonth = forecastNextMonth,
            CategoryExpenses = categoryExpenses,
            LocationExpenses = locationExpenses,
            DailyExpenses = dailyExpenses
        };
    }

    public async Task<List<object>> GetCategoryDistribution(int userId, DateTime? from, DateTime? to)
    {
        var range = NormalizeRange(from, to);
        var query = _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= range.From && e.Date < range.ToExclusive);

        var total = await query.SelectMany(e => e.Items).SumAsync(i => (decimal?)i.Amount) ?? 0m;

        var rows = await query
            .SelectMany(e => e.Items)
            .GroupBy(i => new { i.CategoryId, i.Category.Name })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Name = g.Key.Name,
                Value = g.Sum(i => i.Amount),
                Total = g.Sum(i => i.Amount),
                ItemsCount = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        return rows
            .Select(x => (object)new
            {
                x.CategoryId,
                x.CategoryName,
                x.Name,
                x.Value,
                x.Total,
                x.ItemsCount,
                Percentage = total > 0 ? Math.Round((x.Total / total) * 100m, 2) : 0m
            })
            .ToList();
    }

    public async Task<List<object>> GetMonthlyExpenses(int userId, DateTime? from, DateTime? to)
    {
        var range = NormalizeRange(from, to);

        return await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= range.From && e.Date < range.ToExclusive)
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Label = g.Key.Month.ToString() + "/" + g.Key.Year.ToString(),
                Total = g.SelectMany(e => e.Items).Sum(i => i.Amount),
                PurchasesCount = g.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync<object>();
    }

    public async Task<List<object>> GetPaymentMethods(int userId, DateTime? from, DateTime? to)
    {
        var range = NormalizeRange(from, to);

        return await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= range.From && e.Date < range.ToExclusive)
            .GroupBy(e => e.PaymentMethod)
            .Select(g => new
            {
                Method = g.Key,
                Total = g.Sum(e => e.Items.Sum(i => i.Amount)),
                PurchasesCount = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync<object>();
    }

    public async Task<List<object>> GetExpensesSummary(
        int userId,
        DateTime? from,
        DateTime? to,
        int? categoryId,
        int? locationId,
        int? paymentMethod)
    {
        var range = NormalizeRange(from, to);
        var query = _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= range.From && e.Date < range.ToExclusive);

        if (locationId.HasValue)
            query = query.Where(e => e.LocationId == locationId.Value);

        if (paymentMethod.HasValue)
            query = query.Where(e => (int)e.PaymentMethod == paymentMethod.Value);

        if (categoryId.HasValue)
            query = query.Where(e => e.Items.Any(i => i.CategoryId == categoryId.Value));

        return await query
            .SelectMany(e => e.Items)
            .Where(i => !categoryId.HasValue || i.CategoryId == categoryId.Value)
            .Select(i => new
            {
                i.Description,
                i.Quantity,
                i.UnitPrice,
                i.Amount,
                i.CategoryId,
                CategoryName = i.Category.Name
            })
            .ToListAsync<object>();
    }
}
