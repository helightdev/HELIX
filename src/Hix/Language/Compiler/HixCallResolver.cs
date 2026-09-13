using System;
using System.Collections.Generic;

namespace Hix.Compiler;

public sealed record HixCallResolution(SignatureHixPattern Signature,
  IReadOnlyDictionary<HixTypeParameter, HixPattern> Substitutions, ExpressionIr[] Arguments,
  HixCallBinding Binding, int Score, bool Generic) {
  public string DeclarationSignature { get; init; }
}

/// <summary>Compatibility entry points for the shared pattern binder.</summary>
public static class HixCallResolver {
  public static HixCallResolution Resolve(CallExpressionIr call,
    IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions, HixBackend backend,
    IReadOnlyDictionary<string, HixPattern> patterns, Func<ExpressionIr, HixPattern> infer,
    out bool ambiguous) => PatternBinding.Resolve(call, functions, backend, patterns, infer, out ambiguous);
  public static bool Unify(HixPattern expected, HixPattern actual,
    IDictionary<HixTypeParameter, HixPattern> substitutions, IReadOnlyDictionary<string, HixPattern> patterns) =>
    PatternBinding.Unify(expected, actual, substitutions, patterns);
  public static bool Arrange(CallExpressionIr call, IReadOnlyList<HixPatternField> fields,
    IReadOnlyList<ExpressionIr> defaults, bool variadic, out ExpressionIr[] arguments) =>
    PatternBinding.Arrange(call, fields, defaults, variadic, out arguments);
}
