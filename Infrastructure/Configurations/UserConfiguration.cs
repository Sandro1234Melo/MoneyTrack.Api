using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .HasColumnName("id");
            entity.Property(u => u.FullName)
                .HasColumnName("full_name");
            entity.Property(u => u.Email)
                .HasColumnName("email");
            entity.Property(u => u.PasswordHash)
                .HasColumnName("password_hash");
            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(u => u.Currency_Code)
                .HasColumnName("currency_code");
            entity.Property(u => u.Country_Code)
                .HasColumnName("country_code");
            entity.Property(u => u.Language)
                .HasColumnName("language");
            entity.Property(u => u.Theme)
                .HasColumnName("theme");
            entity.Property(u => u.DateFormat)
                .HasColumnName("date_format")
                .HasDefaultValue("dd/MM/yyyy");
            entity.Property(u => u.AccentColor)
                .HasColumnName("accent_color")
                .HasDefaultValue("purple");
            entity.Property(u => u.CompactMode)
                .HasColumnName("compact_mode")
                .HasDefaultValue(false);
            entity.Property(u => u.InterfaceAnimations)
                .HasColumnName("interface_animations")
                .HasDefaultValue(true);
            entity.Property(u => u.NotifyGoal80)
                .HasColumnName("notify_goal_80")
                .HasDefaultValue(true);
            entity.Property(u => u.NotifySpendingIncrease)
                .HasColumnName("notify_spending_increase")
                .HasDefaultValue(true);
            entity.Property(u => u.NotifyPendingLists)
                .HasColumnName("notify_pending_lists")
                .HasDefaultValue(false);
            entity.Property(u => u.LastBackupAt)
                .HasColumnName("last_backup_at");
            entity.Property(u => u.BottomNavConfig)
                .HasColumnName("bottom_nav_config");
            entity.Property(u => u.ProfileImageUrl)
                .HasColumnName("profile_image_url");
        }
    }
}