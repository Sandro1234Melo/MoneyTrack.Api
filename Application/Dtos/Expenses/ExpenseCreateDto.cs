using MoneyTrack.Api.Application.Dtos.ExpenseItem;
using MoneyTrack.Api.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrack.Api.Application.Dtos.Expenses
{
    public class ExpenseCreateDto
    {
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public int? LocationId { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public List<ExpenseItemCreateDto> Items { get; set; } = new();
    }

}

