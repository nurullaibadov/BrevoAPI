using BrevoApi.Application.DTOs.Email;

namespace BrevoApi.Application.Interfaces.Services;

public interface IBrevoEmailService
{
    Task<SendEmailResponseDto> SendTransactionalEmailAsync(SendEmailRequestDto request);
    Task<SendEmailResponseDto> SendTemplateEmailAsync(SendTemplateEmailRequestDto request);
    Task<bool> SendBulkEmailAsync(BulkEmailRequestDto request);
    Task<bool> SendSmtpEmailAsync(SmtpEmailRequestDto request);
    Task<EmailStatsDto> GetEmailStatsAsync(string messageId);
}
