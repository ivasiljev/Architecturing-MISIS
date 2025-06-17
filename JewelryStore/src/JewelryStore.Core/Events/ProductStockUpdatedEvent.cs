namespace JewelryStore.Core.Events;

public class ProductStockUpdatedEvent : IEvent
{
    public int ProductId { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => nameof(ProductStockUpdatedEvent);
}