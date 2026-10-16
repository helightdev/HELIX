using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Helix.Weave;

/// <summary>
/// Reads the documentation a symbol already carries, so mixins can emit human-facing labels and
/// descriptions without the author repeating the text in an attribute. A member's description comes
/// from its own <c>&lt;summary&gt;</c>; a parameter's comes from the <c>&lt;param&gt;</c> entry on its
/// containing method.
/// </summary>
/// <remarks>
/// Everything here returns plain strings, so a prelude can read it and hand the result to a carry
/// record without the late pass ever touching a symbol.
/// </remarks>
public sealed class DocComments {
  private readonly ConditionalWeakTable<ISymbol, Entry> _entries = new();

  private sealed class Entry {
    public string Summary = "";
    public Dictionary<string, string> Parameters;
  }

  /// <summary>The symbol's own summary text, or an empty string when it has none.</summary>
  public string Summary(ISymbol symbol) => symbol is null ? "" : Read(symbol).Summary;

  /// <summary>
  /// The description for a parameter or field: its own summary when it has one, else the
  /// <c>&lt;param&gt;</c> entry its containing method documents for it.
  /// </summary>
  public string Describe(ISymbol symbol) {
    if (symbol is null) return "";
    var own = Read(symbol).Summary;
    if (own.Length > 0) return own;
    if (symbol is not IParameterSymbol parameter || parameter.ContainingSymbol is null) return "";
    var parameters = Read(parameter.ContainingSymbol).Parameters;
    return parameters != null && parameters.TryGetValue(parameter.Name, out var text) ? text : "";
  }

  private Entry Read(ISymbol symbol) {
    if (_entries.TryGetValue(symbol, out var cached)) return cached;
    // Roslyn only fills in the comment XML when the trees were parsed with documentation on, and
    // Unity compiles without /doc. For source symbols the trivia is there either way, so read it.
    var xml = symbol.GetDocumentationCommentXml(expandIncludes: false);
    var entry = Parse(string.IsNullOrWhiteSpace(xml) ? FromTrivia(symbol) : xml);
    _entries.Add(symbol, entry);
    return entry;
  }

  /// <summary>
  /// The symbol's documentation lines recovered from its leading trivia and wrapped the way
  /// <c>GetDocumentationCommentXml</c> would have returned them. With documentation parsing off the
  /// lines are ordinary comments, so they are matched by their <c>///</c> prefix rather than by kind.
  /// </summary>
  private static string FromTrivia(ISymbol symbol) {
    var references = symbol.DeclaringSyntaxReferences;
    if (references.Length == 0) return null;
    var node = references[0].GetSyntax();
    if (node is null) return null;
    // A field's declaring node is its variable declarator, and the comment sits on the declaration
    // above it. Nothing else is walked up: a parameter has no documentation of its own, and taking
    // its method's summary would describe every parameter with the method's text.
    if (node is VariableDeclaratorSyntax declarator)
      node = declarator.FirstAncestorOrSelf<MemberDeclarationSyntax>() ?? node;

    StringBuilder builder = null;
    foreach (var trivia in node.GetLeadingTrivia()) {
      if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
          trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)) {
        if (trivia.GetStructure() is DocumentationCommentTriviaSyntax structured)
          (builder ??= new StringBuilder()).Append(Strip(structured.ToFullString()));
        continue;
      }
      if (!trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)) continue;
      var text = trivia.ToString();
      if (!text.StartsWith("///", StringComparison.Ordinal)) continue;
      (builder ??= new StringBuilder()).Append(text, 3, text.Length - 3).Append('\n');
    }
    // A single <summary> element parses on its own, but several siblings need a root.
    return builder is null ? null : "<member>" + builder + "</member>";
  }

  /// <summary>Drops the <c>///</c> that opens each documentation line.</summary>
  private static string Strip(string text) {
    var builder = new StringBuilder(text.Length);
    foreach (var line in text.Split('\n')) {
      var trimmed = line.TrimStart();
      builder.Append(
        trimmed.StartsWith("///", StringComparison.Ordinal) ? trimmed.Substring(3) : line
      ).Append('\n');
    }
    return builder.ToString();
  }

  private static Entry Parse(string xml) {
    var entry = new Entry();
    if (string.IsNullOrWhiteSpace(xml)) return entry;
    XElement root;
    try {
      // Roslyn hands back a <member> wrapper; a malformed comment yields a <!--badly formed--> node.
      root = XElement.Parse(xml, LoadOptions.PreserveWhitespace);
    } catch (System.Xml.XmlException) {
      return entry;
    }

    var summary = root.Element("summary");
    if (summary != null) entry.Summary = Flatten(summary);

    foreach (var element in root.Elements("param")) {
      var name = element.Attribute("name")?.Value;
      if (string.IsNullOrEmpty(name)) continue;
      (entry.Parameters ??= new Dictionary<string, string>(StringComparer.Ordinal))[name] = Flatten(element);
    }
    return entry;
  }

  /// <summary>
  /// The element's text as one line. Inline tags contribute the text a reader would see:
  /// <c>&lt;see cref="X.Y"/&gt;</c> becomes <c>Y</c>, since the crefs are of no use in a label.
  /// </summary>
  private static string Flatten(XElement element) {
    var builder = new StringBuilder();
    Append(builder, element);
    // Collapse the runs of whitespace that line-leading /// and indentation leave behind.
    var text = builder.ToString();
    builder.Clear();
    var space = true;
    for (var i = 0; i < text.Length; i++) {
      var c = text[i];
      if (c is ' ' or '\t' or '\r' or '\n') {
        if (!space) builder.Append(' ');
        space = true;
        continue;
      }
      builder.Append(c);
      space = false;
    }
    return builder.ToString().Trim();
  }

  private static void Append(StringBuilder builder, XElement element) {
    foreach (var node in element.Nodes())
      switch (node) {
        case XText text:
          builder.Append(text.Value);
          break;
        case XElement child when child.Name == "see" || child.Name == "paramref" ||
                                 child.Name == "typeparamref":
          builder.Append(Reference(child));
          break;
        case XElement child:
          Append(builder, child);
          break;
      }
  }

  private static string Reference(XElement element) {
    var text = element.Value;
    if (!string.IsNullOrWhiteSpace(text)) return text;
    var target = element.Attribute("cref")?.Value ??
                 element.Attribute("name")?.Value ??
                 element.Attribute("langword")?.Value ?? "";
    // A cref is prefixed with its kind, as in "T:Some.Namespace.Thing".
    if (target.Length > 2 && target[1] == ':') target = target.Substring(2);
    var dot = target.LastIndexOf('.');
    return dot >= 0 && dot + 1 < target.Length ? target.Substring(dot + 1) : target;
  }
}
