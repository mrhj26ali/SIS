using System.Threading.Channels;

namespace SIS.Infrastructure.Webhooks;

public sealed class WebhookChannel
{
    private readonly Channel<WebhookMessage> _channel = Channel.CreateUnbounded<WebhookMessage>(new UnboundedChannelOptions { SingleReader = true });

    public ChannelWriter<WebhookMessage> Writer => _channel.Writer;
    public ChannelReader<WebhookMessage> Reader => _channel.Reader;
}

public record WebhookMessage(string EventType, string EventId, string Payload);
