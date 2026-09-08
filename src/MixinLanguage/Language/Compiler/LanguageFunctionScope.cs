using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Compiler;

/// <summary>Lexical declarations only: bindings never retain an execution context or caller locals.</summary>
internal sealed class LanguageFunctionScope {
  private readonly LanguageFunctionScope parent;
  private readonly IReadOnlyDictionary<string, FunctionDeclarationAst[]> declarations;
  private readonly Dictionary<string, LanguageFunctionCandidate[]> candidates = new(StringComparer.Ordinal);

  internal LanguageFunctionScope(IEnumerable<FunctionDeclarationAst> functions, LanguageFunctionScope parent = null) {
    this.parent = parent;
    declarations = functions.GroupBy(function => function.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    foreach (var name in declarations.Keys.Concat(parent?.candidates.Keys ?? Enumerable.Empty<string>()).Distinct())
      candidates.Add(name, BuildCandidates(name).ToArray());
  }

  internal IReadOnlyList<LanguageFunctionCandidate> Candidates(string name) =>
    candidates.TryGetValue(name, out var result) ? result : Array.Empty<LanguageFunctionCandidate>();

  private IEnumerable<LanguageFunctionCandidate> BuildCandidates(string name) {
    var shadowed = new HashSet<string>(StringComparer.Ordinal);
    if (declarations.TryGetValue(name, out var functions))
      foreach (var function in functions) {
        foreach (var signature in function.Signatures.Count == 0
          ? new FunctionSignature[] {null} : function.Signatures) {
          shadowed.Add(SignatureKey(signature));
          yield return new LanguageFunctionCandidate(function, signature, this);
        }
      }
    if (parent != null)
      foreach (var candidate in parent.Candidates(name))
        if (!shadowed.Contains(SignatureKey(candidate.Signature))) yield return candidate;
  }

  internal bool Contains(string name) => candidates.ContainsKey(name);
  internal NamedFunctionMixinValue Bind(string name) => Contains(name)
    ? new NamedFunctionMixinValue(name) { Scope = this } : null;

  internal static string SignatureKey(FunctionSignature signature) => signature == null ? "(*)" :
    signature.Inputs == null ? signature.InputKind : "(" + string.Join(",", signature.Inputs.Select(field =>
      (field.Variadic ? "..." : "") + field.Kind)) + ")";

  internal void Fingerprint(MixinFingerprintBuilder builder) {
    parent?.Fingerprint(builder);
    foreach (var group in declarations.OrderBy(entry => entry.Key, StringComparer.Ordinal)) {
      builder.Append(group.Key);
      foreach (var function in group.Value) {
        HixAst root = function;
        while (root.Parent != null) root = root.Parent;
        builder.Append(root is CompilationUnitAst unit ? unit.Source : "");
        builder.Append(function.SourceRange.Start);
        builder.Append(function.SourceRange.End);
      }
    }
  }
}

internal sealed record LanguageFunctionCandidate(FunctionDeclarationAst Function, FunctionSignature Signature,
  LanguageFunctionScope Owner);
