using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Infrastructure.Configurations
{
    public class ExpenseSplitConfiguration : IEntityTypeConfiguration<ExpenseSplit>
    {
        public void Configure(EntityTypeBuilder<ExpenseSplit> entity)
        {
            entity.ToTable("expense_splits"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id"); entity.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(x => x.PaidByUserId).HasColumnName("paid_by_user_id"); entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(12, 2); entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasMany(x => x.Participants).WithOne().HasForeignKey(x => x.ExpenseSplitId).OnDelete(DeleteBehavior.Cascade);
        }
    }
    public class SplitParticipantConfiguration : IEntityTypeConfiguration<SplitParticipant>
    {
        public void Configure(EntityTypeBuilder<SplitParticipant> entity)
        {
            entity.ToTable("split_participants"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id"); entity.Property(x => x.ExpenseSplitId).HasColumnName("expense_split_id");
            entity.Property(x => x.UserId).HasColumnName("user_id"); entity.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12, 2);
            entity.Property(x => x.IsPaid).HasColumnName("is_paid"); entity.Property(x => x.PaidAt).HasColumnName("paid_at");
            entity.HasIndex(x => new { x.ExpenseSplitId, x.UserId }).IsUnique();
        }
    }
}
