using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TasksApi.Domain.Entities;
using TasksApi.Domain.Enums;
using TaskStatus = TasksApi.Domain.Enums.TaskStatus;

namespace TasksApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var statusConverter = new ValueConverter<TaskStatus, string>(
            v => ToDbValue(v),
            v => FromDbValue(v));

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Descripcion).HasMaxLength(2000);
            entity.Property(t => t.Estado).HasConversion(statusConverter).IsRequired().HasMaxLength(20);
            entity.Property(t => t.Responsable).IsRequired().HasMaxLength(200);
            entity.Property(t => t.FechaLimite).IsRequired();
        });
    }

    private static string ToDbValue(TaskStatus status) => status switch
    {
        TaskStatus.Pendiente => "pendiente",
        TaskStatus.EnCurso => "en curso",
        TaskStatus.Hecha => "hecha",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Estado inválido")
    };

    private static TaskStatus FromDbValue(string value) => value switch
    {
        "pendiente" => TaskStatus.Pendiente,
        "en curso" => TaskStatus.EnCurso,
        "hecha" => TaskStatus.Hecha,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Estado inválido")
    };
}
