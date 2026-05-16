using MoneyTrack.Api.Domain.Enum;

namespace MoneyTrack.Api.Domain.Entities
{
    public class Goal
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }

        public int UserId { get; private set; }
        public User User { get; private set; } = null!;

        public GoalTypeEnum Type { get; private set; }
        public GoalScopeEnum Scope { get; private set; }
        public GoalPeriodEnum Period { get; private set; }

        public decimal TargetAmount { get; private set; }
        public decimal SavedAmount { get; private set; }

        public int? CategoryId { get; private set; }
        public Category? Category { get; private set; }

        public int? LocationId { get; private set; }
        public Location? Location { get; private set; }

        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public int AlertPercentage { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFavorite { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public Goal(
            string name,
            int userId,
            GoalTypeEnum type,
            GoalScopeEnum scope,
            GoalPeriodEnum period,
            decimal targetAmount,
            decimal savedAmount,
            int? categoryId,
            int? locationId,
            DateTime startDate,
            DateTime? endDate,
            int alertPercentage,
            string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Nome da meta é obrigatório");
            if (targetAmount <= 0) throw new Exception("Valor da meta precisa ser maior que zero");

            Name = name.Trim();
            UserId = userId;
            Type = type;
            Scope = scope;
            Period = period;
            TargetAmount = targetAmount;
            SavedAmount = savedAmount < 0 ? 0 : savedAmount;
            CategoryId = categoryId;
            LocationId = locationId;
            StartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            EndDate = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc) : null;
            AlertPercentage = alertPercentage <= 0 ? 80 : alertPercentage;
            Description = description;
            IsActive = true;
            IsFavorite = false;
            CreatedAt = DateTime.UtcNow;
        }

        private Goal() { }

        public void Update(
            string name,
            GoalTypeEnum type,
            GoalScopeEnum scope,
            GoalPeriodEnum period,
            decimal targetAmount,
            decimal savedAmount,
            int? categoryId,
            int? locationId,
            DateTime startDate,
            DateTime? endDate,
            int alertPercentage,
            bool isActive,
            string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Nome da meta é obrigatório");
            if (targetAmount <= 0) throw new Exception("Valor da meta precisa ser maior que zero");

            Name = name.Trim();
            Type = type;
            Scope = scope;
            Period = period;
            TargetAmount = targetAmount;
            SavedAmount = savedAmount < 0 ? 0 : savedAmount;
            CategoryId = categoryId;
            LocationId = locationId;
            StartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            EndDate = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc) : null;
            AlertPercentage = alertPercentage <= 0 ? 80 : alertPercentage;
            IsActive = isActive;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ToggleFavorite()
        {
            IsFavorite = !IsFavorite;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
