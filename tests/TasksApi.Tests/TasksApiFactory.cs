using System.Data.Common;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TasksApi.Infrastructure.Persistence;

namespace TasksApi.Tests;

public class TasksApiFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TEST_ONLY_super_secret_key_for_integration_tests_123456",
                ["Jwt:Issuer"] = "TasksApi",
                ["Jwt:Audience"] = "TasksApi.Client",
                ["Jwt:AccessTokenMinutes"] = "15",
                ["Jwt:RefreshTokenDays"] = "7"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            const string key = "TEST_ONLY_super_secret_key_for_integration_tests_123456";
            services.PostConfigure<TasksApi.Infrastructure.Auth.JwtSettings>(opts =>
            {
                opts.Key = key;
                opts.Issuer = "TasksApi";
                opts.Audience = "TasksApi.Client";
                opts.AccessTokenMinutes = 15;
                opts.RefreshTokenDays = 7;
            });
            services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters.IssuerSigningKey =
                        new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
                    options.TokenValidationParameters.ValidIssuer = "TasksApi";
                    options.TokenValidationParameters.ValidAudience = "TasksApi.Client";
                });

            services.Configure<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>(options =>
            {
                options.RejectionStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(_ =>
                    System.Threading.RateLimiting.RateLimitPartition.GetNoLimiter("tests"));
            });
            // Replace endpoint policy with no-op limiter by reconfiguring the specific endpoint metadata at runtime is hard;
            // instead, remove the EnableRateLimiting attribute effect by making the policy resolver return no limiter.
            // Simpler: unregister and re-register rate limiter services with a permissive "auth" policy.
            var rlDescriptors = services.Where(d => d.ServiceType == typeof(Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>)
                || d.ServiceType == typeof(Microsoft.Extensions.Options.IPostConfigureOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>)).ToList();
            foreach (var d in rlDescriptors) services.Remove(d);
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(_ =>
                    System.Threading.RateLimiting.RateLimitPartition.GetNoLimiter("tests"));
                options.AddPolicy("auth", _ =>
                    System.Threading.RateLimiting.RateLimitPartition.GetNoLimiter("tests-auth"));
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
