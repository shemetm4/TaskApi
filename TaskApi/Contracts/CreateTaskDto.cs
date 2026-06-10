using TaskApi.Entities;

namespace TaskApi.Contracts;

public record CreateTaskDto(string Title, Priority? Priority);
