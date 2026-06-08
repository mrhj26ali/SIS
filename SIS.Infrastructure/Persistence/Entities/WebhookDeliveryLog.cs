namespace SIS.Infrastructure.Persistence.Entities;

public class WebhookDeliveryLog
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public WebhookSubscription Subscription { get; set; } = null!;
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int? HttpStatusCode { get; set; }
    public string Status { get; set; } = "Pending";
    public int AttemptCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
}
