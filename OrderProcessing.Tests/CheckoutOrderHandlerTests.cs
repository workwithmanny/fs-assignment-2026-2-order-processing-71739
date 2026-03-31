using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.API.CQRS.Commands;
using OrderManagement.API.CQRS.Handlers;
using OrderManagement.API.Data;
using OrderManagement.API.Messaging;
using Shared.Contracts.DTOs;
using Xunit;

namespace OrderProcessing.Tests;

public class CheckoutOrderHandlerTests
{
    private AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidOrder_ReturnsOrderId()
    {
        var db = CreateInMemoryDb();
        var mockPublisher = new Mock<IRabbitMqPublisher>();
        var mockLogger = new Mock<ILogger<CheckoutOrderHandler>>();

        var handler = new CheckoutOrderHandler(db, mockPublisher.Object, mockLogger.Object);

        var command = new CheckoutOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ShippingAddress = "123 Main St",
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = 1,
                    ProductName = "Football",
                    Quantity = 2,
                    UnitPrice = 19.99m
                }
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.Equal(1, db.Orders.Count());
    }

    [Fact]
    public async Task Handle_ValidOrder_PublishesEvent()
    {
        var db = CreateInMemoryDb();
        var mockPublisher = new Mock<IRabbitMqPublisher>();
        var mockLogger = new Mock<ILogger<CheckoutOrderHandler>>();

        var handler = new CheckoutOrderHandler(db, mockPublisher.Object, mockLogger.Object);

        var command = new CheckoutOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            ShippingAddress = "456 Oak Ave",
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = 2,
                    ProductName = "Tennis Racket",
                    Quantity = 1,
                    UnitPrice = 49.99m
                }
            }
        };

        await handler.Handle(command, CancellationToken.None);

        mockPublisher.Verify(p => p.Publish(It.IsAny<object>(), "order.submitted"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidOrder_CalculatesTotalCorrectly()
    {
        var db = CreateInMemoryDb();
        var mockPublisher = new Mock<IRabbitMqPublisher>();
        var mockLogger = new Mock<ILogger<CheckoutOrderHandler>>();

        var handler = new CheckoutOrderHandler(db, mockPublisher.Object, mockLogger.Object);

        var command = new CheckoutOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            ShippingAddress = "789 Test Rd",
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = 1, ProductName = "Football", Quantity = 2, UnitPrice = 19.99m },
                new OrderItemDto { ProductId = 2, ProductName = "Basketball", Quantity = 1, UnitPrice = 29.99m }
            }
        };

        var orderId = await handler.Handle(command, CancellationToken.None);
        var order = await db.Orders.FindAsync(orderId);

        Assert.Equal(69.97m, order!.TotalAmount);
    }
}