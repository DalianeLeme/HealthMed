using HealthMed.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using HealthMed.Appointments.Domain.Interfaces;

public class SlotDeletedConsumer : BackgroundService
{
    private const string ExchangeName = nameof(SlotDeleted);          
    private const string QueueName = "appointments.slot.deleted";

    private readonly IModel _ch;
    private readonly IServiceScopeFactory _scf;

    public SlotDeletedConsumer(IConnection conn, IServiceScopeFactory scf)
    {
        _scf = scf;
        _ch = conn.CreateModel();
        _ch.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);
        _ch.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _ch.QueueBind(QueueName, ExchangeName, routingKey: "");
        _ch.BasicQos(0, 1, false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_ch);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var evt = JsonSerializer.Deserialize<SlotDeleted>(
                    Encoding.UTF8.GetString(ea.Body.ToArray())
                )!;
                using var scope = _scf.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IAvailableSlotProjectionRepository>();
                await repo.DeleteAsync(evt.Id);
                _ch.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch
            {
                _ch.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _ch.BasicConsume(QueueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }
}