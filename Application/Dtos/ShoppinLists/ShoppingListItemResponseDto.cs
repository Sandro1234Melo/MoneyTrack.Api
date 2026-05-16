namespace MoneyTrack.Api.Application.Dtos.ShoppinLists
{
    public class ShoppingListItemResponseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal Total { get; set; }
        public bool Checked { get; set; }
    }
}
