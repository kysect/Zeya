using FluentAssertions;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.Tests;

public class ExtendedSyntaxFactory2Tests
{
    [Test]
    public void XmlTextLiteralToken_ReturnSameValueAsArgument()
    {
        const string value = "Value";

        var xmlTextLiteralToken = SyntaxFactory.XmlTextLiteralToken(value, null, null);

        xmlTextLiteralToken.ToFullString().Should().Be(value);
    }
}