using JewelryStore.Core.Events;

namespace JewelryStore.Core.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : class, IEvent;
}