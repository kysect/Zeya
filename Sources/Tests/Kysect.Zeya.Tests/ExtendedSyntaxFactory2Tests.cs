using FluentAssertions;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.Tests;

public class ExtendedSyntaxFactory2Tests
{
    [Test]
    public void CreateXmlElement_ReturnExpectedResul()
    {
        var expected = $"""

                          <Name>
                          </Name>
                        """;
        var result = ExtendedSyntaxFactory2.XmlElement("Name", 1);


        result.ToFullString().Should().Be(expected);
    }

    [Test]
    public void PropertyGroupParameter_ReturnExpectedResult()
    {
        var expected = $"""

                          <Name>
                            <Key>Value</Key>
                          </Name>
                        """;

        var result = ExtendedSyntaxFactory2.XmlElement("Name", 1).AddChild(ExtendedSyntaxFactory2.PropertyGroupParameter("Key", "Value"));

        result.ToFullString().Should().Be(expected);
    }

    [Test]
    public void XmlTextLiteralToken_ReturnSameValueAsArgument()
    {
        const string value = "Value";

        var xmlTextLiteralToken = SyntaxFactory.XmlTextLiteralToken(value, null, null);

        xmlTextLiteralToken.ToFullString().Should().Be(value);
    }
}