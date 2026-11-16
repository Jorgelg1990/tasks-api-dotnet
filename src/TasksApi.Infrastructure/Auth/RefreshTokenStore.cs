using TasksApi.Application.Auth;
using TasksApi.Domain.Entities;
using TasksApi.Infrastructure.Persistence;

namespace TasksApi.Infrastructure.Auth;

public class RefreshTokenStore(AppDbContext db) : IRefreshTokenStore
{
    public async Task StoreAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync(cancellationToken);
    }
}
