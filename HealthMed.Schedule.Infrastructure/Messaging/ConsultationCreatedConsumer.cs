using HealthMed.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using HealthMed.Schedule.Application.Interfaces;

namespace HealthMed.Schedule.Infrastructure.Messaging
{
    public class ConsultationCreatedConsumer : BackgroundService
    {
        private const string Exchange = nameof(ConsultationCreated);
        private const string Queue = "schedule.consultation.created";

        private readonly IModel _ch;
        private readonly IServiceScopeFactory _scf;

        public ConsultationCreatedConsumer(IConnection conn, IServiceScopeFactory scf)
        {
            _scf = scf;
            _ch = conn.CreateModel();
            _ch.ExchangeDeclare(Exchange, ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);
            _ch.QueueDeclare(Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _ch.QueueBind(Queue, Exchange, routingKey: "");
        }

        protected override Task ExecuteAsync(CancellationToken _)
        {
            var consumer = new EventingBasicConsumer(_ch);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"[Schedule] Received ConsultationCreated: {json}");
                    var evt = JsonSerializer.Deserialize<ConsultationCreated>(json)!;

                    using var scope = _scf.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<IAvailableSlotService>();

                    await svc.DeleteAsync(evt.SlotId);
                    Console.WriteLine($"[Schedule] Slot {evt.SlotId} removed");

                    _ch.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[Schedule] Error processing event: {ex}");
                    _ch.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _ch.BasicConsume(queue: Queue, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
    }
}