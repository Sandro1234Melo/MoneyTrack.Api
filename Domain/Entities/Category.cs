using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrack.Api.Domain.Entities
{
    public class Category
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public User User { get; private set; } = null!;
        public int UserId { get; private set; }

        public Category(string name, int userId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Nome da categoria é obrigatório");

            Name = name;
            UserId = userId;
        }

        private Category() { }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Nome inválido");

            Name = name;
        }

        public ICollection<ExpenseItem>? ExpenseItems { get; set; }
        public ICollection<ShoppingListItem> ShoppingListItems { get; set; } = new List<ShoppingListItem>();
    }
}
