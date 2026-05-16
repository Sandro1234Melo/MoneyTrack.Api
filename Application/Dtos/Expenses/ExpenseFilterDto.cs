namespace MoneyTrack.Api.Application.Dtos.Expenses
{
    public class ExpenseFilterDto
    {
        public int? UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? LocationId { get; set; }
        public int? CategoryId { get; set; }
        public int? NoteId { get; set; }
        public string? Description { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
    }
}