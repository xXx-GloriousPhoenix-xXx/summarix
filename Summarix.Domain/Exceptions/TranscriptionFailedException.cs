namespace Summarix.Domain.Exceptions;

public class TranscriptionFailedException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}