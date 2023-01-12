using Newtonsoft.Json;

namespace EventBusBase.Events
{
    public  class IntegrationEvent
    {
        [JsonProperty]
        public Guid Id { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public IntegrationEvent()
        {
            Id= Guid.NewGuid();
            CreatedDate= DateTime.UtcNow;
        }
        [JsonConstructor]
        public IntegrationEvent(Guid ıd, DateTime createdDate)
        {
            Id = ıd;
            CreatedDate = createdDate;
        }
    }
}
