namespace Kysect.Zeya.Abstractions;

public class ZeyaException : Exception
{
    public ZeyaException()
    {
    }

    public ZeyaException(string? message) : base(message)
    {
    }

    public ZeyaException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}