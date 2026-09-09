using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mixins.Roslyn;
using Mixins.Env;

namespace Mixins.Runtime;

internal sealed partial class RoslynMixinContext {
  private static readonly ConditionalWeakTable<CSharpCompilation, AnnotatedTypes> annotatedTypes = new();

  internal bool IsAccessible(ISymbol symbol) => _compilation.IsSymbolAccessibleWithin(symbol, CurrentType);

  internal IMixinValue CollectAnnotatedTypes(string name) {
    using var profile = MixinProfiler.Measure("roslyn.collect_annotated_types");
    var attribute = ResolveType(name) ?? ResolveType(name + "Attribute");
    if (attribute is not INamedTypeSymbol type) return Error("unknown attribute type '" + name + "'");
    return annotatedTypes.GetValue(_compilation, compilation => new AnnotatedTypes(compilation)).Get(type);
  }

  // Index only this compilation's assembly. Symbol identity keeps aliases equivalent
  // and derived attributes distinct; weak ownership avoids retaining old compilations.
  private sealed class AnnotatedTypes {
    private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> types = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, TupleMixinValue> results = new(SymbolEqualityComparer.Default);

    internal AnnotatedTypes(CSharpCompilation compilation) {
      var pending = new Stack<INamespaceOrTypeSymbol>();
      pending.Push(compilation.Assembly.GlobalNamespace);
      while (pending.Count != 0) {
        var current = pending.Pop();
        foreach (var member in current.GetMembers()) {
          if (member is INamespaceSymbol ns) pending.Push(ns);
          if (member is not INamedTypeSymbol type) continue;
          pending.Push(type);
          var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
          foreach (var attribute in type.GetAttributes()) {
            var kind = attribute.AttributeClass;
            if (kind == null || !seen.Add(kind)) continue;
            if (!types.TryGetValue(kind, out var list)) types.Add(kind, list = new());
            list.Add(type);
          }
        }
      }
    }

    internal TupleMixinValue Get(INamedTypeSymbol attribute) {
      lock (results) {
        if (results.TryGetValue(attribute, out var result)) return result;
        if (!types.TryGetValue(attribute, out var matches)) result = TupleMixinValue.Empty;
        else {
          matches.Sort((left, right) => StringComparer.Ordinal.Compare(
            left.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat), right.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat)));
          var values = new IMixinValue[matches.Count];
          for (var i = 0; i < values.Length; i++) values[i] = new RoslynMixinValue(matches[i]);
          result = new TupleMixinValue(values);
        }
        results.Add(attribute, result);
        return result;
      }
    }
  }
}
