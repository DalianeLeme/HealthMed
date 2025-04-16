using HealthMed.Shared.DTOs;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using HealthMed.Schedule.Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class ConsultationRejectedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IModel _channel;

    public ConsultationRejectedConsumer(IServiceProvider serviceProvider, IModel channel)
    {
        _serviceProvider = serviceProvider;
        _channel = channel;

        _channel.ExchangeDeclare("consulta.recusada", ExchangeType.Fanout);
        _channel.QueueDeclare("schedule_recusada", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("schedule_recusada", "consulta.recusada", "");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, args) =>
        {
            var message = Encoding.UTF8.GetString(args.Body.ToArray());
            var data = JsonConvert.DeserializeObject<AppointmentDto>(message);

            if (data == null) return;

            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<AvailableSlotService>();

            await service.AddAsync(new HealthMed.Schedule.Domain.Entities.AvailableSlot
            {
                Id = Guid.NewGuid(),
                DoctorId = data.DoctorId,
                StartTime = data.ScheduledTime,
                EndTime = data.ScheduledTime.AddMinutes(30)
            });
        };

        _channel.BasicConsume("schedule_recusada", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}
