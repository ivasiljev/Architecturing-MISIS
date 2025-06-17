using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using JewelryStore.Core.Events;
using JewelryStore.Core.Interfaces;

namespace JewelryStore.Infrastructure.Services;

public class RabbitMQEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQEventPublisher> _logger;
    private readonly Dictionary<Type, ExchangeConfig> _eventRouting;

    public RabbitMQEventPublisher(IConfiguration configuration, ILogger<RabbitMQEventPublisher> logger)
    {
        _logger = logger;

        var connectionString = configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672/";
        var factory = new ConnectionFactory()
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection("JewelryStore-EventPublisher");
        _channel = _connection.CreateModel();

        _eventRouting = new Dictionary<Type, ExchangeConfig>
        {
            {
                typeof(OrderCreatedEvent),
                new ExchangeConfig("orders.exchange", ExchangeType.Direct, "orders.created")
            },
            {
                typeof(ProductStockUpdatedEvent),
                new ExchangeConfig("products.exchange", ExchangeType.Fanout, "")
            }
        };

        // Объявляем необходимые обмены и очереди
        SetupRabbitMQInfrastructure();
    }

    private void SetupRabbitMQInfrastructure()
    {
        try
        {
            // Настройка обменов различных типов для демонстрации в лабе 3

            // 1. Direct Exchange для заказов
            _channel.ExchangeDeclare("orders.exchange", ExchangeType.Direct, durable: true);
            _channel.QueueDeclare("orders.created.queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("orders.created.queue", "orders.exchange", "orders.created");

            // 2. Fanout Exchange для уведомлений о товарах (рассылка всем)
            _channel.ExchangeDeclare("products.exchange", ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare("products.inventory.queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("products.notifications.queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("products.inventory.queue", "products.exchange", "");
            _channel.QueueBind("products.notifications.queue", "products.exchange", "");

            // 3. Topic Exchange для системы уведомлений
            _channel.ExchangeDeclare("notifications.exchange", ExchangeType.Topic, durable: true);
            _channel.QueueDeclare("notifications.email.queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("notifications.sms.queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("notifications.email.queue", "notifications.exchange", "notification.email.*");
            _channel.QueueBind("notifications.sms.queue", "notifications.exchange", "notification.sms.*");

            // 4. Headers Exchange для фильтрации по метаданным
            _channel.ExchangeDeclare("logs.exchange", ExchangeType.Headers, durable: true);
            _channel.QueueDeclare("logs.error.queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("logs.info.queue", durable: true, exclusive: false, autoDelete: false);

            var errorHeaders = new Dictionary<string, object> { { "level", "error" } };
            var infoHeaders = new Dictionary<string, object> { { "level", "info" } };
            _channel.QueueBind("logs.error.queue", "logs.exchange", "", errorHeaders);
            _channel.QueueBind("logs.info.queue", "logs.exchange", "", infoHeaders);

            _logger.LogInformation("RabbitMQ infrastructure setup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup RabbitMQ infrastructure");
            throw;
        }
    }

    public async Task PublishAsync<T>(T @event) where T : class, IEvent
    {
        try
        {
            var eventType = typeof(T);
            var config = _eventRouting.GetValueOrDefault(eventType,
                new ExchangeConfig("default.exchange", ExchangeType.Direct, eventType.Name.ToLowerInvariant()));

            // Декларируем обмен если его нет
            _channel.ExchangeDeclare(config.ExchangeName, config.ExchangeType, durable: true);

            var messageBody = JsonSerializer.SerializeToUtf8Bytes(@event);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Type = eventType.Name;

            // Добавляем заголовки для Headers Exchange
            properties.Headers = new Dictionary<string, object>
            {
                { "event-type", eventType.Name },
                { "timestamp", DateTimeOffset.UtcNow.ToString() },
                { "source", "JewelryStore.API" },
                { "level", "info" }
            };

            await Task.Run(() =>
            {
                _channel.BasicPublish(
                    exchange: config.ExchangeName,
                    routingKey: config.RoutingKey,
                    basicProperties: properties,
                    body: messageBody
                );
            });

            _logger.LogInformation("Event {EventType} published to exchange {Exchange} with routing key {RoutingKey}",
                eventType.Name, config.ExchangeName, config.RoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    private record ExchangeConfig(string ExchangeName, string ExchangeType, string RoutingKey);
}