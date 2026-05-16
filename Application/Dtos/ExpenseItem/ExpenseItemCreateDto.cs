namespace MoneyTrack.Api.Application.Dtos.ExpenseItem
{
    public class ExpenseItemCreateDto
    {
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }

}
