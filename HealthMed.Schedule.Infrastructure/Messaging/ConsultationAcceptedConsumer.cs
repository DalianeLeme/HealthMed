using HealthMed.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class ConsultationAcceptedConsumer : BackgroundService
{
    private const string ExchangeName = nameof(ConsultationAccepted);
    private const string QueueName = nameof(ConsultationAccepted);

    private readonly IModel _ch;
    private readonly IServiceScopeFactory _scf;

    public ConsultationAcceptedConsumer(IConnection conn, IServiceScopeFactory scf)
    {
        _scf = scf;
        _ch = conn.CreateModel();
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
                var evt = JsonSerializer.Deserialize<ConsultationAccepted>(json)
                           ?? throw new InvalidOperationException("Payload vazio em ConsultationAccepted");

                Console.WriteLine(
                    $"[Schedule] Consulta aceita: " +
                    $"ConsultationId={evt.ConsultationId}, DoctorId={evt.DoctorId}, PatientId={evt.PatientId}, Time={evt.ScheduledTime:O}"
                );

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