using TasksApi.Application.Abstractions;
using TasksApi.Application.Common;
using TasksApi.Application.Tasks.Dtos;
using TasksApi.Domain.Entities;
using TaskStatus = TasksApi.Domain.Enums.TaskStatus;

namespace TasksApi.Application.Tasks;

public class TaskService(ITaskRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<ServiceResult<PagedResult<TaskResponseDto>>> GetPagedAsync(
        string userId,
        string? estado,
        string? responsable,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return ServiceResult<PagedResult<TaskResponseDto>>.Invalid("page debe ser mayor o igual a 1.");
        if (pageSize < 1 || pageSize > 100)
            return ServiceResult<PagedResult<TaskResponseDto>>.Invalid("pageSize debe estar entre 1 y 100.");

        TaskStatus? status = null;
        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (!TryParseStatus(estado, out var parsed))
                return ServiceResult<PagedResult<TaskResponseDto>>.Invalid("estado debe ser 'pendiente', 'en curso' o 'hecha'.");
            status = parsed;
        }

        var (items, totalItems) = await repository.GetPagedAsync(userId, status, responsable, page, pageSize, cancellationToken);

        var result = new PagedResult<TaskResponseDto>
        {
            Items = items.Select(ToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };

        return ServiceResult<PagedResult<TaskResponseDto>>.Success(result);
    }

    public async Task<ServiceResult<TaskResponseDto>> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (task is null)
            return NotFound(id);

        return ServiceResult<TaskResponseDto>.Success(ToResponse(task));
    }

    public async Task<TaskResponseDto> CreateAsync(TaskCreateDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Estado = dto.Estado!.Value,
            Responsable = dto.Responsable.Trim(),
            FechaLimite = dto.FechaLimite!.Value,
            UserId = userId
        };

        await repository.AddAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(task);
    }

    public async Task<ServiceResult<TaskResponseDto>> UpdateAsync(int id, TaskUpdateDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (task is null)
            return NotFound(id);

        task.Titulo = dto.Titulo.Trim();
        task.Descripcion = dto.Descripcion?.Trim();
        task.Estado = dto.Estado!.Value;
        task.Responsable = dto.Responsable.Trim();
        task.FechaLimite = dto.FechaLimite!.Value;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<TaskResponseDto>.Success(ToResponse(task));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (task is null)
            return ServiceResult<bool>.NotFound($"No existe la tarea con id {id}.");

        repository.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }

    private static ServiceResult<TaskResponseDto> NotFound(int id) =>
        ServiceResult<TaskResponseDto>.NotFound($"No existe la tarea con id {id}.");

    private static bool TryParseStatus(string value, out TaskStatus status)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "pendiente":
                status = TaskStatus.Pendiente;
                return true;
            case "en curso":
                status = TaskStatus.EnCurso;
                return true;
            case "hecha":
                status = TaskStatus.Hecha;
                return true;
            default:
                status = default;
                return false;
        }
    }

    private static TaskResponseDto ToResponse(TaskItem task) => new()
    {
        Id = task.Id,
        Titulo = task.Titulo,
        Descripcion = task.Descripcion,
        Estado = task.Estado,
        Responsable = task.Responsable,
        FechaLimite = task.FechaLimite
    };
}
