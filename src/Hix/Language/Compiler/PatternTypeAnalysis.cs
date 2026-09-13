using System.Collections.Generic;

namespace Hix.Compiler;

/// <summary>Compatibility entry points for the shared pattern binder.</summary>
public static class PatternTypeAnalysis {
  public static DelegateHixPattern InferCallable(ExpressionIr expression, DelegateHixPattern context,
    IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions, IReadOnlyDictionary<string, HixPattern> patterns,
    HixBackend backend, out bool valid, bool invocationContext = false) =>
    PatternBinding.InferCallable(expression, context, functions, patterns, backend, out valid, invocationContext);
  public static void Validate(IReadOnlyList<HixIrNode> declarations, IReadOnlyDictionary<string, HixPattern> patterns,
    ICollection<HixParseDiagnostic> diagnostics, HixBackend backend = null) =>
    PatternBinding.Validate(declarations, patterns, diagnostics, backend);
  internal static HixPattern Member(HixPattern receiver, string name, IReadOnlyDictionary<string, HixPattern> patterns) =>
    PatternBinding.Member(receiver, name, patterns);
}
