using Confluent.Kafka;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using JewelryStore.Core.Events;
using JewelryStore.Core.Interfaces;

namespace JewelryStore.Infrastructure.Services;

public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly Dictionary<Type, string> _topicMapping;

    public KafkaEventPublisher(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration.GetConnectionString("Kafka") ?? "localhost:9092"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();

        _topicMapping = new Dictionary<Type, string>
        {
            { typeof(OrderCreatedEvent), "order-created" },
            { typeof(ProductStockUpdatedEvent), "product-stock-updated" }
        };
    }

    public async Task PublishAsync<T>(T @event) where T : class, IEvent
    {
        try
        {
            var eventType = typeof(T);
            if (!_topicMapping.TryGetValue(eventType, out var topic))
            {
                topic = eventType.Name.ToLowerInvariant();
            }

            var json = JsonConvert.SerializeObject(@event);
            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = json,
                Headers = new Headers
                {
                    {"event-type", System.Text.Encoding.UTF8.GetBytes(eventType.Name)},
                    {"timestamp", System.Text.Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString())}
                }
            };

            await _producer.ProduceAsync(topic, message);
        }
        catch (Exception ex)
        {
            // Log error in production
            Console.WriteLine($"Failed to publish event: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}