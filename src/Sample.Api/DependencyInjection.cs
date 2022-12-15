// Copyright (C) 2022 Thrive. Version 1.0.

using Microsoft.Data.SqlClient;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sample.Api.Application.Writes;
using Sample.Domain.SampleAggregate;
using Sample.Domain.Services;
using ThriveEventFlow.Diagnostics.OpenTelemetry;
using ThriveEventFlow.SqlServer;

namespace Sample.Api; 

public static class DependencyInjection {
    public static void AddThrive(this IServiceCollection services, IConfiguration configuration) {
        SqlConnection GetConnection() =>
            new(configuration["SqlServer:ConnectionString"]);
        services.AddSingleton((GetSqlServerConnection)GetConnection); 
        
        services.AddSingleton(new SqlServerStoreOptions());
        services.AddAggregateStore<SqlServerStore>();
        
        services.AddApplicationService<InvoiceCommandService, Invoice>();
        
        services.AddSingleton<ServiceExtensions.IsInvoiceAvailable>((id, thriveId) => new ValueTask<bool>(false));

        services.AddSingleton<ServiceExtensions.ConvertCurrency>((from, currency) => new Money(from.Amount * 2, currency));

    }
    public static void AddThriveOpenTelemetry(this IServiceCollection services) {
        var otelEnabled = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") != null;
        services.AddOpenTelemetryMetrics(
            builder => {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("invoices"))
                    .AddAspNetCoreInstrumentation()
                    .AddThriveEventFlow()
                    .AddThriveEventFlowSubscriptions()
                    .AddPrometheusExporter();
                if (otelEnabled) builder.AddOtlpExporter();
            }
        );

        // services.AddOpenTelemetryMetrics(
        //     builder => {
        //         builder
        //             .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("invoices"))
        //             .SetSampler(new AlwaysOnSampler())
        //             .AddAspNetCoreInstrumentation()
        //             .AddSqlClientInstrumentation()
        //             .AddThriveEventFlowTracing()
        //             .AddMongoDBInstrumentation();
        //
        //         if (otelEnabled)
        //             builder.AddOtlpExporter();
        //         else
        //             builder.AddZipkin();
        //     }
        // );
    }
}