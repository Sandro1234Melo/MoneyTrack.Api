using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Infrastructure.Configurations
{
    public class GoalConfiguration : IEntityTypeConfiguration<Goal>
    {
        public void Configure(EntityTypeBuilder<Goal> entity)
        {
            entity.ToTable("goals");
            entity.HasKey(g => g.Id);

            entity.Property(g => g.Id).HasColumnName("id");
            entity.Property(g => g.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(g => g.Description).HasColumnName("description");
            entity.Property(g => g.UserId).HasColumnName("user_id");
            entity.Property(g => g.Type).HasColumnName("type").HasConversion<int>();
            entity.Property(g => g.Scope).HasColumnName("scope").HasConversion<int>();
            entity.Property(g => g.Period).HasColumnName("period").HasConversion<int>();
            entity.Property(g => g.TargetAmount).HasColumnName("target_amount").HasColumnType("numeric(18,2)");
            entity.Property(g => g.SavedAmount).HasColumnName("saved_amount").HasColumnType("numeric(18,2)");
            entity.Property(g => g.CategoryId).HasColumnName("category_id");
            entity.Property(g => g.LocationId).HasColumnName("location_id");
            entity.Property(g => g.StartDate).HasColumnName("start_date");
            entity.Property(g => g.EndDate).HasColumnName("end_date");
            entity.Property(g => g.AlertPercentage).HasColumnName("alert_percentage");
            entity.Property(g => g.IsActive).HasColumnName("is_active");
            entity.Property(g => g.IsFavorite).HasColumnName("is_favorite");
            entity.Property(g => g.CreatedAt).HasColumnName("created_at");
            entity.Property(g => g.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.Category)
                .WithMany()
                .HasForeignKey(g => g.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(g => g.Location)
                .WithMany()
                .HasForeignKey(g => g.LocationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
