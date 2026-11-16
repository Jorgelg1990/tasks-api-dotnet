using Microsoft.EntityFrameworkCore;
using TasksApi.Application.Auth;
using TasksApi.Domain.Entities;
using TasksApi.Infrastructure.Persistence;

namespace TasksApi.Infrastructure.Auth;

public class RefreshTokenStore(AppDbContext db) : IRefreshTokenStore
{
    public async Task StoreAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        db.RefreshTokens.Add(token);
        await Task.CompletedTask;
    }

    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);

    public Task RevokeAsync(RefreshToken token, string? replacedByTokenHash, CancellationToken cancellationToken = default)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByTokenHash = replacedByTokenHash;
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var tokens = await db.RefreshTokens.Where(r => r.UserId == userId && r.RevokedAt == null).ToListAsync(cancellationToken);
        foreach (var t in tokens)
            t.RevokedAt = now;
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
