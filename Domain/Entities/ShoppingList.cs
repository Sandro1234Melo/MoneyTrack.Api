using MoneyTrack.Api.Domain.Enum;

namespace MoneyTrack.Api.Domain.Entities
{
    public class ShoppingList
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public User User { get; private set; } = null!;
        public string Name { get; private set; } = string.Empty;
        public int? LocationId { get; private set; }
        public Location? Location { get; private set; }
        public DateTime PlannedDate { get; private set; }
        public ShoppingListStatusEnum Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public ICollection<ShoppingListItem> Items { get; private set; } = new List<ShoppingListItem>();

        public ShoppingList(string name, int userId, DateTime plannedDate, int? locationId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Informe o nome da lista.");

            Name = name.Trim();
            UserId = userId;
            PlannedDate = ToUtc(plannedDate);
            LocationId = locationId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Status = ShoppingListStatusEnum.Draft;
        }

        public void Update(string name, DateTime plannedDate, int? locationId)
        {
            if (Status == ShoppingListStatusEnum.Converted)
                throw new Exception("Não é possível alterar uma lista já convertida.");

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Informe o nome da lista.");

            Name = name.Trim();
            PlannedDate = ToUtc(plannedDate);
            LocationId = locationId;
            Touch();
        }

        public void AddItem(ShoppingListItem item)
        {
            if (Status == ShoppingListStatusEnum.Converted)
                throw new Exception("Não é possível adicionar itens em uma lista já convertida.");

            Items.Add(item);
            Touch();
        }

        public void RemoveItem(ShoppingListItem item)
        {
            if (Status == ShoppingListStatusEnum.Converted)
                throw new Exception("Não é possível remover itens de uma lista já convertida.");

            Items.Remove(item);
            Touch();
            UpdateStatusFromItems();
        }

        public void Start()
        {
            if (Status == ShoppingListStatusEnum.Converted)
                throw new Exception("Lista já convertida.");

            Status = ShoppingListStatusEnum.InProgress;
            Touch();
        }

        public void UpdateStatusFromItems()
        {
            if (Status == ShoppingListStatusEnum.Converted)
                return;

            if (Items.Count > 0 && Items.All(i => i.Checked))
                Status = ShoppingListStatusEnum.Completed;
            else if (Items.Any(i => i.Checked) || Status == ShoppingListStatusEnum.InProgress)
                Status = ShoppingListStatusEnum.InProgress;
            else
                Status = ShoppingListStatusEnum.Draft;

            Touch();
        }

        public void MarkAsConverted()
        {
            if (Status == ShoppingListStatusEnum.Converted)
                throw new Exception("Lista já convertida.");

            Status = ShoppingListStatusEnum.Converted;
            Touch();
        }

        public void Touch() => UpdatedAt = DateTime.UtcNow;

        private static DateTime ToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc) return value;
            if (value.Kind == DateTimeKind.Local) return value.ToUniversalTime();
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private ShoppingList() { }
    }
}
