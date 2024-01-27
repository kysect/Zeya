﻿using Microsoft.Language.Xml;
using System;
using System.Text;

namespace Kysect.Zeya.ProjectSystemIntegration.Tools;

// TODO: move all this code to lib
public static class ExtendedSyntaxFactory2
{
    public const string DefaultIndention = "  ";

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

    public static SyntaxTrivia XmlWhiteIndention(int depth)
    {
        var sb = new StringBuilder(Environment.NewLine);

        for (int i = 0; i < depth; i++)
            sb = sb.Append(DefaultIndention);

        return SyntaxFactory.WhitespaceTrivia(sb.ToString());
    }
}