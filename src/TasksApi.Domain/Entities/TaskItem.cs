using TasksApi.Domain.Enums;
using TaskStatus = TasksApi.Domain.Enums.TaskStatus;

namespace TasksApi.Domain.Entities;

public class TaskItem
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public TaskStatus Estado { get; set; } = TaskStatus.Pendiente;

    public string Responsable { get; set; } = string.Empty;

    public DateOnly FechaLimite { get; set; }
}
