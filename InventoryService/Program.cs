using Serilog;
using InventoryService;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/inventory-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<InventoryConsumer>();
builder.Services.AddSerilog();

var host = builder.Build();
host.Run();
