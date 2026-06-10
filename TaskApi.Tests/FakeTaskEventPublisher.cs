using TaskApi.Abstractions;
using TaskApi.Contracts;

namespace TaskApi.Tests;

public class FakeTaskEventPublisher : ITaskEventPublisher
{
    public List<TaskCompletedEvent> Published { get; } = new();

    public void PublishTaskCompleted(TaskCompletedEvent message)
        => Published.Add(message);
}
