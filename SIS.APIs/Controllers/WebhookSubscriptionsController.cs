using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIS.Application.Interfaces;
using SIS.Infrastructure.Persistence.Contexts;
using SIS.Infrastructure.Persistence.Entities;

namespace SIS.APIs.Controllers;

[ApiController]
[Route("api/webhooks/subscriptions")]
[Authorize(Roles = "Admin")]
public class WebhookSubscriptionsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public WebhookSubscriptionsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterWebhookDto dto)
    {
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var subscription = new WebhookSubscription
        {
            EndpointUrl = dto.EndpointUrl,
            EventType = dto.EventType,
            Secret = secret,
            IsActive = true
        };

        _db.WebhookSubscriptions.Add(subscription);
        await _db.SaveChangesAsync();

        return Ok(new { subscription.Id, subscription.EndpointUrl, subscription.EventType, secret });
    }

    [HttpGet("{id}/logs")]
    public async Task<IActionResult> GetLogs(int id)
    {
        var logs = await _db.WebhookDeliveryLogs
            .Where(l => l.SubscriptionId == id)
            .OrderByDescending(l => l.CreatedAt)
            .Take(50)
            .Select(l => new
            {
                l.Id,
                l.EventId,
                l.EventType,
                l.Status,
                l.HttpStatusCode,
                l.AttemptCount,
                l.CreatedAt,
                l.DeliveredAt,
                l.ErrorMessage
            })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpPost("{id}/replay/{logId}")]
    public async Task<IActionResult> Replay(int id, int logId, [FromServices] IWebhookPublisher publisher)
    {
        var entry = await _db.WebhookDeliveryLogs
            .Include(l => l.Subscription)
            .FirstOrDefaultAsync(l => l.Id == logId && l.SubscriptionId == id);

        if (entry is null)
            return NotFound();

        var payloadObj = JsonSerializer.Deserialize<object>(entry.Payload);
        await publisher.PublishAsync(entry.EventType, payloadObj!);
        return Accepted();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var subscription = await _db.WebhookSubscriptions.FindAsync(id);
        if (subscription is null)
            return NotFound();

        subscription.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record RegisterWebhookDto(string EndpointUrl, string EventType);
