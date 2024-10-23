using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// Enable model diagnostics with sensitive data.
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

// Load configuration from environment variables or user secrets.
IConfigurationRoot configRoot = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

TestConfiguration.Initialize(configRoot);
var connectionString = TestConfiguration.ApplicationInsights.ConnectionString;
var otlpEndpoint = TestConfiguration.OpenTelemetry.Endpoint;
var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService("TelemetryExample");

using var traceProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource("Microsoft.SemanticKernel*")
    .AddSource("Telemetry.Example")
    .AddOtlpExporter(options => options.Endpoint = new(otlpEndpoint))
    .AddAzureMonitorTraceExporter(options => options.ConnectionString = connectionString)
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddMeter("Microsoft.SemanticKernel*")
    .AddOtlpExporter(options => options.Endpoint = new(otlpEndpoint))
    .AddAzureMonitorMetricExporter(options => options.ConnectionString = connectionString)
    .Build();

const LogLevel MinLogLevel = LogLevel.Information;
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.AddOtlpExporter(options => options.Endpoint = new(otlpEndpoint));
        options.AddAzureMonitorLogExporter(options => options.ConnectionString = connectionString);
        // Format log messages. This is default to false.
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(MinLogLevel);
});

var kernel = Application.GetKernel(loggerFactory);

ActivitySource activitySource = new("Telemetry.Example");
var program = new Application(activitySource);

using var activity = activitySource.StartActivity("Main");
Console.WriteLine($"Operation/Trace ID: {Activity.Current?.TraceId}");
Console.WriteLine();

Console.WriteLine("Write a poem about John Doe and translate it to Italian.");
using (var _ = activitySource.StartActivity("Chat"))
{
    await program.RunOpenAIChatAsync(kernel);
    Console.WriteLine();
    // await RunAzureOpenAIChatAsync(kernel);
    // Console.WriteLine();
    // await RunGoogleAIChatAsync(kernel);
    // Console.WriteLine();
    // await RunHuggingFaceChatAsync(kernel);
    // Console.WriteLine();
    // await RunMistralAIChatAsync(kernel);
}

Console.WriteLine();
Console.WriteLine();

Console.WriteLine("Get weather.");
using (var _ = activitySource.StartActivity("ToolCalls"))
{
    await program.RunOpenAIToolCallsAsync(kernel);
    Console.WriteLine();
}
