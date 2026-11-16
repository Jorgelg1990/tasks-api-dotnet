using Microsoft.EntityFrameworkCore;
using TasksApi.Application.Abstractions;
using TasksApi.Domain.Entities;
using TaskStatus = TasksApi.Domain.Enums.TaskStatus;

namespace TasksApi.Infrastructure.Persistence;

public class TaskRepository(AppDbContext db) : ITaskRepository
{
    public async Task<(IReadOnlyList<TaskItem> Items, int TotalItems)> GetPagedAsync(
        string userId,
        TaskStatus? estado,
        string? responsable,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TaskItem> query = db.Tasks.AsNoTracking().Where(t => t.UserId == userId);

        if (estado is not null)
            query = query.Where(t => t.Estado == estado.Value);

        if (!string.IsNullOrWhiteSpace(responsable))
            query = query.Where(t => t.Responsable == responsable);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public Task<TaskItem?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default) =>
        await db.Tasks.AddAsync(task, cancellationToken);

    public void Remove(TaskItem task) => db.Tasks.Remove(task);
}
