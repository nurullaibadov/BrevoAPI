using BrevoApi.Application.Interfaces.Repositories;
using BrevoApi.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BrevoApi.API.Controllers;

[ApiVersion("1.0")]
public class WebhooksController : BaseController
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(IUnitOfWork uow, ILogger<WebhooksController> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    /// <summary>Brevo webhook (delivered, opened, clicked, bounce, unsubscribe)</summary>
    [HttpPost("brevo")]
    public async Task<IActionResult> BrevoWebhook([FromBody] object payload)
    {
        try
        {
            var json = payload?.ToString() ?? string.Empty;
            dynamic? data = JsonConvert.DeserializeObject(json);
            var eventType = data?.@event?.ToString() ?? "";
            var messageId = data?["message-id"]?.ToString() ?? data?.messageId?.ToString() ?? "";

            _logger.LogInformation("Brevo webhook: {Event} | {MessageId}", eventType, messageId);

            if (!string.IsNullOrEmpty(messageId))
            {
                var log = await _uow.EmailLogs.FirstOrDefaultAsync(l => l.BrevoMessageId == messageId);
                if (log != null)
                {
                    log.Status = eventType switch
                    {
                        "delivered" => EmailLogStatus.Delivered,
                        "opened" => EmailLogStatus.Opened,
                        "clicked" => EmailLogStatus.Clicked,
                        "hard_bounce" or "soft_bounce" => EmailLogStatus.Bounced,
                        "unsubscribed" => EmailLogStatus.Unsubscribed,
                        "spam" => EmailLogStatus.Failed,
                        _ => log.Status
                    };
                    if (eventType == "opened" && log.OpenedAt == null) log.OpenedAt = DateTime.UtcNow;
                    if (eventType == "clicked" && log.ClickedAt == null) log.ClickedAt = DateTime.UtcNow;
                    log.UpdatedAt = DateTime.UtcNow;
                    await _uow.UpdateAsync(log);
                    await _uow.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook işleme hatası");
        }
        return Ok(new { received = true });
    }
}
