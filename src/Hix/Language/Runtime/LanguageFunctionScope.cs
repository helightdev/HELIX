using Hix.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Runtime;

/// <summary>Lexical declarations only: bindings never retain an execution context or caller locals.</summary>
public sealed class LanguageFunctionScope {
    private readonly LanguageFunctionScope parent;
    private readonly IReadOnlyDictionary<string, BytecodeFunction[]> declarations;
    private readonly Dictionary<string, LanguageFunctionCandidate[]> candidates = new(StringComparer.Ordinal);

    public LanguageFunctionScope(IEnumerable<BytecodeFunction> functions, LanguageFunctionScope parent = null) {
        this.parent = parent;
        declarations = functions.GroupBy(function => function.Name, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        foreach (var name in declarations.Keys.Concat(parent?.candidates.Keys ?? Enumerable.Empty<string>()).Distinct())
            candidates.Add(name, BuildCandidates(name).ToArray());
    }

    internal LanguageFunctionScope Parent => parent;
    public HixProgramImage Program { get; private set; }

    public IReadOnlyList<LanguageFunctionCandidate> Candidates(string name) => candidates.TryGetValue(name, out var result) ? result : Array.Empty<LanguageFunctionCandidate>();

    public IEnumerable<LanguageFunctionCandidate> AllCandidates() => candidates.Values.SelectMany(value => value).Distinct();

    public IEnumerable<BytecodeFunctionEntry> Functions(bool includeParent = true) {
        if (includeParent && parent != null)
            foreach (var entry in parent.Functions())
                yield return entry;
        foreach (var function in declarations.Values.SelectMany(group => group))
            yield return new BytecodeFunctionEntry(function, this);
    }

    public LanguageFunctionCandidate Resolve(SignatureHixPattern signature) {
        var identity = HixGenericTypes.SignatureIdentity(signature);
        return Candidates(signature.Name).SingleOrDefault(candidate => candidate.SignatureIdentity == identity);
    }

    public bool Contains(string name) => candidates.ContainsKey(name);

    public NamedFunctionHixValue Bind(string name) => Contains(name) ? new NamedFunctionHixValue(Program.StringPool.Get(name)) {
        Scope = this
    }

    : null;

    public static string SignatureKey(BytecodeSignature signature) => signature == null ? "(*)" : signature.Inputs == null ? signature.InputPattern == null ? "(*)" : HixGenericTypes.PatternIdentity(signature.InputPattern) : "(" + string.Join(",", signature.Inputs.Select(field => (field.Variadic ? "..." : "") + HixGenericTypes.PatternIdentity(field.Pattern))) + ")";

    public IEnumerable<(string Scope, BytecodeFunction Function)> DisassemblyFunctions(string name, bool includeParent = true) {
        if (includeParent && parent != null)
            foreach (var item in parent.DisassemblyFunctions("global"))
                yield return item;
        foreach (var function in declarations.Values.SelectMany(group => group))
            yield return (name, function);
    }

    public void Fingerprint(HixFingerprintBuilder builder) {
        Program?.Fingerprint(builder);
        parent?.Fingerprint(builder);
        foreach (var group in declarations.OrderBy(entry => entry.Key, StringComparer.Ordinal)) {
            builder.Append(group.Key);
            foreach (var function in group.Value) {
                builder.Append(function.Body);
                builder.Append(function.IsPure ? 1 : 0);
                HixMetadataCompiler.Fingerprint(builder, function.Metadata);
                foreach (var signature in function.Signatures) {
                    builder.Append(signature.InputPattern == null ? null : HixGenericTypes.PatternIdentity(signature.InputPattern));
                    builder.Append(signature.OutputPattern == null ? null : HixGenericTypes.PatternIdentity(signature.OutputPattern));
                    foreach (var field in (signature.Inputs ?? Array.Empty<BytecodeField>()).Concat(signature.Outputs ?? Array.Empty<BytecodeField>())) {
                        builder.Append(field.Name);
                        builder.Append(HixGenericTypes.PatternIdentity(field.Pattern));
                        builder.Append(field.Variadic);
                        HixMetadataCompiler.Fingerprint(builder, field.Metadata);
                    }
                }
            }
        }
    }

    internal void Attach(HixProgramImage program) {
        Program = program;
        parent?.Attach(program);
    }

    private IEnumerable<LanguageFunctionCandidate> BuildCandidates(string name) {
        var shadowed = new HashSet<string>(StringComparer.Ordinal);
        if (declarations.TryGetValue(name, out var functions))
            foreach (var function in functions) {
                foreach (var signature in function.Signatures.Count == 0 ? new BytecodeSignature[] {
                    null
                }

                : function.Signatures) {
                    shadowed.Add(SignatureKey(signature));
                    yield return new LanguageFunctionCandidate(function, signature, this);
                }
            }

        if (parent != null)
            foreach (var candidate in parent.Candidates(name))
                if (!shadowed.Contains(SignatureKey(candidate.Signature)))
                    yield return candidate;
    }
}

public sealed record LanguageFunctionCandidate(BytecodeFunction Function, BytecodeSignature Signature, LanguageFunctionScope Owner) {
    public string SignatureDisplay { get; } = Signature?.Constant(Function.Name).Display;
    public string SignatureIdentity { get; } = Signature == null ? null : HixGenericTypes.SignatureIdentity(Signature.Constant(Function.Name));
    public bool Variadic { get; } = Signature?.Inputs is { Count: > 0 } fields && fields[fields.Count - 1].Variadic;
    public int FixedCount { get; } = Signature?.Inputs is { } fields ? fields.Count(field => !field.AllowsMissing && !field.Variadic) : 0;
    public int BaseScore { get; } = Signature == null ? -10000 : Signature.Inputs == null ? Signature.InputPattern == null ? -9999 : Signature.InputKind == "any" ? 1000 : 1001 : (Signature.Inputs.Count > 0 && Signature.Inputs[Signature.Inputs.Count - 1].Variadic ? 0 : 1000) + Signature.Inputs.Count(field => field.Kind != "any");
}
