using TasksApi.Domain.Entities;
using TaskStatus = TasksApi.Domain.Enums.TaskStatus;

namespace TasksApi.Application.Abstractions;

public interface ITaskRepository
{
    Task<(IReadOnlyList<TaskItem> Items, int TotalItems)> GetPagedAsync(
        TaskStatus? estado,
        string? responsable,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    void Remove(TaskItem task);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
