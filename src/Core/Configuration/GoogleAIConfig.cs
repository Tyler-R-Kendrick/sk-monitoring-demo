public record GoogleAIConfig(string ApiKey, string EmbeddingModelId, GoogleAIConfig.GeminiConfig Gemini)
{
    public record GeminiConfig(string ModelId);
}