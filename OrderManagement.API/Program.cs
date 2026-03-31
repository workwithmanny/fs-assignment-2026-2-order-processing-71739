using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using OrderManagement.API.Messaging;
using OrderManagement.API.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/orderapi-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=orderprocessing.db"));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<OrderStatusConsumer>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Name = "Football", Description = "Official size football", Price = 19.99m, Category = "Soccer", StockQuantity = 100 },
            new Product { Name = "Running Shoes", Description = "Lightweight running shoes", Price = 89.99m, Category = "Footwear", StockQuantity = 50 },
            new Product { Name = "Tennis Racket", Description = "Professional tennis racket", Price = 49.99m, Category = "Tennis", StockQuantity = 30 },
            new Product { Name = "Basketball", Description = "Indoor/outdoor basketball", Price = 29.99m, Category = "Basketball", StockQuantity = 75 },
            new Product { Name = "Yoga Mat", Description = "Non-slip yoga mat", Price = 24.99m, Category = "Fitness", StockQuantity = 60 },
            new Product { Name = "Cycling Helmet", Description = "Lightweight safety helmet", Price = 59.99m, Category = "Cycling", StockQuantity = 40 }
        );
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();