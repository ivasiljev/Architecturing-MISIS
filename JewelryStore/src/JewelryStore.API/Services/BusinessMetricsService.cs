using JewelryStore.Core.Interfaces;
using Prometheus;

namespace JewelryStore.API.Services;

public class BusinessMetricsService : BackgroundService
{
    private readonly ILogger<BusinessMetricsService> _logger;
    private readonly IServiceProvider _serviceProvider;

    // Статические метрики для обновления
    private static readonly Gauge ProductsInStockGauge = Metrics.CreateGauge(
        "jewelrystore_products_in_stock", "Current products in stock", new[] { "category", "material" });

    private static readonly Gauge AverageOrderValueGauge = Metrics.CreateGauge(
        "jewelrystore_average_order_value_rubles", "Average order value in rubles");

    private static readonly Gauge ActiveUsersGauge = Metrics.CreateGauge(
        "jewelrystore_active_users_daily", "Daily active users");

    public BusinessMetricsService(ILogger<BusinessMetricsService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Инициализируем метрики при запуске
        try
        {
            await InitializeCountersFromDatabase();
            _logger.LogInformation("Business metrics initialized from database at startup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing business metrics from database");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateBusinessMetrics();
                _logger.LogInformation("Business metrics updated successfully at {Time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating business metrics");
            }

            // Обновляем метрики каждые 5 минут
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task UpdateBusinessMetrics()
    {
        using var scope = _serviceProvider.CreateScope();
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        // Обновляем количество товаров в наличии по категориям и материалам
        var products = await productRepository.GetAllAsync();
        var activeProducts = products.Where(p => p.IsActive && p.StockQuantity > 0);

        // Группируем по категориям и материалам
        var stockByCategory = activeProducts
            .GroupBy(p => new { p.Category, p.Material })
            .ToDictionary(g => g.Key, g => g.Sum(p => p.StockQuantity));

        // Устанавливаем новые значения (старые автоматически перезаписываются)
        foreach (var item in stockByCategory)
        {
            ProductsInStockGauge.WithLabels(item.Key.Category, item.Key.Material).Set(item.Value);
        }

        // Рассчитываем средний чек за последние 24 часа
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var recentOrders = await orderRepository.GetAllAsync();
        var ordersLast24h = recentOrders
            .Where(o => o.OrderDate >= yesterday && o.Status != Core.Entities.OrderStatus.Cancelled)
            .ToList();

        if (ordersLast24h.Any())
        {
            var averageOrderValue = ordersLast24h.Average(o => (double)o.TotalAmount);
            AverageOrderValueGauge.Set(averageOrderValue);
        }

        // Имитируем подсчет активных пользователей (в реальном приложении это должно быть из базы данных)
        var activeUsersCount = ordersLast24h.Select(o => o.UserId).Distinct().Count();
        ActiveUsersGauge.Set(activeUsersCount);

        _logger.LogDebug("Updated stock metrics for {Categories} categories, average order value: {AOV}, active users: {Users}",
            stockByCategory.Count,
            ordersLast24h.Any() ? ordersLast24h.Average(o => o.TotalAmount) : 0,
            activeUsersCount);
    }

    private async Task InitializeCountersFromDatabase()
    {
        using var scope = _serviceProvider.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        // Получаем все заказы из базы данных
        var allOrders = await orderRepository.GetAllAsync();

        // Получаем статические Counter'ы через Metrics
        var ordersCreatedCounter = Metrics.CreateCounter(
            "jewelrystore_orders_created_total", "Total orders created", new[] { "status" });

        var salesAmountCounter = Metrics.CreateCounter(
            "jewelrystore_sales_amount_total", "Total sales amount in rubles");

        // Инициализируем Counter'ы с текущими данными из БД
        var createdOrders = allOrders.Where(o => o.Status != Core.Entities.OrderStatus.Cancelled).ToList();
        var cancelledOrders = allOrders.Where(o => o.Status == Core.Entities.OrderStatus.Cancelled).ToList();

        // Устанавливаем значения счетчиков
        if (createdOrders.Any())
        {
            ordersCreatedCounter.WithLabels("created").IncTo(createdOrders.Count);
            var totalSales = createdOrders.Sum(o => (double)o.TotalAmount);
            salesAmountCounter.IncTo(totalSales);
        }

        if (cancelledOrders.Any())
        {
            ordersCreatedCounter.WithLabels("cancelled").IncTo(cancelledOrders.Count);
        }

        _logger.LogInformation("Initialized counters: {CreatedOrders} created orders, {CancelledOrders} cancelled orders, {TotalSales} total sales",
            createdOrders.Count, cancelledOrders.Count, createdOrders.Sum(o => o.TotalAmount));
    }
}