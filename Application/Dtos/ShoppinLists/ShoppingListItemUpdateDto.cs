namespace MoneyTrack.Api.Application.Dtos.ShoppinLists
{
    public class ShoppingListItemUpdateDto
    {
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public bool Checked { get; set; }
    }
}
