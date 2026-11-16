using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TasksApi.Application.Common;
using TasksApi.Application.Tasks.Dtos;
using TaskStatus = TasksApi.Domain.Enums.TaskStatus;

namespace TasksApi.Tests;

public class TasksEndpointsTests(TasksApiFactory factory) : IClassFixture<TasksApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateTask_ThenGetById_ReturnsCreatedTask()
    {
        var createDto = new TaskCreateDto
        {
            Titulo = "Implementar API",
            Descripcion = "Crear endpoints CRUD",
            Estado = TaskStatus.Pendiente,
            Responsable = "Ana",
            FechaLimite = new DateOnly(2026, 12, 31)
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Titulo.Should().Be("Implementar API");
        created.Estado.Should().Be(TaskStatus.Pendiente);
        created.Responsable.Should().Be("Ana");

        var getResponse = await _client.GetAsync($"/api/tasks/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        fetched.Should().NotBeNull();
        fetched!.Titulo.Should().Be("Implementar API");
    }

    [Fact]
    public async Task GetTasks_WithStatusAndResponsibleFiltersAndPagination_ReturnsExpectedPage()
    {
        await CreateSampleTask("F1", TaskStatus.Pendiente, "FiltroAna");
        await CreateSampleTask("F2", TaskStatus.Pendiente, "FiltroAna");
        await CreateSampleTask("F3", TaskStatus.Hecha, "FiltroAna");
        await CreateSampleTask("F4", TaskStatus.Pendiente, "FiltroLuis");

        var response = await _client.GetAsync("/api/tasks?estado=pendiente&responsable=FiltroAna&page=1&pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<TaskResponseDto>>();
        page.Should().NotBeNull();
        page!.TotalItems.Should().Be(2);
        page.TotalPages.Should().Be(2);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(1);
        page.Items.Should().HaveCount(1);
        page.Items.Single().Responsable.Should().Be("FiltroAna");
        page.Items.Single().Estado.Should().Be(TaskStatus.Pendiente);
    }

    [Fact]
    public async Task CreateTask_WithInvalidData_ReturnsBadRequest()
    {
        var invalid = new
        {
            titulo = "",
            descripcion = "Sin título",
            estado = "pendiente",
            responsable = "",
            fechaLimite = "2026-12-31"
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", invalid);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAndDeleteTask_Work_AndMissingTaskReturnsNotFound()
    {
        var created = await CreateSampleTask("Editar", TaskStatus.EnCurso, "Beto");

        var updateDto = new TaskUpdateDto
        {
            Titulo = "Editada",
            Descripcion = "Actualizada",
            Estado = TaskStatus.Hecha,
            Responsable = "Beto",
            FechaLimite = new DateOnly(2027, 1, 15)
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{created.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        updated!.Titulo.Should().Be("Editada");
        updated.Estado.Should().Be(TaskStatus.Hecha);

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getDeleted = await _client.GetAsync($"/api/tasks/{created.Id}");
        getDeleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<TaskResponseDto> CreateSampleTask(string titulo, TaskStatus estado, string responsable)
    {
        var dto = new TaskCreateDto
        {
            Titulo = titulo,
            Descripcion = "desc",
            Estado = estado,
            Responsable = responsable,
            FechaLimite = new DateOnly(2026, 10, 10)
        };
        var response = await _client.PostAsJsonAsync("/api/tasks", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponseDto>())!;
    }
}
