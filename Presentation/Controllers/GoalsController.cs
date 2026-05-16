using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Domain.Enum;
using MoneyTrack.Api.Infrastructure.Data;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoalsController : ControllerBase
    {
        private readonly MoneyTrackDbContext _context;

        public GoalsController(MoneyTrackDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId, [FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? type, [FromQuery] string? orderBy)
        {
            try
            {
                var goals = await _context.Goals
                    .AsNoTracking()
                    .Include(g => g.Category)
                    .Include(g => g.Location)
                    .Where(g => g.UserId == userId)
                    .ToListAsync();

                var result = new List<GoalResponseDto>();
                foreach (var goal in goals)
                {
                    result.Add(await MapGoal(goal));
                }

                if (!string.IsNullOrWhiteSpace(search))
                    result = result.Where(g => g.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrWhiteSpace(type) && type != "all")
                {
                    if (type == "savings") result = result.Where(g => g.Type == (int)GoalTypeEnum.Savings).ToList();
                    if (type == "limits") result = result.Where(g => g.Type == (int)GoalTypeEnum.ExpenseLimit).ToList();
                    if (type == "completed") result = result.Where(g => g.StatusKey == "completed").ToList();
                }

                if (!string.IsNullOrWhiteSpace(status) && status != "all")
                    result = result.Where(g => g.StatusKey == status).ToList();

                result = orderBy switch
                {
                    "progress" => result.OrderByDescending(g => g.ProgressPercentage).ToList(),
                    "risk" => result.OrderByDescending(g => g.RiskLevel).ToList(),
                    "amount" => result.OrderByDescending(g => g.TargetAmount).ToList(),
                    _ => result.OrderBy(g => g.DaysRemaining ?? 99999).ThenByDescending(g => g.RiskLevel).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(GetByUser));
            }
        }

        [HttpGet("user/{userId}/summary")]
        public async Task<IActionResult> Summary(int userId)
        {
            try
            {
                var goals = await _context.Goals
                    .AsNoTracking()
                    .Include(g => g.Category)
                    .Include(g => g.Location)
                    .Where(g => g.UserId == userId)
                    .ToListAsync();

                var mapped = new List<GoalResponseDto>();
                foreach (var goal in goals)
                    mapped.Add(await MapGoal(goal));

                var active = mapped.Where(g => g.IsActive).ToList();
                var savings = active.Where(g => g.Type == (int)GoalTypeEnum.Savings).ToList();
                var risk = active.Where(g => g.RiskLevel >= 2).ToList();

                var dto = new GoalSummaryDto
                {
                    ActiveGoals = active.Count,
                    PlannedSavings = savings.Sum(g => g.TargetAmount),
                    SavedAmount = savings.Sum(g => g.CurrentAmount),
                    RiskGoals = risk.Count,
                    ProjectedMonthlySpending = await GetMonthSpending(userId),
                    DefinedLimits = active.Where(g => g.Type == (int)GoalTypeEnum.ExpenseLimit).Sum(g => g.TargetAmount),
                    RiskGoalsList = risk.OrderByDescending(g => g.ProgressPercentage).Take(3).ToList(),
                    UpcomingGoals = active.Where(g => g.EndDate.HasValue).OrderBy(g => g.EndDate).Take(3).ToList()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Summary));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GoalCreateDto dto)
        {
            try
            {
                var goal = new Goal(
                    dto.Name,
                    dto.UserId,
                    (GoalTypeEnum)dto.Type,
                    (GoalScopeEnum)dto.Scope,
                    (GoalPeriodEnum)dto.Period,
                    dto.TargetAmount,
                    dto.SavedAmount,
                    dto.CategoryId,
                    dto.LocationId,
                    ToUtc(dto.StartDate),
                    dto.EndDate.HasValue ? ToUtc(dto.EndDate.Value) : null,
                    dto.AlertPercentage,
                    dto.Description
                );

                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                var created = await _context.Goals.Include(g => g.Category).Include(g => g.Location).FirstAsync(g => g.Id == goal.Id);
                return Ok(await MapGoal(created));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Create));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GoalUpdateDto dto)
        {
            try
            {
                var goal = await _context.Goals.FirstOrDefaultAsync(g => g.Id == id);
                if (goal == null) return NotFound(new { message = "Meta não encontrada" });

                goal.Update(
                    dto.Name,
                    (GoalTypeEnum)dto.Type,
                    (GoalScopeEnum)dto.Scope,
                    (GoalPeriodEnum)dto.Period,
                    dto.TargetAmount,
                    dto.SavedAmount,
                    dto.CategoryId,
                    dto.LocationId,
                    ToUtc(dto.StartDate),
                    dto.EndDate.HasValue ? ToUtc(dto.EndDate.Value) : null,
                    dto.AlertPercentage,
                    dto.IsActive,
                    dto.Description
                );

                await _context.SaveChangesAsync();
                var updated = await _context.Goals.AsNoTracking().Include(g => g.Category).Include(g => g.Location).FirstAsync(g => g.Id == id);
                return Ok(await MapGoal(updated));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Update));
            }
        }

        [HttpPatch("{id}/favorite")]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            try
            {
                var goal = await _context.Goals.FirstOrDefaultAsync(g => g.Id == id);
                if (goal == null) return NotFound(new { message = "Meta não encontrada" });
                goal.ToggleFavorite();
                await _context.SaveChangesAsync();
                return Ok(new { goal.Id, goal.IsFavorite });
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(ToggleFavorite));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var goal = await _context.Goals.FirstOrDefaultAsync(g => g.Id == id);
                if (goal == null) return NotFound(new { message = "Meta não encontrada" });
                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Delete));
            }
        }

        private async Task<GoalResponseDto> MapGoal(Goal goal)
        {
            var current = goal.Type == GoalTypeEnum.Savings ? goal.SavedAmount : await GetGoalSpending(goal);
            var remaining = Math.Max(goal.TargetAmount - current, 0);
            var progress = goal.TargetAmount <= 0 ? 0 : Math.Round((current / goal.TargetAmount) * 100, 0);
            if (progress > 999) progress = 999;

            var days = goal.EndDate.HasValue ? Math.Max((goal.EndDate.Value.Date - DateTime.UtcNow.Date).Days, 0) : (int?)null;
            var status = GetStatus(goal, progress);

            return new GoalResponseDto
            {
                Id = goal.Id,
                Name = goal.Name,
                Description = goal.Description,
                UserId = goal.UserId,
                Type = (int)goal.Type,
                TypeName = goal.Type == GoalTypeEnum.Savings ? "Economia" : "Limite",
                Scope = (int)goal.Scope,
                Period = (int)goal.Period,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = current,
                RemainingAmount = remaining,
                ProgressPercentage = progress,
                CategoryId = goal.CategoryId,
                CategoryName = goal.Category?.Name,
                LocationId = goal.LocationId,
                LocationName = goal.Location?.Name,
                StartDate = goal.StartDate,
                EndDate = goal.EndDate,
                DaysRemaining = days,
                AlertPercentage = goal.AlertPercentage,
                IsActive = goal.IsActive,
                IsFavorite = goal.IsFavorite,
                StatusKey = status.key,
                StatusLabel = status.label,
                RiskLevel = status.riskLevel,
                CreatedAt = goal.CreatedAt,
                UpdatedAt = goal.UpdatedAt
            };
        }

        private async Task<decimal> GetGoalSpending(Goal goal)
        {
            var from = ToUtc(goal.StartDate);
            var to = goal.EndDate.HasValue ? ToUtc(goal.EndDate.Value).Date.AddDays(1).AddTicks(-1) : DateTime.UtcNow;

            var query = _context.ExpenseItems.AsNoTracking()
                .Include(i => i.Expense)
                .Where(i => i.Expense.UserId == goal.UserId && i.Expense.Date >= from && i.Expense.Date <= to);

            if (goal.Scope == GoalScopeEnum.Category && goal.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == goal.CategoryId.Value);

            if (goal.Scope == GoalScopeEnum.Location && goal.LocationId.HasValue)
                query = query.Where(i => i.Expense.LocationId == goal.LocationId.Value);

            return await query.SumAsync(i => i.Amount);
        }

        private async Task<decimal> GetMonthSpending(int userId)
        {
            var now = DateTime.UtcNow;
            var first = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var last = first.AddMonths(1).AddTicks(-1);
            return await _context.ExpenseItems.AsNoTracking()
                .Include(i => i.Expense)
                .Where(i => i.Expense.UserId == userId && i.Expense.Date >= first && i.Expense.Date <= last)
                .SumAsync(i => i.Amount);
        }

        private static (string key, string label, int riskLevel) GetStatus(Goal goal, decimal progress)
        {
            if (!goal.IsActive) return ("inactive", "Inativa", 0);
            if (goal.Type == GoalTypeEnum.Savings)
            {
                if (progress >= 100) return ("completed", "Concluída", 0);
                if (progress >= 50) return ("on-track", "No caminho", 0);
                return ("starting", "Iniciando", 0);
            }

            if (progress >= 100) return ("exceeded", "Estourou", 3);
            if (progress >= goal.AlertPercentage) return ("attention", "Atenção", 2);
            if (progress >= Math.Max(goal.AlertPercentage - 10, 50)) return ("near-limit", "Quase no limite", 1);
            return ("on-track", "No caminho", 0);
        }

        private static DateTime ToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc) return value;
            return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
        }

        private ObjectResult HandleError(Exception ex, string action)
        {
            return StatusCode(500, new
            {
                error = ex.Message,
                details = ex.InnerException?.Message,
                controller = nameof(GoalsController),
                action
            });
        }
    }

    public class GoalCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserId { get; set; }
        public int Type { get; set; }
        public int Scope { get; set; }
        public int Period { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal SavedAmount { get; set; }
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int AlertPercentage { get; set; } = 80;
    }

    public class GoalUpdateDto : GoalCreateDto
    {
        public bool IsActive { get; set; } = true;
    }

    public class GoalResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserId { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int Scope { get; set; }
        public int Period { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysRemaining { get; set; }
        public int AlertPercentage { get; set; }
        public bool IsActive { get; set; }
        public bool IsFavorite { get; set; }
        public string StatusKey { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public int RiskLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class GoalSummaryDto
    {
        public int ActiveGoals { get; set; }
        public decimal PlannedSavings { get; set; }
        public decimal SavedAmount { get; set; }
        public int RiskGoals { get; set; }
        public decimal ProjectedMonthlySpending { get; set; }
        public decimal DefinedLimits { get; set; }
        public List<GoalResponseDto> RiskGoalsList { get; set; } = new();
        public List<GoalResponseDto> UpcomingGoals { get; set; } = new();
    }
}
