using HealthMed.Shared.Events;
using HealthMed.Schedule.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HealthMed.Schedule.Infrastructure.Messaging
{
    public class ConsultationRescheduledConsumer : BackgroundService
    {
        private const string Exchange = nameof(ConsultationRescheduled);
        private const string Queue = nameof(ConsultationRescheduled);

        private readonly IModel _ch;
        private readonly IServiceScopeFactory _scf;

        public ConsultationRescheduledConsumer(IConnection conn, IServiceScopeFactory scf)
        {
            _scf = scf;
            _ch = conn.CreateModel();

            // 1) garante exchange + fila
            _ch.ExchangeDeclare(Exchange, ExchangeType.Fanout, durable: true, autoDelete: false);
            _ch.QueueDeclare(Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _ch.QueueBind(Queue, Exchange, routingKey: "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_ch);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evt = JsonSerializer.Deserialize<ConsultationRescheduled>(json)!;

                    using var scope = _scf.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<IAvailableSlotService>();

                    // Remove o antigo e adiciona o novo
                    await svc.RemoveSlotByTimeAsync(evt.DoctorId, evt.OldTime);
                    await svc.AddAsync(new HealthMed.Schedule.Domain.Entities.AvailableSlot
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = evt.DoctorId,
                        StartTime = evt.NewTime,
                        EndTime = evt.NewTime.AddMinutes(30)
                    });

                    _ch.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[Schedule] erro ao processar remarcação: {ex}");
                    _ch.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _ch.BasicConsume(queue: Queue, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
    }
}
