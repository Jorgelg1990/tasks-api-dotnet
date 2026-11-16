using System.ComponentModel.DataAnnotations;

namespace TasksApi.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Descripcion { get; set; }

    [Required]
    public TaskStatus Estado { get; set; } = TaskStatus.Pendiente;

    [Required]
    [MaxLength(200)]
    public string Responsable { get; set; } = string.Empty;

    [Required]
    public DateOnly FechaLimite { get; set; }
}
