namespace Kysect.Zeya.Common;

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