using FluentAssertions;
using Moq;
using Summarix.Application.DTOs;
using Summarix.Application.Interfaces;
using Summarix.Application.Services;
using Summarix.Domain.Exceptions;

namespace Summarix.UnitTests.Application;

public class SummaryServiceTests
{
    private readonly Mock<ITextifyClient> _textifyClientMock;
    private readonly Mock<IOllamaClient> _ollamaClientMock;
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock;
    private readonly SummaryService _sut;

    public SummaryServiceTests()
    {
        _textifyClientMock = new Mock<ITextifyClient>();
        _ollamaClientMock = new Mock<IOllamaClient>();
        _pdfGeneratorMock = new Mock<IPdfGenerator>();

        _sut = new SummaryService(
            _textifyClientMock.Object,
            _ollamaClientMock.Object,
            _pdfGeneratorMock.Object);
    }

    [Fact]
    public async Task GetSummaryFromVideoAsync_WhenWorkflowSucceeds_ShouldReturnValidResponse()
    {
        // Arrange
        using var inputStream = new MemoryStream([1, 2, 3]);
        using var outputPdfStream = new MemoryStream([4, 5, 6]);

        var request = new SummaryRequestDto(
            VideoStream: inputStream,
            FileName: "lecture.mp4",
            ContentType: "video/mp4",
            TargetLanguage: "en"
        );

        _textifyClientMock
            .Setup(x => x.GetTextFromVideoAsync(inputStream, "lecture.mp4", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync("This is a transcribed video lecture.");

        _ollamaClientMock
            .Setup(x => x.GetSummaryFromTextAsync("This is a transcribed video lecture.", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("• Key takeaway 1\n• Key takeaway 2");

        _pdfGeneratorMock
            .Setup(x => x.GetPdfFromTxtAsync("lecture", "• Key takeaway 1\n• Key takeaway 2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(outputPdfStream);

        // Act
        var result = await _sut.GetSummaryFromVideoAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("lecture_summary.pdf");
        result.PdfStream.Should().BeSameAs(outputPdfStream);

        _textifyClientMock.Verify(x => x.GetTextFromVideoAsync(inputStream, "lecture.mp4", "en", It.IsAny<CancellationToken>()), Times.Once);
        _ollamaClientMock.Verify(x => x.GetSummaryFromTextAsync("This is a transcribed video lecture.", null, It.IsAny<CancellationToken>()), Times.Once);
        _pdfGeneratorMock.Verify(x => x.GetPdfFromTxtAsync("lecture", "• Key takeaway 1\n• Key takeaway 2", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSummaryFromVideoAsync_WhenTranscriptionReturnsEmpty_ShouldThrowTranscriptionFailedException()
    {
        // Arrange
        using var inputStream = new MemoryStream([1]);
        var request = new SummaryRequestDto(inputStream, "meeting.mp4", "video/mp4", "en");

        _textifyClientMock
            .Setup(x => x.GetTextFromVideoAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        // Act
        Func<Task> act = async () => await _sut.GetSummaryFromVideoAsync(request);

        // Assert
        await act.Should().ThrowAsync<TranscriptionFailedException>()
            .WithMessage("Transcription service returned empty text.");

        _ollamaClientMock.Verify(x => x.GetSummaryFromTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _pdfGeneratorMock.Verify(x => x.GetPdfFromTxtAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSummaryFromVideoAsync_WhenSummarizationReturnsEmpty_ShouldThrowSummarizationFailedException()
    {
        // Arrange
        using var inputStream = new MemoryStream([1]);
        var request = new SummaryRequestDto(inputStream, "meeting.mp4", "video/mp4", "en");

        _textifyClientMock
            .Setup(x => x.GetTextFromVideoAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Some speech");

        _ollamaClientMock
            .Setup(x => x.GetSummaryFromTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("   ");

        // Act
        Func<Task> act = async () => await _sut.GetSummaryFromVideoAsync(request);

        // Assert
        await act.Should().ThrowAsync<SummarizationFailedException>()
            .WithMessage("Summarization service returned empty text.");

        _pdfGeneratorMock.Verify(x => x.GetPdfFromTxtAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
