using HealthMed.Shared.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HealthMed.Appointments.Application.Clients
{
    public class ScheduleClient
    {
        private readonly IConnection _connection;

        public ScheduleClient(IConnection connection)
        {
            _connection = connection;
        }
        public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(Guid doctorId)
        {
            using var channel = _connection.CreateModel();
            var replyQueue = channel.QueueDeclare("", false, true, true, null).QueueName;
            var corrId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<string>();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == corrId)
                {
                    tcs.TrySetResult(Encoding.UTF8.GetString(ea.Body.ToArray()));
                }
                return Task.CompletedTask;
            };
            channel.BasicConsume(replyQueue, true, consumer);

            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueue;
            props.CorrelationId = corrId;

            channel.BasicPublish("", "agenda.request", props,
                Encoding.UTF8.GetBytes(doctorId.ToString()));

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using (cts.Token.Register(() => tcs.TrySetCanceled()))
            {
                try
                {
                    var json = await tcs.Task;
                    return JsonSerializer.Deserialize<List<AvailableSlotDto>>(json)
                           ?? new List<AvailableSlotDto>();
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("[Appointments] Timeout aguardando resposta do Schedule");
                    return new List<AvailableSlotDto>();
                }
            }
        }

        public async Task<bool> IsSlotAvailable(Guid doctorId, DateTime startTime)
        {
            var slots = await GetAvailableSlotsAsync(doctorId);
            return slots.Any(s => s.StartTime == startTime);
        }
    }
}
