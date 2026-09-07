using Summarix.Application.DTOs;

namespace Summarix.Application.Interfaces;

public interface ISummaryService
{
    Task<SummaryResponseDto> GetSummaryFromVideoAsync(SummaryRequestDto request, CancellationToken cancellationToken = default);
}
