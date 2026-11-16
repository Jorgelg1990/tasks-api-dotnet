using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TasksApi.Application.Auth;
using TasksApi.Application.Common;
using TasksApi.Application.Tasks.Dtos;
using TaskStatus = TasksApi.Domain.Enums.TaskStatus;

namespace TasksApi.Tests;

public class AuthEndpointsTests(TasksApiFactory factory) : IClassFixture<TasksApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_Login_And_AccessProtectedEndpoint_Works()
    {
        var auth = await RegisterAndGetAuthAsync("auth-flow@test.local", "Pass123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/tasks");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TasksEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_RotatesToken_And_ReuseRevokesAllSessions()
    {
        var auth = await RegisterAndGetAuthAsync("refresh@test.local", "Pass123!");
        var firstRefresh = auth.RefreshToken;

        var refresh1 = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto { RefreshToken = firstRefresh });
        refresh1.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotated = await refresh1.Content.ReadFromJsonAsync<AuthResponseDto>();
        rotated.Should().NotBeNull();
        rotated!.RefreshToken.Should().NotBe(firstRefresh);

        // Reuse of revoked refresh token must revoke all sessions.
        var reuse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto { RefreshToken = firstRefresh });
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // The rotated token must also be invalid after reuse detection.
        var afterReuse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto { RefreshToken = rotated.RefreshToken });
        afterReuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var auth = await RegisterAndGetAuthAsync("logout@test.local", "Pass123!");

        var logout = await _client.PostAsJsonAsync("/api/auth/logout", new RefreshRequestDto { RefreshToken = auth.RefreshToken });
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto { RefreshToken = auth.RefreshToken });
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UserA_Cannot_Read_Edit_Or_Delete_UserB_Task()
    {
        var userA = await RegisterAndGetAuthAsync("usera@test.local", "Pass123!");
        var userB = await RegisterAndGetAuthAsync("userb@test.local", "Pass123!");

        var createDto = new TaskCreateDto
        {
            Titulo = "Privada",
            Descripcion = "Solo B",
            Estado = TaskStatus.Pendiente,
            Responsable = "B",
            FechaLimite = new DateOnly(2026, 12, 31)
        };

        using (var createReq = AuthorizedJsonRequest(HttpMethod.Post, "/api/tasks", createDto, userB.AccessToken))
        {
            var createRes = await _client.SendAsync(createReq);
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createRes.Content.ReadFromJsonAsync<TaskResponseDto>();
            created.Should().NotBeNull();
            var id = created!.Id;

            using var getReq = AuthorizedRequest(HttpMethod.Get, $"/api/tasks/{id}", userA.AccessToken);
            (await _client.SendAsync(getReq)).StatusCode.Should().Be(HttpStatusCode.NotFound);

            var updateDto = new TaskUpdateDto
            {
                Titulo = "Hack",
                Descripcion = "x",
                Estado = TaskStatus.Hecha,
                Responsable = "A",
                FechaLimite = new DateOnly(2027, 1, 1)
            };
            using var putReq = AuthorizedJsonRequest(HttpMethod.Put, $"/api/tasks/{id}", updateDto, userA.AccessToken);
            (await _client.SendAsync(putReq)).StatusCode.Should().Be(HttpStatusCode.NotFound);

            using var delReq = AuthorizedRequest(HttpMethod.Delete, $"/api/tasks/{id}", userA.AccessToken);
            (await _client.SendAsync(delReq)).StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Owner can still see it.
            using var ownerGet = AuthorizedRequest(HttpMethod.Get, $"/api/tasks/{id}", userB.AccessToken);
            (await _client.SendAsync(ownerGet)).StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        await RegisterAndGetAuthAsync("dup@test.local", "Pass123!");
        var second = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto { Email = "dup@test.local", Password = "Pass123!" });
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        await RegisterAndGetAuthAsync("badlogin@test.local", "Pass123!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = "badlogin@test.local", Password = "wrong" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<AuthResponseDto> RegisterAndGetAuthAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto { Email = email, Password = password });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        auth.Should().NotBeNull();
        return auth!;
    }

    private static HttpRequestMessage AuthorizedRequest(HttpMethod method, string url, string token)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    private static HttpRequestMessage AuthorizedJsonRequest<T>(HttpMethod method, string url, T body, string token)
    {
        var req = AuthorizedRequest(method, url, token);
        req.Content = JsonContent.Create(body);
        return req;
    }
}
