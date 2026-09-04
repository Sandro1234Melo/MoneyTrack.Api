namespace MoneyTrack.Api.Domain.Entities
{
    public class ExpenseSplit
    {
        public int Id { get; private set; }
        public int CreatedByUserId { get; private set; }
        public int PaidByUserId { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public decimal TotalAmount { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public ICollection<SplitParticipant> Participants { get; private set; } = new List<SplitParticipant>();

        public ExpenseSplit(int createdByUserId, int paidByUserId, string description, decimal totalAmount)
        {
            if (totalAmount <= 0) throw new ArgumentException("O valor deve ser maior que zero.");
            CreatedByUserId = createdByUserId; PaidByUserId = paidByUserId;
            Description = description.Trim(); TotalAmount = totalAmount;
        }
        private ExpenseSplit() { }
        public void AddParticipant(SplitParticipant participant) => Participants.Add(participant);
    }

    public class SplitParticipant
    {
        public int Id { get; private set; }
        public int ExpenseSplitId { get; private set; }
        public int UserId { get; private set; }
        public decimal Amount { get; private set; }
        public bool IsPaid { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public SplitParticipant(int userId, decimal amount) { UserId = userId; Amount = amount; }
        private SplitParticipant() { }
        public void MarkPaid() { IsPaid = true; PaidAt = DateTime.UtcNow; }
    }
}
