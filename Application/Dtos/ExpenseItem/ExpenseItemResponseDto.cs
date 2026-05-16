namespace MoneyTrack.Api.Application.Dtos.ExpenseItem
{
    public class ExpenseItemResponseDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal UnitPrice { get; set; }
        

    }
}
