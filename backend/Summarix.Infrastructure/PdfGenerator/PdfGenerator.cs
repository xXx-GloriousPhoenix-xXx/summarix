using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Summarix.Application.Interfaces;

namespace Summarix.Infrastructure.PdfGenerator;

public class PdfGenerator(IOptions<PdfSettings> settings) : IPdfGenerator
{
    private readonly PdfSettings _settings = settings.Value;

    static PdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<Stream> GetPdfFromTxtAsync(
        string title,
        string summaryText,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var primaryColor = Color.FromHex(_settings.PrimaryColorHex);
        var textColor = Color.FromHex(_settings.TextColorHex);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(_settings.PageMarginCm, Unit.Centimetre);
                page.PageColor(Colors.White);

                page.DefaultTextStyle(x => x
                    .FontSize(_settings.DefaultFontSize)
                    .FontColor(textColor)
                    .LineHeight(_settings.LineHeight));

                page.Header()
                    .Column(col =>
                    {
                        col.Item().Text("Summarix — Video Summary")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Medium)
                            .SemiBold();

                        col.Item().PaddingTop(5).Text(title)
                            .FontSize(_settings.TitleFontSize)
                            .Bold()
                            .FontColor(primaryColor);

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Column(col =>
                    {
                        var lines = summaryText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

                        foreach (var line in lines)
                        {
                            var trimmed = line.Trim();

                            if (string.IsNullOrWhiteSpace(trimmed))
                            {
                                col.Item().PaddingVertical(4);
                                continue;
                            }

                            if (trimmed.StartsWith('#'))
                            {
                                col.Item().PaddingTop(8).Text(trimmed.TrimStart('#', ' '))
                                    .FontSize(_settings.HeaderFontSize)
                                    .Bold()
                                    .FontColor(textColor);
                            }
                            else if (trimmed.StartsWith('-') || trimmed.StartsWith('*'))
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(15).Text("•").Bold().FontColor(primaryColor);
                                    row.RelativeItem().Text(trimmed.TrimStart('-', '*', ' '));
                                });
                            }
                            else
                            {
                                col.Item().Text(trimmed);
                            }
                        }
                    });

                page.Footer()
                    .AlignRight()
                    .Text(text =>
                    {
                        if (_settings.ShowFooterDate)
                        {
                            text.Span($"Generated on {DateTime.UtcNow:yyyy-MM-dd} | ");
                        }
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        });

        var stream = new MemoryStream();
        document.GeneratePdf(stream);
        stream.Position = 0;

        return Task.FromResult<Stream>(stream);
    }
}
