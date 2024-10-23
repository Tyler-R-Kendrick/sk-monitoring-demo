using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Services;

public sealed partial class Program
{
    private sealed class CustomAIServiceSelector : IAIServiceSelector
    {
        public bool TrySelectAIService<T>(
            Kernel kernel, KernelFunction function, KernelArguments arguments,
            [NotNullWhen(true)] out T? service, out PromptExecutionSettings? serviceSettings) where T : class, IAIService
        {
            var targetServiceKey = kernel.Data.TryGetValue("TargetService", out object? value) ? value : null;
            if (targetServiceKey is not null)
            {
                var targetService = kernel.Services.GetKeyedServices<T>(targetServiceKey).FirstOrDefault();
                if (targetService is not null)
                {
                    service = targetService;
                    serviceSettings = targetServiceKey switch
                    {
                        OpenAIServiceKey => new OpenAIPromptExecutionSettings
                        {
                            Temperature = 0,
                            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                        },
                        AzureOpenAIServiceKey => new OpenAIPromptExecutionSettings
                        {
                            Temperature = 0,
                            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                        },
                        GoogleAIGeminiServiceKey => new GeminiPromptExecutionSettings
                        {
                            Temperature = 0,
                            // Not show casing the AutoInvokeKernelFunctions behavior for Gemini due the following issue:
                            // https://github.com/microsoft/semantic-kernel/issues/6282
                            // ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
                        },
                        HuggingFaceServiceKey => new HuggingFacePromptExecutionSettings
                        {
                            Temperature = 0,
                        },
                        MistralAIServiceKey => new MistralAIPromptExecutionSettings
                        {
                            Temperature = 0,
                            ToolCallBehavior = MistralAIToolCallBehavior.AutoInvokeKernelFunctions
                        },
                        _ => null,
                    };

                    return true;
                }
            }

            service = null;
            serviceSettings = null;
            return false;
        }
    }
}
