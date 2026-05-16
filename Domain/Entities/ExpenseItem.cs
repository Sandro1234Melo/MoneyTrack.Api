using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrack.Api.Domain.Entities
{
    public class ExpenseItem
    {
        public int Id { get; private set; }

        public int ExpenseId { get; private set; }
        public Expense Expense { get; private set; } = null!;

        public string Description { get; private set; } = string.Empty;
        public decimal Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Amount { get; private set; }

        public int CategoryId { get; private set; }
        public Category Category { get; private set; } = null!;

        public ExpenseItem(string description, decimal quantity, decimal unitPrice, int categoryId)
        {
            if (quantity <= 0)
                throw new Exception("Quantidade inválida");

            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Amount = quantity * unitPrice;
            CategoryId = categoryId;
        }

        private ExpenseItem() { }
    }
}