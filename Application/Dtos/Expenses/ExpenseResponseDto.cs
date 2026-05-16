using MoneyTrack.Api.Application.Dtos.ExpenseItem;
using MoneyTrack.Api.Domain.Enum;

namespace MoneyTrack.Api.Application.Dtos.Expenses
{
    public class ExpenseResponseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public List<ExpenseItemResponseDto> Items { get; set; } = new();
    }

}
