using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Infrastructure.Persistence.Entities;

namespace SIS.Infrastructure.Persistence.Configurations;

public class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable("WebhookSubscriptions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.EndpointUrl)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(x => x.Secret)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => new { x.EventType, x.IsActive });
    }
}
