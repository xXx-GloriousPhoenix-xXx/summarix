namespace Summarix.Infrastructure.PdfGenerator;

public class PdfSettings
{
    public const string SectionName = "Pdf";

    public float PageMarginCm { get; set; } = 2.0f;
    public float DefaultFontSize { get; set; } = 11f;
    public float TitleFontSize { get; set; } = 20f;
    public float HeaderFontSize { get; set; } = 14f;
    public float LineHeight { get; set; } = 1.4f;
    public string PrimaryColorHex { get; set; } = "#1565C0";
    public string TextColorHex { get; set; } = "#37474F";
    public bool ShowFooterDate { get; set; } = true;
}
