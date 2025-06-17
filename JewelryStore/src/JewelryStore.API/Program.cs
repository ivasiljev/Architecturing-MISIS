using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using StackExchange.Redis;
using Prometheus;
using JewelryStore.Infrastructure.Data;
using JewelryStore.Infrastructure.Services;
using JewelryStore.Infrastructure.Repositories;
using JewelryStore.Core.Interfaces;
using JewelryStore.API.Services;
using JewelryStore.API.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// Database
builder.Services.AddDbContext<JewelryStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("JewelryStore.API")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connectionString ?? "localhost:6379");
});

// Services
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IEventPublisher, KafkaEventPublisher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJwtTokensMinimum256BitsLong";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "JewelryStore",
        ValidAudience = jwtSettings["Audience"] ?? "JewelryStore",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    // Добавляем диагностику JWT событий
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"=== JWT AUTHENTICATION FAILED ===");
            Console.WriteLine($"Exception: {context.Exception.Message}");
            Console.WriteLine($"Request path: {context.Request.Path}");
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                Console.WriteLine($"Authorization header: {authHeader.Substring(0, Math.Min(50, authHeader.Length))}...");
            }
            Console.WriteLine($"=== END JWT AUTHENTICATION FAILED ===");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("=== JWT TOKEN VALIDATED SUCCESSFULLY ===");
            Console.WriteLine($"User: {context.Principal.Identity?.Name}");
            Console.WriteLine("Claims:");
            foreach (var claim in context.Principal.Claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }
            Console.WriteLine("=== END JWT TOKEN VALIDATED ===");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("=== JWT CHALLENGE ===");
            Console.WriteLine($"Error: {context.Error}");
            Console.WriteLine($"ErrorDescription: {context.ErrorDescription}");
            Console.WriteLine($"Request path: {context.Request.Path}");
            Console.WriteLine("=== END JWT CHALLENGE ===");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JewelryStore API",
        Version = "v1",
        Description = "API для управления ювелирным интернет-магазином"
    });

    // JWT Bearer схема для Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7001",
                "http://localhost:5001",
                "https://localhost:5217",
                "http://localhost:5216",
                "https://localhost:7216",
                "http://localhost:5015"
            )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Prometheus Metrics
builder.Services.AddSingleton<Counter>(provider =>
    Metrics.CreateCounter("jewelrystore_requests_total", "Total requests to JewelryStore API"));

// Business Metrics для ювелирного магазина
builder.Services.AddSingleton<Counter>(provider =>
    Metrics.CreateCounter("jewelrystore_orders_created_total", "Total orders created", new[] { "status" }));

builder.Services.AddSingleton<Counter>(provider =>
    Metrics.CreateCounter("jewelrystore_sales_amount_total", "Total sales amount in rubles"));

builder.Services.AddSingleton<Gauge>(provider =>
    Metrics.CreateGauge("jewelrystore_products_in_stock", "Current products in stock", new[] { "category", "material" }));

builder.Services.AddSingleton<Counter>(provider =>
    Metrics.CreateCounter("jewelrystore_popular_products_views", "Product views counter", new[] { "product_id", "category" }));

builder.Services.AddSingleton<Histogram>(provider =>
    Metrics.CreateHistogram("jewelrystore_order_value_rubles", "Order value distribution in rubles",
        new HistogramConfiguration
        {
            Buckets = new[] { 5000.0, 10000.0, 25000.0, 50000.0, 100000.0, 250000.0, 500000.0, 1000000.0 }
        }));

builder.Services.AddSingleton<Gauge>(provider =>
    Metrics.CreateGauge("jewelrystore_average_order_value_rubles", "Average order value in rubles"));

builder.Services.AddSingleton<Counter>(provider =>
    Metrics.CreateCounter("jewelrystore_user_registrations_total", "Total user registrations"));

builder.Services.AddSingleton<Gauge>(provider =>
    Metrics.CreateGauge("jewelrystore_active_users_daily", "Daily active users"));

// Background service для обновления бизнес-метрик
builder.Services.AddHostedService<JewelryStore.API.Services.BusinessMetricsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JewelryStore API v1");
        c.RoutePrefix = string.Empty; // Swagger будет доступен на корневом пути
    });
}

app.UseHttpsRedirection();

// Prometheus metrics endpoint
app.UseHttpMetrics();
app.MapMetrics();

app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
