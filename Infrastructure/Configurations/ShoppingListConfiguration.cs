using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Infrastructure.Configurations
{
    public class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingList>
    {
        public void Configure(EntityTypeBuilder<ShoppingList> entity)
        {
            entity.ToTable("shopping_lists");

            entity.HasKey(sl => sl.Id);

            entity.Property(sl => sl.Id)
                .HasColumnName("id");
            entity.Property(sl => sl.UserId)
                .HasColumnName("user_id");
            entity.Property(sl => sl.LocationId)
                .HasColumnName("location_id");
            entity.Property(sl => sl.Name)
                .HasColumnName("name");
            entity.Property(sl => sl.PlannedDate)
                .HasColumnName("planned_date");
            entity.Property(sl => sl.Status)
                .HasColumnName("status")
                .HasColumnType("integer")
                .HasConversion<int>();
            entity.Property(sl => sl.CreatedAt)
                .HasColumnName("created_at");
            entity.Ignore(sl => sl.UpdatedAt);

            entity.HasOne(sl => sl.User)
                .WithMany(u => u.ShoppingLists)
                .HasForeignKey(sl => sl.UserId);

            entity.HasOne(sl => sl.Location)
                .WithMany(l => l.ShoppingLists)
                .HasForeignKey(sl => sl.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(sl => sl.Items)
                .WithOne(i => i.ShoppingList)
                .HasForeignKey(i => i.ShoppingListId);
        }
    }
}