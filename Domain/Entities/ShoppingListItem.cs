namespace MoneyTrack.Api.Domain.Entities
{
    public class ShoppingListItem
    {
        public int Id { get; private set; }
        public int ShoppingListId { get; private set; }
        public ShoppingList ShoppingList { get; private set; } = null!;
        public string Description { get; private set; } = string.Empty;
        public int CategoryId { get; private set; }
        public Category Category { get; private set; } = null!;
        public int Quantity { get; private set; }
        public decimal? Price { get; private set; }
        public bool Checked { get; private set; }

        public decimal Total => (Price ?? 0) * Quantity;

        public ShoppingListItem(string description, int categoryId, int quantity, decimal? price, bool isChecked = false)
        {
            Update(description, categoryId, quantity, price);
            Checked = isChecked;
        }

        public void Update(string description, int categoryId, int quantity, decimal? price)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new Exception("Informe a descrição do item.");

            if (categoryId <= 0)
                throw new Exception("Informe a categoria do item.");

            if (quantity <= 0)
                throw new Exception("A quantidade deve ser maior que zero.");

            if (price.HasValue && price.Value < 0)
                throw new Exception("O preço não pode ser negativo.");

            Description = description.Trim();
            CategoryId = categoryId;
            Quantity = quantity;
            Price = price;
        }

        public void SetChecked(bool value) => Checked = value;
        public void Check() => Checked = true;
        public void Uncheck() => Checked = false;

        private ShoppingListItem() { }
    }
}
