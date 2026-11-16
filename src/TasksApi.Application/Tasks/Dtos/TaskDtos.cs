using System.ComponentModel.DataAnnotations;
using TasksApi.Domain.Enums;
using TaskStatus = TasksApi.Domain.Enums.TaskStatus;

namespace TasksApi.Application.Tasks.Dtos;

public class TaskCreateDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede superar los 2000 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public TaskStatus? Estado { get; set; }

    [Required(ErrorMessage = "El responsable es obligatorio.")]
    [StringLength(200, ErrorMessage = "El responsable no puede superar los 200 caracteres.")]
    public string Responsable { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha límite es obligatoria.")]
    public DateOnly? FechaLimite { get; set; }
}

public class TaskUpdateDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede superar los 2000 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public TaskStatus? Estado { get; set; }

    [Required(ErrorMessage = "El responsable es obligatorio.")]
    [StringLength(200, ErrorMessage = "El responsable no puede superar los 200 caracteres.")]
    public string Responsable { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha límite es obligatoria.")]
    public DateOnly? FechaLimite { get; set; }
}

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public TaskStatus Estado { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public DateOnly FechaLimite { get; set; }
}
