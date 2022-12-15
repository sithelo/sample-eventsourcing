using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using OpenTelemetry;
using Sample.Api;
using Sample.Domain.Events;
using Serilog;
using Serilog.Events;
using ThriveEventFlow;
using ThriveEventFlow.AspNetCore;
using ThriveEventFlow.Diagnostics.Logging;
using ThriveEventFlow.SqlServer;

TypeMap.RegisterKnownEventTypes(typeof(InvoiceEvents.V1.InvoiceCreated).Assembly);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Grpc", LogEventLevel.Information)
    .MinimumLevel.Override("Grpc.Net.Client.Internal.GrpcCall", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services
    .AddControllers()
    .AddJsonOptions(cfg => cfg.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddThriveOpenTelemetry();
builder.Services.AddThrive(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("SqlServer:InitializeDatabase")) {
    await InitialiseSchema(app);
}

app.UseSerilogRequestLogging();
app.AddThriveEventFlowLogs();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
app.MapControllers();
//app.UseOpenTelemetryPrometheusScrapingEndpoint();

var factory  = app.Services.GetRequiredService<ILoggerFactory>();
var listener = new LoggingEventListener(factory, "OpenTelemetry");

try {
    app.Run("http://*:5051");
    return 0;
}
catch (Exception e) {
    Log.Fatal(e, "Host terminated unexpectedly");
    return 1;
}
finally {
    Log.CloseAndFlush();
    listener.Dispose();
}

async Task InitialiseSchema(IHost webApplication) {
    var options           = webApplication.Services.GetRequiredService<SqlServerStoreOptions>();
    var schema            = new Schema(options.Schema);
    var connectionFactory = webApplication.Services.GetRequiredService<GetSqlServerConnection>();
    await schema.CreateSchema(connectionFactory);
}