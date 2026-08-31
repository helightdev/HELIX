using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mixins.Compiler;
using Mixins.Env;

namespace Mixins.Runtime;

internal sealed class RoslynValueCache {
  private readonly Dictionary<object, Dictionary<string, IMixinValue>> _derived =
    new(ReferenceObjectComparer.Instance);
  private readonly Dictionary<string, IMixinValue> _roots = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<object, object> _snapshots =
    new(ReferenceObjectComparer.Instance);

  internal bool TryGetRoot(string key, out IMixinValue value) {
    if (_roots.TryGetValue(key, out value)) {
      MixinProfiler.Increment("cache.root.hit");
      return true;
    }
    MixinProfiler.Increment("cache.root.miss");
    value = null;
    return false;
  }

  internal void StoreRoot(string key, IMixinValue value) {
    _roots.Add(key, value ?? NullMixinValue.Instance);
  }

  internal bool TryGetDerived(object subject, string member, out IMixinValue value) {
    if (_derived.TryGetValue(subject, out var members) && members.TryGetValue(member, out value)) {
      MixinProfiler.Increment("cache.derived.hit");
      return true;
    }
    MixinProfiler.Increment("cache.derived.miss");
    value = null;
    return false;
  }

  internal void StoreDerived(object subject, string member, IMixinValue value) {
    if (!_derived.TryGetValue(subject, out var members)) {
      members = new Dictionary<string, IMixinValue>(StringComparer.OrdinalIgnoreCase);
      _derived.Add(subject, members);
    }
    value ??= NullMixinValue.Instance;
    members.Add(member, value);
  }

  internal object Snapshot(IMixinValue subject, Func<object> create) {
    if (_snapshots.TryGetValue(subject, out var snapshot)) {
      MixinProfiler.Increment("cache.snapshot.hit");
      return snapshot;
    }
    MixinProfiler.Increment("cache.snapshot.miss");
    snapshot = create();
    _snapshots.Add(subject, snapshot);
    return snapshot;
  }

  private sealed class ReferenceObjectComparer : IEqualityComparer<object> {
    internal static readonly ReferenceObjectComparer Instance = new();

    public new bool Equals(object x, object y) => ReferenceEquals(x, y);

    public int GetHashCode(object value) => RuntimeHelpers.GetHashCode(value);
  }
}

internal sealed class RoslynHostExpressionCache {
  private readonly Dictionary<AttributeData, RoslynValueCache> _attributeValues = new();
  private readonly Dictionary<ISymbol, RoslynValueCache> _targetValues =
    new(SymbolEqualityComparer.Default);
  private readonly Dictionary<ISymbol, MixinValueDictionary> _targetVariables =
    new(SymbolEqualityComparer.Default);
  private readonly Dictionary<INamedTypeSymbol, RoslynValueCache> _thisValues =
    new(SymbolEqualityComparer.Default);

  internal RoslynValueCache ForThis(INamedTypeSymbol type) {
    return Get(_thisValues, type);
  }

  internal RoslynValueCache ForTarget(ISymbol target) {
    return Get(_targetValues, target);
  }

  internal RoslynValueCache ForAttribute(AttributeData attribute) {
    return Get(_attributeValues, attribute);
  }

  internal MixinValueDictionary TargetVariables(ISymbol target) {
    if (target is null) return new MixinValueDictionary();
    if (_targetVariables.TryGetValue(target, out var values)) return values;
    values = new MixinValueDictionary();
    _targetVariables.Add(target, values);
    return values;
  }

  internal IEnumerable<KeyValuePair<MixinString, IMixinValue>> TargetVariableFingerprintValues(
    MixinStringPool strings
  ) {
    foreach (var target in _targetVariables.OrderBy(
      item => TargetIdentity(item.Key), StringComparer.Ordinal
    )) {
      var identity = TargetIdentity(target.Key);
      foreach (var variable in target.Value.OrderBy(
        item => item.Key.Resolve(strings), StringComparer.Ordinal
      )) {
        yield return new KeyValuePair<MixinString, IMixinValue>(
          MixinString.Dynamic(identity + "\u001f" + variable.Key.Resolve(strings)), variable.Value
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