using Microsoft.Language.Xml;
using System;
using System.Text;

namespace Kysect.Zeya.ProjectSystemIntegration.Tools;

public static class ExtendedSyntaxFactory
{
    public static XmlNameSyntax XmlName(string name)
    {
        return SyntaxFactory.XmlName(null, SyntaxFactory.XmlNameToken(name, null, null));
    }

    public static XmlStringSyntax XmlString(string name)
    {
        return SyntaxFactory.XmlString(
            SyntaxFactory.Punctuation(SyntaxKind.DoubleQuoteToken, "\"", null, null),
            XmlName(name),
            SyntaxFactory.Punctuation(SyntaxKind.DoubleQuoteToken, "\"", null, null));
    }

    public static XmlAttributeSyntax XmlAttribute(string key, string value)
    {
        return SyntaxFactory.XmlAttribute(
            SyntaxFactory.XmlName(null, SyntaxFactory.XmlNameToken(key, SyntaxFactory.WhitespaceTrivia(" "), null)),
            SyntaxFactory.Punctuation(SyntaxKind.EqualsToken, "=", null, null),
            XmlString(value));
    }

    public static XmlEmptyElementSyntax XmlEmptyElement(string name, int depth)
    {
        return SyntaxFactory.XmlEmptyElement(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanToken, "<", XmlWhiteIndention(depth), null),
            XmlName(name),
            SyntaxFactory.List<XmlAttributeSyntax>(),
            SyntaxFactory.Punctuation(SyntaxKind.SlashGreaterThanToken, "/>", SyntaxFactory.WhitespaceTrivia(" "), null));
    }

    public static XmlElementSyntax XmlElement(string name, int depth)
    {
        var startTagSyntax = SyntaxFactory.XmlElementStartTag(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanToken, "<", XmlWhiteIndention(depth), null),
            XmlName(name),
            SyntaxFactory.List<XmlAttributeSyntax>(),
            SyntaxFactory.Punctuation(SyntaxKind.GreaterThanToken, ">", SyntaxFactory.WhitespaceTrivia(string.Empty), null)
        );

        var endTagSyntax = SyntaxFactory.XmlElementEndTag(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanSlashToken, "</", XmlWhiteIndention(depth), null),
            XmlName(name),
            SyntaxFactory.Punctuation(SyntaxKind.GreaterThanToken, ">", SyntaxFactory.WhitespaceTrivia(string.Empty), null)
        );


        return SyntaxFactory.XmlElement(startTagSyntax, SyntaxFactory.List<SyntaxNode>(), endTagSyntax);
    }

    public static XmlElementSyntax PropertyGroupParameter(string key, string value)
    {
        var startTagSyntax = SyntaxFactory.XmlElementStartTag(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanToken, "<", XmlWhiteIndention(2), null),
            XmlName(key),
            SyntaxFactory.List<XmlAttributeSyntax>(),
            SyntaxFactory.Punctuation(SyntaxKind.GreaterThanToken, ">", null, null)
        );

        var endTagSyntax = SyntaxFactory.XmlElementEndTag(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanSlashToken, "</", null, null),
            XmlName(key),
            SyntaxFactory.Punctuation(SyntaxKind.GreaterThanToken, ">", null, null)
        );

        return SyntaxFactory.XmlElement(startTagSyntax, SyntaxFactory.List<SyntaxNode>(SyntaxFactory.XmlText(SyntaxFactory.XmlTextLiteralToken(value, null, null))), endTagSyntax);
    }

    // TODO: specify indention
    public static SyntaxTrivia XmlWhiteIndention(int depth)
    {
        var sb = new StringBuilder(Environment.NewLine);

        for (var i = 0; i < depth; i++)
            sb = sb.Append('\t');

        return SyntaxFactory.WhitespaceTrivia(sb.ToString());
    }
}