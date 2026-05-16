namespace MoneyTrack.Api.Application.Dtos.ShoppinLists
{
    public class ShoppingListResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public DateTime PlannedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalItems { get; set; }
        public int CheckedItems { get; set; }
        public decimal EstimatedTotal { get; set; }
        public decimal ProgressPercent { get; set; }
        public List<ShoppingListItemResponseDto> Items { get; set; } = new();
    }
}
