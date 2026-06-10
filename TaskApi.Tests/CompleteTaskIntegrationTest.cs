using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskApi.Entities;

namespace TaskApi.Tests;

public class CompleteTaskIntegrationTest : IClassFixture<TaskApiFactory>
{
    private readonly TaskApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public CompleteTaskIntegrationTest(TaskApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteTask_WhenTaskCreated_PublishesTaskCompletedEvent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/tasks",
            new { title = "TestTask", priority = "High" });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<TaskItem>(JsonOptions);

        Assert.NotNull(created);

        // Act
        var completeResponse = await _client.PutAsync($"/tasks/{created!.Id}/complete", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var published = Assert.Single(_factory.FakePublisher.Published);

        Assert.Equal(created.Id, published.TaskId);
        Assert.Equal("TestTask", published.Title);
        Assert.Equal(Priority.High, published.Priority);
        Assert.NotNull(published.CompletedAt);

        await _client.DeleteAsync($"/tasks/{created.Id}");
    }
}
