namespace Summarix.Domain.Exceptions;

public class InvalidMediaAudioException : Exception
{
    public InvalidMediaAudioException(string message = "The uploaded file does not contain a valid audio track for transcription.")
        : base(message)
    {
    }

    public InvalidMediaAudioException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
