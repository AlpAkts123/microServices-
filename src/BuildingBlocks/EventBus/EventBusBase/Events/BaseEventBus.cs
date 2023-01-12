using EventBusBase.Abstraction;
using EventBusBase.SubManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EventBusBase.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventBusSubscriptionManager _subManager;
        private EventBusConfig eventBusConfig;

        protected BaseEventBus(IServiceProvider serviceProvider, EventBusConfig eventBusConfig)
        {
            _serviceProvider = serviceProvider;
            _subManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
            this.eventBusConfig = eventBusConfig;
        }
        public virtual string ProcessEventName(string eventName)
        {
            if (eventBusConfig.DeleteEventPrefix) eventName = eventName.Trim(eventBusConfig.EventNamePrefix.ToArray());
            if (eventBusConfig.DeleteEventSuffix) eventName = eventName.Trim(eventBusConfig.EventNameSuffix.ToArray());
            return eventName;
           
        }
        public virtual string GetSubName(string eventName) => $"{eventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        public virtual void Dispose()
        {
            eventBusConfig = null;
        }
        public async Task<bool> ProcessEvent(string eventName,string message)
        {
            eventName=ProcessEventName(eventName);
            var processed = false;
            if (_subManager.HasSubscriptionForEvent(eventName))
            {
                var subscriptions=_subManager.GetHandlersForEvent(eventName);
                using (var scope=_serviceProvider.CreateScope())
                {
                    foreach (var sub in subscriptions)
                    {
                        var handler=_serviceProvider.GetService(sub.HandlerType);
                        if (handler == null) continue;
                        var eventType = _subManager.GetEventTypeByName($"{eventBusConfig.EventNamePrefix}{eventName}{eventBusConfig.EventNameSuffix}");
                        var integrationEvent=JsonConvert.DeserializeObject(message,eventType);
                        var concreteType=typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }
                processed = true;
            }
            return processed;
        }

        public abstract void Publish(IntegrationEvent @event);


        public abstract void Subscribe<T, TH>(IntegrationEvent @event)where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
        

        public abstract void UnSubscribe<T, TH>(IntegrationEvent @event)where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
        
    }
}
