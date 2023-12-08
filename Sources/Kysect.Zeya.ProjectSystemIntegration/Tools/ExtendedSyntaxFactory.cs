using System;
using System.Collections.Generic;
using Microsoft.Language.Xml;

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

    public static XmlEmptyElementSyntax XmlEmptyElement(string name, IReadOnlyCollection<XmlAttributeSyntax> attributes)
    {
        return SyntaxFactory.XmlEmptyElement(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanToken, "<", SyntaxFactory.WhitespaceTrivia(Environment.NewLine + "    "), null),
            XmlName(name),
            SyntaxFactory.List(attributes),
            SyntaxFactory.Punctuation(SyntaxKind.SlashGreaterThanToken, "/>", SyntaxFactory.WhitespaceTrivia(" "), null));
    }
}