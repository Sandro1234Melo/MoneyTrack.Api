using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Domain.Enum;

public class Expense
{
    public int Id { get; private set; }
    public DateTime Date { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public int UserId { get; private set; }
    public User User { get; private set; } = null!;

    public PaymentMethodEnum PaymentMethod { get; private set; }

    public int? LocationId { get; private set; }
    public Location? Location { get; private set; }

    public ICollection<ExpenseItem> Items { get; private set; } = new List<ExpenseItem>();

    public Expense(DateTime date, int userId, PaymentMethodEnum paymentMethod, int? locationId = null)
    {
        Date = date;
        CreatedAt = DateTime.UtcNow;
        UserId = userId;
        PaymentMethod = paymentMethod;
        LocationId = locationId;
    }

    private Expense() { }

    // Atualizar dados principais
    public void Update(DateTime date, int? locationId, PaymentMethodEnum paymentMethod)
    {
        Date = date;
        LocationId = locationId;
        PaymentMethod = paymentMethod;
    }

    // Limpar itens
    public void ClearItems()
    {
        Items.Clear();
    }

    // Adicionar item com regra
    public void AddItem(ExpenseItem item)
    {
        if (item == null)
            throw new Exception("Item inválido");

        Items.Add(item);
    }
}