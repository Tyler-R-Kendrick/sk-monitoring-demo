using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SemanticKernel.Community.Core.Filters;

/// <summary>
/// Example of telemetry in Semantic Kernel using Application Insights within console application.
/// </summary>
public class Application(ActivitySource activitySource)
{
    #region Private

    internal const string
        OpenAIServiceKey = "OpenAI",
        AzureOpenAIServiceKey = "AzureOpenAI",
        GoogleAIGeminiServiceKey = "GoogleAIGemini",
        HuggingFaceServiceKey = "HuggingFace",
        MistralAIServiceKey = "MistralAI";

    #region chat completion
    internal async Task RunOpenAIChatAsync(Kernel kernel)
    {
        Console.WriteLine("============= OpenAI Chat Completion =============");

        using var activity = activitySource.StartActivity(OpenAIServiceKey);
        SetTargetService(kernel, OpenAIServiceKey);
        try
        {
            await RunChatAsync(kernel);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task RunAzureOpenAIChatAsync(Kernel kernel)
    {
        Console.WriteLine("============= Azure OpenAI Chat Completion =============");

        using var activity = activitySource.StartActivity(AzureOpenAIServiceKey);
        SetTargetService(kernel, AzureOpenAIServiceKey);
        try
        {
            await RunChatAsync(kernel);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task RunGoogleAIChatAsync(Kernel kernel)
    {
        Console.WriteLine("============= Google Gemini Chat Completion =============");

        using var activity = activitySource.StartActivity(GoogleAIGeminiServiceKey);
        SetTargetService(kernel, GoogleAIGeminiServiceKey);

        try
        {
            await RunChatAsync(kernel);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task RunHuggingFaceChatAsync(Kernel kernel)
    {
        Console.WriteLine("============= HuggingFace Chat Completion =============");

        using var activity = activitySource.StartActivity(HuggingFaceServiceKey);
        SetTargetService(kernel, HuggingFaceServiceKey);

        try
        {
            await RunChatAsync(kernel);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task RunMistralAIChatAsync(Kernel kernel)
    {
        Console.WriteLine("============= MistralAI Chat Completion =============");

        using var activity = activitySource.StartActivity(MistralAIServiceKey);
        SetTargetService(kernel, MistralAIServiceKey);

        try
        {
            await RunChatAsync(kernel);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task RunChatAsync(Kernel kernel)
    {
        // Create the plugin from the sample plugins folder without registering it to the kernel.
        // We do not advise registering plugins to the kernel and then invoking them directly,
        // especially when the service supports function calling. Doing so will cause unexpected behavior,
        // such as repeated calls to the same function.
        var folder = RepoFiles.SamplePluginsPath();
        var plugin = kernel.CreatePluginFromPromptDirectory(Path.Combine(folder, "WriterPlugin"));

        // Using non-streaming to get the poem.
        var poem = await kernel.InvokeAsync<string>(
            plugin["ShortPoem"],
            new KernelArguments { ["input"] = "Write a poem about John Doe." });
        Console.WriteLine($"Poem:\n{poem}\n");

        // Use streaming to translate the poem.
        Console.WriteLine("Translated Poem:");
        await foreach (var update in kernel.InvokeStreamingAsync<string>(
            plugin["Translate"],
            new KernelArguments
            {
                ["input"] = poem,
                ["language"] = "Italian"
            }))
        {
            Console.Write(update);
        }
    }
    #endregion

    #region tool calls
    internal async Task RunOpenAIToolCallsAsync(Kernel kernel)
    {
        Console.WriteLine("============= Azure OpenAI ToolCalls =============");

        using var activity = activitySource.StartActivity(OpenAIServiceKey);
        SetTargetService(kernel, OpenAIServiceKey);
        try
        {
            await RunAutoToolCallAsync(kernel);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task RunAzureOpenAIToolCallsAsync(Kernel kernel)
    {
        Console.WriteLine("============= Azure OpenAI ToolCalls =============");

        using var activity = activitySource.StartActivity(AzureOpenAIServiceKey);
        SetTargetService(kernel, AzureOpenAIServiceKey);
        try
        {
            await RunAutoToolCallAsync(kernel);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task RunAutoToolCallAsync(Kernel kernel)
    {
        var result = await kernel.InvokePromptAsync("What is the weather like in my location?");

        Console.WriteLine(result);
    }
    #endregion

    internal static Kernel GetKernel(ILoggerFactory loggerFactory)
    {
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddLoggingFilters();
        builder.Services.AddSingleton(loggerFactory);
        builder.Services.AddSingleton(_ => loggerFactory.CreateLogger("SemanticKernel"));
        builder
            // .AddAzureOpenAIChatCompletion(
            //     deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
            //     modelId: TestConfiguration.AzureOpenAI.ChatModelId,
            //     endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            //     apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            //     serviceId: AzureOpenAIServiceKey)
            // .AddGoogleAIGeminiChatCompletion(
            //     modelId: TestConfiguration.GoogleAI.Gemini.ModelId,
            //     apiKey: TestConfiguration.GoogleAI.ApiKey,
            //     serviceId: GoogleAIGeminiServiceKey)
            // .AddHuggingFaceChatCompletion(
            //     model: TestConfiguration.HuggingFace.ModelId,
            //     endpoint: new Uri("https://api-inference.huggingface.co"),
            //     apiKey: TestConfiguration.HuggingFace.ApiKey,
            //     serviceId: HuggingFaceServiceKey)
            // .AddMistralChatCompletion(
            //     modelId: TestConfiguration.MistralAI.ChatModelId,
            //     apiKey: TestConfiguration.MistralAI.ApiKey,
            //     serviceId: MistralAIServiceKey)
            // .AddOpenAITextEmbeddingGeneration(
            //     modelId: TestConfiguration.OpenAI.ModelId,
            //     apiKey: TestConfiguration.OpenAI.ApiKey,
            //     orgId: TestConfiguration.OpenAI.OrgId,
            //     serviceId: OpenAIServiceKey)
            .AddOpenAIChatCompletion(
                modelId: TestConfiguration.OpenAI.ModelId,
                apiKey: TestConfiguration.OpenAI.ApiKey,
                orgId: TestConfiguration.OpenAI.OrgId,
                serviceId: OpenAIServiceKey);

        builder.Services.AddSingleton<IAIServiceSelector>(new CustomAIServiceSelector());
        builder.Plugins.AddFromType<WeatherPlugin>();
        builder.Plugins.AddFromType<LocationPlugin>();

        return builder.Build();
    }

    private static void SetTargetService(Kernel kernel, string targetServiceKey)
    {
        if (kernel.Data.ContainsKey("TargetService"))
        {
            kernel.Data["TargetService"] = targetServiceKey;
        }
        else
        {
            kernel.Data.Add("TargetService", targetServiceKey);
        }
    }
}

#endregion
