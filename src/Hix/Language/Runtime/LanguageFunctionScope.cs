using Hix.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Runtime;

/// <summary>Lexical declarations only: bindings never retain an execution context or caller locals.</summary>
public sealed class LanguageFunctionScope {
  private readonly LanguageFunctionScope parent;
  public HixProgramImage Program { get; private set; }
  internal void Attach(HixProgramImage program) { Program = program; parent?.Attach(program); }
  private readonly IReadOnlyDictionary<string, BytecodeFunction[]> declarations;
  private readonly Dictionary<string, LanguageFunctionCandidate[]> candidates = new(StringComparer.Ordinal);

  public LanguageFunctionScope(IEnumerable<BytecodeFunction> functions, LanguageFunctionScope parent = null) {
    this.parent = parent;
    declarations = functions.GroupBy(function => function.Name, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    foreach (var name in declarations.Keys.Concat(parent?.candidates.Keys ?? Enumerable.Empty<string>()).Distinct())
      candidates.Add(name, BuildCandidates(name).ToArray());
  }

  public IReadOnlyList<LanguageFunctionCandidate> Candidates(string name) =>
    candidates.TryGetValue(name, out var result) ? result : Array.Empty<LanguageFunctionCandidate>();
  public IEnumerable<LanguageFunctionCandidate> AllCandidates() => candidates.Values.SelectMany(value => value).Distinct();
  public LanguageFunctionCandidate Resolve(SignatureHixPattern signature) {
    var display = signature.Display;
    return Candidates(signature.Name).SingleOrDefault(candidate => candidate.SignatureDisplay == display);
  }

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

  public bool Contains(string name) => candidates.ContainsKey(name);
  public NamedFunctionHixValue Bind(string name) => Contains(name)
    ? new NamedFunctionHixValue(Program.StringPool.Get(name)) { Scope = this } : null;

  public static string SignatureKey(BytecodeSignature signature) => signature == null ? "(*)" :
    signature.Inputs == null ? signature.InputPattern == null ? "(*)" : signature.InputKind : "(" + string.Join(",", signature.Inputs.Select(field =>
      (field.Variadic ? "..." : "") + field.Kind)) + ")";

  public IEnumerable<(string Scope, BytecodeFunction Function)> DisassemblyFunctions(string name, bool includeParent = true) {
    if (includeParent && parent != null)
      foreach (var item in parent.DisassemblyFunctions("global")) yield return item;
    foreach (var function in declarations.Values.SelectMany(group => group)) yield return (name, function);
  }

  public void Fingerprint(HixFingerprintBuilder builder) {
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

public sealed record LanguageFunctionCandidate(BytecodeFunction Function, BytecodeSignature Signature,
  LanguageFunctionScope Owner) {
  public string SignatureDisplay { get; } = Signature?.Constant(Function.Name).Display;
  public bool Variadic { get; } = Signature?.Inputs is {Count: > 0} fields && fields[fields.Count - 1].Variadic;
  public int FixedCount { get; } = Signature?.Inputs is { } fields
    ? fields.Count(field => !field.AllowsMissing && !field.Variadic) : 0;
  public int BaseScore { get; } = Signature == null ? -10000 : Signature.Inputs == null
    ? Signature.InputPattern == null ? -9999 : Signature.InputKind == "any" ? 1000 : 1001
    : (Signature.Inputs.Count > 0 && Signature.Inputs[Signature.Inputs.Count - 1].Variadic ? 0 : 1000)
      + Signature.Inputs.Count(field => field.Kind != "any");
}
