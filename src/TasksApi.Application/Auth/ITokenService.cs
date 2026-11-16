using TasksApi.Domain.Entities;

namespace TasksApi.Application.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(ApplicationUser user);
    (string Token, string TokenHash, DateTime ExpiresAt) CreateRefreshToken();
}
