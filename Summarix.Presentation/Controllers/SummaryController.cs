using Microsoft.AspNetCore.Mvc;
using Summarix.Application.DTOs;
using Summarix.Application.Interfaces;

namespace Summarix.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummaryController(ISummaryService summaryService) : ControllerBase
{
    private readonly ISummaryService _summaryService = summaryService;

    [HttpPost("generate")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateSummaryPdfAsync(
        [FromForm] IFormFile file,
        [FromQuery] string? language = "en",
        CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            return BadRequest("A valid video file is required.");
        }

        await using var videoStream = file.OpenReadStream();

        var request = new SummaryRequestDto(
            VideoStream: videoStream,
            FileName: file.FileName,
            ContentType: file.ContentType,
            TargetLanguage: language
        );

        var pdfResult = await this._summaryService.GetSummaryFromVideoAsync(request, cancellationToken);

        return File(
            fileStream: pdfResult.PdfStream,
            contentType: "application/pdf",
            fileDownloadName: $"{Path.GetFileNameWithoutExtension(file.FileName)}_summary.pdf"
        );
    }
}
