using Serilog;
using ShippingService;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/shipping-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ShippingConsumer>();
builder.Services.AddSerilog();

var host = builder.Build();
host.Run();
