using EventBusBase.Events;

namespace EventBusBase.Abstraction
{
    public interface  IEventBus
    {
        void Publish(IntegrationEvent @event);
        void Subscribe<T, TH>(IntegrationEvent @event) where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
        void UnSubscribe<T, TH>(IntegrationEvent @event) where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
        
    }
}
