using System;
using System.Collections.Generic;
using System.Linq;

namespace MixinLanguage.Analysis;

using global::MixinLanguage.Compiler;

public enum MixinEditorSymbolKind {
  Function, Label, Local, Variable, TargetVariable, Carry, Annotation, Derivation, Target
}
public enum MixinEditorReferenceKind {
  Function, Label, Local, Variable, TargetVariable, Carry, Root, Member, CSharpType
}
public enum MixinEditorDiagnosticSeverity { Warning, Error }
public enum MixinEditorCompletionKind {
  None, Directive, Root, Function, Predicate, Local, Variable, TargetVariable, Carry, Label,
  DeclaredFunction, OutputTarget, CSharpType
}

public sealed record MixinEditorSymbol(
  string Name, MixinEditorSymbolKind Kind, MixinSourceRange Range, int ScopeStart, int ScopeEnd
);

public sealed record MixinEditorReference(
  string Name, MixinEditorReferenceKind Kind, MixinSourceRange Range, int ScopeStart, int ScopeEnd
);

public sealed record MixinEditorDiagnostic(
  string Message, MixinSourceRange Range, MixinEditorDiagnosticSeverity Severity = MixinEditorDiagnosticSeverity.Error
);

public sealed record MixinEditorCompletionContext(
  MixinEditorCompletionKind Kind, MixinSourceRange ReplacementRange, string Prefix,
  MixinReceiverKind ReceiverKind = MixinReceiverKind.Any
);

public sealed record MixinEditorCompletionSite(
  MixinSourceRange ActivationRange, MixinEditorCompletionContext Context
);

public sealed class MixinEditorAnalysisResult(
  MixinEditorSyntaxTree syntax, IReadOnlyList<MixinEditorSymbol> declarations,
  IReadOnlyList<MixinEditorReference> references, IReadOnlyList<MixinEditorDiagnostic> diagnostics
) {
  public MixinEditorSyntaxTree Syntax { get; } = syntax;
  public IReadOnlyList<MixinEditorSymbol> Declarations { get; } = declarations;
  public IReadOnlyList<MixinEditorReference> References { get; } = references;
  public IReadOnlyList<MixinEditorDiagnostic> Diagnostics { get; } = diagnostics;
}

public static class MixinEditorAnalyzer {
  public static MixinEditorAnalysisResult Analyze(string source) {
    source ??= "";
    var syntax = MixinEditorSyntaxParser.Parse(source);
    var declarations = new List<MixinEditorSymbol>();
    var references = new List<MixinEditorReference>();
    var diagnostics = new List<MixinEditorDiagnostic>();
    var tokens = MixinEditorLexer.Lex(source);
    var lineStarts = LineStarts(source);

    foreach (var diagnostic in syntax.Program.Diagnostics) {
      if (IsCataloguedOuterDirectiveDiagnostic(diagnostic.Message)) continue;
      var line = Math.Max(1, diagnostic.Line);
      var range = line <= lineStarts.Count
        ? LineRange(source, lineStarts, line - 1)
        : new MixinSourceRange(source.Length, source.Length);
      diagnostics.Add(new MixinEditorDiagnostic(diagnostic.Message, range));
    }

    var instructionNodes = syntax.Root.Children.Where(node => node.Kind == MixinEditorSyntaxKind.Directive).ToArray();
    foreach (var node in instructionNodes) AnalyzeDirective(source, node, declarations, references);
    AnalyzeFunctionCalls(source, syntax.Root, references, diagnostics);
    AnalyzeReceiverConstraints(source, tokens, diagnostics);
    AnalyzeStructure(source, instructionNodes, diagnostics);
    AnalyzeExpressionReferences(source, syntax.Root, references);
    ApplyProgramScopes(source, instructionNodes, declarations, references);
    ValidateSymbols(source, declarations, references, diagnostics);
    return new MixinEditorAnalysisResult(syntax, declarations, references, diagnostics);
  }

  private static bool IsCataloguedOuterDirectiveDiagnostic(string message) =>
    message is not null && (message.Contains("'@DERIVATION'") || message.Contains("'@CONFIG'"));

  private static void AnalyzeFunctionCalls(string source, MixinEditorSyntaxNode node,
    ICollection<MixinEditorReference> references, ICollection<MixinEditorDiagnostic> diagnostics) {
    if (node.Kind == MixinEditorSyntaxKind.FunctionCall) {
      var range = node.SourceRange;
      var start = range.Start;
      while (start < range.End && source[start] is ':' or '!' or '?') start++;
      var end = start;
      while (end < range.End && (char.IsLetterOrDigit(source[end]) || source[end] == '_')) end++;
      var name = end > start ? source.Substring(start, end - start) : "";
      MixinFunctionDescriptor descriptor = null;
      if (!string.IsNullOrEmpty(name) && !MixinLanguageCatalog.TryGetFunction(
            name == "typeSymbol" ? "type" : name, out descriptor))
        diagnostics.Add(new MixinEditorDiagnostic("unknown function ':" + name + "'", new MixinSourceRange(start, end)));
      else if (descriptor is not null) {
        for (var index = 0; index < node.Children.Count && index < descriptor.ArgumentRoles.Count; index++) {
          var role = descriptor.ArgumentRoles[index];
          if (role is not (MixinArgumentRole.Function or MixinArgumentRole.CSharpType)) continue;
          var argument = node.Children[index];
          if (argument.Kind != MixinEditorSyntaxKind.LiteralArgument) continue;
          var inner = InnerArgumentRange(source, argument.SourceRange);
          var function = Slice(source, inner);
          if (!string.IsNullOrWhiteSpace(function)) references.Add(new MixinEditorReference(
            function, role == MixinArgumentRole.Function ? MixinEditorReferenceKind.Function : MixinEditorReferenceKind.CSharpType,
            inner, 0, source.Length));
        }
      }
    }
    foreach (var child in node.Children) AnalyzeFunctionCalls(source, child, references, diagnostics);
  }

  private static MixinSourceRange InnerArgumentRange(string source, MixinSourceRange range) {
    var start = range.Start;
    var end = range.End;
    if (start < end && source[start] == '<') start++;
    if (end > start && source[end - 1] == '>') end--;
    return new MixinSourceRange(start, end);
  }

  private static void AnalyzeStructure(string source, IReadOnlyList<MixinEditorSyntaxNode> directives,
    ICollection<MixinEditorDiagnostic> diagnostics) {
    var blocks = new List<EditorBlock>();
    var rootScopeOpen = false;
    foreach (var directive in directives) {
      var nameNode = directive.Children.FirstOrDefault(child => child.Kind == MixinEditorSyntaxKind.DirectiveName);
      if (nameNode is null) continue;
      var name = Slice(source, nameNode.SourceRange).TrimStart('@').ToUpperInvariant();
      if (name is "FUNC" or "ANNOTATION" or "DERIVATION" or "PRELUDE") {
        blocks.Add(new EditorBlock(name, nameNode.SourceRange, directive.SourceRange.Start));
        continue;
      }
      var program = blocks.LastOrDefault();
      if (name == "SCOPE") {
        if (program is null) rootScopeOpen = true; else program.ScopeOpen = true;
        continue;
      }
      if (name == "LABEL") {
        if (program is null) rootScopeOpen = false; else program.ScopeOpen = false;
        continue;
      }
      if (name != "END") continue;
      if (blocks.Count > 0 && blocks[blocks.Count - 1].ScopeOpen)
        blocks[blocks.Count - 1].ScopeOpen = false;
      else if (rootScopeOpen) rootScopeOpen = false;
      else if (blocks.Count == 0) diagnostics.Add(new MixinEditorDiagnostic("unmatched @END", nameNode.SourceRange));
      else blocks.RemoveAt(blocks.Count - 1);
    }
    foreach (var block in blocks) diagnostics.Add(new MixinEditorDiagnostic(
      "unterminated @" + block.Name + " block", block.Range));
  }

  public static MixinEditorCompletionContext GetCompletionContext(string source, int offset) {
    source ??= "";
    offset = Math.Max(0, Math.Min(offset, source.Length));
    var tokens = MixinEditorLexer.Lex(source);
    return GetCompletionContext(source, offset, tokens, null);
  }

  public static IReadOnlyList<MixinEditorCompletionSite> GetCompletionSites(string source) {
    source ??= "";
    var tokens = MixinEditorLexer.Lex(source);
    var result = new List<MixinEditorCompletionSite>();
    foreach (var token in tokens) {
      if (token.Kind is not (MixinEditorTokenKind.Directive or MixinEditorTokenKind.Function or
          MixinEditorTokenKind.Value or MixinEditorTokenKind.Path or MixinEditorTokenKind.Argument)) continue;
      var context = GetCompletionContext(source, token.End, tokens, token);
      if (context.Kind != MixinEditorCompletionKind.None)
        result.Add(new MixinEditorCompletionSite(new MixinSourceRange(token.Start, token.End), context));
    }
    for (var offset = 1; offset <= source.Length; offset++) {
      if (source[offset - 1] is not ('@' or ':' or '#' or '<')) continue;
      var context = GetCompletionContext(source, offset, tokens, null);
      if (context.Kind != MixinEditorCompletionKind.None)
        result.Add(new MixinEditorCompletionSite(new MixinSourceRange(offset, offset), context));
    }
    return result.AsReadOnly();
  }

  private static MixinEditorCompletionContext GetCompletionContext(string source, int offset,
    IReadOnlyList<MixinEditorToken> tokens, MixinEditorToken? selectedToken) {
    var token = selectedToken ?? tokens.LastOrDefault(item => item.Start <= offset && offset <= item.End);
    // Prefix matching stops at the caret, but accepting an item must replace the
    // complete token. Keeping ReplacementRange truncated at the caret caused an
    // accepted item to be inserted before the untyped suffix.
    var replacement = token.End >= token.Start && token.Start <= offset
      ? new MixinSourceRange(token.Start, token.End)
      : new MixinSourceRange(offset, offset);
    var prefixEnd = Math.Max(replacement.Start, Math.Min(offset, replacement.End));
    var prefix = replacement.Start < prefixEnd ? source.Substring(replacement.Start, prefixEnd - replacement.Start) : "";
    if (token.Kind == MixinEditorTokenKind.Directive) {
      var start = replacement.Start < replacement.End && source[replacement.Start] == '@'
        ? replacement.Start + 1 : replacement.Start;
      return new(MixinEditorCompletionKind.Directive, new MixinSourceRange(start, replacement.End),
        source.Substring(start, Math.Max(0, prefixEnd - start)));
    }
    if (token.Kind == MixinEditorTokenKind.Function) {
      var operatorToken = tokens.LastOrDefault(item => item.End <= token.Start && item.Kind == MixinEditorTokenKind.Operator);
      return new(operatorToken.End - operatorToken.Start >= 2 ? MixinEditorCompletionKind.Predicate : MixinEditorCompletionKind.Function,
        replacement, prefix, InferReceiver(source, tokens, token.Start));
    }
    if (token.Kind == MixinEditorTokenKind.Value && prefix.StartsWith("@", StringComparison.Ordinal)) {
      var start = replacement.Start + 1;
      return new(MixinEditorCompletionKind.Root, new MixinSourceRange(start, replacement.End),
        source.Substring(start, Math.Max(0, prefixEnd - start)));
    }
    if (token.Kind is MixinEditorTokenKind.Argument or MixinEditorTokenKind.Path) {
      var contextual = ArgumentOrPathContext(source, tokens, token, offset);
      if (contextual.Kind != MixinEditorCompletionKind.None) return contextual;
    }
    var before = offset == 0 ? '\0' : source[offset - 1];
    if (before == '@') return new(IsLineStart(source, offset - 1) ? MixinEditorCompletionKind.Directive : MixinEditorCompletionKind.Root,
      new MixinSourceRange(offset, offset), "");
    if (before == ':') return new(MixinEditorCompletionKind.Function, new MixinSourceRange(offset, offset), "",
      InferReceiver(source, tokens, offset));
    return new(MixinEditorCompletionKind.None, new MixinSourceRange(offset, offset), "");
  }

  private static MixinEditorCompletionContext ArgumentOrPathContext(string source,
    IReadOnlyList<MixinEditorToken> tokens, MixinEditorToken token, int offset) {
    var index = tokens.ToList().IndexOf(token);
    if (token.Kind == MixinEditorTokenKind.Path) {
      var root = tokens.Take(index).LastOrDefault(item => item.Kind == MixinEditorTokenKind.Value);
      var name = root.End > root.Start ? source.Substring(root.Start, root.End - root.Start).TrimStart('@') : "";
      var pathKind = name switch {
        "local" => MixinEditorCompletionKind.Local, "var" => MixinEditorCompletionKind.Variable,
        "tar" => MixinEditorCompletionKind.TargetVariable, "carry" => MixinEditorCompletionKind.Carry,
        _ => MixinEditorCompletionKind.None
      };
      var start = token.Start < token.End && source[token.Start] == '#' ? token.Start + 1 : token.Start;
      return new MixinEditorCompletionContext(pathKind, new MixinSourceRange(start, offset),
        source.Substring(start, Math.Max(0, offset - start)));
    }
    var functionToken = tokens.Take(index + 1).LastOrDefault(item => item.Kind == MixinEditorTokenKind.Function);
    if (functionToken.End > functionToken.Start) {
      var functionName = source.Substring(functionToken.Start, functionToken.End - functionToken.Start);
      if (MixinLanguageCatalog.TryGetFunction(functionName, out var function)) {
        var callbackArgumentIndex = tokens.SkipWhile(item => !item.Equals(functionToken)).TakeWhile(item => item.Start < token.Start)
          .Count(item => item.Kind == MixinEditorTokenKind.ExpressionArgumentDelimiter && source[item.Start] == '<') - 1;
        if (callbackArgumentIndex >= 0 && callbackArgumentIndex < function.ArgumentRoles.Count) {
          var functionKind = function.ArgumentRoles[callbackArgumentIndex] switch {
            MixinArgumentRole.Function => MixinEditorCompletionKind.DeclaredFunction,
            MixinArgumentRole.CSharpType => MixinEditorCompletionKind.CSharpType,
            _ => MixinEditorCompletionKind.None
          };
          if (functionKind != MixinEditorCompletionKind.None) return new MixinEditorCompletionContext(
            functionKind, new MixinSourceRange(token.Start, offset),
            source.Substring(token.Start, Math.Max(0, offset - token.Start)));
        }
      }
    }
    var directive = tokens.Take(index + 1).LastOrDefault(item => item.Kind == MixinEditorTokenKind.Directive);
    if (directive.End <= directive.Start) return new MixinEditorCompletionContext(
      MixinEditorCompletionKind.None, new MixinSourceRange(offset, offset), "");
    var command = source.Substring(directive.Start + 1, directive.End - directive.Start - 1);
    if (!MixinLanguageCatalog.TryGetDirective(command, out var descriptor)) return new(
      MixinEditorCompletionKind.None, new MixinSourceRange(offset, offset), "");
    var argumentIndex = tokens.SkipWhile(item => !item.Equals(directive)).TakeWhile(item => item.Start < token.Start)
      .Count(item => item.Kind == MixinEditorTokenKind.DirectiveArgumentDelimiter &&
                     source[item.Start] == '<') - 1;
    if (argumentIndex < 0 || argumentIndex >= descriptor.ArgumentRoles.Count) return new(
      MixinEditorCompletionKind.None, new MixinSourceRange(offset, offset), "");
    var kind = descriptor.ArgumentRoles[argumentIndex] switch {
      MixinArgumentRole.Label => MixinEditorCompletionKind.Label,
      MixinArgumentRole.Function => MixinEditorCompletionKind.DeclaredFunction,
      MixinArgumentRole.OutputTarget => MixinEditorCompletionKind.OutputTarget,
      MixinArgumentRole.CSharpType => MixinEditorCompletionKind.CSharpType,
      _ => MixinEditorCompletionKind.None
    };
    return new MixinEditorCompletionContext(kind, new MixinSourceRange(token.Start, offset),
      source.Substring(token.Start, Math.Max(0, offset - token.Start)));
  }

  private static void AnalyzeDirective(string source, MixinEditorSyntaxNode node,
    ICollection<MixinEditorSymbol> declarations, ICollection<MixinEditorReference> references) {
    var nameNode = node.Children.FirstOrDefault(child => child.Kind == MixinEditorSyntaxKind.DirectiveName);
    if (nameNode is null) return;
    var command = Slice(source, nameNode.SourceRange).TrimStart('@').ToUpperInvariant();
    var arguments = node.Children.Where(child => IsDirectiveArgument(child.Kind)).ToArray();
    MixinSourceRange Argument(int index) {
      if (index >= arguments.Length) return new MixinSourceRange(node.SourceRange.End, node.SourceRange.End);
      var range = arguments[index].SourceRange;
      return new MixinSourceRange(Math.Min(range.End, range.Start + 1), Math.Max(range.Start + 1, range.End - 1));
    }
    string Text(int index) => Slice(source, Argument(index));
    void Declare(int index, MixinEditorSymbolKind kind) {
      var range = Argument(index); var name = Text(index);
      if (!string.IsNullOrWhiteSpace(name)) declarations.Add(new MixinEditorSymbol(name, kind, range, 0, source.Length));
    }
    void Refer(int index, MixinEditorReferenceKind kind) {
      var range = Argument(index); var name = Text(index);
      if (!string.IsNullOrWhiteSpace(name)) references.Add(new MixinEditorReference(name, kind, range, 0, source.Length));
    }
    switch (command) {
      case "FUNC": Declare(0, MixinEditorSymbolKind.Function); break;
      case "SCOPE": if (arguments.Length > 0) Declare(0, MixinEditorSymbolKind.Label); break;
      case "LABEL": Declare(0, MixinEditorSymbolKind.Label); break;
      case "LOCAL": Declare(0, MixinEditorSymbolKind.Local); break;
      case "VAR": Declare(0, MixinEditorSymbolKind.Variable); break;
      case "TAR": Declare(0, MixinEditorSymbolKind.TargetVariable); break;
      case "CARRY": Declare(0, MixinEditorSymbolKind.Carry); break;
      case "ANNOTATION": Declare(0, MixinEditorSymbolKind.Annotation); Refer(0, MixinEditorReferenceKind.CSharpType); break;
      case "DERIVATION": Declare(0, MixinEditorSymbolKind.Derivation); Refer(0, MixinEditorReferenceKind.CSharpType); break;
      case "DEFINE_TARGET": Declare(0, MixinEditorSymbolKind.Target); break;
      case "GOTO": Refer(0, MixinEditorReferenceKind.Label); break;
      case "MATCH": if (arguments.Length > 0) Refer(0, MixinEditorReferenceKind.Label); break;
      case "INLINE": Refer(0, MixinEditorReferenceKind.Function); break;
      case "CALL": Refer(arguments.Length == 2 ? 1 : 0, MixinEditorReferenceKind.Function); break;
    }
  }

  private static bool IsDirectiveArgument(MixinEditorSyntaxKind kind) => kind is
    MixinEditorSyntaxKind.DirectiveArgument or
    MixinEditorSyntaxKind.DeclarationDirectiveArgument or
    MixinEditorSyntaxKind.ReferenceDirectiveArgument or
    MixinEditorSyntaxKind.DeclarationReferenceDirectiveArgument;

  private static void AnalyzeExpressionReferences(string source, MixinEditorSyntaxNode node,
    ICollection<MixinEditorReference> references) {
    if (node.Kind is MixinEditorSyntaxKind.Reference or MixinEditorSyntaxKind.ParenthesizedReference) {
      var root = node.Children.FirstOrDefault(child => child.Kind == MixinEditorSyntaxKind.Root);
      if (root is not null) {
        var name = Slice(source, root.SourceRange);
        var kind = name switch {
          "local" => MixinEditorReferenceKind.Local, "var" => MixinEditorReferenceKind.Variable,
          "tar" => MixinEditorReferenceKind.TargetVariable, "carry" => MixinEditorReferenceKind.Carry,
          _ => MixinEditorReferenceKind.Root
        };
        references.Add(new MixinEditorReference(name, kind, root.SourceRange, 0, source.Length));
        if (kind is MixinEditorReferenceKind.Local or MixinEditorReferenceKind.Variable or
            MixinEditorReferenceKind.TargetVariable or MixinEditorReferenceKind.Carry) {
          var member = node.Children.FirstOrDefault(child => child.Kind == MixinEditorSyntaxKind.Member);
          if (member is not null) references.Add(new MixinEditorReference(
            Slice(source, member.SourceRange), kind, member.SourceRange, 0, source.Length));
        }
      }
    }
    foreach (var child in node.Children) AnalyzeExpressionReferences(source, child, references);
  }

  private static void AnalyzeReceiverConstraints(string source, IReadOnlyList<MixinEditorToken> tokens,
    ICollection<MixinEditorDiagnostic> diagnostics) {
    foreach (var token in tokens.Where(item => item.Kind == MixinEditorTokenKind.Function)) {
      var name = Slice(source, new MixinSourceRange(token.Start, token.End));
      if (!MixinLanguageCatalog.TryGetFunction(name, out var descriptor) ||
          descriptor.ReceiverKind == MixinReceiverKind.Any) continue;
      var actual = InferReceiver(source, tokens, token.Start);
      if (actual == MixinReceiverKind.Any || actual == descriptor.ReceiverKind) continue;
      diagnostics.Add(new MixinEditorDiagnostic(
        $"function ':{name}' expects a {descriptor.ReceiverKind.ToString().ToLowerInvariant()} receiver",
        new MixinSourceRange(token.Start, token.End), MixinEditorDiagnosticSeverity.Warning));
    }
  }

  private static MixinReceiverKind InferReceiver(string source, IReadOnlyList<MixinEditorToken> tokens, int offset) {
    var preceding = tokens.Where(item => item.End <= offset).Reverse().ToArray();
    var root = preceding.FirstOrDefault(item => item.Kind == MixinEditorTokenKind.Value);
    if (root.End <= root.Start) return MixinReceiverKind.Any;
    if (preceding.Any(item => item.Kind == MixinEditorTokenKind.Function && item.Start > root.End))
      return MixinReceiverKind.Any;
    var name = Slice(source, new MixinSourceRange(root.Start, root.End)).TrimStart('@');
    return name switch {
      "table" => MixinReceiverKind.Table,
      "true" or "false" => MixinReceiverKind.Boolean,
      _ => MixinReceiverKind.Any
    };
  }

  private static void ValidateSymbols(string source, IReadOnlyCollection<MixinEditorSymbol> declarations,
    IReadOnlyCollection<MixinEditorReference> references, ICollection<MixinEditorDiagnostic> diagnostics) {
    foreach (var group in declarations.Where(item => item.Kind is MixinEditorSymbolKind.Function or MixinEditorSymbolKind.Label)
      .GroupBy(item => (item.Kind, item.Name, item.ScopeStart, item.ScopeEnd)).Where(group => group.Count() > 1))
      foreach (var duplicate in group.Skip(1)) diagnostics.Add(new MixinEditorDiagnostic(
        "duplicate " + group.Key.Kind.ToString().ToLowerInvariant() + " '" + group.Key.Name + "'", duplicate.Range));
    foreach (var reference in references.Where(item => item.Kind is MixinEditorReferenceKind.Function or MixinEditorReferenceKind.Label)) {
      var declarationKind = reference.Kind == MixinEditorReferenceKind.Function ? MixinEditorSymbolKind.Function : MixinEditorSymbolKind.Label;
      if (declarations.Any(item => item.Kind == declarationKind && item.Name == reference.Name &&
          (declarationKind == MixinEditorSymbolKind.Function ||
           item.ScopeStart == reference.ScopeStart && item.ScopeEnd == reference.ScopeEnd))) continue;
      diagnostics.Add(new MixinEditorDiagnostic(
        "unresolved " + reference.Kind.ToString().ToLowerInvariant() + " '" + reference.Name + "'", reference.Range));
    }
  }

  private static void ApplyProgramScopes(string source, IReadOnlyList<MixinEditorSyntaxNode> directives,
    IList<MixinEditorSymbol> declarations, IList<MixinEditorReference> references) {
    var sourceLength = source.Length;
    var scopeByOffset = new Dictionary<int, (int Start, int End)>();
    var blocks = new List<EditorBlock>();
    var rootScopeOpen = false;
    foreach (var directive in directives) {
      var nameNode = directive.Children.FirstOrDefault(child => child.Kind == MixinEditorSyntaxKind.DirectiveName);
      if (nameNode is null) continue;
      var name = Slice(source, nameNode.SourceRange).TrimStart('@').ToUpperInvariant();
      foreach (var block in blocks) block.Offsets.Add(directive.SourceRange.Start);
      if (name is "FUNC" or "ANNOTATION" or "DERIVATION" or "PRELUDE")
        blocks.Add(new EditorBlock(name, nameNode.SourceRange, directive.SourceRange.Start));
      else if (name == "SCOPE") {
        if (blocks.LastOrDefault() is { } opened) opened.ScopeOpen = true; else rootScopeOpen = true;
      }
      else if (name == "LABEL") {
        if (blocks.LastOrDefault() is { } labelled) labelled.ScopeOpen = false; else rootScopeOpen = false;
      }
      else if (name == "END" && blocks.Count > 0) {
        var block = blocks[blocks.Count - 1];
        if (block.ScopeOpen) { block.ScopeOpen = false; continue; }
        blocks.RemoveAt(blocks.Count - 1);
        var end = directive.SourceRange.End;
        foreach (var offset in block.Offsets) scopeByOffset[offset] = (block.Start, end);
      } else if (name == "END" && rootScopeOpen) rootScopeOpen = false;
    }
    foreach (var block in blocks)
      foreach (var offset in block.Offsets) scopeByOffset[offset] = (block.Start, sourceLength);

    (int Start, int End) Scope(int offset) {
      var directive = directives.FirstOrDefault(item =>
        item.SourceRange.Start <= offset && offset <= item.SourceRange.End);
      return directive is not null && scopeByOffset.TryGetValue(directive.SourceRange.Start, out var value)
        ? value : (0, sourceLength);
    }
    for (var index = 0; index < declarations.Count; index++) {
      if (declarations[index].Kind == MixinEditorSymbolKind.Function) continue;
      var scope = Scope(declarations[index].Range.Start);
      declarations[index] = declarations[index] with { ScopeStart = scope.Start, ScopeEnd = scope.End };
    }
    for (var index = 0; index < references.Count; index++) {
      if (references[index].Kind == MixinEditorReferenceKind.Function) continue;
      var scope = Scope(references[index].Range.Start);
      references[index] = references[index] with { ScopeStart = scope.Start, ScopeEnd = scope.End };
    }
  }

  private sealed class EditorBlock(string name, MixinSourceRange range, int start) {
    public string Name { get; } = name;
    public MixinSourceRange Range { get; } = range;
    public int Start { get; } = start;
    public bool ScopeOpen { get; set; }
    public List<int> Offsets { get; } = [start];
  }

  private static IReadOnlyList<int> LineStarts(string source) {
    var result = new List<int> { 0 };
    for (var index = 0; index < source.Length; index++) if (source[index] == '\n') result.Add(index + 1);
    return result;
  }
  private static MixinSourceRange LineRange(string source, IReadOnlyList<int> starts, int line) =>
    new(starts[line], line + 1 < starts.Count ? starts[line + 1] : source.Length);
  private static string Slice(string source, MixinSourceRange range) =>
    range.Start >= 0 && range.End >= range.Start && range.End <= source.Length
      ? source.Substring(range.Start, range.Length) : "";
  private static bool IsLineStart(string source, int offset) {
    for (var index = offset - 1; index >= 0 && source[index] is not ('\r' or '\n'); index--)
      if (!char.IsWhiteSpace(source[index])) return false;
    return true;
  }
}

public static class MixinEditorTypingFacts {
  public static bool CanOpenPair(string source, int offset, char open) {
    source ??= "";
    if (offset <= 0 || offset > source.Length) return false;
    var tokens = MixinEditorLexer.Lex(source.Substring(0, offset));
    if (tokens.Count == 0) return false;
    var last = tokens[tokens.Count - 1];
    if (open == '<') return last.Kind is MixinEditorTokenKind.Directive or MixinEditorTokenKind.Function;
    if (open == '(') return source[offset - 1] is '@' or '<';
    return false;
  }

  public static bool IsEmptyPair(string source, int offset) =>
    source is not null && offset > 0 && offset < source.Length &&
    (source[offset - 1] == '<' && source[offset] == '>' ||
     source[offset - 1] == '(' && source[offset] == ')');

  public static bool TryGetContinuationIndent(string source, int offset, out string indent) {
    source ??= "";
    offset = Math.Max(0, Math.Min(offset, source.Length));
    var depth = 0;
    foreach (var token in MixinEditorLexer.Lex(source.Substring(0, offset))) {
      if (token.Kind is not (MixinEditorTokenKind.DirectiveArgumentDelimiter or
          MixinEditorTokenKind.ExpressionArgumentDelimiter)) continue;
      depth += source[token.Start] == '<' ? 1 : -1;
    }
    if (depth <= 0) { indent = ""; return false; }
    var lineStart = source.LastIndexOf('\n', Math.Max(0, offset - 1));
    lineStart = lineStart < 0 ? 0 : lineStart + 1;
    var cursor = lineStart;
    while (cursor < offset && source[cursor] is ' ' or '\t') cursor++;
    indent = source.Substring(lineStart, cursor - lineStart) + "  ";
    return true;
  }
}
