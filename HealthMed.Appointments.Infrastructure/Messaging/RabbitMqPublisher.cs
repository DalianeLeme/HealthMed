﻿using HealthMed.Shared.Messaging;
using RabbitMQ.Client;
using System.Text.Json;

namespace HealthMed.Appointments.Infrastructure.Messaging
{
    public class RabbitMqPublisher : IEventPublisher, IDisposable
    {
        private readonly IModel _ch;
        public RabbitMqPublisher(IConnection connection)
        {
            _ch = connection.CreateModel();
        }

        public void Publish<T>(string exchangeName, T @event)
        {
            _ch.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);

            var body = JsonSerializer.SerializeToUtf8Bytes(@event);
            var props = _ch.CreateBasicProperties();
            props.Persistent = true;

            _ch.BasicPublish(
                exchange: exchangeName, // publica no exchange
                routingKey: "",           // ignorado pelo fanout
                basicProperties: props,
                body: body);
        }

        public void Dispose() => _ch?.Dispose();
    }
}