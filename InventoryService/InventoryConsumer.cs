using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;

namespace InventoryService;

public class InventoryConsumer : BackgroundService
{
    private readonly ILogger<InventoryConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string ExchangeName = "order_processing";
    private const string QueueName = "inventory_queue";

    public InventoryConsumer(ILogger<InventoryConsumer> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:Username"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(QueueName, ExchangeName, "order.submitted", cancellationToken: cancellationToken);

        _logger.LogInformation("InventoryService connected to RabbitMQ and listening on {Queue}", QueueName);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            _logger.LogInformation("InventoryService received message: {Message}", json);

            try
            {
                var order = JsonSerializer.Deserialize<OrderSubmittedEvent>(json);
                if (order == null) return;

                _logger.LogInformation("Processing inventory check for Order {OrderId}, CorrelationId: {CorrelationId}", 
                    order.OrderId, order.CorrelationId);

                // Simulate inventory check - 90% success rate
                var random = new Random();
                var inventoryAvailable = random.Next(100) < 90;

                if (inventoryAvailable)
                {
                    _logger.LogInformation("Inventory confirmed for Order {OrderId}", order.OrderId);
                    await PublishEvent(new InventoryConfirmedEvent
                    {
                        OrderId = order.OrderId,
                        CorrelationId = order.CorrelationId
                    }, "inventory.confirmed");
                }
                else
                {
                    _logger.LogWarning("Inventory failed for Order {OrderId}", order.OrderId);
                    await PublishEvent(new InventoryFailedEvent
                    {
                        OrderId = order.OrderId,
                        Reason = "Insufficient stock",
                        CorrelationId = order.CorrelationId
                    }, "inventory.failed");
                }

                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inventory for message: {Message}", json);
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await _channel!.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task PublishEvent<T>(T eventMessage, string routingKey)
    {
        var json = JsonSerializer.Serialize(eventMessage);
        var body = Encoding.UTF8.GetBytes(json);
        var props = new BasicProperties { Persistent = true };

        await _channel!.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body
        );

        _logger.LogInformation("InventoryService published {RoutingKey} for Order", routingKey);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync(cancellationToken);
        if (_connection != null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
