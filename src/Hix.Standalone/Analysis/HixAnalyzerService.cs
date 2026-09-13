using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Hix.Compiler;

namespace Hix.Standalone.Analysis;

/// Stateful, transport-neutral language service. IDE adapters provide documents and translate
/// immutable snapshots to their protocol; no Rider or IntelliJ types enter this layer.
public sealed class HixAnalyzerService {
  private readonly object gate = new();
  private readonly HixBackend backend;
  private readonly IHixAnalyzerHost host;
  private readonly Dictionary<string, CachedDocument> documents = new(StringComparer.OrdinalIgnoreCase);
  private readonly HixDefinition[] definitions;

  public HixAnalyzerService(HixBackend backend = null, IHixAnalyzerHost host = null,
    IEnumerable<HixDefinition> additionalDefinitions = null) {
    this.backend = backend ?? HixCoreBackend.Instance;
    this.host = host;
    definitions = BuildDefinitions(this.backend).Concat(additionalDefinitions ?? []).ToArray();
  }

  public IReadOnlyList<HixDefinition> Definitions => definitions;

  public IReadOnlyList<HixDocumentSnapshot> Synchronize(IEnumerable<HixDocument> workspaceDocuments) {
    lock (gate) {
      var changed = workspaceDocuments?.ToArray() ?? [];
      foreach (var directory in changed.Select(item => DirectoryOf(item.Path)).Distinct(StringComparer.OrdinalIgnoreCase)) {
        var present = new HashSet<string>(changed.Where(item => string.Equals(DirectoryOf(item.Path), directory,
          StringComparison.OrdinalIgnoreCase)).Select(item => Normalize(item.Path)), StringComparer.OrdinalIgnoreCase);
        foreach (var obsolete in documents.Keys.Where(path => string.Equals(DirectoryOf(path), directory,
          StringComparison.OrdinalIgnoreCase) && !present.Contains(path)).ToArray()) documents.Remove(obsolete);
      }
      foreach (var document in changed) {
        var path = Normalize(document.Path);
        var text = document.Text ?? string.Empty;
        var hash = SourceHash(text);
        if (!documents.TryGetValue(path, out var cached) || cached.Hash != hash)
          documents[path] = new CachedDocument(new HixDocument(path, text, document.Revision), hash,
            new LanguageAnalysis(text, backend, recoverValidDeclarations: true));
        else if (cached.Document.Revision != document.Revision)
          documents[path] = cached with {Document = cached.Document with {Revision = document.Revision}};
      }
      var directories = changed.Select(item => DirectoryOf(item.Path)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
      return documents.Values.Where(item => directories.Contains(DirectoryOf(item.Document.Path),
          StringComparer.OrdinalIgnoreCase)).OrderBy(item => item.Document.Path, StringComparer.OrdinalIgnoreCase)
        .Select(item => Snapshot(item, documents.Values.Where(candidate => SameDirectory(
          candidate.Document.Path, item.Document.Path)).ToArray())).ToArray();
    }
  }

  public bool Remove(string path) { lock (gate) return documents.Remove(Normalize(path)); }

  public HixDocumentSnapshot Get(string path) {
    lock (gate) {
      if (!documents.TryGetValue(Normalize(path), out var document)) return null;
      var siblings = documents.Values.Where(item => SameDirectory(item.Document.Path, path)).ToArray();
      return Snapshot(document, siblings);
    }
  }

  public IReadOnlyList<HixCompletion> Complete(string kind, string prefix) =>
    host?.Complete(kind ?? string.Empty, prefix ?? string.Empty) ?? [];

  public IReadOnlyList<HixDefinition> QueryDefinitions(string kind, string receiverType,
    string operandType, string prefix) => definitions.Where(definition =>
      (string.IsNullOrEmpty(kind) || definition.Kind == kind) &&
      (string.IsNullOrEmpty(receiverType) || string.Equals(definition.ReceiverType, "any", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(definition.ReceiverType, receiverType, StringComparison.OrdinalIgnoreCase)) &&
      (string.IsNullOrEmpty(operandType) || definition.OperandType == "Pattern" ||
        string.Equals(definition.OperandType, operandType, StringComparison.OrdinalIgnoreCase)) &&
      (string.IsNullOrEmpty(prefix) || definition.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
    .ToArray();

  private HixDocumentSnapshot Snapshot(CachedDocument file, IReadOnlyList<CachedDocument> siblings) {
    var siblingPatterns = new HashSet<string>(siblings.SelectMany(item => item.Analysis.Declarations)
      .Where(item => item.Kind == "Pattern").Select(item => item.Name), StringComparer.Ordinal);
    var references = file.Analysis.References.Select(reference => {
      if (reference.Kind == "CSharpType")
        return new HixReference(reference.Name, reference.Kind, reference.Range,
          LanguageAnalysis.Scope(reference.Node), host?.Resolve(reference.Kind, reference.Name));
      var effective = reference.Kind == "Function" && siblingPatterns.Contains(reference.Name)
        ? new LanguageAnalysis.Reference(reference.Name, "Pattern", reference.Range, reference.Node) : reference;
      var target = file.Analysis.Resolve(effective, siblings.Select(item => item.Analysis), out var owner);
      var targetDocument = target == null ? null : siblings.First(item => ReferenceEquals(item.Analysis, owner));
      return new HixReference(effective.Name, effective.Kind, effective.Range, LanguageAnalysis.Scope(reference.Node),
        target == null ? null : new HixLocation(targetDocument.Document.Path, target.Range));
    }).ToArray();
    var diagnostics = file.Analysis.Program.Diagnostics.Where(item => !ResolvedPatternDiagnostic(
      item.Message, siblingPatterns)).Select(item => {
        var range = DiagnosticRange(file, item);
        return new HixDiagnostic(item.Message, "Error", range);
      }).ToArray();
    return new HixDocumentSnapshot(file.Document.Path, file.Document.Revision, file.Hash,
      file.Analysis.Declarations.Select(item => new HixSymbol(item.Name, item.Kind, item.Range, item.Scope)).ToArray(),
      references, diagnostics, file.Analysis.TypeFacts.Select(item => new HixTypeFact(item.Range,
        item.Type, item.Documentation ?? string.Empty, item.Inlay, item.Kind)).ToArray());
  }

  private static HixDefinition[] BuildDefinitions(HixBackend backend) {
    var all = backend.Functions.EnumerateAll().ToArray();
    var functions = all.Where(definition => definition.Metadata == HixMetadataKind.None)
      .SelectMany(definition => definition.Signatures.Select(signature =>
      new HixDefinition(definition.Name, "Function", signature.ArgumentCount, signature.IsVariadic, "None",
        (signature.ArgumentPatterns.Count == 0 ? HixPattern.Any : signature.ArgumentPatterns[0]).Display,
        signature.ResultPattern.Display, signature.ArgumentPatterns.Select(type => type.Display).ToArray(),
        definition.Documentation)));
    var roots = backend.Roots.Values.Select(root => new HixDefinition(root.Name, "Root", 0, false, "None",
      "None", root.Pattern.Display, [], "Backend root"));
    var primitives = HixPatterns.PrimitivePatterns.Keys.Select(name => new HixDefinition(
      name, "PrimitivePattern", 0, false, "None", "None", "pattern", [],
      "Primitive pattern matching `" + name + "` values."));
    var types = backend.Functions.Types.Select(type => new HixDefinition(type.Name, "Type", 0, false,
      "None", "None", "pattern", [], type.Name));
    var metadata = all.Where(definition => (definition.Metadata &
        (HixMetadataKind.Pattern | HixMetadataKind.PatternField)) != 0)
      .SelectMany(definition => definition.Signatures.Select(signature => new HixDefinition(definition.Name,
        "PatternMetadata", signature.ArgumentCount, signature.IsVariadic,
        definition.Metadata == HixMetadataKind.PatternField ? "Field" : "Pattern", "None", "Pattern",
        signature.ArgumentPatterns.Select(type => type.Display).ToArray(), definition.Documentation)));
    var fileMetadata = all.Where(definition => (definition.Metadata & HixMetadataKind.File) != 0)
      .SelectMany(definition => definition.Signatures.Select(signature => new HixDefinition(definition.Name,
        "FileMetadata", signature.ArgumentCount, signature.IsVariadic, "File", "None", "None",
        signature.ArgumentPatterns.Select(type => type.Display).ToArray(), definition.Documentation)));
    var typeMetadata = all.Where(definition => (definition.Metadata & HixMetadataKind.TypeDefinition) != 0)
      .SelectMany(definition => definition.Signatures.Select(signature => new HixDefinition(definition.Name,
        "TypeMetadata", signature.ArgumentCount, signature.IsVariadic, "Type", "None", "Pattern",
        signature.ArgumentPatterns.Select(type => type.Display).ToArray(), definition.Documentation)));
    var declarationMetadata = all.Where(definition => (definition.Metadata &
        (HixMetadataKind.FunctionDefinition | HixMetadataKind.MixinDefinition)) != 0)
      .SelectMany(definition => definition.Signatures.Select(signature => new HixDefinition(definition.Name,
        "DeclarationMetadata", signature.ArgumentCount, signature.IsVariadic, "Declaration", "None", "None",
        signature.ArgumentPatterns.Select(type => type.Display).ToArray(), definition.Documentation)));
    var opaqueTypes = backend.Functions.OpaqueTypes.Select(type => new HixDefinition(type.Name, "Type", 0, false,
      "None", "None", "opaque", [], type.Documentation));
    return functions.Concat(roots).Concat(primitives).Concat(types).Concat(opaqueTypes).Concat(metadata).Concat(typeMetadata)
      .Concat(declarationMetadata).Concat(fileMetadata).ToArray();
  }

  private static bool ResolvedPatternDiagnostic(string message, ISet<string> patterns) {
    const string prefix = "unknown pattern '";
    return message != null && message.StartsWith(prefix, StringComparison.Ordinal) &&
      message.EndsWith("'", StringComparison.Ordinal) &&
      patterns.Contains(message.Substring(prefix.Length, message.Length - prefix.Length - 1));
  }
  private static HixSourceRange DiagnosticRange(CachedDocument file, HixParseDiagnostic diagnostic) {
    var visible = file.Analysis.Program.Tokens.Where(token => token.Channel == TokenConstants.DefaultChannel &&
      token.Type >= 0 && token.Line >= diagnostic.Line).ToArray();
    var line = visible.Where(token => token.Line == diagnostic.Line).ToArray();
    var quoted = new HashSet<string>(QuotedNames(diagnostic.Message), StringComparer.Ordinal);
    var token = line.FirstOrDefault(candidate => quoted.Contains(candidate.Text)) ?? line.FirstOrDefault() ??
      visible.FirstOrDefault();
    return token == null ? new HixSourceRange(file.Document.Text.Length, file.Document.Text.Length) :
      HixSourceRange.FromToken(token);
  }
  private static IEnumerable<string> QuotedNames(string message) {
    if (string.IsNullOrEmpty(message)) yield break;
    for (var start = message.IndexOf('\''); start >= 0;) {
      var end = message.IndexOf('\'', start + 1);
      if (end < 0) yield break;
      if (end > start + 1) yield return message.Substring(start + 1, end - start - 1);
      start = message.IndexOf('\'', end + 1);
    }
  }
  private static bool SameDirectory(string left, string right) => string.Equals(DirectoryOf(left),
    DirectoryOf(right), StringComparison.OrdinalIgnoreCase);
  private static string DirectoryOf(string path) => Path.GetDirectoryName(Normalize(path)) ?? string.Empty;
  private static string Normalize(string path) => (path ?? string.Empty).Replace('\\', '/');
  public static long SourceHash(string source) { unchecked { var hash = 1469598103934665603L;
    foreach (var character in source ?? string.Empty) { hash ^= character; hash *= 1099511628211L; } return hash; } }
  private sealed record CachedDocument(HixDocument Document, long Hash, LanguageAnalysis Analysis);
}
