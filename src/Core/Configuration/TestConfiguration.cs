using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

public sealed partial class TestConfiguration
{
    private readonly IConfigurationRoot _configRoot;
    private static TestConfiguration? s_instance;

    private TestConfiguration(IConfigurationRoot configRoot) => this._configRoot = configRoot;

    public static void Initialize(IConfigurationRoot configRoot) => s_instance = new(configRoot);

    public static ApplicationInsightsConfig ApplicationInsights => LoadSection<ApplicationInsightsConfig>();

    public static OpenAIConfig OpenAI => LoadSection<OpenAIConfig>();
    
    public static AzureOpenAIConfig AzureOpenAI => LoadSection<AzureOpenAIConfig>();

    public static GoogleAIConfig GoogleAI => LoadSection<GoogleAIConfig>();

    public static HuggingFaceConfig HuggingFace => LoadSection<HuggingFaceConfig>();

    public static MistralAIConfig MistralAI => LoadSection<MistralAIConfig>();

    private static T LoadSection<T>([CallerMemberName] string? caller = null)
    {
        if (s_instance is null)
        {
            throw new InvalidOperationException(
                "TestConfiguration must be initialized with a call to Initialize(IConfigurationRoot) before accessing configuration values.");
        }

        if (string.IsNullOrEmpty(caller))
        {
            throw new ArgumentNullException(nameof(caller));
        }

        return s_instance._configRoot.GetSection(caller).Get<T>() ??
            throw new KeyNotFoundException($"Could not find configuration section {caller}");
    }
}