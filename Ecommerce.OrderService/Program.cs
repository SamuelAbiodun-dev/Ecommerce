using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Infrastructure;
using Ecommerce.OrderService.Logic;
using Ecommerce.OrderService.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<OrderDbContext>(option =>
option.UseNpgsql(builder.Configuration.GetConnectionString("OrderDbConnection")));

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQBus").Get<RabbitMQConfig>();
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderConsumer>();
    config.UsingRabbitMq((ctx, cfg) =>
    {
        // Corrected the URL to include the VirtualHost
        var rabbitMqUri = $"amqp://{rabbitMqSettings.Username}:{rabbitMqSettings.Password}@{rabbitMqSettings.Host}/{rabbitMqSettings.VirtualHost}";
        cfg.Host(new Uri(rabbitMqUri));

        cfg.ReceiveEndpoint("product-queue", c =>
        {
            c.ConfigureConsumer<OrderConsumer>(ctx);
        });

        cfg.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5));  // Retry 3 times, with an initial delay of 30 seconds, and an increment of 5 seconds
        });
    });
});


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
