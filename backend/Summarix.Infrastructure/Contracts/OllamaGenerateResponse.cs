using System.Text.Json.Serialization;

namespace Summarix.Infrastructure.Contracts;

public record OllamaGenerateResponse(
    [property: JsonPropertyName("response")] string Response,
    [property: JsonPropertyName("done")] bool Done
);