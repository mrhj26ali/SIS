namespace SIS.Infrastructure.Persistence.Entities;

public class WebhookSubscription
{
    public int Id { get; set; }
    public string EndpointUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<WebhookDeliveryLog> DeliveryLogs { get; set; } = new List<WebhookDeliveryLog>();
}
