using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthMed.Appointments.Infrastructure.Messaging
{
    public class ScheduleClient
    {
        private readonly IModel _channel;

        public ScheduleClient(IModel channel)
        {
            _channel = channel;
        }

        public async Task<List<DateTime>> GetAvailableSlotsAsync(Guid doctorId)
        {
            var correlationId = Guid.NewGuid().ToString();
            var replyQueue = _channel.QueueDeclare().QueueName;

            var consumer = new AsyncEventingBasicConsumer(_channel);
            var tcs = new TaskCompletionSource<string>();

            consumer.Received += async (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    tcs.SetResult(message);
                }

                await Task.Yield(); // necessário para evitar warning
            };

            _channel.BasicConsume(queue: replyQueue, autoAck: true, consumer: consumer);

            var props = _channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueue;

            var bodyBytes = Encoding.UTF8.GetBytes(doctorId.ToString());
            _channel.BasicPublish(exchange: "", routingKey: "agenda.request", basicProperties: props, body: bodyBytes);

            var json = await tcs.Task;

            var slots = JsonSerializer.Deserialize<List<DateTime>>(json);
            return slots ?? new();
        }
    }
}
