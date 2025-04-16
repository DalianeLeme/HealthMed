using HealthMed.Shared.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace HealthMed.Appointments.Application.Events
{
    public class RabbitMQPublisher
    {
        private readonly IModel _channel;

        public RabbitMQPublisher(IModel channel)
        {
            _channel = channel;
            _channel.ExchangeDeclare("consultations", ExchangeType.Fanout, durable: true);
        }

        public void PublishConsultationCreated(ConsultationCreatedMessage message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.BasicPublish("consultations", "", null, body);
        }

        public void PublishConsultationCancelled(ConsultationCancelledMessage message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.ExchangeDeclare("consultations.cancelled", ExchangeType.Fanout);
            _channel.BasicPublish("consultations.cancelled", "", null, body);
        }

        public void PublishConsultationRescheduled(ConsultationRescheduledMessage message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.ExchangeDeclare("consultations.rescheduled", ExchangeType.Fanout);
            _channel.BasicPublish("consultations.rescheduled", "", null, body);
        }
    }
}
