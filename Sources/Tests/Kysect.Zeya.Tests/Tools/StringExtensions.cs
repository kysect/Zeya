using Kysect.CommonLib.BaseTypes.Extensions;

namespace Kysect.Zeya.Tests.Tools;

public static class StringExtensions
{
    public static string NormalizeEndLines(this string value)
    {
        value.ThrowIfNull();

        return value
            .Replace("\r\n", "\n")
            .Replace("\n", Environment.NewLine);
    }
}