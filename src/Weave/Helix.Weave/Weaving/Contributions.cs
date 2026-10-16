using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// Where a prelude writes. One buffer per annotated symbol, holding nothing but value data, so
/// the whole thing can be a cache key for the incremental pipeline.
/// </summary>
public sealed class ContributionBuffer {
  private readonly List<Emission> _emissions = new();
  private readonly List<ResolvedTarget> _targets = new();
  private readonly List<KeyValuePairModel> _aliases = new();
  private readonly List<ReportedDiagnostic> _diagnostics = new();
  private readonly List<UniqueClaim> _uniques = new();
  private readonly Dictionary<int, object> _state = new();
  private readonly HashSet<string> _claimed = new(StringComparer.Ordinal);
  private int _sequence;

  /// <summary>Set while inside a shared scope; tags emissions for merge-time deduplication.</summary>
  internal string SharedKey { get; private set; }

  public IReadOnlyList<ResolvedTarget> Targets => _targets;

  internal void Add(EmissionTarget target, string text, string injectionTarget = null, int priority = 0) {
    if (string.IsNullOrWhiteSpace(text)) return;
    _emissions.Add(new Emission(target, Code.Dedent(text), injectionTarget, priority, _sequence++, SharedKey));
  }

  internal void AddTarget(ResolvedTarget target) {
    foreach (var existing in _targets)
      if (string.Equals(existing.Name, target.Name, StringComparison.Ordinal)) return;
    _targets.Add(target);
  }

  internal void AddAlias(KeyValuePairModel alias) => _aliases.Add(alias);
  internal void Report(ReportedDiagnostic diagnostic) => _diagnostics.Add(diagnostic);
  internal void Claim(UniqueClaim claim) => _uniques.Add(claim);

  /// <summary>
  /// Opens a shared scope: every symbol that needs the block emits it, and the merge keeps one
  /// copy per type. Replaces cross-symbol state, which a cached prelude cannot have.
  /// </summary>
  internal IDisposable Shared(string key) {
    var previous = SharedKey;
    SharedKey = key;
    return new Scope(this, previous);
  }

  internal bool ClaimOnce(string key) => _claimed.Add(key);

  internal T GetState<T>(Slot<T> slot) => _state.TryGetValue(slot.Id, out var value) ? (T)value : slot.Default;
  internal void SetState<T>(Slot<T> slot, T value) => _state[slot.Id] = value;

  internal SymbolContribution Build(TypeShape type, int order, string mixinName, object carry, int lateIndex) =>
    new(type, order, mixinName, _emissions.ToImmutableArray(), _targets.ToImmutableArray(),
        _aliases.ToImmutableArray(), _diagnostics.ToImmutableArray(), _uniques.ToImmutableArray(),
        carry, lateIndex);

  private sealed class Scope : IDisposable {
    private readonly ContributionBuffer _owner;
    private readonly string _previous;
    public Scope(ContributionBuffer owner, string previous) {
      _owner = owner;
      _previous = previous;
    }
    public void Dispose() => _owner.SharedKey = _previous;
  }
}

/// <summary>
/// Resolves an injection target against a type once, while symbols are in hand, so that nothing
/// downstream needs them. Port of what the weaver used to do at render time.
/// </summary>
public static class TargetResolver {
  public static ResolvedTarget Resolve(INamedTypeSymbol type, Compilation compilation,
                                       SymbolFacts facts, InjectionTarget target) {
    var existing = FindMethod(type, target.Name);
    var modifiers = (target.IsPublic ? "public" : "private") + (target.IsStatic ? " static" : "");

    // A partial declaration in the user's own type wins: we implement it.
    if (existing is { IsPartialDefinition: true })
      return new ResolvedTarget(target.Name, DeclarationFrom(facts, existing, partial: true), null);

    // An inherited virtual or abstract method: override and chain.
    if (existing is not null && !SymbolEqualityComparer.Default.Equals(existing.ContainingType, type) &&
        (existing.IsVirtual || existing.IsAbstract || existing.IsOverride)) {
      var call = existing.IsAbstract
        ? null
        : "base." + existing.Name + "(" + string.Join(", ", Arguments(existing)) + ");";
      return new ResolvedTarget(target.Name, DeclarationFrom(facts, existing, partial: false), call);
    }

    var parameters = DelegateParameters(compilation, facts, target);
    return new ResolvedTarget(target.Name, modifiers + " void " + target.Name + "(" + parameters + ")", null);
  }

  private static IEnumerable<string> Arguments(IMethodSymbol method) {
    foreach (var parameter in method.Parameters) yield return RefPrefix(parameter.RefKind) + parameter.Name;
  }

  private static IMethodSymbol FindMethod(INamedTypeSymbol type, string name) {
    for (var current = type; current is not null; current = current.BaseType)
      foreach (var member in current.GetMembers(name))
        if (member is IMethodSymbol method) return method;
    return null;
  }

  private static string DeclarationFrom(SymbolFacts facts, IMethodSymbol method, bool partial) {
    var accessibility = method.DeclaredAccessibility switch {
      Accessibility.Public => "public ",
      Accessibility.Internal => "internal ",
      Accessibility.Protected => "protected ",
      _ => partial ? "" : "private ",
    };
    var statics = method.IsStatic ? "static " : "";
    var keyword = partial ? "partial " : "override ";
    var parameters = new List<string>();
    foreach (var parameter in method.Parameters)
      parameters.Add(RefPrefix(parameter.RefKind) + facts.Qualified(parameter.Type) + " " + parameter.Name);
    return accessibility + statics + keyword + facts.Qualified(method.ReturnType) + " " + method.Name +
           "(" + string.Join(", ", parameters) + ")";
  }

  /// <summary>Parameters taken from the target's delegate type, so the signature matches.</summary>
  private static string DelegateParameters(Compilation compilation, SymbolFacts facts, InjectionTarget target) {
    if (target.DelegateType is null) return "";
    var resolved = facts.Lookup(compilation, target.DelegateType);
    if (resolved is not { TypeKind: TypeKind.Delegate } || resolved.DelegateInvokeMethod is null) return "";

    var parameters = new List<string>();
    foreach (var parameter in resolved.DelegateInvokeMethod.Parameters)
      parameters.Add(RefPrefix(parameter.RefKind) + facts.Qualified(parameter.Type) + " " + parameter.Name);
    return string.Join(", ", parameters);
  }

  private static string RefPrefix(RefKind kind) => kind switch {
    RefKind.Ref => "ref ",
    RefKind.Out => "out ",
    RefKind.In => "in ",
    _ => "",
  };
}
