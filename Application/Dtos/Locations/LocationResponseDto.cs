namespace MoneyTrack.Api.Application.Dtos.Locations
{
    public class LocationResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int TotalPurchases { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AveragePerPurchase { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
    }
}
