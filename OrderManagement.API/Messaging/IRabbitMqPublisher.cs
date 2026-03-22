namespace OrderManagement.API.Messaging;

public interface IRabbitMqPublisher
{
    void Publish<T>(T message, string routingKey);
}
