using Microsoft.Extensions.Logging;

namespace SemanticKernel.Community.Core.Filters;

/// <summary>
/// Filter which logs an information available during prompt rendering such as rendered prompt.
/// </summary>
internal sealed class PromptRenderLoggingFilter(ILogger logger) : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next(context);

        logger.LogTrace("Rendered prompt: {Prompt}", context.RenderedPrompt);
    }
}
