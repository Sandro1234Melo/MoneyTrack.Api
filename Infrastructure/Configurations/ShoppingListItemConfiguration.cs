using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Infrastructure.Configurations
{
    public class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItem>
    {
        public void Configure(EntityTypeBuilder<ShoppingListItem> entity)
        {
            entity.ToTable("shopping_list_items");

            entity.HasKey(i => i.Id);

            entity.Property(i => i.Id).HasColumnName("id");
            entity.Property(i => i.ShoppingListId).HasColumnName("shopping_list_id");
            entity.Property(i => i.Description).HasColumnName("description").IsRequired();
            entity.Property(i => i.CategoryId).HasColumnName("category_id");
            entity.Property(i => i.Quantity).HasColumnName("quantity");
            entity.Property(i => i.Price).HasColumnName("price").HasColumnType("numeric(18,2)");
            entity.Property(i => i.Checked).HasColumnName("checked");

            entity.Ignore(i => i.Total);

            entity.HasOne(i => i.ShoppingList)
                .WithMany(l => l.Items)
                .HasForeignKey(i => i.ShoppingListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Category)
                .WithMany(c => c.ShoppingListItems)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
