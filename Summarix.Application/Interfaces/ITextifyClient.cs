namespace Summarix.Application.Interfaces;

public interface ITextifyClient
{
    Task<string> GetTextFromVideoAsync(
        Stream videoStream,
        string fileName,
        string? language = null,
        CancellationToken cancellationToken = default);
}