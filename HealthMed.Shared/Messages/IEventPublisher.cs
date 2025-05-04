namespace HealthMed.Shared.Messaging
{
    public interface IEventPublisher
    {
        void Publish<T>(string queueName, T @event);
    }
}