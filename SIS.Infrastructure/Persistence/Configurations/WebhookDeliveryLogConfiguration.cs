using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Infrastructure.Persistence.Entities;

namespace SIS.Infrastructure.Persistence.Configurations;

public class WebhookDeliveryLogConfiguration : IEntityTypeConfiguration<WebhookDeliveryLog>
{
    public void Configure(EntityTypeBuilder<WebhookDeliveryLog> builder)
    {
        builder.ToTable("WebhookDeliveryLogs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventId)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Payload)
            .IsRequired();
        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => x.EventId).IsUnique();

        builder.HasOne(x => x.Subscription)
            .WithMany(s => s.DeliveryLogs)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
