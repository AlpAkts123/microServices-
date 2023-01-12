using EventBusBase.Abstraction;
using EventBusBase.Events;
using System.Security.Cryptography.X509Certificates;

namespace EventBusBase.SubManagers
{
    public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;
        public Func<string, string> _eventNameGetter;

        public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGetter)
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
            _eventNameGetter = eventNameGetter;
        }

        public bool IsEmpty => _handlers.Keys.Any();
        public void clear() => _handlers.Clear();

        public event EventHandler<string> OnEventRemowed;

        public void AddSubcription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName= GetEventKey<T>();
            AddSubcription(typeof(TH),eventName);
            if (!_eventTypes.Contains(typeof(TH))) _eventTypes.Add(typeof(TH));


        }
        private void AddSubcription(Type handlerType,string eventName)
        {
            if (!HasSubscriptionForEvent(eventName)) _handlers.Add(eventName, new List<SubscriptionInfo>());
            if (_handlers[eventName].Any(s=>s.HandlerType==handlerType)) throw new ArgumentException($"Handler Type '{handlerType.Name}' already registered for '{eventName}'",nameof(handlerType));
            _handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));
        }





        public string GetEventKey<T>()
        {
            var eventName = typeof(T).Name;
            return _eventNameGetter(eventName);
        }

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(s => s.Name == eventName);

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key =GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];
        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemowed;
            handler?.Invoke(this, eventName);
        }
        private SubscriptionInfo FindSubscriptionToRemove<T,TH>() where T:IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return FindSubscriptionToRemove(eventName, typeof(TH));
        }
        private SubscriptionInfo FindSubscriptionToRemove(string eventName,Type handlerType)
        {
            if (!HasSubscriptionForEvent(eventName)) return null;
            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
        }
        public bool HasSubscriptionForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubscriptionForEvent(key);
        }

        public bool HasSubscriptionForEvent(string eventName)=> _handlers.ContainsKey(eventName);


        public void RemoveSubcription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var handlerToRemove=FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            RemoveHandler(eventName,handlerToRemove);
        }
        private void RemoveHandler(string eventName,SubscriptionInfo subsToRemove)
        {
            if (subsToRemove!=null)
            {
                _handlers[eventName].Remove(subsToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType=_eventTypes.SingleOrDefault(e=>e.Name==eventName);
                    if (eventType!=null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(eventName);
                }
            }
        }
        
    }
}
