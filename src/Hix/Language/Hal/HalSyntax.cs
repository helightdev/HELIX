using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Hix.Hal.Generated;

namespace Hix.Hal;

public sealed record HalDiagnostic(int Line, int Column, int Start, int Length, string Message);
public sealed record HalMetadataSyntax(string Name, string Text);
public sealed record HalDocumentSyntax(IReadOnlyList<HalMetadataSyntax> Metadata,
  IReadOnlyList<HalSectionSyntax> Sections, IReadOnlyList<HalDiagnostic> Diagnostics) {
  /// Optional persistent asset identity, declared with %uid("...") before the primary section.
  public string Uid { get; init; }
  public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
  public static HalDocumentSyntax Parse(string source) => HalSyntax.Parse(source ?? string.Empty);
  public static string Format(string source, int indentSize = 2) => HalFormatter.Format(source, indentSize);
}
public sealed record HalSectionSyntax(string Type, string Id, IReadOnlyList<HalFieldSyntax> Fields,
  IReadOnlyList<HalMetadataSyntax> Metadata) {
  public HalListSyntax List { get; init; }
  public HalValueSyntax Value => List ?? (HalValueSyntax)new HalTableSyntax(Fields);
}
public sealed record HalFieldSyntax(string Name, HalValueSyntax Value, IReadOnlyList<HalMetadataSyntax> Metadata);
public abstract record HalValueSyntax;
public sealed record HalScalarSyntax(object Value) : HalValueSyntax;
public sealed record HalListSyntax(IReadOnlyList<HalValueSyntax> Values) : HalValueSyntax;
public sealed record HalTableSyntax(IReadOnlyList<HalFieldSyntax> Fields) : HalValueSyntax;
public sealed record HalTypedSyntax(string Type, HalValueSyntax Value) : HalValueSyntax;
public sealed record HalCallSyntax(string Name, IReadOnlyList<HalArgumentSyntax> Arguments) : HalValueSyntax;
public sealed record HalArgumentSyntax(string Name, HalValueSyntax Value);
public sealed record HalSelectionSyntax(HalValueSyntax Receiver, IReadOnlyList<string> Members) : HalValueSyntax;

/// Foreign asset references are explicit selectors, independent from local ref(section) calls.
public sealed record HalAssetReference(string Kind, string Value, string Section = null) {
  public static HalAssetReference Parse(HalCallSyntax call) {
    if (call.Name != "asset" || call.Arguments.Count is < 1 or > 2 || call.Arguments.Any(a => a.Name != null) ||
        call.Arguments[0].Value is not HalCallSyntax selector || selector.Name is not ("path" or "uid") ||
        selector.Arguments.Count != 1 || selector.Arguments[0].Name != null ||
        selector.Arguments[0].Value is not HalScalarSyntax {Value:string value} || string.IsNullOrWhiteSpace(value))
      throw new ArgumentException("Expected asset(path(\"file.hal\")) or asset(uid(\"asset-id\")), optionally followed by a section argument");
    string section = null;
    if (call.Arguments.Count == 2) {
      if (call.Arguments[1].Value is not HalScalarSyntax {Value: { } id} || id is bool)
        throw new ArgumentException("Asset section must be a string or number");
      section = Convert.ToString(id, CultureInfo.InvariantCulture);
    }
    return new(selector.Name, value, section);
  }
}

public static class HalFormatter {
  public static string Format(string source, int indentSize = 2) {
    source ??= string.Empty;
    if (indentSize < 0) throw new ArgumentOutOfRangeException(nameof(indentSize));
    var lexer = new HalLexer(new AntlrInputStream(source));
    var tokens = lexer.GetAllTokens();
    var result = new StringBuilder();
    var depth = 0; var pendingLines = 0; IToken previous = null;
    var lineHasMetadata = false; var pendingAfterMetadata = false;
    foreach (var token in tokens) {
      if (token.Type == HalLexer.SPACE) continue;
      if (token.Type == HalLexer.NEWLINE) {
        if (pendingLines == 0) pendingAfterMetadata = lineHasMetadata;
        lineHasMetadata = false;
        pendingLines = Math.Min(2, pendingLines + 1);
        continue;
      }
      var closing = token.Type is HalLexer.RBRACE or HalLexer.RBRACKET or HalLexer.RPAREN;
      if (closing) depth = Math.Max(0, depth - 1);
      if (token.Type == HalLexer.SECTION) depth = 0;
      if (pendingLines != 0) {
        while (result.Length != 0 && result[result.Length - 1] == ' ') result.Length--;
        var lines = token.Type == HalLexer.SECTION && result.Length != 0
          ? pendingAfterMetadata ? 1 : Math.Max(2, pendingLines)
          : pendingLines;
        result.Append('\n', lines);
        result.Append(' ', depth * indentSize);
        pendingLines = 0; previous = null;
      } else if (result.Length == 0 || result[result.Length - 1] == '\n') {
        result.Append(' ', depth * indentSize);
      }
      if (token.Type == HalLexer.LINE_COMMENT) {
        if (result.Length != 0 && result[result.Length - 1] is not (' ' or '\n')) result.Append(' ');
        result.Append(token.Text.TrimEnd()); pendingLines = 1; previous = token; continue;
      }
      if (token.Type == HalLexer.PERCENT) lineHasMetadata = true;
      if (NeedsSpace(previous, token, result)) result.Append(' ');
      if (token.Type == HalLexer.COMMA && result.Length != 0 && result[result.Length - 1] == ' ') result.Length--;
      result.Append(token.Text);
      if (token.Type is HalLexer.LBRACE or HalLexer.LBRACKET or HalLexer.LPAREN) depth++;
      previous = token;
    }
    while (result.Length != 0 && char.IsWhiteSpace(result[result.Length - 1])) result.Length--;
    return result.Length == 0 ? string.Empty : result.Append('\n').ToString();
  }

  private static bool NeedsSpace(IToken previous, IToken current, StringBuilder result) {
    if (previous == null || result.Length == 0 || result[result.Length - 1] is ' ' or '\n') return false;
    if (current.Type is HalLexer.COMMA or HalLexer.RBRACE or HalLexer.RBRACKET or HalLexer.RPAREN or
        HalLexer.RANGLE or HalLexer.HASH) return false;
    if (previous.Type is HalLexer.PERCENT or HalLexer.HASH or HalLexer.LANGLE or HalLexer.LBRACE or
        HalLexer.LBRACKET or HalLexer.LPAREN) return false;
    if (current.Type == HalLexer.LPAREN && previous.Type == HalLexer.IDENTIFIER) return false;
    if (current.Type == HalLexer.LANGLE && previous.Type == HalLexer.IDENTIFIER) return false;
    if (current.Type == HalLexer.EQUALS || previous.Type == HalLexer.EQUALS) return true;
    if (current.Type == HalLexer.AMP) return true;
    if (previous.Type == HalLexer.AMP) return false;
    if (previous.Type == HalLexer.SECTION || previous.Type == HalLexer.COMMA) return true;
    return true;
  }
}

internal static class HalSyntax {
  public static HalDocumentSyntax Parse(string source) {
    var diagnostics = new List<HalDiagnostic>();
    var listener = new Errors(diagnostics);
    var lexer = new HalLexer(new AntlrInputStream(source));
    lexer.RemoveErrorListeners(); lexer.AddErrorListener(listener);
    var parser = new HalParser(new CommonTokenStream(lexer));
    parser.RemoveErrorListeners(); parser.AddErrorListener(listener);
    var document = parser.document();
    var blocks = document.sectionBlock();
    var sections = blocks.Select((block, index) => Section(block, index)).ToArray();
    var primary = blocks.FirstOrDefault()?.metadata().Where(m => m.IDENTIFIER()?.GetText() == "uid").ToArray() ?? [];
    string uid = null;
    foreach (var metadata in AllMetadata(document).Where(m => m.IDENTIFIER()?.GetText() == "uid")) {
      string failure = null;
      if (!primary.Contains(metadata)) failure = "uid metadata belongs before the primary asset section";
      else if (primary.Length != 1) failure = "an asset may declare only one uid";
      else {
        var arguments = metadata.metadataArguments();
        var positional = arguments?.argumentList()?.argument();
        if (positional is {Length: 1} && positional[0].EQUALS() == null && Value(positional[0].value()) is HalScalarSyntax {Value:string text}) uid = text;
        else if (arguments?.LANGLE() != null && arguments.metadataArgument().Length == 1 && arguments.metadataArgument()[0].scalarAtom() is { } atom && (atom.STRING() != null || atom.IDENTIFIER() != null || atom.BARE() != null)) uid = AtomText(atom);
        else failure = "uid requires one string argument: %uid(\"asset-id\")";
        if (failure == null && string.IsNullOrWhiteSpace(uid)) failure = "asset uid must not be empty";
      }
      if (failure != null) diagnostics.Add(new(metadata.Start.Line, metadata.Start.Column, metadata.Start.StartIndex, metadata.Stop.StopIndex - metadata.Start.StartIndex + 1, failure));
    }
    var tags = new List<string>();
    var primaryMetadata = blocks.FirstOrDefault()?.metadata() ?? [];
    foreach (var metadata in AllMetadata(document).Where(m => m.IDENTIFIER()?.GetText() == "tag")) {
      var arguments = metadata.metadataArguments();
      var positional = arguments?.argumentList()?.argument();
      string tag = null;
      if (positional is {Length: 1} && positional[0].EQUALS() == null && Value(positional[0].value()) is HalScalarSyntax {Value:string text}) tag = text;
      else if (arguments?.LANGLE() != null && arguments.metadataArgument().Length == 1 && arguments.metadataArgument()[0].scalarAtom() is { } atom && (atom.STRING() != null || atom.IDENTIFIER() != null || atom.BARE() != null)) tag = AtomText(atom);
      var failure = !primaryMetadata.Contains(metadata) ? "tag metadata belongs before the primary asset section" :
        string.IsNullOrWhiteSpace(tag) ? "tag requires one nonempty string argument: %tag(\"name\")" : null;
      if (failure != null) diagnostics.Add(new(metadata.Start.Line, metadata.Start.Column, metadata.Start.StartIndex, metadata.Stop.StopIndex - metadata.Start.StartIndex + 1, failure));
      else if (!tags.Contains(tag)) tags.Add(tag);
    }
    return new HalDocumentSyntax(blocks.FirstOrDefault()?.metadata().Select(Metadata).ToArray() ?? [], sections, diagnostics) { Uid = uid, Tags = tags };
  }

  private static IEnumerable<HalParser.MetadataContext> AllMetadata(ParserRuleContext context) {
    if (context is HalParser.MetadataContext metadata) yield return metadata;
    foreach (var child in context.children?.OfType<ParserRuleContext>() ?? [])
      foreach (var nested in AllMetadata(child)) yield return nested;
  }

  private static HalSectionSyntax Section(HalParser.SectionBlockContext block, int index) {
    var context = block.section();
    return new HalSectionSyntax(context.IDENTIFIER().GetText(),
      context.scalarAtom() == null ? (index == 0 ? "0" : null) : AtomText(context.scalarAtom()),
      context.sectionEntry().Select(entry => Field(entry.field(), entry.metadata())).ToArray(),
      block.metadata().Select(Metadata).ToArray()) { List = context.list() == null ? null : List(context.list()) };
  }

  private static HalFieldSyntax Field(HalParser.FieldContext context, IEnumerable<HalParser.MetadataContext> metadata) =>
    new(FieldName(context.fieldKey()), context.value() != null ? Value(context.value()) : Collection(context.collectionValue()),
      metadata.Select(Metadata).ToArray());

  private static HalFieldSyntax Field(HalParser.TableEntryContext context) =>
    new(FieldName(context.fieldKey()), context.value() != null ? Value(context.value()) : Collection(context.collectionValue()),
      context.metadata().Select(Metadata).ToArray());

  private static HalMetadataSyntax Metadata(HalParser.MetadataContext context) =>
    new(context.IDENTIFIER().GetText(), context.metadataArguments()?.GetText());

  private static HalValueSyntax Value(HalParser.ValueContext context) {
    HalValueSyntax result;
    if (context.typedContainer() != null) {
      var container = context.typedContainer();
      result = new HalTypedSyntax(context.IDENTIFIER().GetText(),
        container.table() != null ? Table(container.table()) : List(container.list()));
    } else if (context.call() != null) result = Call(context.call());
    else if (context.table() != null) result = Table(context.table());
    else if (context.list() != null) result = List(context.list());
    else result = Scalar(context.looseScalar());
    var members = context.selection().Select(item => item.IDENTIFIER().GetText()).ToArray();
    return members.Length == 0 ? result : new HalSelectionSyntax(result, members);
  }

  private static HalValueSyntax Collection(HalParser.CollectionValueContext context) {
    HalValueSyntax value = context.typedContainer().table() != null
      ? Table(context.typedContainer().table()) : List(context.typedContainer().list());
    return context.IDENTIFIER() == null ? value : new HalTypedSyntax(context.IDENTIFIER().GetText(), value);
  }

  private static HalTableSyntax Table(HalParser.TableContext context) =>
    new(context.tableEntry().Select(Field).ToArray());
  private static HalListSyntax List(HalParser.ListContext context) =>
    new(context.value().Select(Value).ToArray());
  private static HalCallSyntax Call(HalParser.CallContext context) => new(context.IDENTIFIER().GetText(),
    context.argumentList()?.argument().Select(argument => new HalArgumentSyntax(
      argument.EQUALS() == null ? null : argument.IDENTIFIER().GetText(), Value(argument.value()))).ToArray() ?? []);

  private static HalScalarSyntax Scalar(HalParser.LooseScalarContext context) {
    var atoms = context.scalarAtom();
    if (atoms.Length != 1) return new HalScalarSyntax(string.Join(" ", atoms.Select(AtomText)));
    var atom = atoms[0];
    if (atom.STRING() != null) return new HalScalarSyntax(Unescape(atom.STRING().GetText()));
    if (atom.NUMBER() != null) return new HalScalarSyntax(double.Parse(atom.GetText(), CultureInfo.InvariantCulture));
    if (atom.TRUE() != null) return new HalScalarSyntax(true);
    if (atom.FALSE() != null) return new HalScalarSyntax(false);
    if (atom.NULL() != null) return new HalScalarSyntax(null);
    return new HalScalarSyntax(atom.GetText());
  }
  private static string AtomText(HalParser.ScalarAtomContext atom) =>
    atom.STRING() == null ? atom.GetText() : Unescape(atom.STRING().GetText());
  private static string FieldName(HalParser.FieldKeyContext field) =>
    field.STRING() == null ? field.GetText() : Unescape(field.STRING().GetText());
  private static string Unescape(string text) {
    var result = new StringBuilder();
    for (var i = 1; i < text.Length - 1; i++) {
      if (text[i] != '\\' || i + 1 >= text.Length - 1) { result.Append(text[i]); continue; }
      result.Append(text[++i] switch {'n' => '\n', 'r' => '\r', 't' => '\t', '"' => '"', '\\' => '\\', var value => value});
    }
    return result.ToString();
  }

  private sealed class Errors(ICollection<HalDiagnostic> diagnostics) : BaseErrorListener, IAntlrErrorListener<int> {
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
      int charPositionInLine, string msg, RecognitionException e) {
      var start = Math.Max(0, offendingSymbol?.StartIndex ?? 0);
      var length = Math.Max(1, (offendingSymbol?.StopIndex ?? start) - start + 1);
      diagnostics.Add(new HalDiagnostic(line, charPositionInLine, start, length, msg));
    }
    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
      int charPositionInLine, string msg, RecognitionException e) =>
      diagnostics.Add(new HalDiagnostic(line, charPositionInLine, 0, 1, msg));
  }
}
