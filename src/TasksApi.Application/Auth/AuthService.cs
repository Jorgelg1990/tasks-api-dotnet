using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TasksApi.Application.Common;
using TasksApi.Domain.Entities;

namespace TasksApi.Application.Auth;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IRefreshTokenStore refreshTokenStore)
{
    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return ServiceResult<AuthResponseDto>.Invalid("conflict:El correo ya está registrado.");

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim().ToLowerInvariant(),
            Email = request.Email.Trim().ToLowerInvariant()
        };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            var msg = string.Join("; ", create.Errors.Select(e => e.Description));
            return ServiceResult<AuthResponseDto>.Invalid(msg);
        }

        var auth = await IssueTokensAsync(user, cancellationToken);
        return ServiceResult<AuthResponseDto>.Success(auth);
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return ServiceResult<AuthResponseDto>.NotFound("Credenciales inválidas."); // mapped to 401 by controller

        var auth = await IssueTokensAsync(user, cancellationToken);
        return ServiceResult<AuthResponseDto>.Success(auth);
    }

    public async Task<ServiceResult<AuthResponseDto>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return ServiceResult<AuthResponseDto>.NotFound("Token de actualización inválido.");

        var hash = ComputeHash(refreshToken);
        var stored = await refreshTokenStore.FindByHashAsync(hash, cancellationToken);
        if (stored is null)
            return ServiceResult<AuthResponseDto>.NotFound("Token de actualización inválido.");

        if (stored.IsRevoked)
        {
            await refreshTokenStore.RevokeAllForUserAsync(stored.UserId, cancellationToken);
            return ServiceResult<AuthResponseDto>.NotFound("Token de actualización inválido.");
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
            return ServiceResult<AuthResponseDto>.NotFound("Token de actualización inválido.");

        var user = await userManager.FindByIdAsync(stored.UserId);
        if (user is null)
            return ServiceResult<AuthResponseDto>.NotFound("Token de actualización inválido.");

        var (access, accessExp) = tokenService.CreateAccessToken(user);
        var (newRefresh, newHash, newExp) = tokenService.CreateRefreshToken();

        await refreshTokenStore.RevokeAsync(stored, newHash, cancellationToken);
        await refreshTokenStore.StoreAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = newExp
        }, cancellationToken);
        await refreshTokenStore.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = access,
            AccessTokenExpiresAt = accessExp,
            RefreshToken = newRefresh,
            RefreshTokenExpiresAt = newExp
        });
    }

    public async Task<ServiceResult<bool>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return ServiceResult<bool>.NotFound("Token de actualización inválido.");

        var hash = ComputeHash(refreshToken);
        var stored = await refreshTokenStore.FindByHashAsync(hash, cancellationToken);
        if (stored is null)
            return ServiceResult<bool>.NotFound("Token de actualización inválido.");

        if (!stored.IsRevoked)
        {
            await refreshTokenStore.RevokeAsync(stored, null, cancellationToken);
            await refreshTokenStore.SaveChangesAsync(cancellationToken);
        }

        return ServiceResult<bool>.Success(true);
    }

    public async Task<AuthResponseDto> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var (access, accessExp) = tokenService.CreateAccessToken(user);
        var (refresh, refreshHash, refreshExp) = tokenService.CreateRefreshToken();

        await refreshTokenStore.StoreAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExp
        }, cancellationToken);
        await refreshTokenStore.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = access,
            AccessTokenExpiresAt = accessExp,
            RefreshToken = refresh,
            RefreshTokenExpiresAt = refreshExp
        };
    }

    private static string ComputeHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
