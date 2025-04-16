using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Appointments.Infra.Messaging;

public class MessagePublisher : IDisposable
{
    private readonly RabbitMqConnection _connection;

    public MessagePublisher()
    {
        _connection = new RabbitMqConnection();
    }

    public void Publish<T>(string queueName, T message)
    {
        _connection.DeclareQueue(queueName);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = _connection.Channel.CreateBasicProperties();
        properties.Persistent = true;

        _connection.Channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: properties,
            body: body
        );
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
