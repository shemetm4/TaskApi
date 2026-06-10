using Microsoft.EntityFrameworkCore;
using TaskApi.Abstractions;
using TaskApi.Contracts;
using TaskApi.Database;
using TaskApi.Entities;

namespace TaskApi.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        app.MapPost("/tasks", async (CreateTaskDto dto, TaskDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return Results.BadRequest("Title is required and cannot be empty.");

            if (dto.Title.Length > 200)
                return Results.BadRequest("Title cannot exceed 200 characters.");

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                IsCompleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                CompletedAt = null,
                Priority = dto.Priority ?? Priority.Medium
            };

            db.Tasks.Add(task);

            await db.SaveChangesAsync();

            return Results.Created($"/tasks/{task.Id}", task);
        });

        app.MapGet("/tasks", async (TaskDbContext db)
            => await db.Tasks.AsNoTracking().ToListAsync());


        app.MapPut("/tasks/{id}/complete", async (Guid id, TaskDbContext db, ITaskEventPublisher publisher) =>
        {
            var task = await db.Tasks.FindAsync(id);

            if (task is null)
                return Results.NotFound();

            if (task.IsCompleted)
                return Results.Conflict("Task is already completed.");

            task.IsCompleted = true;
            task.CompletedAt = DateTimeOffset.UtcNow;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Results.Conflict("Task was modified by another request.");
            }

            publisher.PublishTaskCompleted(new TaskCompletedEvent(
                task.Id,
                task.Title,
                task.CompletedAt.Value,
                task.Priority));

            return Results.Ok(task);
        });

        app.MapDelete("/tasks/{id}", async (Guid id, TaskDbContext db) =>
        {
            var task = await db.Tasks.FindAsync(id);

            if (task is null)
                return Results.NotFound();

            db.Tasks.Remove(task);

            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
