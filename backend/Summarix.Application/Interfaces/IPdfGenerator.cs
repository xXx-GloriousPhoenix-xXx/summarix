namespace Summarix.Application.Interfaces;

public interface IPdfGenerator
{
    Task<Stream> GetPdfFromTxtAsync(
        string title,
        string summaryText,
        CancellationToken cancellationToken = default);
}