using System.Security.Cryptography;
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

        return new AuthResponseDto
        {
            AccessToken = access,
            AccessTokenExpiresAt = accessExp,
            RefreshToken = refresh,
            RefreshTokenExpiresAt = refreshExp
        };
    }
}

public interface IRefreshTokenStore
{
    Task StoreAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
