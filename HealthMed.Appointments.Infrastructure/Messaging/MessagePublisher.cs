using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Appointments.Infra.Messaging;

public class MessagePublisher : IDisposable
{
    private readonly IModel _channel;

    public MessagePublisher(IConnection connection)
    {
        _channel = connection.CreateModel();
    }

    public void Publish<T>(string queueName, T message)
    {
        _channel.QueueDeclare(queue: queueName,
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;

        _channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: props,
            body: body
        );
    }

    public void Dispose() => _channel?.Dispose();
}