using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Infrastructure.Configurations
{
    public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
    {
        public void Configure(EntityTypeBuilder<Friendship> entity)
        {
            entity.ToTable("friendships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SenderId).HasColumnName("sender_id");
            entity.Property(x => x.ReceiverId).HasColumnName("receiver_id");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.RespondedAt).HasColumnName("responded_at");
            entity.HasIndex(x => new { x.SenderId, x.ReceiverId }).IsUnique();
        }
    }
}
