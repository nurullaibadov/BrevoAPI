using BrevoApi.Application.DTOs.Email;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace BrevoApi.Infrastructure.Services.Email;

public class BrevoEmailService : IBrevoEmailService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(IHttpClientFactory httpClientFactory,
        IConfiguration configuration, ILogger<BrevoEmailService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("Brevo");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("api-key", _configuration["Brevo:ApiKey"]);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    }

    public async Task<SendEmailResponseDto> SendTransactionalEmailAsync(SendEmailRequestDto request)
    {
        try
        {
            var client = CreateClient();
            var payload = new
            {
                sender = new { name = request.SenderName, email = request.SenderEmail },
                to = request.To.Select(r => new { email = r.Email, name = r.Name }),
                cc = request.Cc?.Select(r => new { email = r.Email, name = r.Name }),
                bcc = request.Bcc?.Select(r => new { email = r.Email, name = r.Name }),
                subject = request.Subject,
                htmlContent = request.HtmlContent,
                textContent = request.TextContent,
                attachment = request.Attachments?.Select(a => new { content = a.Content, name = a.Name })
            };

            var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Brevo error {Status}: {Body}", response.StatusCode, body);
                return new SendEmailResponseDto { Success = false, Message = body };
            }

            dynamic? result = JsonConvert.DeserializeObject(body);
            return new SendEmailResponseDto
            {
                Success = true,
                MessageId = result?.messageId?.ToString(),
                Message = "Email gönderildi."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email gönderme hatası: {Email}", request.To.FirstOrDefault()?.Email);
            return new SendEmailResponseDto { Success = false, Message = ex.Message };
        }
    }

    public async Task<SendEmailResponseDto> SendTemplateEmailAsync(SendTemplateEmailRequestDto request)
    {
        try
        {
            var client = CreateClient();
            var payload = new
            {
                sender = new { name = request.SenderName, email = request.SenderEmail },
                to = request.To.Select(r => new { email = r.Email, name = r.Name }),
                templateId = request.TemplateId,
                @params = request.Params
            };
            var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new SendEmailResponseDto { Success = false, Message = body };

            dynamic? result = JsonConvert.DeserializeObject(body);
            return new SendEmailResponseDto
            {
                Success = true,
                MessageId = result?.messageId?.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Template email hatası");
            return new SendEmailResponseDto { Success = false, Message = ex.Message };
        }
    }

    public async Task<bool> SendBulkEmailAsync(BulkEmailRequestDto request)
    {
        var tasks = request.Emails.Select(e => SendTransactionalEmailAsync(e));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r.Success);
    }

    public async Task<bool> SendSmtpEmailAsync(SmtpEmailRequestDto request)
    {
        var result = await SendTransactionalEmailAsync(new SendEmailRequestDto
        {
            To = request.To,
            Subject = request.Subject,
            HtmlContent = request.HtmlContent,
            SenderName = request.SenderName,
            SenderEmail = request.SenderEmail
        });
        return result.Success;
    }

    public async Task<EmailStatsDto> GetEmailStatsAsync(string messageId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"https://api.brevo.com/v3/smtp/emails/{messageId}");
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return new EmailStatsDto { MessageId = messageId, Status = "Unknown" };

            dynamic? result = JsonConvert.DeserializeObject(body);
            return new EmailStatsDto
            {
                MessageId = messageId,
                Status = result?.events?[0]?.name?.ToString() ?? "Unknown"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stats alınamadı: {MessageId}", messageId);
            return new EmailStatsDto { MessageId = messageId, Status = "Error" };
        }
    }
}
