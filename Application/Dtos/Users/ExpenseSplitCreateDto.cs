namespace MoneyTrack.Api.Application.Dtos.Users
{
    public class ExpenseSplitCreateDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<SplitParticipantCreateDto> Participants { get; set; } = new();
    }
    public class SplitParticipantCreateDto { public int UserId { get; set; } public decimal Amount { get; set; } }
}
