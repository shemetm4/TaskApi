using TaskApi.Contracts;

namespace TaskApi.Abstractions;

public interface ITaskEventPublisher
{
    void PublishTaskCompleted(TaskCompletedEvent message);
}
