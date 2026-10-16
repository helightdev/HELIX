using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// The target a derivation step sees: the property so far, plus the symbol it came from.
/// Derivations run inside another mixin's prelude, so symbol access is fine here.
/// </summary>
public sealed class DerivationTarget : SymbolTarget {
  internal DerivationTarget(ContributionBuffer buffer, ISymbol symbol, PreludeContext context,
                            DerivedProperty property)
    : base(buffer, symbol, context) => Property = property;

  /// <summary>The property as earlier steps left it.</summary>
  public DerivedProperty Property { get; }

  /// <summary>The declared type of the field or parameter.</summary>
  public TypeRef DeclaredType => Facts.Ref(Symbol switch {
    IFieldSymbol field => field.Type,
    IParameterSymbol parameter => parameter.Type,
    IPropertySymbol property => property.Type,
    _ => null,
  });

  public bool IsParameter => Symbol is IParameterSymbol;
  public bool HasExplicitDefault => Symbol is IParameterSymbol { HasExplicitDefaultValue: true };
  public object ExplicitDefault => Symbol is IParameterSymbol { HasExplicitDefaultValue: true } parameter
    ? parameter.ExplicitDefaultValue
    : null;
}

/// <summary>
/// The ordered derivation steps, applied to every seeded property whose symbol carries the
/// step's attribute. Built once from the registry; consulted by DeriveFields/DeriveParameters.
/// </summary>
public sealed class DerivationPipeline {
  public static readonly DerivationPipeline Empty = new(System.Array.Empty<Mixin>());

  private readonly Mixin[] _steps;

  public DerivationPipeline(IEnumerable<Mixin> steps) =>
    _steps = steps.Where(step => step.Unit == Unit.Derivation)
      .OrderBy(step => step.Order)
      .ToArray();

  public int Count => _steps.Length;

  public ImmutableArray<DerivedProperty> Run(SymbolTarget owner, IEnumerable<ISymbol> symbols) {
    var builder = ImmutableArray.CreateBuilder<DerivedProperty>();
    var order = 0;
    foreach (var symbol in symbols) builder.Add(Run(owner, symbol, order++));
    return builder.ToImmutable();
  }

  public DerivedProperty Run(SymbolTarget owner, ISymbol symbol, int order = 0) {
    var property = PropertySeed.Seed(symbol, owner.Facts, order);
    if (_steps.Length == 0) return property;

    foreach (var step in _steps) {
      var attribute = FindAttribute(owner.Facts, symbol, step.AttributeName);
      if (attribute is null) continue;
      var target = new DerivationTarget(owner.Buffer, symbol, owner.Context, property);
      property = step.Derive(target, attribute, property) ?? property;
    }
    return property;
  }

  private static AttributeData FindAttribute(SymbolFacts facts, ISymbol symbol, string metadataName) {
    foreach (var attribute in facts.Attributes(symbol))
      if (attribute.AttributeClass is not null &&
          facts.MetadataName(attribute.AttributeClass) == metadataName)
        return attribute;
    return null;
  }
}
