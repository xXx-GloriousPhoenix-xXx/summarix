namespace Summarix.Infrastructure.Ollama;

public class OllamaSettings
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:12000";
    public string Model { get; set; } = "llama3.2";
    public int TimeoutMinutes { get; set; } = 5;
    public string DefaultSystemPrompt { get; set; } =
        "You are a professional assistant. Summarize the following video transcript clearly and concisely using bullet points and key takeaways.";
}