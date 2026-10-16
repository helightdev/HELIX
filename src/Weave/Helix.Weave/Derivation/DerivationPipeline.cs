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
  internal DerivationTarget(WeaveSession session, ISymbol symbol, DerivedProperty property)
    : base(session, symbol) => Property = property;

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

  public ImmutableArray<DerivedProperty> Run(WeaveSession session, IEnumerable<ISymbol> symbols) {
    var builder = ImmutableArray.CreateBuilder<DerivedProperty>();
    var order = 0;
    foreach (var symbol in symbols) builder.Add(Run(session, symbol, order++));
    return builder.ToImmutable();
  }

  public DerivedProperty Run(WeaveSession session, ISymbol symbol, int order = 0) {
    var property = PropertySeed.Seed(symbol, session.Facts, order);
    if (_steps.Length == 0) return property;

    foreach (var step in _steps) {
      var attribute = FindAttribute(session, symbol, step.AttributeName);
      if (attribute is null) continue;
      var target = new DerivationTarget(session, symbol, property);
      property = step.Derive(target, attribute, property) ?? property;
    }
    return property;
  }

  private static AttributeData FindAttribute(WeaveSession session, ISymbol symbol, string metadataName) {
    foreach (var attribute in session.Facts.Attributes(symbol))
      if (attribute.AttributeClass is not null &&
          session.Facts.MetadataName(attribute.AttributeClass) == metadataName)
        return attribute;
    return null;
  }
}
