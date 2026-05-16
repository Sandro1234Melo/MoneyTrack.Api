using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Infrastructure.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> entity)
        {
            entity.ToTable("locations");

            entity.HasKey(l => l.Id);

            entity.Property(l => l.Id)
                .HasColumnName("id");
            entity.Property(l => l.Name)
                .HasColumnName("name");
            entity.Property(l => l.UserId)
                .HasColumnName("user_id");

            entity.HasOne(l => l.User)
                .WithMany(u => u.Locations)
                .HasForeignKey(l => l.UserId);
        }
    }
}