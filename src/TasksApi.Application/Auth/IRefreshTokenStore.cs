using TasksApi.Domain.Entities;

namespace TasksApi.Application.Auth;

public interface IRefreshTokenStore
{
    Task StoreAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAsync(RefreshToken token, string? replacedByTokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
