using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Runtime;

/// <summary>Lexical declarations only: bindings never retain an execution context or caller locals.</summary>
internal sealed class LanguageFunctionScope {
  private readonly LanguageFunctionScope parent;
  internal HixExpressionExecutionProgram Program { get; private set; }
  internal void Attach(HixExpressionExecutionProgram program) { Program = program; parent?.Attach(program); }
  private readonly IReadOnlyDictionary<string, BytecodeFunction[]> declarations;
  private readonly Dictionary<string, LanguageFunctionCandidate[]> candidates = new(StringComparer.Ordinal);

  internal LanguageFunctionScope(IEnumerable<BytecodeFunction> functions, LanguageFunctionScope parent = null) {
    this.parent = parent;
    declarations = functions.GroupBy(function => function.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    foreach (var name in declarations.Keys.Concat(parent?.candidates.Keys ?? Enumerable.Empty<string>()).Distinct())
      candidates.Add(name, BuildCandidates(name).ToArray());
  }

  internal IReadOnlyList<LanguageFunctionCandidate> Candidates(string name) =>
    candidates.TryGetValue(name, out var result) ? result : Array.Empty<LanguageFunctionCandidate>();
  internal IEnumerable<LanguageFunctionCandidate> AllCandidates() => candidates.Values.SelectMany(value => value).Distinct();
  internal LanguageFunctionCandidate Resolve(SignatureHixPattern signature) => Candidates(signature.Name)
    .SingleOrDefault(candidate => candidate.Signature?.Constant(signature.Name).Display == signature.Display);

  private IEnumerable<LanguageFunctionCandidate> BuildCandidates(string name) {
    var shadowed = new HashSet<string>(StringComparer.Ordinal);
    if (declarations.TryGetValue(name, out var functions))
      foreach (var function in functions) {
        foreach (var signature in function.Signatures.Count == 0
          ? new BytecodeSignature[] {null} : function.Signatures) {
          shadowed.Add(SignatureKey(signature));
          yield return new LanguageFunctionCandidate(function, signature, this);
        }
      }
    if (parent != null)
      foreach (var candidate in parent.Candidates(name))
        if (!shadowed.Contains(SignatureKey(candidate.Signature))) yield return candidate;
  }

  internal bool Contains(string name) => candidates.ContainsKey(name);
  internal NamedFunctionHixValue Bind(string name) => Contains(name)
    ? new NamedFunctionHixValue(name) { Scope = this } : null;

  internal static string SignatureKey(BytecodeSignature signature) => signature == null ? "(*)" :
    signature.Inputs == null ? signature.InputKind : "(" + string.Join(",", signature.Inputs.Select(field =>
      (field.Variadic ? "..." : "") + field.Kind)) + ")";

  internal IEnumerable<(string Scope, BytecodeFunction Function)> DisassemblyFunctions(string name, bool includeParent = true) {
    if (includeParent && parent != null)
      foreach (var item in parent.DisassemblyFunctions("global")) yield return item;
    foreach (var function in declarations.Values.SelectMany(group => group)) yield return (name, function);
  }

  internal void Fingerprint(HixFingerprintBuilder builder) {
    Program?.Fingerprint(builder);
    parent?.Fingerprint(builder);
    foreach (var group in declarations.OrderBy(entry => entry.Key, StringComparer.Ordinal)) {
      builder.Append(group.Key);
      foreach (var function in group.Value) {
        builder.Append(function.Body);
        builder.Append(function.IsPure ? 1 : 0);
        foreach (var signature in function.Signatures) {
          builder.Append(signature.InputKind); builder.Append(signature.OutputKind);
          foreach (var field in (signature.Inputs ?? Array.Empty<BytecodeField>()).Concat(signature.Outputs ?? Array.Empty<BytecodeField>())) {
            builder.Append(field.Name); builder.Append(field.Kind); builder.Append(field.Variadic);
          }
        }

      }
    }
  }
}

internal sealed record LanguageFunctionCandidate(BytecodeFunction Function, BytecodeSignature Signature,
  LanguageFunctionScope Owner) {
  internal bool Variadic { get; } = Signature?.Inputs is {Count: > 0} fields && fields[fields.Count - 1].Variadic;
  internal int FixedCount { get; } = Signature?.Inputs is { } fields
    ? fields.Count(field => !field.Optional && !field.Variadic) : 0;
  internal int BaseScore { get; } = Signature == null ? -10000 : Signature.Inputs == null
    ? Signature.InputKind == "any" ? 1000 : 1001
    : (Signature.Inputs.Count > 0 && Signature.Inputs[Signature.Inputs.Count - 1].Variadic ? 0 : 1000)
      + Signature.Inputs.Count(field => field.Kind != "any");
}
