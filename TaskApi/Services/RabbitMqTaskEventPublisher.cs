using RabbitMQ.Client;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskApi.Abstractions;
using TaskApi.Contracts;

namespace TaskApi.Services;

public class RabbitMqTaskEventPublisher : ITaskEventPublisher, IDisposable
{
    private const string ExchangeName = "task.events";
    private const string RoutingKey = "task.completed";
    private readonly ILogger<RabbitMqTaskEventPublisher> _logger;
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public RabbitMqTaskEventPublisher(IConfiguration config, ILogger<RabbitMqTaskEventPublisher> logger)
    {
        _logger = logger;

        try
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(config.GetConnectionString("RabbitMq") ?? "amqp://guest:guest@localhost:5672/")
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);

            _logger.LogInformation("RabbitMQ connection established.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ. Events will be skipped.");
        }
    }

    public void PublishTaskCompleted(TaskCompletedEvent message)
    {
        if (_channel is null || _channel.IsClosed)
        {
            _logger.LogWarning($"RabbitMQ channel unavailable. Skipped event for task {message.TaskId}.", message.TaskId);
            
            return;
        }

        try
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);

            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;

            _channel.BasicPublish(ExchangeName, RoutingKey, props, body);

            _logger.LogInformation($"Published task.completed for {message.TaskId}.", message.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to publish event for task {message.TaskId}.", message.TaskId);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
