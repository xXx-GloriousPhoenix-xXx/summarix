namespace Summarix.Domain.Exceptions;

public class SummarizationFailedException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}