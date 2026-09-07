using Summarix.Application.Interfaces;

namespace Summarix.Infrastructure.PdfGenerator;

public class PdfGenerator : IPdfGenerator
{
    public Task<Stream> GetPdfFromTxtAsync(string title, string summaryText, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
