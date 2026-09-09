using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Hix.Compiler;
using Hix.Env;

namespace Hix.Runtime;

public sealed class RoslynValueCache {
  private readonly Dictionary<object, Dictionary<string, IHixValue>> _derived =
    new(ReferenceObjectComparer.Instance);
  private readonly Dictionary<string, IHixValue> _roots = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<object, object> _snapshots =
    new(ReferenceObjectComparer.Instance);

  public bool TryGetRoot(string key, out IHixValue value) {
    if (_roots.TryGetValue(key, out value)) {
      HixProfiler.Increment("cache.root.hit");
      return true;
    }
    HixProfiler.Increment("cache.root.miss");
    value = null;
    return false;
  }

  public void StoreRoot(string key, IHixValue value) {
    _roots.Add(key, value ?? NullHixValue.Instance);
  }

  public bool TryGetDerived(object subject, string member, out IHixValue value) {
    if (_derived.TryGetValue(subject, out var members) && members.TryGetValue(member, out value)) {
      HixProfiler.Increment("cache.derived.hit");
      return true;
    }
    HixProfiler.Increment("cache.derived.miss");
    value = null;
    return false;
  }

  public void StoreDerived(object subject, string member, IHixValue value) {
    if (!_derived.TryGetValue(subject, out var members)) {
      members = new Dictionary<string, IHixValue>(StringComparer.OrdinalIgnoreCase);
      _derived.Add(subject, members);
    }
    value ??= NullHixValue.Instance;
    members.Add(member, value);
  }

  public object Snapshot(IHixValue subject, Func<object> create) {
    if (_snapshots.TryGetValue(subject, out var snapshot)) {
      HixProfiler.Increment("cache.snapshot.hit");
      return snapshot;
    }
    HixProfiler.Increment("cache.snapshot.miss");
    snapshot = create();
    _snapshots.Add(subject, snapshot);
    return snapshot;
  }

  private sealed class ReferenceObjectComparer : IEqualityComparer<object> {
    public static readonly ReferenceObjectComparer Instance = new();

    public new bool Equals(object x, object y) => ReferenceEquals(x, y);

    public int GetHashCode(object value) => RuntimeHelpers.GetHashCode(value);
  }
}

public sealed class RoslynHostExpressionCache {
  private readonly Dictionary<AttributeData, RoslynValueCache> _attributeValues = new();
  private readonly Dictionary<ISymbol, RoslynValueCache> _targetValues =
    new(SymbolEqualityComparer.Default);
  private readonly Dictionary<ISymbol, HixValueDictionary> _targetVariables =
    new(SymbolEqualityComparer.Default);
  private readonly Dictionary<INamedTypeSymbol, RoslynValueCache> _thisValues =
    new(SymbolEqualityComparer.Default);

  public RoslynValueCache ForThis(INamedTypeSymbol type) {
    return Get(_thisValues, type);
  }

  public RoslynValueCache ForTarget(ISymbol target) {
    return Get(_targetValues, target);
  }

  public RoslynValueCache ForAttribute(AttributeData attribute) {
    return Get(_attributeValues, attribute);
  }

  public HixValueDictionary TargetVariables(ISymbol target) {
    if (target is null) return new HixValueDictionary();
    if (_targetVariables.TryGetValue(target, out var values)) return values;
    values = new HixValueDictionary();
    _targetVariables.Add(target, values);
    return values;
  }

  public IEnumerable<KeyValuePair<HixString, IHixValue>> TargetVariableFingerprintValues(
    HixStringPool strings
  ) {
    foreach (var target in _targetVariables.OrderBy(
      item => TargetIdentity(item.Key), StringComparer.Ordinal
    )) {
      var identity = TargetIdentity(target.Key);
      foreach (var variable in target.Value.OrderBy(
        item => item.Key.Resolve(strings), StringComparer.Ordinal
      )) {
        yield return new KeyValuePair<HixString, IHixValue>(
          HixString.Dynamic(identity + "\u001f" + variable.Key.Resolve(strings)), variable.Value
        );
      }
    }
  }

  private static string TargetIdentity(ISymbol symbol) {
    var location = symbol?.Locations.FirstOrDefault(item => item.IsInSource);
    return (symbol?.Kind.ToString() ?? "None") + ":" +
      (symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "") + ":" +
      (location?.SourceTree?.FilePath ?? "") + ":" +
      (location?.SourceSpan.Start.ToString(CultureInfo.InvariantCulture) ?? "");
  }

  private static RoslynValueCache Get<T>(IDictionary<T, RoslynValueCache> values, T key) {
    if (key is null) return new RoslynValueCache();
    if (values.TryGetValue(key, out var cached)) return cached;
    cached = new RoslynValueCache();
    values.Add(key, cached);
    return cached;
  }
}