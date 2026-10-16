using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// What a prelude needs from the compilation while it runs. Lives for one symbol's prelude only;
/// nothing here survives into the cached contribution.
/// </summary>
public sealed class PreludeContext {
  private readonly Dictionary<string, InjectionTarget> _aliases = new(System.StringComparer.Ordinal);

  public PreludeContext(INamedTypeSymbol type, Compilation compilation, SymbolFacts facts,
                        DerivationPipeline derivations) {
    Type = type;
    Compilation = compilation;
    Facts = facts;
    Derivations = derivations ?? DerivationPipeline.Empty;
  }

  /// <summary>The type the generated partial is for.</summary>
  public INamedTypeSymbol Type { get; }
  public Compilation Compilation { get; }
  public SymbolFacts Facts { get; }
  public DerivationPipeline Derivations { get; }

  internal void DefineAlias(string alias, InjectionTarget target) => _aliases[alias] = target;

  internal InjectionTarget ResolveAlias(string alias) =>
    _aliases.TryGetValue(alias, out var target) ? target : null;
}
