using System.Net;
using System.Net.Http.Json;
using DemoApi.Domain;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DemoApi.Tests.Integration;

[Collection(ApiCollection.Name)]
public class ProjectsApiTests(WebApplicationFactory<Program> factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ReturnsCreatedWithLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { name = TestApi.Unique("project") });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsConflict()
    {
        var name = TestApi.Unique("duplicate");
        (await _client.PostAsJsonAsync("/api/projects", new { name })).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/projects", new { name });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Unknown_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/projects/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStats_Unknown_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/projects/{int.MaxValue}/stats");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStats_ReturnsCorrectCounts()
    {
        var project = await _client.CreateProjectAsync();

        // 1 задача со статусом Todo
        await _client.CreateTaskAsync(project.Id, "Задача 1");

        // 1 задача со статусом InProgress
        var task2 = await _client.CreateTaskAsync(project.Id, "Задача 2");
        (await _client.ChangeStatusAsync(task2.Id, TaskItemStatus.InProgress)).EnsureSuccessStatusCode();

        // 1 задача со сроком позавчера (просрочена, статус Todo)
        var responseOverdue = await _client.PostAsJsonAsync("/api/tasks", new
        {
            projectId = project.Id,
            title = "Просрочена",
            dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2))
        });
        responseOverdue.EnsureSuccessStatusCode();

        var response = await _client.GetAsync($"/api/projects/{project.Id}/stats");
        response.EnsureSuccessStatusCode();

        var stats = await response.Content.ReadFromJsonAsync<ProjectStats>(TestApi.Json);
        Assert.NotNull(stats);
        Assert.Equal(project.Id, stats.ProjectId);
        Assert.Equal(3, stats.Total);
        Assert.Equal(2, stats.Todo);
        Assert.Equal(1, stats.InProgress);
        Assert.Equal(0, stats.Done);
        Assert.Equal(1, stats.Overdue);
    }

    [Fact]
    public async Task Rename_ReturnsOk()
    {
        var project = await _client.CreateProjectAsync();
        var newName = TestApi.Unique("renamed");

        var response = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", new { name = newName });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<Project>(TestApi.Json);
        Assert.NotNull(updated);
        Assert.Equal(project.Id, updated.Id);
        Assert.Equal(newName, updated.Name);
    }

    [Fact]
    public async Task Rename_DuplicateName_ReturnsConflict()
    {
        var existing = await _client.CreateProjectAsync();
        var target = await _client.CreateProjectAsync();

        var response = await _client.PutAsJsonAsync($"/api/projects/{target.Id}", new { name = existing.Name });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}