namespace JewelryStore.Core.Events;

public class OrderCreatedEvent : IEvent
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => nameof(OrderCreatedEvent);
    public List<OrderItemData> Items { get; set; } = new();

    public class OrderItemData
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}