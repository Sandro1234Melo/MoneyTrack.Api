namespace MoneyTrack.Api.Application.Dtos.Expenses
{
    public class ExpenseUpdateDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int? LocationId { get; set; }
    }
}
