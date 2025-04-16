using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using HealthMed.Shared.Messages;
using Microsoft.Extensions.DependencyInjection;
using HealthMed.Schedule.Application.Services;

public class ConsultationCancelledConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMQ.Client.IModel _channel;

    public ConsultationCancelledConsumer(IServiceProvider serviceProvider, RabbitMQ.Client.IModel channel)
    {
        _serviceProvider = serviceProvider;
        _channel = channel;

        _channel.ExchangeDeclare("consultations.cancelled", ExchangeType.Fanout);
        _channel.QueueDeclare("schedule_cancelled", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("schedule_cancelled", "consultations.cancelled", "");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, args) =>
        {
            var message = Encoding.UTF8.GetString(args.Body.ToArray());
            var data = JsonConvert.DeserializeObject<ConsultationCancelledMessage>(message);

            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<AvailableSlotService>();

            await service.AddAsync(new HealthMed.Schedule.Domain.Entities.AvailableSlot
            {
                Id = Guid.NewGuid(),
                DoctorId = data.DoctorId,
                StartTime = data.ScheduledTime,
                EndTime = data.ScheduledTime.AddMinutes(30) // ou tempo da consulta
            });
        };

        _channel.BasicConsume("schedule_cancelled", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}
