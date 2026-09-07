namespace Summarix.Application.DTOs;

public record SummaryResponseDto(
    Stream PdfStream,
    string FileName,
    string ContentType = "application/pdf"
);