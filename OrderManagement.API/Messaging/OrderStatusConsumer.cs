using System.Text;
using System.Text.Json;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using OrderManagement.API.CQRS.Commands;
using Shared.Contracts.Enums;

namespace OrderManagement.API.Messaging;

public class OrderStatusConsumer : BackgroundService
{
    private readonly ILogger<OrderStatusConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string ExchangeName = "order_processing";
    private const string QueueName = "order_status_queue";

    public OrderStatusConsumer(
        ILogger<OrderStatusConsumer> logger,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
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

        await _channel.QueueBindAsync(QueueName, ExchangeName, "inventory.confirmed", cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(QueueName, ExchangeName, "inventory.failed", cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(QueueName, ExchangeName, "payment.approved", cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(QueueName, ExchangeName, "payment.rejected", cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(QueueName, ExchangeName, "shipping.created", cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(QueueName, ExchangeName, "shipping.failed", cancellationToken: cancellationToken);

        _logger.LogInformation("OrderStatusConsumer connected to RabbitMQ and listening on {Queue}", QueueName);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            _logger.LogInformation("OrderStatusConsumer received message on {RoutingKey}: {Message}", routingKey, json);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                OrderStatus? newStatus = null;
                Guid? orderId = null;

                switch (routingKey)
                {
                    case "inventory.confirmed":
                        var invConfirmed = JsonSerializer.Deserialize<InventoryConfirmedEvent>(json);
                        orderId = invConfirmed?.OrderId;
                        newStatus = OrderStatus.InventoryConfirmed;
                        break;

                    case "inventory.failed":
                        var invFailed = JsonSerializer.Deserialize<InventoryFailedEvent>(json);
                        orderId = invFailed?.OrderId;
                        newStatus = OrderStatus.Failed;
                        _logger.LogWarning("Inventory failed for Order {OrderId}: {Reason}", invFailed?.OrderId, invFailed?.Reason);
                        break;

                    case "payment.approved":
                        var payApproved = JsonSerializer.Deserialize<PaymentApprovedEvent>(json);
                        orderId = payApproved?.OrderId;
                        newStatus = OrderStatus.PaymentApproved;
                        break;

                    case "payment.rejected":
                        var payRejected = JsonSerializer.Deserialize<PaymentRejectedEvent>(json);
                        orderId = payRejected?.OrderId;
                        newStatus = OrderStatus.Failed;
                        _logger.LogWarning("Payment rejected for Order {OrderId}: {Reason}", payRejected?.OrderId, payRejected?.Reason);
                        break;

                    case "shipping.created":
                        var shipCreated = JsonSerializer.Deserialize<ShippingCreatedEvent>(json);
                        orderId = shipCreated?.OrderId;
                        newStatus = OrderStatus.Completed;
                        break;

                    case "shipping.failed":
                        var shipFailed = JsonSerializer.Deserialize<ShippingFailedEvent>(json);
                        orderId = shipFailed?.OrderId;
                        newStatus = OrderStatus.Failed;
                        break;
                }

                if (orderId.HasValue && newStatus.HasValue)
                {
                    await mediator.Send(new UpdateOrderStatusCommand
                    {
                        OrderId = orderId.Value,
                        NewStatus = newStatus.Value
                    });

                    _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, newStatus);
                }

                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order status update for message: {Message}", json);
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await _channel!.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync(cancellationToken);
        if (_connection != null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}