using FluentAssertions;
using Summarix.Domain.Exceptions;
using Summarix.Domain.Models;

namespace Summarix.UnitTests.Domain;

public class VideoSourceTests
{
    [Theory]
    [InlineData("video.mp4", ".mp4")]
    [InlineData("sample.mkv", ".mkv")]
    [InlineData("recording.mov", ".mov")]
    [InlineData("audio.mp3", ".mp3")]
    public void Constructor_WithValidExtension_ShouldCreateInstance(string fileName, string expectedExt)
    {
        // Act
        var result = new VideoSource(fileName);

        // Assert
        result.FileName.Should().Be(fileName);
        result.Extension.Should().BeEquivalentTo(expectedExt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyFileName_ShouldThrowInvalidVideoSourceException(string? invalidName)
    {
        // Act
        Action act = () =>
        {
            VideoSource videoSource = new(invalidName!);
        };

        // Assert
        act.Should().Throw<InvalidVideoSourceException>()
            .WithMessage("File name cannot be empty.");
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("image.png")]
    [InlineData("executable.exe")]
    public void Constructor_WithUnsupportedExtension_ShouldThrowInvalidVideoSourceException(string invalidFile)
    {
        // Act
        Action act = () =>
        {
            VideoSource videoSource = new(invalidFile);
        };

        // Assert
        act.Should().Throw<InvalidVideoSourceException>()
            .WithMessage($"Unsupported file format '{Path.GetExtension(invalidFile)}'.");
    }
}
