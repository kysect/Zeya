using FluentAssertions;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.Tests;

public class ExtendedSyntaxFactoryTests
{
    [Test]
    public void CreateXmlElement_ReturnExpectedResul()
    {
        var expected = $"""

                        {'\t'}<Name>
                        {'\t'}</Name>
                        """;
        var result = ExtendedSyntaxFactory.XmlElement("Name", 1);


        result.ToFullString().Should().Be(expected);
    }

    [Test]
    public void PropertyGroupParameter_ReturnExpectedResult()
    {
        var expected = $"""

                        {'\t'}<Name>
                        {'\t'}{'\t'}<Key>Value</Key>
                        {'\t'}</Name>
                        """;

        var result = ExtendedSyntaxFactory.XmlElement("Name", 1).AddChild(ExtendedSyntaxFactory.PropertyGroupParameter("Key", "Value"));
        
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