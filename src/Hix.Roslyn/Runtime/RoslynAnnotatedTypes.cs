using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Hix.Roslyn;
using Hix.Env;

namespace Hix.Runtime;

public partial class HixRoslynContext {
  private static readonly ConditionalWeakTable<CSharpCompilation, AnnotatedTypes> annotatedTypes = new();

  public bool IsAccessible(ISymbol symbol) => _compilation.IsSymbolAccessibleWithin(symbol, CurrentType);

  public IHixValue CollectAnnotatedTypes(HixThread thread, string name) {
    using var profile = HixProfiler.Measure("roslyn.collect_annotated_types");
    var attribute = ResolveType(name) ?? ResolveType(name + "Attribute");
    if (attribute is not INamedTypeSymbol type) return thread.Error("unknown attribute type '" + name + "'");
    return annotatedTypes.GetValue(_compilation, compilation => new AnnotatedTypes(compilation)).Get(type);
  }

  // Index only this compilation's assembly. Symbol identity keeps aliases equivalent
  // and derived attributes distinct; weak ownership avoids retaining old compilations.
  private sealed class AnnotatedTypes {
    private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> types = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, TupleHixValue> results = new(SymbolEqualityComparer.Default);

    public AnnotatedTypes(CSharpCompilation compilation) {
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

    public TupleHixValue Get(INamedTypeSymbol attribute) {
      lock (results) {
        if (results.TryGetValue(attribute, out var result)) return result;
        if (!types.TryGetValue(attribute, out var matches)) result = TupleHixValue.Empty;
        else {
          matches.Sort((left, right) => StringComparer.Ordinal.Compare(
            left.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat), right.ToDisplayString(GeneratorAnalysis.TypeDisplayFormat)));
          var values = new IHixValue[matches.Count];
          for (var i = 0; i < values.Length; i++) values[i] = new RoslynHixValue(matches[i]);
          result = new TupleHixValue(values);
        }
        results.Add(attribute, result);
        return result;
      }
    }
  }
}
