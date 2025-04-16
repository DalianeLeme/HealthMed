using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

public class ConsultationAcceptedConsumer : BackgroundService
{
    private readonly IModel _channel;

    public ConsultationAcceptedConsumer(IModel channel)
    {
        _channel = channel;

        _channel.ExchangeDeclare("consulta.aceita", ExchangeType.Fanout);
        _channel.QueueDeclare("schedule_aceita", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("schedule_aceita", "consulta.aceita", "");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (sender, args) =>
        {
            var message = Encoding.UTF8.GetString(args.Body.ToArray());
            Console.WriteLine($"Consulta aceita: {message}");
        };

        _channel.BasicConsume("schedule_aceita", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}
