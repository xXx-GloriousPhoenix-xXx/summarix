using Summarix.Domain.Exceptions;

namespace Summarix.Domain.Models;

public class VideoSource
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mp3", ".wav", ".m4a", ".ogg", ".flac", ".aac"
    };

    public string FileName { get; }
    public string Extension { get; }

    public VideoSource(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidVideoSourceException("File name cannot be empty.");
        }

        FileName = fileName;
        Extension = Path.GetExtension(fileName);

        if (!AllowedExtensions.Contains(Extension))
        {
            throw new InvalidVideoSourceException($"Unsupported file format '{Extension}'.");
        }
    }
}
