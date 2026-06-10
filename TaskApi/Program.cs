using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TaskApi.Abstractions;
using TaskApi.Database;
using TaskApi.Endpoints;
using TaskApi.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<TaskDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddSingleton<ITaskEventPublisher, RabbitMqTaskEventPublisher>();

        var app = builder.Build();

        // HACK: создаю publisher при старте
        app.Services.GetRequiredService<ITaskEventPublisher>();

        app.MapTaskEndpoints();

        app.Run();
    }
}