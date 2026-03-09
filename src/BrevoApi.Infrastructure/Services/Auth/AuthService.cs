using BrevoApi.Application.DTOs.Auth;
using BrevoApi.Application.DTOs.Email;
using BrevoApi.Application.Interfaces.Services;
using BrevoApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace BrevoApi.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IBrevoEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        ITokenService tokenService, IBrevoEmailService emailService, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return new AuthResponseDto { Success = false, Message = "Bu email zaten kayıtlı." };

        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return new AuthResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };

        await _userManager.AddToRoleAsync(user, "User");

        var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await SendConfirmationEmailAsync(user, confirmToken);

        return new AuthResponseDto { Success = true, Message = "Kayıt başarılı. Email adresinizi onaylayın." };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.IsDeleted)
            return new AuthResponseDto { Success = false, Message = "Email veya şifre hatalı." };

        if (!user.IsActive)
            return new AuthResponseDto { Success = false, Message = "Hesabınız deaktif edilmiş." };

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return new AuthResponseDto { Success = false, Message = "Hesap kilitlendi. Lütfen bekleyin." };
            return new AuthResponseDto { Success = false, Message = "Email veya şifre hatalı." };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = request.RememberMe
            ? DateTime.UtcNow.AddDays(30)
            : DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60")),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Roles = roles,
                IsEmailConfirmed = user.EmailConfirmed
            }
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return new AuthResponseDto { Success = false, Message = "Geçersiz token." };

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !int.TryParse(userId, out var id))
            return new AuthResponseDto { Success = false, Message = "Geçersiz token." };

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.RefreshToken != request.RefreshToken
            || user.RefreshTokenExpiry <= DateTime.UtcNow)
            return new AuthResponseDto { Success = false, Message = "Refresh token geçersiz veya süresi dolmuş." };

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60")),
            User = new UserInfoDto
            {
                Id = user.Id, Email = user.Email!,
                FullName = user.FullName, Roles = roles,
                IsEmailConfirmed = user.EmailConfirmed
            }
        };
    }

    public async Task<bool> RevokeTokenAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) return true;
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(2);
        await _userManager.UpdateAsync(user);
        await SendPasswordResetEmailAsync(user, token);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow) return false;
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded) return false;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return result.Succeeded;
    }

    public async Task<bool> ConfirmEmailAsync(ConfirmEmailRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) return false;
        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (result.Succeeded) { user.EmailConfirmationToken = null; await _userManager.UpdateAsync(user); }
        return result.Succeeded;
    }

    public async Task<bool> ResendConfirmationEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.EmailConfirmed) return false;
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await SendConfirmationEmailAsync(user, token);
        return true;
    }

    public async Task LogoutAsync(int userId) => await RevokeTokenAsync(userId);

    private async Task SendConfirmationEmailAsync(AppUser user, string token)
    {
        var baseUrl = _configuration["AppSettings:FrontendUrl"];
        var encodedToken = Uri.EscapeDataString(token);
        var confirmUrl = $"{baseUrl}/confirm-email?email={user.Email}&token={encodedToken}";
        await _emailService.SendTransactionalEmailAsync(new SendEmailRequestDto
        {
            To = new List<EmailRecipientDto> { new() { Email = user.Email!, Name = user.FullName } },
            Subject = "Email Adresinizi Doğrulayın",
            SenderName = _configuration["AppSettings:SenderName"] ?? "BrevoApp",
            SenderEmail = _configuration["AppSettings:SenderEmail"] ?? "noreply@example.com",
            HtmlContent = $@"<h2>Merhaba {user.FullName}!</h2>
<p>Hesabınızı aktifleştirmek için aşağıdaki linke tıklayın:</p>
<a href='{confirmUrl}' style='background:#4F46E5;color:white;padding:12px 24px;
text-decoration:none;border-radius:6px;display:inline-block;margin:16px 0;'>
Email Adresimi Doğrula</a>
<p>Bu link 24 saat geçerlidir.</p>"
        });
    }

    private async Task SendPasswordResetEmailAsync(AppUser user, string token)
    {
        var baseUrl = _configuration["AppSettings:FrontendUrl"];
        var encodedToken = Uri.EscapeDataString(token);
        var resetUrl = $"{baseUrl}/reset-password?email={user.Email}&token={encodedToken}";
        await _emailService.SendTransactionalEmailAsync(new SendEmailRequestDto
        {
            To = new List<EmailRecipientDto> { new() { Email = user.Email!, Name = user.FullName } },
            Subject = "Şifre Sıfırlama Talebi",
            SenderName = _configuration["AppSettings:SenderName"] ?? "BrevoApp",
            SenderEmail = _configuration["AppSettings:SenderEmail"] ?? "noreply@example.com",
            HtmlContent = $@"<h2>Şifre Sıfırlama</h2>
<p>Merhaba {user.FullName},</p>
<p>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
<a href='{resetUrl}' style='background:#EF4444;color:white;padding:12px 24px;
text-decoration:none;border-radius:6px;display:inline-block;margin:16px 0;'>
Şifremi Sıfırla</a>
<p>Bu link 2 saat geçerlidir. Eğer bu talebi siz yapmadıysanız bu emaili görmezden gelin.</p>"
        });
    }
}
