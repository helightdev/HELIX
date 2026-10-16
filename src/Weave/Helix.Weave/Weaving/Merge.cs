using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Helix.Weave;

/// <summary>The merged result for one type: emissions, methods and diagnostics, as text.</summary>
public sealed class MergedType {
  public MergedType(TypeShape type, ImmutableArray<Emission> emissions,
                    ImmutableArray<ResolvedTarget> targets,
                    ImmutableArray<ReportedDiagnostic> diagnostics) {
    Type = type;
    Emissions = emissions;
    Targets = targets;
    Diagnostics = diagnostics;
  }

  public TypeShape Type { get; }
  public ImmutableArray<Emission> Emissions { get; }
  public ImmutableArray<ResolvedTarget> Targets { get; }
  public ImmutableArray<ReportedDiagnostic> Diagnostics { get; }
}

/// <summary>
/// Brings a type's contributions together: drops the duplicate copies of shared blocks, keeps
/// one declaration per injection target, and checks uniqueness claims.
///
/// All of this is pure data work. The expensive symbol analysis already happened in the preludes,
/// which the pipeline cached per symbol.
/// </summary>
public static class Merge {
  public static MergedType Run(TypeContribution contribution, IReadOnlyList<Mixin> mixins) {
    var ordered = contribution.Contributions
      .OrderBy(item => item.Order)
      .ThenBy(item => item.MixinName, StringComparer.Ordinal)
      .ToList();

    var aliases = new Dictionary<string, InjectionTarget>(StringComparer.Ordinal);
    foreach (var item in ordered)
      foreach (var alias in item.Aliases)
        aliases[alias.Key] = alias.ToTarget();

    var emissions = new List<Emission>();
    var targets = new List<ResolvedTarget>();
    var diagnostics = new List<ReportedDiagnostic>();
    // Which contribution owns each shared block: the first one to emit it keeps *all* of its
    // emissions for that key, and later contributions' copies are dropped wholesale.
    var shared = new Dictionary<string, int>(StringComparer.Ordinal);
    var position = 0;
    var unique = new Dictionary<string, UniqueClaim>(StringComparer.Ordinal);
    var sequence = 0;

    foreach (var item in ordered) {
      diagnostics.AddRange(item.Diagnostics);

      foreach (var target in item.Targets)
        if (!targets.Any(existing => string.Equals(existing.Name, target.Name, StringComparison.Ordinal)))
          targets.Add(target);

      // A shared block is emitted by every symbol that needs it; keep one copy.
      foreach (var emission in item.Emissions) {
        if (emission.SharedKey is not null) {
          if (!shared.TryGetValue(emission.SharedKey, out var owner)) shared[emission.SharedKey] = owner = position;
          if (owner != position) continue;
        }
        emissions.Add(new Emission(emission.Target, emission.Text, emission.InjectionTarget,
                                   emission.Priority, sequence++, emission.SharedKey));
      }
      position++;

      foreach (var claim in item.Uniques)
        if (unique.TryGetValue(claim.Key, out var first))
          diagnostics.Add(new ReportedDiagnostic(claim.Id, claim.Message, "Error", null, 0, 0));
        else unique[claim.Key] = claim;
    }

    return new MergedType(contribution.Type, emissions.ToImmutableArray(),
                          targets.ToImmutableArray(), diagnostics.ToImmutableArray());
  }

  /// <summary>Groups per-symbol contributions by the type each belongs to.</summary>
  public static ImmutableArray<TypeContribution> ByType(ImmutableArray<SymbolContribution> contributions) {
    if (contributions.IsDefaultOrEmpty) return ImmutableArray<TypeContribution>.Empty;

    var grouped = new Dictionary<string, List<SymbolContribution>>(StringComparer.Ordinal);
    var shapes = new Dictionary<string, TypeShape>(StringComparer.Ordinal);

    foreach (var contribution in contributions) {
      if (contribution is null) continue;
      var key = contribution.Type.Key;
      if (!grouped.TryGetValue(key, out var list)) {
        grouped.Add(key, list = new List<SymbolContribution>());
        shapes[key] = contribution.Type;
      }
      list.Add(contribution);
    }

    var builder = ImmutableArray.CreateBuilder<TypeContribution>(grouped.Count);
    foreach (var key in grouped.Keys.OrderBy(item => item, StringComparer.Ordinal))
      builder.Add(new TypeContribution(shapes[key], grouped[key].ToImmutableArray()));
    return builder.MoveToImmutable();
  }
}
