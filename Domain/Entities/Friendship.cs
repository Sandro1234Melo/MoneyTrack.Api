namespace MoneyTrack.Api.Domain.Entities
{
    public enum FriendshipStatus { Pending = 0, Accepted = 1, Declined = 2 }

    public class Friendship
    {
        public int Id { get; private set; }
        public int SenderId { get; private set; }
        public int ReceiverId { get; private set; }
        public FriendshipStatus Status { get; private set; } = FriendshipStatus.Pending;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; private set; }

        public Friendship(int senderId, int receiverId)
        {
            if (senderId == receiverId) throw new ArgumentException("Não é possível adicionar a própria conta.");
            SenderId = senderId;
            ReceiverId = receiverId;
        }

        private Friendship() { }
        public void Accept() { Status = FriendshipStatus.Accepted; RespondedAt = DateTime.UtcNow; }
        public void Decline() { Status = FriendshipStatus.Declined; RespondedAt = DateTime.UtcNow; }
    }
}
