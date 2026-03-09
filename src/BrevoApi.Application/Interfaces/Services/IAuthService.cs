using BrevoApi.Application.DTOs.Auth;

namespace BrevoApi.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<bool> RevokeTokenAsync(int userId);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
    Task<bool> ConfirmEmailAsync(ConfirmEmailRequestDto request);
    Task<bool> ResendConfirmationEmailAsync(string email);
    Task LogoutAsync(int userId);
}
