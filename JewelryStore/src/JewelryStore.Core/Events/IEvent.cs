namespace JewelryStore.Core.Events;

public interface IEvent
{
    DateTime OccurredAt { get; }
    string EventType { get; }
}