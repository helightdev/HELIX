using System;
using System.Collections.Generic;
using System.Linq;
using global::MixinLanguage;
using global::MixinLanguage.Compiler;

namespace MixinLanguage.Analysis;

/// <summary>
/// Source-oriented view of the compiler syntax used by editor hosts.  It is deliberately a
/// projection of <see cref="MixinExpressionParser"/>, not a second language parser.
/// </summary>
public enum MixinEditorSyntaxKind {
  Document,
  Directive,
  DirectiveName,
  DirectiveArgument,
  DeclarationDirectiveArgument,
  ReferenceDirectiveArgument,
  DeclarationReferenceDirectiveArgument,
  Operand,
  Reference,
  ParenthesizedReference,
  Root,
  Member,
  Path,
  FunctionCall,
  LiteralArgument,
  ExpressionArgument,
  Comment,
  Continuation,
  Escape,
  Error
}

public sealed class MixinEditorSyntaxNode(
  MixinEditorSyntaxKind kind,
  MixinSourceRange sourceRange,
  IReadOnlyList<MixinEditorSyntaxNode> children = null
) {
  public MixinEditorSyntaxKind Kind { get; } = kind;
  public MixinSourceRange SourceRange { get; } = sourceRange;
  public IReadOnlyList<MixinEditorSyntaxNode> Children { get; } = children ?? [];
}

public sealed class MixinEditorSyntaxTree(
  string source,
  MixinProgramSyntax program,
  MixinEditorSyntaxNode root
) {
  public string Source { get; } = source ?? "";
  public MixinProgramSyntax Program { get; } = program;
  public MixinEditorSyntaxNode Root { get; } = root;
}

public static class MixinEditorSyntaxParser {
  /// <summary>
  /// Parses with the compiler parser and maps that result back onto the unchanged source text.
  /// Trivia and invalid lines remain represented so an editor can construct a lossless tree.
  /// </summary>
  public static MixinEditorSyntaxTree Parse(string source) {
    source ??= "";
    var program = MixinExpressionParser.Parse(source);
    var diagnosticLines = new HashSet<int>(program.Diagnostics.Select(item => item.Line));
    var logicalLines = MixinExpressionLexer.GetLogicalSourceLines(source);
    var children = new List<MixinEditorSyntaxNode>();
    for (var index = 0; index < program.Instructions.Count; index++) {
      if (logicalLines[index].ContinuedFromLine >= 0) continue;
      var instruction = program.Instructions[index];
      children.Add(logicalLines[index].Continuations.Count == 0
        ? ProjectPhysicalLine(source, instruction, diagnosticLines.Contains(instruction.Line))
        : ProjectLogicalLine(logicalLines[index], diagnosticLines.Contains(instruction.Line)));
    }
    return new MixinEditorSyntaxTree(
      source, program,
      new MixinEditorSyntaxNode(MixinEditorSyntaxKind.Document, new MixinSourceRange(0, source.Length), children)
    );
  }

  private static MixinEditorSyntaxNode ProjectPhysicalLine(
    string source, DirectiveInstruction instruction, bool hasDiagnostic
  ) => ProjectSourceLine(source, instruction.SourceRange, hasDiagnostic);

  private static MixinEditorSyntaxNode ProjectSourceLine(
    string source, MixinSourceRange range, bool hasDiagnostic
  ) {
    var contentEnd = range.End;
    while (contentEnd > range.Start && source[contentEnd - 1] is '\r' or '\n') contentEnd--;
    var marker = range.Start;
    while (marker < contentEnd && source[marker] is ' ' or '\t') marker++;

    if (marker == contentEnd)
      return new MixinEditorSyntaxNode(MixinEditorSyntaxKind.Operand, range);
    if (StartsWith(source, marker, "@#"))
      return new MixinEditorSyntaxNode(MixinEditorSyntaxKind.Comment, range);
    if (StartsWith(source, marker, "@\\") || StartsWith(source, marker, "@+")) {
      var expressionStart = marker + 2;
      var continuationChildren = ProjectContinuationExpression(source, expressionStart, contentEnd).ToList();
      if (hasDiagnostic) continuationChildren.Add(new MixinEditorSyntaxNode(
        MixinEditorSyntaxKind.Error, new MixinSourceRange(contentEnd, contentEnd)
      ));
      return new MixinEditorSyntaxNode(
        MixinEditorSyntaxKind.Continuation, range, continuationChildren
      );
    }
    if (source[marker] != '@')
      return new MixinEditorSyntaxNode(MixinEditorSyntaxKind.Error, range);

    var nameEnd = marker + 1;
    while (nameEnd < contentEnd && (char.IsLetter(source[nameEnd]) || source[nameEnd] == '_')) nameEnd++;
    if (nameEnd == marker + 1)
      return new MixinEditorSyntaxNode(MixinEditorSyntaxKind.Error, range);

    var directiveChildren = new List<MixinEditorSyntaxNode> {
      new(MixinEditorSyntaxKind.DirectiveName, new MixinSourceRange(marker, nameEnd))
    };
    var command = source.Substring(marker + 1, nameEnd - marker - 1).ToUpperInvariant();
    var position = nameEnd;
    var argumentIndex = 0;
    while (position < contentEnd && source[position] == '<') {
      var end = FindDelimitedEnd(source, position, contentEnd);
      if (end < 0) {
        directiveChildren.Add(new MixinEditorSyntaxNode(
          MixinEditorSyntaxKind.Error, new MixinSourceRange(position, contentEnd)
        ));
        position = contentEnd;
        break;
      }
      var innerStart = position + 1;
      var innerEnd = end - 1;
      IReadOnlyList<MixinEditorSyntaxNode> argumentChildren;
      if (innerEnd - innerStart >= 2 && source[innerStart] == '(' && source[innerEnd - 1] == ')')
        argumentChildren = ProjectExpression(source, innerStart + 1, innerEnd - 1);
      else argumentChildren = [];
      directiveChildren.Add(new MixinEditorSyntaxNode(
        DirectiveArgumentKind(command, argumentIndex++),
        new MixinSourceRange(position, end), argumentChildren
      ));
      position = end;
    }
    while (position < contentEnd && source[position] is ' ' or '\t') position++;
    if (position < contentEnd) {
      directiveChildren.Add(new MixinEditorSyntaxNode(
        MixinEditorSyntaxKind.Operand, new MixinSourceRange(position, contentEnd),
        ProjectExpression(source, position, contentEnd)
      ));
    }
    if (hasDiagnostic && directiveChildren.All(child => child.Kind != MixinEditorSyntaxKind.Error))
      directiveChildren.Add(new MixinEditorSyntaxNode(
        MixinEditorSyntaxKind.Error, new MixinSourceRange(contentEnd, contentEnd)
      ));
    return new MixinEditorSyntaxNode(MixinEditorSyntaxKind.Directive, range, directiveChildren);
  }

  private static MixinEditorSyntaxKind DirectiveArgumentKind(string command, int index) {
    var declares = command switch {
      "FUNC" when index == 0 => true,
      "SCOPE" when index == 0 => true,
      "LABEL" when index == 0 => true,
      "LOCAL" when index == 0 => true,
      "VAR" when index == 0 => true,
      "TAR" when index == 0 => true,
      "CARRY" when index == 0 => true,
      "ANNOTATION" when index == 0 => true,
      "DERIVATION" when index == 0 => true,
      "DEFINE_TARGET" when index == 0 => true,
      _ => false
    };
    var references = command switch {
      "GOTO" when index == 0 => true,
      "MATCH" when index == 0 => true,
      "INLINE" when index == 0 => true,
      "CALL" when index is 0 or 1 => true,
      "ANNOTATION" when index == 0 => true,
      "DERIVATION" when index == 0 => true,
      _ => false
    };
    if (declares && references) return MixinEditorSyntaxKind.DeclarationReferenceDirectiveArgument;
    if (declares) return MixinEditorSyntaxKind.DeclarationDirectiveArgument;
    if (references) return MixinEditorSyntaxKind.ReferenceDirectiveArgument;
    return MixinEditorSyntaxKind.DirectiveArgument;
  }

  private static MixinEditorSyntaxNode ProjectLogicalLine(
    MixinLogicalSourceLine line, bool hasDiagnostic
  ) {
    var logical = ProjectSourceLine(
      line.Text, new MixinSourceRange(0, line.Text.Length), hasDiagnostic
    );
    var mapped = MapNode(logical, line);
    var entries = new List<IntervalEntry>();
    var order = 0;
    foreach (var child in mapped.Children) Flatten(child, 1, ref order, entries);
    foreach (var continuation in line.Continuations)
      entries.Add(new IntervalEntry(
        new MixinEditorSyntaxNode(MixinEditorSyntaxKind.Continuation, continuation), 1, order++
      ));
    return RebuildIntervalTree(
      mapped.Kind,
      new MixinSourceRange(line.PhysicalRange.Start, line.SourceEnd),
      entries
    );
  }

  private static MixinEditorSyntaxNode MapNode(
    MixinEditorSyntaxNode node, MixinLogicalSourceLine line
  ) => new(
    node.Kind, line.MapRange(node.SourceRange),
    node.Children.Select(child => MapNode(child, line)).ToArray()
  );

  private static void Flatten(
    MixinEditorSyntaxNode node, int depth, ref int order, ICollection<IntervalEntry> entries
  ) {
    entries.Add(new IntervalEntry(
      new MixinEditorSyntaxNode(node.Kind, node.SourceRange), depth, order++
    ));
    foreach (var child in node.Children) Flatten(child, depth + 1, ref order, entries);
  }

  private static MixinEditorSyntaxNode RebuildIntervalTree(
    MixinEditorSyntaxKind kind, MixinSourceRange range, IEnumerable<IntervalEntry> entries
  ) {
    var root = new MutableEditorNode(kind, range);
    var stack = new Stack<MutableEditorNode>();
    stack.Push(root);
    foreach (var entry in entries
      .OrderBy(item => item.Node.SourceRange.Start)
      .ThenByDescending(item => item.Node.SourceRange.End)
      .ThenBy(item => item.Depth)
      .ThenBy(item => item.Order)) {
      var child = new MutableEditorNode(entry.Node.Kind, entry.Node.SourceRange);
      while (stack.Count > 1 && !Contains(stack.Peek().Range, child.Range)) stack.Pop();
      if (!Contains(stack.Peek().Range, child.Range)) continue;
      stack.Peek().Children.Add(child);
      stack.Push(child);
    }
    return root.Freeze();
  }

  private static bool Contains(MixinSourceRange outer, MixinSourceRange inner) =>
    inner.Start >= outer.Start && inner.End <= outer.End;

  private sealed record IntervalEntry(MixinEditorSyntaxNode Node, int Depth, int Order);

  private sealed class MutableEditorNode(MixinEditorSyntaxKind kind, MixinSourceRange range) {
    internal MixinEditorSyntaxKind Kind { get; } = kind;
    internal MixinSourceRange Range { get; } = range;
    internal List<MutableEditorNode> Children { get; } = [];

    internal MixinEditorSyntaxNode Freeze() => new(
      Kind, Range, Children.Select(child => child.Freeze()).ToArray()
    );
  }

  private static IReadOnlyList<MixinEditorSyntaxNode> ProjectExpression(string source, int start, int end) {
    if (end <= start) return [];
    var text = source.Substring(start, end - start);
    var result = new List<MixinEditorSyntaxNode>();
    foreach (var value in MixinExpressionParser.ParseExpressionValues(text))
      if (value.Reference is not null) result.Add(ProjectReference(value.Reference, start));

    var tokens = MixinExpressionLexer.LexExpression(text);
    foreach (var token in tokens.Where(token =>
      token.Kind == MixinExpressionTokenKind.Literal && token.Text == "@" && token.End - token.Start == 2
    )) result.Add(new MixinEditorSyntaxNode(
      MixinEditorSyntaxKind.Escape,
      new MixinSourceRange(start + token.Start, start + token.End)
    ));
    foreach (var token in tokens.Where(token => token.Kind == MixinExpressionTokenKind.Invalid))
      result.Add(new MixinEditorSyntaxNode(
        MixinEditorSyntaxKind.Error,
        new MixinSourceRange(
          start + token.Start,
          start + Math.Min(text.Length, Math.Max(token.End, token.Start + 1))
        )
      ));
    result.Sort((left, right) => left.SourceRange.Start.CompareTo(right.SourceRange.Start));
    return result.AsReadOnly();
  }

  private static IReadOnlyList<MixinEditorSyntaxNode> ProjectContinuationExpression(
    string source, int start, int end
  ) {
    var result = ProjectExpression(source, start, end).ToList();
    var suffixStart = start;
    while (suffixStart < end && source[suffixStart] is ' ' or '\t') suffixStart++;
    if (suffixStart >= end || source[suffixStart] is not (':' or '#')) return result.AsReadOnly();

    // A continuation suffix has no root of its own. Supplying a synthetic root lets the
    // authoritative expression lexer/parser parse the suffix, after which only its path and
    // function-call children are projected back to their original coordinates.
    // A leading '#' would otherwise become the synthetic root's member. Give that root a
    // disposable member so the real suffix is parsed as a path, exactly as it is after the
    // preceding physical line has been joined by the compiler lexer.
    var syntheticRoot = source[suffixStart] == '#' ? "@this#_" : "@null";
    var synthetic = syntheticRoot + source.Substring(suffixStart, end - suffixStart);
    var value = MixinExpressionParser.ParseExpressionValues(synthetic).FirstOrDefault(item => item.Reference is not null);
    if (value?.Reference is null || value.Reference.SourceRange.Start != 0) return result.AsReadOnly();
    var projected = ProjectReference(value.Reference, suffixStart - syntheticRoot.Length);
    result.AddRange(projected.Children.Where(child =>
      child.Kind is MixinEditorSyntaxKind.Path or MixinEditorSyntaxKind.FunctionCall
    ));
    result.Sort((left, right) => left.SourceRange.Start.CompareTo(right.SourceRange.Start));
    return result.AsReadOnly();
  }

  private static MixinEditorSyntaxNode ProjectReference(MixinExpressionReference reference, int offset) {
    var children = new List<MixinEditorSyntaxNode>();
    children.Add(new MixinEditorSyntaxNode(
      MixinEditorSyntaxKind.Root, Shift(reference.RootRange, offset)
    ));
    if (reference.Member is not null) {
      children.Add(new MixinEditorSyntaxNode(
        MixinEditorSyntaxKind.Member, Shift(reference.MemberRange, offset)
      ));
    }
    foreach (var property in reference.Properties) {
      var propertyChildren = new List<MixinEditorSyntaxNode>();
      // A path is represented by the compiler as a synthetic "path" property whose argument
      // carries the identifier. It is one semantic path segment, not a function argument.
      if (property.Name != "path")
        foreach (var argument in property.ParsedArguments) {
          var argumentKind = argument.Literal is null
            ? MixinEditorSyntaxKind.ExpressionArgument
            : MixinEditorSyntaxKind.LiteralArgument;
          propertyChildren.Add(new MixinEditorSyntaxNode(
            argumentKind, Shift(argument.SourceRange, offset),
            argumentKind == MixinEditorSyntaxKind.ExpressionArgument
              ? ProjectExpressionFromArgument(argument, offset)
              : []
          ));
        }
      children.Add(new MixinEditorSyntaxNode(
        property.Name == "path" ? MixinEditorSyntaxKind.Path : MixinEditorSyntaxKind.FunctionCall,
        Shift(property.SourceRange, offset), propertyChildren
      ));
    }
    return new MixinEditorSyntaxNode(
      reference.Parenthesized ? MixinEditorSyntaxKind.ParenthesizedReference : MixinEditorSyntaxKind.Reference,
      Shift(reference.SourceRange, offset), children
    );
  }

  private static IReadOnlyList<MixinEditorSyntaxNode> ProjectExpressionFromArgument(
    MixinPropertyArgumentSyntax argument, int offset
  ) {
    var expression = argument.ValueExpression;
    if (expression is null) return [];
    return expression.Where(value => value.Reference is not null)
      // Nested expression tokens retain the containing expression's coordinates; the compiler
      // lexer shifts them while lexing the dynamic argument.
      .Select(value => ProjectReference(value.Reference, offset))
      .ToArray();
  }

  private static MixinSourceRange Shift(MixinSourceRange range, int offset) =>
    new(range.Start + offset, range.End + offset);

  private static int FindDelimitedEnd(string source, int start, int limit) {
    var depth = 0;
    for (var position = start; position < limit; position++) {
      if (source[position] == '<') depth++;
      else if (source[position] == '>' && --depth == 0) return position + 1;
    }
    return -1;
  }

  private static bool StartsWith(string source, int position, string value) =>
    position + value.Length <= source.Length &&
    string.CompareOrdinal(source, position, value, 0, value.Length) == 0;

}
