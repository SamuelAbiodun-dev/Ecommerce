using Ecommerce.OrderService.Utils;
using Ecommerce.ProductService.Data;
using Ecommerce.ProductService.Infrastructure;
using Ecommerce.ProductService.Logic;
using Ecommerce.ProductService.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>(option =>
    option.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=samucolon;Database=Productdb"));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Get RabbitMQ configuration from appsettings.json
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQBus").Get<RabbitMQConfig>();

// Add MassTransit and configure RabbitMQ
builder.Services.AddMassTransit(config =>
{
    // Register the consumer
    config.AddConsumer<OrderCreatedEventConsumer>();

    // Configure RabbitMQ with the VirtualHost included
    config.UsingRabbitMq((ctx, cfg) =>
    {
        // Set up the RabbitMQ host with virtual host and authentication
        var rabbitMqUri = $"amqps://{rabbitMqSettings.Username}:{rabbitMqSettings.Password}@{rabbitMqSettings.Host}/{rabbitMqSettings.VirtualHost}";
        cfg.Host(new Uri(rabbitMqUri));

        // Configure the receive endpoint and associate it with the consumer
        cfg.ReceiveEndpoint("order-created-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(ctx);
        });

        // Configure message retry policy if needed
        cfg.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5));
        });
    });
});

// Add MassTransit Hosted Service to manage background services
builder.Services.AddMassTransitHostedService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
