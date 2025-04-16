using HealthMed.Schedule.Application.Models;
using HealthMed.Schedule.Application.Services;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HealthMed.Schedule.Infrastructure.Messaging
{
    public class GetAvailableSlotsConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly AvailableSlotService _service;

        public GetAvailableSlotsConsumer(IModel channel, AvailableSlotService service)
        {
            _channel = channel;
            _service = service;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel.QueueDeclare(queue: "agenda.request",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var doctorIdStr = Encoding.UTF8.GetString(body);

                if (Guid.TryParse(doctorIdStr, out var doctorId))
                {
                    var slots = await _service.GetByDoctorAsync(doctorId);
                    var response = JsonSerializer.Serialize(slots);

                    var props = ea.BasicProperties;
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    _channel.BasicPublish(exchange: "",
                                          routingKey: props.ReplyTo,
                                          basicProperties: replyProps,
                                          body: responseBytes);
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: "agenda.request",
                                  autoAck: false,
                                  consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
