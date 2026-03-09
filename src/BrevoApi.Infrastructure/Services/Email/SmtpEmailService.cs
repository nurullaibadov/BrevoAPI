using BrevoApi.Application.DTOs.Email;
using BrevoApi.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace BrevoApi.Infrastructure.Services.Email;

public class SmtpEmailService : ISmtpEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private SmtpSettings GetSettings()
    {
        var section = _configuration.GetSection("SmtpSettings");
        return new SmtpSettings
        {
            Host = section["Host"] ?? "smtp.gmail.com",
            Port = int.Parse(section["Port"] ?? "587"),
            Username = section["Username"] ?? string.Empty,
            Password = section["Password"] ?? string.Empty,
            EnableSsl = bool.Parse(section["EnableSsl"] ?? "true"),
            UseStartTls = bool.Parse(section["UseStartTls"] ?? "true"),
            DefaultSenderName = section["DefaultSenderName"]
                ?? _configuration["AppSettings:SenderName"]
                ?? "BrevoApp",
            DefaultSenderEmail = section["DefaultSenderEmail"]
                ?? _configuration["AppSettings:SenderEmail"]
                ?? "noreply@example.com"
        };
    }

    public async Task<SmtpSendResponseDto> SendAsync(SmtpSendRequestDto request)
    {
        try
        {
            var settings = GetSettings();
            var message = BuildMimeMessage(request, settings);
            return await SendMessageAsync(message, settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP gönderme hatası");
            return new SmtpSendResponseDto { Success = false, Message = ex.Message };
        }
    }

    public async Task<SmtpSendResponseDto> SendAsync(
        string toEmail, string toName, string subject, string htmlContent,
        string? textContent = null, string? senderName = null, string? senderEmail = null)
    {
        return await SendAsync(new SmtpSendRequestDto
        {
            To = new List<EmailRecipientDto> { new() { Email = toEmail, Name = toName } },
            Subject = subject,
            HtmlContent = htmlContent,
            TextContent = textContent,
            SenderName = senderName,
            SenderEmail = senderEmail
        });
    }

    public async Task<SmtpSendResponseDto> SendWithTemplateAsync(
        string toEmail, string toName, string subject, string templateHtml,
        Dictionary<string, string>? variables = null,
        string? senderName = null, string? senderEmail = null)
    {
        var html = templateHtml;
        if (variables != null)
            foreach (var kv in variables)
                html = html.Replace($"{{{{{kv.Key}}}}}", kv.Value);

        return await SendAsync(toEmail, toName, subject, html,
            senderName: senderName, senderEmail: senderEmail);
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var settings = GetSettings();
            using var client = new SmtpClient();
            await ConnectAsync(client, settings);
            await client.DisconnectAsync(true);
            _logger.LogInformation("SMTP bağlantı testi başarılı: {Host}:{Port}", settings.Host, settings.Port);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP bağlantı testi başarısız");
            return false;
        }
    }

    public async Task<SmtpSendResponseDto> SendTestEmailAsync(SmtpTestRequestDto request)
    {
        return await SendAsync(
            toEmail: request.ToEmail,
            toName: request.ToName ?? request.ToEmail,
            subject: "SMTP Test Email - BrevoApi",
            htmlContent: @"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px'>
                    <h2 style='color:#4F46E5'>SMTP Test Başarılı!</h2>
                    <p>Bu email BrevoApi SMTP servisinden gönderilmiştir.</p>
                    <p><strong>Gönderim zamanı:</strong> " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @" UTC</p>
                    <hr style='border:1px solid #e5e7eb'>
                    <p style='color:#6b7280;font-size:12px'>BrevoApi - ASP.NET Core 8 Onion Architecture</p>
                </div>"
        );
    }

    private MimeMessage BuildMimeMessage(SmtpSendRequestDto request, SmtpSettings settings)
    {
        var message = new MimeMessage();

        // Sender
        var fromName = request.SenderName ?? settings.DefaultSenderName;
        var fromEmail = request.SenderEmail ?? settings.DefaultSenderEmail;
        message.From.Add(new MailboxAddress(fromName, fromEmail));

        // To
        foreach (var r in request.To)
            message.To.Add(new MailboxAddress(r.Name ?? r.Email, r.Email));

        // CC
        if (request.Cc != null)
            foreach (var r in request.Cc)
                message.Cc.Add(new MailboxAddress(r.Name ?? r.Email, r.Email));

        // BCC
        if (request.Bcc != null)
            foreach (var r in request.Bcc)
                message.Bcc.Add(new MailboxAddress(r.Name ?? r.Email, r.Email));

        message.Subject = request.Subject;

        // Priority
        message.Priority = request.Priority switch
        {
            1 => MessagePriority.Urgent,
            5 => MessagePriority.NonUrgent,
            _ => MessagePriority.Normal
        };

        // Body
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = request.HtmlContent,
            TextBody = request.TextContent
        };

        // Attachments
        if (request.Attachments != null)
        {
            foreach (var attachment in request.Attachments)
            {
                var bytes = Convert.FromBase64String(attachment.Content);
                bodyBuilder.Attachments.Add(attachment.Name, bytes);
            }
        }

        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }

    private async Task<SmtpSendResponseDto> SendMessageAsync(MimeMessage message, SmtpSettings settings)
    {
        using var client = new SmtpClient();
        try
        {
            await ConnectAsync(client, settings);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("SMTP email gönderildi: {To} | {Subject}",
                string.Join(", ", message.To.Mailboxes.Select(m => m.Address)),
                message.Subject);

            return new SmtpSendResponseDto { Success = true, Message = "Email gönderildi." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP gönderme hatası: {To}", message.To);
            return new SmtpSendResponseDto { Success = false, Message = ex.Message };
        }
    }

    private async Task ConnectAsync(SmtpClient client, SmtpSettings settings)
    {
        var secureOption = settings.UseStartTls
            ? SecureSocketOptions.StartTls
            : settings.EnableSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.None;

        await client.ConnectAsync(settings.Host, settings.Port, secureOption);

        if (!string.IsNullOrEmpty(settings.Username))
            await client.AuthenticateAsync(settings.Username, settings.Password);
    }

    private class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public bool UseStartTls { get; set; }
        public string DefaultSenderName { get; set; } = string.Empty;
        public string DefaultSenderEmail { get; set; } = string.Empty;
    }
}
