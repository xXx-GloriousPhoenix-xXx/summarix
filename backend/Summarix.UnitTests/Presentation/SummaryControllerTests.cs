using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Summarix.Application.DTOs;
using Summarix.Application.Interfaces;
using Summarix.Presentation.Contracts;
using Summarix.Presentation.Controllers;
using System.Text;

namespace Summarix.UnitTests.Presentation;

public class SummaryControllerTests
{
    private readonly Mock<ISummaryService> _summaryServiceMock;
    private readonly SummaryController _controller;

    public SummaryControllerTests()
    {
        _summaryServiceMock = new Mock<ISummaryService>();
        _controller = new SummaryController(_summaryServiceMock.Object);
    }

    [Fact]
    public async Task GenerateSummaryPdfAsync_WithValidFile_ShouldReturnFileStreamResult()
    {
        // Arrange
        var content = "dummy-video-binary";
        var fileMock = CreateMockFormFile("video.mp4", "video/mp4", content);
        var request = new GenerateSummaryRequest { File = fileMock.Object, Language = "en" };

        var generatedPdfStream = new MemoryStream(Encoding.UTF8.GetBytes("PDF-DATA"));
        var responseDto = new SummaryResponseDto(generatedPdfStream, "video_summary.pdf");

        _summaryServiceMock
            .Setup(x => x.GetSummaryFromVideoAsync(It.IsAny<SummaryRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _controller.GenerateSummaryPdfAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var fileResult = result.Should().BeOfType<FileStreamResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("video_summary.pdf");
        fileResult.FileStream.Should().BeSameAs(generatedPdfStream);
    }

    [Fact]
    public async Task GenerateSummaryPdfAsync_WithEmptyFile_ShouldReturnBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        var request = new GenerateSummaryRequest { File = fileMock.Object };

        // Act
        var result = await _controller.GenerateSummaryPdfAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("A valid video file is required.");

        _summaryServiceMock.Verify(x => x.GetSummaryFromVideoAsync(It.IsAny<SummaryRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Mock<IFormFile> CreateMockFormFile(string fileName, string contentType, string content)
    {
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        return fileMock;
    }
}
