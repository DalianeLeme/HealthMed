// Appointments.API/Infrastructure/Messaging/SlotCreatedConsumer.cs
using HealthMed.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using HealthMed.Appointments.Domain.Interfaces;
using HealthMed.Appointments.Domain.Entities;

public class SlotCreatedConsumer : BackgroundService
{
    private const string ExchangeName = nameof(SlotCreated); // "SlotCreated"
    private const string QueueName = "appointments.slot.created";

    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;

    public SlotCreatedConsumer(IConnection connection, IServiceScopeFactory scopeFactory)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            arguments: null
        );

        channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
        channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: ""
        );

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<SlotCreated>(json)!;

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IAvailableSlotProjectionRepository>();

            var projection = new AvailableSlotProjection
            {
                Id = evt.Id,
                DoctorId = evt.DoctorId,
                StartTime = evt.StartTime,
                EndTime = evt.EndTime
            };

            await repo.AddAsync(projection);
            channel.BasicAck(ea.DeliveryTag, multiple: false);
        };

        channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer
        );

        return Task.CompletedTask;
    }
}
