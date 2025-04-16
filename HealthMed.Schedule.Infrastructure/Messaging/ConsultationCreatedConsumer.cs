using HealthMed.Schedule.Application.Services;
using HealthMed.Shared.Messages;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace HealthMed.Schedule.Infrastructure.Messaging
{
    public class ConsultationCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQ.Client.IModel _channel;

        public ConsultationCreatedConsumer(IServiceProvider serviceProvider, RabbitMQ.Client.IModel channel)
        {
            _serviceProvider = serviceProvider;
            _channel = channel;

            _channel.ExchangeDeclare("consultations", ExchangeType.Fanout);
            _channel.QueueDeclare("schedule_consultations", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("schedule_consultations", "consultations", "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, args) =>
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                var data = JsonConvert.DeserializeObject<ConsultationCreatedMessage>(message);

                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<AvailableSlotService>();

                await service.RemoveSlotByTimeAsync(data.DoctorId, data.ScheduledTime);
            };

            _channel.BasicConsume("schedule_consultations", autoAck: true, consumer: consumer);

            return Task.CompletedTask;

        }
    }
}
