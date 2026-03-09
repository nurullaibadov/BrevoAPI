using BrevoApi.Application.DTOs.Email;

namespace BrevoApi.Application.Interfaces.Services;

public interface ISmtpEmailService
{
    Task<SmtpSendResponseDto> SendAsync(SmtpSendRequestDto request);
    Task<SmtpSendResponseDto> SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        string? textContent = null,
        string? senderName = null,
        string? senderEmail = null);
    Task<SmtpSendResponseDto> SendWithTemplateAsync(
        string toEmail,
        string toName,
        string subject,
        string templateHtml,
        Dictionary<string, string>? variables = null,
        string? senderName = null,
        string? senderEmail = null);
    Task<bool> TestConnectionAsync();
    Task<SmtpSendResponseDto> SendTestEmailAsync(SmtpTestRequestDto request);
}
