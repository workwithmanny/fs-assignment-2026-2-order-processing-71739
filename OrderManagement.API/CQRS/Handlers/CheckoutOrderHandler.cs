using MediatR;
using OrderManagement.API.CQRS.Commands;
using OrderManagement.API.Data;
using OrderManagement.API.Models;
using OrderManagement.API.Messaging;
using Shared.Contracts.DTOs;
using Shared.Contracts.Events;

namespace OrderManagement.API.CQRS.Handlers;

public class CheckoutOrderHandler : IRequestHandler<CheckoutOrderCommand, Guid>
{
    private readonly AppDbContext _context;
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<CheckoutOrderHandler> _logger;

    public CheckoutOrderHandler(AppDbContext context, IRabbitMqPublisher publisher, ILogger<CheckoutOrderHandler> logger)
    {
        _context = context;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Guid> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            ShippingAddress = request.ShippingAddress,
            TotalAmount = request.Items.Sum(i => i.UnitPrice * i.Quantity),
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} created for customer {CustomerId}", order.Id, order.CustomerId);

        var event_ = new OrderSubmittedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            SubmittedAt = order.CreatedAt,
            CorrelationId = order.CorrelationId,
            Items = request.Items
        };

        _publisher.Publish(event_, "order.submitted");
        _logger.LogInformation("OrderSubmittedEvent published for Order {OrderId}", order.Id);

        return order.Id;
    }
}
