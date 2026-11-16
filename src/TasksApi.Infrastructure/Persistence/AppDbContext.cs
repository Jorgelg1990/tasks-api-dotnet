using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TasksApi.Domain.Entities;
using TasksApi.Domain.Enums;
using TaskStatus = TasksApi.Domain.Enums.TaskStatus;

namespace TasksApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var statusConverter = new ValueConverter<TaskStatus, string>(
            v => ToDbValue(v),
            v => FromDbValue(v));

        builder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Descripcion).HasMaxLength(2000);
            entity.Property(t => t.Estado).HasConversion(statusConverter).IsRequired().HasMaxLength(20);
            entity.Property(t => t.Responsable).IsRequired().HasMaxLength(200);
            entity.Property(t => t.FechaLimite).IsRequired();
            entity.Property(t => t.UserId).HasMaxLength(450);
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(t => t.UserId);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TokenHash).IsRequired().HasMaxLength(256);
            entity.Property(r => r.UserId).IsRequired().HasMaxLength(450);
            entity.Property(r => r.ExpiresAt).IsRequired();
            entity.HasIndex(r => r.TokenHash).IsUnique();
            entity.HasIndex(r => r.UserId);
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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
