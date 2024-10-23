using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernel.Community.Core.Filters;

public static class KernelBuilderFiltersExtensions
{
    public static IKernelBuilder AddLoggingFilters(this IKernelBuilder builder)
    {
        // Add filters with logging.
        builder.Services.AddSingleton<IFunctionInvocationFilter, FunctionInvocationLoggingFilter>();
        builder.Services.AddSingleton<IPromptRenderFilter, PromptRenderLoggingFilter>();
        builder.Services.AddSingleton<IAutoFunctionInvocationFilter, AutoFunctionInvocationLoggingFilter>();
        return builder;
    }
}