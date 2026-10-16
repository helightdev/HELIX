using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>A diagnostic a mixin reported, resolved against the symbol it was reported on.</summary>
public readonly struct WeaveDiagnostic {
  public WeaveDiagnostic(string id, string message, Location location, DiagnosticSeverity severity) {
    Id = id;
    Message = message;
    Location = location;
    Severity = severity;
  }

  public string Id { get; }
  public string Message { get; }
  public Location Location { get; }
  public DiagnosticSeverity Severity { get; }

  public Diagnostic ToDiagnostic() => Diagnostic.Create(
    new DiagnosticDescriptor(Id, "Weave", "{0}", "Weave", Severity, isEnabledByDefault: true),
    Location ?? Location.None, Message);
}

/// <summary>
/// Everything accumulated for one generated type: emissions, state slots and diagnostics.
/// One session per target type per generation pass.
/// </summary>
public sealed class WeaveSession {
  private readonly Dictionary<(int, ISymbol), object> _state = new();
  private readonly List<WeaveDiagnostic> _diagnostics = new();
  private readonly Dictionary<string, InjectionTarget> _targets = new(System.StringComparer.Ordinal);
  private readonly Dictionary<string, InjectionTarget> _aliases = new(System.StringComparer.Ordinal);

  public WeaveSession(INamedTypeSymbol type, SymbolFacts facts, Compilation compilation = null,
                      DerivationPipeline derivations = null) {
    Type = type;
    Facts = facts;
    Pool = facts.Pool;
    Compilation = compilation;
    Derivations = derivations ?? DerivationPipeline.Empty;
    Outputs = new OutputBuffer();
  }

  public INamedTypeSymbol Type { get; }
  public Compilation Compilation { get; }
  public DerivationPipeline Derivations { get; }
  public SymbolFacts Facts { get; }
  public StringPool Pool { get; }
  public OutputBuffer Outputs { get; }
  public IReadOnlyList<WeaveDiagnostic> Diagnostics => _diagnostics;

  /// <summary>Injection targets any mixin wove into, so the renderer knows their signatures.</summary>
  public IReadOnlyDictionary<string, InjectionTarget> Targets => _targets;

  internal void DeclareTarget(InjectionTarget target) {
    if (!_targets.ContainsKey(target.Name)) _targets.Add(target.Name, target);
  }

  /// <summary>
  /// Per-type target alias, Hix's <c>defineTarget</c>. The same alias means different methods on
  /// different types — a boundary element and a boundary mixin both define <c>$Compose</c>.
  /// </summary>
  internal void DefineAlias(string alias, InjectionTarget target) => _aliases[alias] = target;

  internal InjectionTarget ResolveAlias(string alias) =>
    _aliases.TryGetValue(alias, out var target) ? target : null;

  internal void Report(WeaveDiagnostic diagnostic) => _diagnostics.Add(diagnostic);

  internal T GetState<T>(Slot<T> slot, ISymbol target) {
    var key = (slot.Id, slot.Scope == Scope.Type ? Type : target);
    return _state.TryGetValue(key, out var value) ? (T)value : slot.Default;
  }

  internal void SetState<T>(Slot<T> slot, ISymbol target, T value) {
    var key = (slot.Id, slot.Scope == Scope.Type ? (ISymbol)Type : target);
    _state[key] = value;
  }
}
