namespace Summarix.Application.Interfaces;

public interface IOllamaClient
{
    Task<string> GetSummaryFromTextAsync(
        string text,
        string? promptInstruction = null,
        CancellationToken cancellationToken = default);
}