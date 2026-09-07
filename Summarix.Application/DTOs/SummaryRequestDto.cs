namespace Summarix.Application.DTOs;

public record SummaryRequestDto(
    Stream VideoStream,
    string FileName,
    string ContentType,
    string? TargetLanguage = "en"
    );