using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using HealthMed.Shared.Messages;
using Microsoft.Extensions.DependencyInjection;
using HealthMed.Schedule.Application.Services;

public class ConsultationRescheduledConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMQ.Client.IModel _channel;

    public ConsultationRescheduledConsumer(IServiceProvider serviceProvider, RabbitMQ.Client.IModel channel)
    {
        _serviceProvider = serviceProvider;
        _channel = channel;

        _channel.ExchangeDeclare("consultations.rescheduled", ExchangeType.Fanout);
        _channel.QueueDeclare("schedule_rescheduled", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("schedule_rescheduled", "consultations.rescheduled", "");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, args) =>
        {
            var message = Encoding.UTF8.GetString(args.Body.ToArray());
            var data = JsonConvert.DeserializeObject<ConsultationRescheduledMessage>(message);

            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<AvailableSlotService>();

            // Remove horário antigo
            await service.RemoveSlotByTimeAsync(data.DoctorId, data.OldTime);

            // Adiciona novo horário
            await service.AddAsync(new HealthMed.Schedule.Domain.Entities.AvailableSlot
            {
                Id = Guid.NewGuid(),
                DoctorId = data.DoctorId,
                StartTime = data.NewTime,
                EndTime = data.NewTime.AddMinutes(30)
            });
        };

        _channel.BasicConsume("schedule_rescheduled", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}
