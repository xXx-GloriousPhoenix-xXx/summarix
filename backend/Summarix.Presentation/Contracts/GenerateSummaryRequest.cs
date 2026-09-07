namespace Summarix.Presentation.Contracts;

public class GenerateSummaryRequest
{
    public required IFormFile File { get; set; }
    public string? Language { get; set; } = "en";
}
