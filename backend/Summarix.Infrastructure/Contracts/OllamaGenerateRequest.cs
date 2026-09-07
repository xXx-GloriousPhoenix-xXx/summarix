using System.Text.Json.Serialization;

namespace Summarix.Infrastructure.Contracts;

public record OllamaGenerateRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("system")] string System,
    [property: JsonPropertyName("stream")] bool Stream
);