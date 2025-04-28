// ConsultationRejectedConsumer.cs
using HealthMed.Schedule.Application.Interfaces;
using HealthMed.Schedule.Domain.Entities;
using HealthMed.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class ConsultationRejectedConsumer : BackgroundService
{
    private const string ExchangeName = nameof(ConsultationRejected);
    private const string QueueName = nameof(ConsultationRejected);

    private readonly IModel _ch;
    private readonly IServiceScopeFactory _scf;

    public ConsultationRejectedConsumer(IConnection conn, IServiceScopeFactory scf)
    {
        _scf = scf;
        _ch = conn.CreateModel();

        // 1) declara exchange/fila
        _ch.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false);
        _ch.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
        _ch.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: "");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_ch);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<ConsultationRejected>(json)
                           ?? throw new InvalidOperationException("Payload vazio em ConsultationRejected");

                using var scope = _scf.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IAvailableSlotService>();

                // ■ no caso de REJECTED, devolvemos o horário à agenda:
                var slot = new AvailableSlot(
                    Guid.NewGuid(),          // novo Id para o slot
                    evt.DoctorId,
                    evt.ScheduledTime,
                    evt.ScheduledTime.AddMinutes(30)
                );
                var ok = await svc.AddAsync(slot);
                Console.WriteLine($"[Schedule] Consulta recusada: devolvendo slot {slot.Id} (ok={ok})");

                _ch.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Schedule] Erro processando {ExchangeName}: {ex}");
                _ch.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _ch.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }
}
