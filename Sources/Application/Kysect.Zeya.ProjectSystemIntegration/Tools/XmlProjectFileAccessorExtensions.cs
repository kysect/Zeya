using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Parsers;

namespace Kysect.Zeya.ProjectSystemIntegration.Tools;

public static class XmlProjectFileAccessorExtensions
{
    public static bool IsEmpty(this XmlProjectFileAccessor document)
    {
        document.ThrowIfNull();

        return document.Document.RootSyntax is not null;
    }
}