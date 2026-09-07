namespace Summarix.Infrastructure.Textify;

public class TextifySettings
{
    public const string SectionName = "Textify";

    public string BaseUrl { get; set; } = "http://localhost:11000";
    public string Model { get; set; } = "base";
    public string DefaultLanguage { get; set; } = "en";
    public int TimeoutMinutes { get; set; } = 10;
}
