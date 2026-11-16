using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TasksApi.Application.Abstractions;
using TasksApi.Infrastructure.Persistence;

namespace TasksApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
