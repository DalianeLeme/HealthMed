using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HealthMed.Schedule.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HealthMed.Schedule.Infrastructure.Messaging
{
    public class GetAvailableSlotsConsumer : BackgroundService
    {
        private const string QueueName = "agenda.request";

        private readonly IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        public GetAvailableSlotsConsumer(IConnection connection, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _channel = connection.CreateModel();

            // 1) declara fila RPC como durável
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // 2) QoS para 1 mensagem por vez
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var props = ea.BasicProperties;
                var replyTo = props.ReplyTo;
                var corrId = props.CorrelationId;
                var body = ea.Body.ToArray();
                var idStr = Encoding.UTF8.GetString(body);

                try
                {
                    if (!string.IsNullOrEmpty(replyTo)
                     && !string.IsNullOrEmpty(corrId)
                     && Guid.TryParse(idStr, out var doctorId))
                    {
                        // 3) obtém os slots dentro de um escopo
                        using var scope = _scopeFactory.CreateScope();
                        var svc = scope.ServiceProvider.GetRequiredService<IAvailableSlotService>();
                        var slots = await svc.GetByDoctorAsync(doctorId);

                        // 4) serializa e envia resposta
                        var respBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(slots));
                        var replyProps = _channel.CreateBasicProperties();
                        replyProps.CorrelationId = corrId;

                        _channel.BasicPublish(
                            exchange: "",
                            routingKey: replyTo,
                            basicProperties: replyProps,
                            body: respBytes
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[Schedule] erro ao processar RPC de agenda: {ex}");
                    // não reenfileirar, afinal é RPC
                }
                finally
                {
                    // 5) ack sempre
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );

            return Task.CompletedTask;
        }
    }
}
