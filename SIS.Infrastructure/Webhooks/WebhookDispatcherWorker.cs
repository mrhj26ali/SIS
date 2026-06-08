using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SIS.Infrastructure.Persistence.Contexts;
using SIS.Infrastructure.Persistence.Entities;

namespace SIS.Infrastructure.Webhooks;

public class WebhookDispatcherWorker : BackgroundService
{
    private readonly WebhookChannel _channel;
    private readonly IServiceScopeFactory _scopes;
    private readonly IHttpClientFactory _http;
    private readonly ILogger<WebhookDispatcherWorker> _logger;

    private const int MaxAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public WebhookDispatcherWorker(
        WebhookChannel channel,
        IServiceScopeFactory scopes,
        IHttpClientFactory http,
        ILogger<WebhookDispatcherWorker> logger)
    {
        _channel = channel;
        _scopes = scopes;
        _http = http;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var msg in _channel.Reader.ReadAllAsync(ct))
        {
            await using var scope = _scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var subscribers = await db.WebhookSubscriptions
                .Where(s => s.EventType == msg.EventType && s.IsActive)
                .AsNoTracking()
                .ToListAsync(ct);

            var tasks = subscribers.Select(sub => DispatchAsync(sub, msg, ct));
            await Task.WhenAll(tasks);
        }
    }

    private async Task DispatchAsync(WebhookSubscription subscription, WebhookMessage msg, CancellationToken ct)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var deliveryLog = new WebhookDeliveryLog
        {
            SubscriptionId = subscription.Id,
            EventId = msg.EventId,
            EventType = msg.EventType,
            Payload = msg.Payload,
            Status = "Pending"
        };

        db.WebhookDeliveryLogs.Add(deliveryLog);
        await db.SaveChangesAsync(ct);

        var client = _http.CreateClient("webhook");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            deliveryLog.AttemptCount = attempt;
            deliveryLog.ErrorMessage = null;
            deliveryLog.HttpStatusCode = null;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, subscription.EndpointUrl)
                {
                    Content = new StringContent(msg.Payload, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("X-Webhook-Event-Id", deliveryLog.EventId);
                request.Headers.Add("X-Webhook-Event-Type", msg.EventType);
                request.Headers.Add("X-Webhook-Sent-At", DateTime.UtcNow.ToString("o"));
                request.Headers.Add("X-Webhook-Signature", CreateSignature(subscription.Secret, msg.Payload));

                var response = await client.SendAsync(request, ct);
                deliveryLog.HttpStatusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    deliveryLog.Status = "Delivered";
                    deliveryLog.DeliveredAt = DateTime.UtcNow;
                    await db.SaveChangesAsync(ct);
                    return;
                }

                deliveryLog.ErrorMessage = $"Non-success status code: {(int)response.StatusCode} {response.ReasonPhrase}";
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                deliveryLog.ErrorMessage = "Dispatcher shutdown before delivery completed.";
                deliveryLog.Status = "Failed";
                await db.SaveChangesAsync(ct);
                return;
            }
            catch (Exception ex)
            {
                deliveryLog.ErrorMessage = ex.Message;
            }

            deliveryLog.Status = attempt < MaxAttempts ? "Pending" : "Failed";
            await db.SaveChangesAsync(ct);

            if (attempt < MaxAttempts)
            {
                _logger.LogWarning("Webhook delivery attempt {Attempt} failed for subscription {SubscriptionId}. Retrying in {Delay}.", attempt, subscription.Id, RetryDelay);
                await Task.Delay(RetryDelay, ct);
            }
        }
    }

    private static string CreateSignature(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(signatureBytes);
    }
}
