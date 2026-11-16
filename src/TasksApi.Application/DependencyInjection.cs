using Microsoft.Extensions.DependencyInjection;
using TasksApi.Application.Auth;
using TasksApi.Application.Tasks;

namespace TasksApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<TaskService>();
        services.AddScoped<AuthService>();
        return services;
    }
}
