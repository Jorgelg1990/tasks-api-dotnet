using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksApi.Data;
using TasksApi.Dtos;
using TasksApi.Models;
using TaskStatus = TasksApi.Models.TaskStatus;

namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskResponseDto>>> GetTasks(
        [FromQuery] string? estado,
        [FromQuery] string? responsable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1)
            return BadRequest(new ProblemDetails { Title = "Parámetro inválido", Detail = "page debe ser mayor o igual a 1.", Status = StatusCodes.Status400BadRequest });
        if (pageSize < 1 || pageSize > 100)
            return BadRequest(new ProblemDetails { Title = "Parámetro inválido", Detail = "pageSize debe estar entre 1 y 100.", Status = StatusCodes.Status400BadRequest });

        IQueryable<TaskItem> query = db.Tasks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (!TryParseStatus(estado, out var status))
                return BadRequest(new ProblemDetails { Title = "Parámetro inválido", Detail = "estado debe ser 'pendiente', 'en curso' o 'hecha'.", Status = StatusCodes.Status400BadRequest });
            query = query.Where(t => t.Estado == status);
        }

        if (!string.IsNullOrWhiteSpace(responsable))
        {
            query = query.Where(t => t.Responsable == responsable);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = await query
            .OrderBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => ToResponse(t))
            .ToListAsync();

        return Ok(new PagedResult<TaskResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
    {
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
            return NotFound(new ProblemDetails { Title = "No encontrada", Detail = $"No existe la tarea con id {id}.", Status = StatusCodes.Status404NotFound });

        return Ok(ToResponse(task));
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponseDto>> CreateTask([FromBody] TaskCreateDto dto)
    {
        var task = new TaskItem
        {
            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Estado = dto.Estado!.Value,
            Responsable = dto.Responsable.Trim(),
            FechaLimite = dto.FechaLimite!.Value
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var response = ToResponse(task);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, [FromBody] TaskUpdateDto dto)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
            return NotFound(new ProblemDetails { Title = "No encontrada", Detail = $"No existe la tarea con id {id}.", Status = StatusCodes.Status404NotFound });

        task.Titulo = dto.Titulo.Trim();
        task.Descripcion = dto.Descripcion?.Trim();
        task.Estado = dto.Estado!.Value;
        task.Responsable = dto.Responsable.Trim();
        task.FechaLimite = dto.FechaLimite!.Value;

        await db.SaveChangesAsync();
        return Ok(ToResponse(task));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
            return NotFound(new ProblemDetails { Title = "No encontrada", Detail = $"No existe la tarea con id {id}.", Status = StatusCodes.Status404NotFound });

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return NoContent();
    }

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
