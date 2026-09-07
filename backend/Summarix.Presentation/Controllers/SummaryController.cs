using Microsoft.AspNetCore.Mvc;
using Summarix.Application.DTOs;
using Summarix.Application.Interfaces;
using Summarix.Presentation.Contracts;

namespace Summarix.Presentation.Controllers;

[ApiController]
[Route("api/summary")]
public class SummaryController(ISummaryService summaryService) : ControllerBase
{
    private readonly ISummaryService _summaryService = summaryService;

    [HttpPost("generate")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateSummaryPdfAsync(
        [FromForm] GenerateSummaryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.File.Length == 0)
        {
            return BadRequest("A valid video file is required.");
        }

        await using var videoStream = request.File.OpenReadStream();

        var serviceRequest = new SummaryRequestDto(
            VideoStream: videoStream,
            FileName: request.File.FileName,
            ContentType: request.File.ContentType,
            TargetLanguage: request.Language
        );

        var pdfResult = await _summaryService.GetSummaryFromVideoAsync(serviceRequest, cancellationToken);

        return File(
            fileStream: pdfResult.PdfStream,
            contentType: "application/pdf",
            fileDownloadName: $"{Path.GetFileNameWithoutExtension(request.File.FileName)}_summary.pdf"
        );
    }
}
