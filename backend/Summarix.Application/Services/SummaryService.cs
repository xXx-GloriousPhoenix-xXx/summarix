using Summarix.Application.DTOs;
using Summarix.Application.Interfaces;
using Summarix.Domain.Exceptions;
using Summarix.Domain.Models;

namespace Summarix.Application.Services;

public class SummaryService(ITextifyClient textifyClient, IOllamaClient ollamaClient, IPdfGenerator pdfGenerator) : ISummaryService
{
private readonly ITextifyClient _textifyClient = textifyClient;
private readonly IOllamaClient _ollamaClient = ollamaClient;
private readonly IPdfGenerator _pdfGenerator = pdfGenerator;

    public async Task<SummaryResponseDto> GetSummaryFromVideoAsync(
        SummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var videoSource = new VideoSource(request.FileName);

        if (request.VideoStream is null || request.VideoStream.Length == 0)
        {
            throw new InvalidVideoSourceException("Video stream is empty or inaccessible.");
        }

        var transcriptText = await _textifyClient.GetTextFromVideoAsync(
            request.VideoStream,
            request.FileName,
            request.TargetLanguage,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(transcriptText))
        {
            throw new TranscriptionFailedException("Transcription service returned empty text.");
        }

        var summaryText = await _ollamaClient.GetSummaryFromTextAsync(
            transcriptText,
            promptInstruction: null,
            cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(summaryText))
        {
            throw new SummarizationFailedException("Summarization service returned empty text.");
        }

        var documentTitle = Path.GetFileNameWithoutExtension(videoSource.FileName);
        var pdfStream = await _pdfGenerator.GetPdfFromTxtAsync(
            title: documentTitle,
            summaryText: summaryText,
            cancellationToken: cancellationToken);

        var outputFileName = $"{documentTitle}_summary.pdf";

        return new SummaryResponseDto(
            PdfStream: pdfStream,
            FileName: outputFileName
        );
    }
}
