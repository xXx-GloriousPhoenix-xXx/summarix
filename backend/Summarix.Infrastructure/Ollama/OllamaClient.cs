using Summarix.Application.Interfaces;

namespace Summarix.Infrastructure.Ollama;

public class OllamaClient : IOllamaClient
{
    public Task<string> GetSummaryFromTextAsync(string text, string? promptInstruction = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}