using System.Collections.Generic;
using Hix.Compiler;

namespace Hix.Standalone.Analysis;

public sealed record HixDocument(string Path, string Text, long Revision);
public sealed record HixLocation(string Path, HixSourceRange Range);
public sealed record HixSymbol(string Name, string Kind, HixSourceRange Range, HixSourceRange Scope);
public sealed record HixReference(string Name, string Kind, HixSourceRange Range, HixSourceRange Scope,
  HixLocation Target = null);
public sealed record HixDiagnostic(string Message, string Severity, HixSourceRange Range);
public sealed record HixCompletion(string Name, string InsertText, string Kind, string Documentation,
  HixLocation Target = null);
public sealed record HixTypeFact(HixSourceRange Range, string Type, string Documentation, bool Inlay,
  string Kind);
public sealed record HixDefinition(string Name, string Kind, int ArgumentCount, bool Variadic,
  string OperandType, string ReceiverType, string ResultType, IReadOnlyList<string> ArgumentTypes,
  string Documentation);
public sealed record HixDocumentSnapshot(string Path, long Revision, long SourceHash,
  IReadOnlyList<HixSymbol> Declarations, IReadOnlyList<HixReference> References,
  IReadOnlyList<HixDiagnostic> Diagnostics, IReadOnlyList<HixTypeFact> TypeFacts);

/// Optional host-language bridge. A standalone server can omit it; Rider supplies C# symbols.
public interface IHixAnalyzerHost {
  HixLocation Resolve(string kind, string name);
  IReadOnlyList<HixCompletion> Complete(string kind, string prefix);
}
