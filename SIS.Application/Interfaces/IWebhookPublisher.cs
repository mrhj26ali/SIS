namespace SIS.Application.Interfaces;

public interface IWebhookPublisher
{
    Task PublishAsync(string eventType, object payload);
}
