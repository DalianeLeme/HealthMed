using HealthMed.Schedule.Domain.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace HealthMed.Appointments.Application.Clients
{
    public class ScheduleClient
    {
        private readonly IModel _channel;
        private readonly HttpClient _http;

        public ScheduleClient(IModel channel, HttpClient http)
        {
            _channel = channel;
            _http = http;
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

                await Task.Yield();
            };

            _channel.BasicConsume(queue: replyQueue, autoAck: true, consumer: consumer);

            var props = _channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueue;

            var messageBytes = Encoding.UTF8.GetBytes(doctorId.ToString());

            _channel.BasicPublish(
                exchange: "",
                routingKey: "agenda.request",
                basicProperties: props,
                body: messageBytes
            );

            var response = await tcs.Task;
            return System.Text.Json.JsonSerializer.Deserialize<List<DateTime>>(response) ?? new List<DateTime>();
        }

        public async Task<bool> IsSlotAvailable(Guid doctorId, DateTime startTime)
        {
            var response = await _http.GetAsync($"/api/slots/{doctorId}");
            if (!response.IsSuccessStatusCode) return false;

            var slots = JsonConvert.DeserializeObject<List<AvailableSlot>>(await response.Content.ReadAsStringAsync());

            return slots.Any(s => s.StartTime == startTime);
        }
    }
}
