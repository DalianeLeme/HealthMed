using RabbitMQ.Client;

namespace Appointments.Infra.Messaging;

public class RabbitMqConnection : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public IModel Channel => _channel;

    public RabbitMqConnection(string hostName = "rabbitmq", string user = "guest", string pass = "guest")
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = user,
            Password = pass
        };

        _connection = factory.CreateConnection("appointments_connection");
        _channel = _connection.CreateModel();
    }

    public void DeclareQueue(string queueName)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
