using Microsoft.AspNetCore.Mvc;
using TasksApi.Application.Common;
using TasksApi.Application.Tasks;
using TasksApi.Application.Tasks.Dtos;

namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(TaskService tasks) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskResponseDto>>> GetTasks(
        [FromQuery] string? estado,
        [FromQuery] string? responsable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await tasks.GetPagedAsync(estado, responsable, page, pageSize, cancellationToken);
        if (result.Kind != ResultKind.Success)
            return ToProblem(result.Kind, result.Message);

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponseDto>> GetTask(int id, CancellationToken cancellationToken = default)
    {
        var result = await tasks.GetByIdAsync(id, cancellationToken);
        if (result.Kind != ResultKind.Success)
            return ToProblem(result.Kind, result.Message);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponseDto>> CreateTask([FromBody] TaskCreateDto dto, CancellationToken cancellationToken = default)
    {
        var response = await tasks.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, [FromBody] TaskUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var result = await tasks.UpdateAsync(id, dto, cancellationToken);
        if (result.Kind != ResultKind.Success)
            return ToProblem(result.Kind, result.Message);

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken = default)
    {
        var result = await tasks.DeleteAsync(id, cancellationToken);
        if (result.Kind != ResultKind.Success)
            return ToProblem(result.Kind, result.Message);

        return NoContent();
    }

    private ObjectResult ToProblem(ResultKind kind, string? message) => kind switch
    {
        ResultKind.NotFound => NotFound(new ProblemDetails { Title = "No encontrada", Detail = message, Status = StatusCodes.Status404NotFound }),
        _ => BadRequest(new ProblemDetails { Title = "Parámetro inválido", Detail = message, Status = StatusCodes.Status400BadRequest })
    };
}
