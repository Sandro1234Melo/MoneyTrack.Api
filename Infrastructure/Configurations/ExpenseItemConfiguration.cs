using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Infrastructure.Configurations
{
    public class ExpenseItemConfiguration : IEntityTypeConfiguration<ExpenseItem>
    {
        public void Configure(EntityTypeBuilder<ExpenseItem> entity)
        {
            entity.ToTable("expense_items");

            entity.HasKey(i => i.Id);

            entity.Property(i => i.Id)
                .HasColumnName("id");

            entity.Property(i => i.ExpenseId)
                .HasColumnName("expense_id");

            entity.Property(i => i.CategoryId)
                .HasColumnName("category_id");

            entity.Property(i => i.Description)
                .HasColumnName("description");

            entity.Property(i => i.Quantity)
                .HasColumnName("quantity");

            entity.Property(i => i.UnitPrice)
                .HasColumnName("unit_price");

            entity.Property(i => i.Amount)
                .HasColumnName("amount");

            entity.HasOne(i => i.Expense)
                .WithMany(e => e.Items)
                .HasForeignKey(i => i.ExpenseId);

            entity.HasOne(i => i.Category)
                .WithMany(c => c.ExpenseItems)
                .HasForeignKey(i => i.CategoryId);
        }
    }
}
