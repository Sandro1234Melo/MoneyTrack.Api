namespace MoneyTrack.Api.Application.Dtos.ShoppinLists
{
    public class ShoppingListSummaryDto
    {
        public int TotalLists { get; set; }
        public int PendingItems { get; set; }
        public decimal EstimatedTotal { get; set; }
        public decimal PotentialSavings { get; set; }
    }
}
