using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SemanticKernel.Community.Core.Filters;

/// <summary>
/// Filter which logs an information available during function invocation such as:
/// Function name, arguments, execution settings, result, duration, token usage.
/// </summary>
internal sealed class FunctionInvocationLoggingFilter(ILogger logger) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        long startingTimestamp = Stopwatch.GetTimestamp();

        logger.LogInformation("Function {FunctionName} invoking.", context.Function.Name);

        if (context.Arguments.Count > 0)
        {
            logger.LogTrace("Function arguments: {Arguments}", JsonSerializer.Serialize(context.Arguments));
        }

        if (logger.IsEnabled(LogLevel.Information) && context.Arguments.ExecutionSettings is not null)
        {
            logger.LogInformation("Execution settings: {Settings}", JsonSerializer.Serialize(context.Arguments.ExecutionSettings));
        }

        try
        {
            await next(context);

            logger.LogInformation("Function {FunctionName} succeeded.", context.Function.Name);
            logger.LogTrace("Function result: {Result}", context.Result.ToString());

            if (logger.IsEnabled(LogLevel.Information))
            {
                var usage = context.Result.Metadata?["Usage"];

                if (usage is not null)
                {
                    logger.LogInformation("Usage: {Usage}", JsonSerializer.Serialize(usage));
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Function failed. Error: {Message}", exception.Message);
            throw;
        }
        finally
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                TimeSpan duration = new((long)((Stopwatch.GetTimestamp() - startingTimestamp) * (10_000_000.0 / Stopwatch.Frequency)));

                // Capturing the duration in seconds as per OpenTelemetry convention for instrument units:
                // More information here: https://opentelemetry.io/docs/specs/semconv/general/metrics/#instrument-units
                logger.LogInformation("Function completed. Duration: {Duration}s", duration.TotalSeconds);
            }
        }
    }
}
