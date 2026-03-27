using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;

namespace ShippingService;

public class ShippingConsumer : BackgroundService
{
    private readonly ILogger<ShippingConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string ExchangeName = "order_processing";
    private const string QueueName = "shipping_queue";

    public ShippingConsumer(ILogger<ShippingConsumer> logger, IConfiguration configuration)
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
        await _channel.QueueBindAsync(QueueName, ExchangeName, "payment.approved", cancellationToken: cancellationToken);

        _logger.LogInformation("ShippingService connected to RabbitMQ and listening on {Queue}", QueueName);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            _logger.LogInformation("ShippingService received message: {Message}", json);

            try
            {
                var paymentEvent = JsonSerializer.Deserialize<PaymentApprovedEvent>(json);
                if (paymentEvent == null) return;

                _logger.LogInformation("Creating shipment for Order {OrderId}, CorrelationId: {CorrelationId}",
                    paymentEvent.OrderId, paymentEvent.CorrelationId);

                var shipmentReference = $"SHIP-{Guid.NewGuid().ToString()[..8].ToUpper()}";
                var estimatedDispatch = DateTime.UtcNow.AddDays(2);

                _logger.LogInformation("Shipment created for Order {OrderId}, Reference: {Reference}",
                    paymentEvent.OrderId, shipmentReference);

                await PublishEvent(new ShippingCreatedEvent
                {
                    OrderId = paymentEvent.OrderId,
                    ShipmentReference = shipmentReference,
                    EstimatedDispatchDate = estimatedDispatch,
                    CorrelationId = paymentEvent.CorrelationId
                }, "shipping.created");

                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shipment for message: {Message}", json);
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

        _logger.LogInformation("ShippingService published {RoutingKey} for Order", routingKey);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync(cancellationToken);
        if (_connection != null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}