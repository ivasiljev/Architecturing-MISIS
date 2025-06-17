using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JewelryStore.Core.DTOs;
using JewelryStore.Core.Interfaces;
using JewelryStore.Core.Entities;
using JewelryStore.Core.Events;
using AutoMapper;
using System.Security.Claims;
using Prometheus;

namespace JewelryStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    // Business Metrics
    private static readonly Counter OrdersCreatedCounter = Metrics.CreateCounter(
        "jewelrystore_orders_created_total", "Total orders created", new[] { "status" });

    private static readonly Counter SalesAmountCounter = Metrics.CreateCounter(
        "jewelrystore_sales_amount_total", "Total sales amount in rubles");

    private static readonly Histogram OrderValueHistogram = Metrics.CreateHistogram(
        "jewelrystore_order_value_rubles", "Order value distribution in rubles",
        new HistogramConfiguration
        {
            Buckets = new[] { 5000.0, 10000.0, 25000.0, 50000.0, 100000.0, 250000.0, 500000.0, 1000000.0 }
        });

    public OrdersController(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEventPublisher eventPublisher,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    /// <summary>
    /// Получить заказы текущего пользователя
    /// </summary>
    /// <returns>Список заказов пользователя</returns>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetCurrentUserId();
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

        return Ok(orderDtos);
    }

    /// <summary>
    /// Получить заказ по ID
    /// </summary>
    /// <param name="id">ID заказа</param>
    /// <returns>Заказ</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = GetCurrentUserId();
        var order = await _orderRepository.GetByIdAsync(id);

        if (order == null || order.UserId != userId)
            return NotFound(new { message = "Заказ не найден" });

        var orderDto = _mapper.Map<OrderDto>(order);
        return Ok(orderDto);
    }

    /// <summary>
    /// Создать новый заказ
    /// </summary>
    /// <param name="createOrderDto">Данные заказа</param>
    /// <returns>Созданный заказ</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        // ДИАГНОСТИКА: Проверяем что получили в заголовках
        Console.WriteLine("=== ДИАГНОСТИКА API ===");
        Console.WriteLine($"Request.Headers.Authorization: {Request.Headers.Authorization}");
        Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");
        Console.WriteLine("Все claims пользователя:");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"  {claim.Type}: {claim.Value}");
        }
        Console.WriteLine("=== КОНЕЦ ДИАГНОСТИКИ API ===");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            Console.WriteLine("ОШИБКА: Не удалось получить userId из токена");
            return Unauthorized(new { message = "Не удалось определить пользователя" });
        }

        // Создаем заказ
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = createOrderDto.ShippingAddress,
            Notes = createOrderDto.Notes,
            OrderItems = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        // Добавляем позиции заказа
        foreach (var itemDto in createOrderDto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
            if (product == null || !product.IsActive)
                return BadRequest(new { message = $"Продукт с ID {itemDto.ProductId} не найден" });

            if (product.StockQuantity < itemDto.Quantity)
                return BadRequest(new { message = $"Недостаточно товара {product.Name} на складе" });

            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            };

            order.OrderItems.Add(orderItem);
            totalAmount += orderItem.Quantity * orderItem.UnitPrice;

            // Обновляем количество на складе
            product.StockQuantity -= itemDto.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);
        }

        order.TotalAmount = totalAmount;
        await _orderRepository.CreateAsync(order);

        // Отслеживание бизнес-метрик
        OrdersCreatedCounter.WithLabels("created").Inc();
        SalesAmountCounter.Inc((double)totalAmount);
        OrderValueHistogram.Observe((double)totalAmount);

        // Публикуем событие создания заказа
        await _eventPublisher.PublishAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = userId,
            TotalAmount = totalAmount,
            OccurredAt = order.OrderDate
        });

        var orderDto = _mapper.Map<OrderDto>(order);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
    }

    /// <summary>
    /// Отменить заказ
    /// </summary>
    /// <param name="id">ID заказа</param>
    /// <returns>Результат отмены</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = GetCurrentUserId();
        var order = await _orderRepository.GetByIdAsync(id);

        if (order == null || order.UserId != userId)
            return NotFound(new { message = "Заказ не найден" });

        if (order.Status != OrderStatus.Pending)
            return BadRequest(new { message = "Можно отменить только заказы в статусе 'Ожидает обработки'" });

        order.Status = OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync(order);

        // Отслеживание отмены заказа
        OrdersCreatedCounter.WithLabels("cancelled").Inc();

        // Восстанавливаем количество товаров на складе
        foreach (var item in order.OrderItems)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);
            }
        }

        return Ok(new { message = "Заказ успешно отменен" });
    }

    /// <summary>
    /// Получить все заказы (только для администраторов)
    /// </summary>
    /// <param name="status">Фильтр по статусу</param>
    /// <returns>Список всех заказов</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAllOrders([FromQuery] OrderStatus? status = null)
    {
        // В реальности здесь должна быть проверка роли администратора
        var orders = await _orderRepository.GetAllAsync();

        if (status.HasValue)
        {
            orders = orders.Where(o => o.Status == status.Value);
        }

        var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
        return Ok(orderDtos);
    }

    /// <summary>
    /// Обновить статус заказа (только для администраторов)
    /// </summary>
    /// <param name="id">ID заказа</param>
    /// <param name="status">Новый статус</param>
    /// <returns>Обновленный заказ</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound(new { message = "Заказ не найден" });

        order.Status = statusDto.Status;

        // Устанавливаем даты в зависимости от статуса
        switch (statusDto.Status)
        {
            case OrderStatus.Shipped:
                order.ShippedDate = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                order.DeliveredDate = DateTime.UtcNow;
                break;
        }

        await _orderRepository.UpdateAsync(order);

        var orderDto = _mapper.Map<OrderDto>(order);
        return Ok(orderDto);
    }

    /// <summary>
    /// Тестовый метод для проверки авторизации
    /// </summary>
    [HttpGet("test-auth")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public IActionResult TestAuth()
    {
        Console.WriteLine("=== ТЕСТ АВТОРИЗАЦИИ ===");
        Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");
        Console.WriteLine("Все claims:");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"  {claim.Type}: {claim.Value}");
        }

        var userId = GetCurrentUserId();
        Console.WriteLine($"GetCurrentUserId returned: {userId}");
        Console.WriteLine("=== КОНЕЦ ТЕСТА ===");

        return Ok(new
        {
            isAuthenticated = User.Identity?.IsAuthenticated,
            userId = userId,
            claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
        });
    }

    private int GetCurrentUserId()
    {
        // Ищем по различным возможным именам claim'ов
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                         User.FindFirst("nameid") ??
                         User.FindFirst("sub");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        // Для отладки: выводим все доступные claims
        Console.WriteLine("=== ДОСТУПНЫЕ CLAIMS В API ===");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"  {claim.Type}: {claim.Value}");
        }
        Console.WriteLine("=== КОНЕЦ CLAIMS ===");

        return 0;
    }
}