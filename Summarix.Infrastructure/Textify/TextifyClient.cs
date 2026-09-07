using Summarix.Application.Interfaces;

namespace Summarix.Infrastructure.Textify;

public class TextifyClient : ITextifyClient
{
    public Task<string> GetTextFromVideoAsync(Stream videoStream, string fileName, string? language = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
