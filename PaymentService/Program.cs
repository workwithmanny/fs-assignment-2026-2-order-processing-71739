using Serilog;
using PaymentService;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/payment-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<PaymentConsumer>();
builder.Services.AddSerilog();

var host = builder.Build();
host.Run();
