using Summarix.Application.DTOs;
using Summarix.Application.Interfaces;

namespace Summarix.Application.Services;

public class SummaryService(ITextifyClient textifyClient, IOllamaClient ollamaClient, IPdfGenerator pdfGenerator) : ISummaryService
{
private readonly ITextifyClient _textifyClient = textifyClient;
private readonly IOllamaClient _ollamaClient = ollamaClient;
private readonly IPdfGenerator _pdfGenerator = pdfGenerator;

    public Task<SummaryResponseDto> GetSummaryFromVideoAsync(SummaryRequestDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
