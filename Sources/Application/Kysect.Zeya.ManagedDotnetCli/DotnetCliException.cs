using System;

namespace Kysect.Zeya.ManagedDotnetCli;

public class DotnetCliException : Exception
{
    public DotnetCliException(string message) : base(message)
    {
    }

    public DotnetCliException() : base("Failed to execute cmd command")
    {
    }

    public DotnetCliException(string message, Exception innerException) : base(message, innerException)
    {
    }
}