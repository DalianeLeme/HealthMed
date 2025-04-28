using HealthMed.Schedule.Application.Interfaces;
using HealthMed.Schedule.Domain.Entities;
using HealthMed.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HealthMed.Schedule.Infrastructure.Messaging
{
    public class ConsultationCancelledConsumer : BackgroundService
    {
        private const string ExchangeName = nameof(ConsultationCancelled);
        private const string QueueName = nameof(ConsultationCancelled);

        private readonly IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        public ConsultationCancelledConsumer(IConnection connection, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _channel = connection.CreateModel();

            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evt = JsonSerializer.Deserialize<ConsultationCancelled>(json)
                               ?? throw new InvalidOperationException("Payload vazio em ConsultationCancelled");

                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IAvailableSlotService>();

                    // usa o mesmo Id e o mesmo EndTime do slot original
                    // em vez de evt.EndTime use:
                    var endTime = evt.ScheduledTime.AddMinutes(30);

                    var slot = new AvailableSlot(
                        Guid.NewGuid(),          // ou evt.SlotId, se você passar o SlotId no evento
                        evt.DoctorId,
                        evt.ScheduledTime,
                        evt.EndTime              // usa o EndTime vindo do evento
                    );


                    await service.AddAsync(slot);

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
    }
}
