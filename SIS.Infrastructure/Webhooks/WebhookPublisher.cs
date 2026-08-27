using System.Text.Json;
using SIS.Application.Interfaces;

namespace SIS.Infrastructure.Webhooks;

public class WebhookPublisher : IWebhookPublisher
{
    private readonly WebhookChannel _channel;

    public WebhookPublisher(WebhookChannel channel) => _channel = channel;

    public async Task PublishAsync(string eventType, object payload)
    {
        var eventId = Guid.NewGuid().ToString();
        var json = JsonSerializer.Serialize(new
        {
            eventId,
            eventType,
            occurredAt = DateTime.UtcNow,
            data = payload
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await _channel.Writer.WriteAsync(new WebhookMessage(eventType, eventId, json));
    }
}
