using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// Memoized symbol queries behind <see cref="TypeRef"/>, keyed by symbol reference.
/// Replaces Hix's RoslynValueCache; same idea, fewer layers.
/// </summary>
public sealed class SymbolFacts {
  public static readonly SymbolDisplayFormat QualifiedFormat =
    SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
      SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

  public static readonly SymbolDisplayFormat MetadataFormat =
    new(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

  private readonly Dictionary<ISymbol, string> _qualified = new(SymbolEqualityComparer.Default);
  private readonly Dictionary<ISymbol, string> _metadata = new(SymbolEqualityComparer.Default);
  private readonly Dictionary<(ISymbol, string), bool> _implements = new();
  private readonly Dictionary<(ISymbol, string), bool> _inherits = new();
  private readonly Dictionary<ISymbol, PureTypeRef> _detached = new(SymbolEqualityComparer.Default);
  private readonly ConditionalWeakTable<ISymbol, IReadOnlyList<AttributeData>> _attributes = new();
  private readonly Dictionary<string, INamedTypeSymbol> _byMetadataName = new(System.StringComparer.Ordinal);
  private readonly Dictionary<string, List<INamedTypeSymbol>> _annotated = new(System.StringComparer.Ordinal);

  public SymbolFacts(StringPool pool) => Pool = pool;
  public StringPool Pool { get; }

  /// <summary>
  /// Metadata-name lookup, memoized. Uncached this is one of the most expensive things a
  /// generator can do in a loop: it searches every referenced assembly.
  /// </summary>
  public INamedTypeSymbol Lookup(Compilation compilation, string metadataName) {
    if (compilation is null || string.IsNullOrEmpty(metadataName)) return null;
    var key = metadataName.Replace("global::", "");
    if (_byMetadataName.TryGetValue(key, out var cached)) { Hits++; return cached; }
    Misses++;
    var result = compilation.GetTypeByMetadataName(key);
    _byMetadataName[key] = result;
    return result;
  }

  /// <summary>
  /// Types in this compilation's own assembly carrying an attribute, memoized. The walk visits
  /// every type, so it happens once per attribute per run rather than once per asking mixin.
  /// </summary>
  public IReadOnlyList<INamedTypeSymbol> AnnotatedTypes(Compilation compilation, string attributeMetadataName) {
    if (compilation is null) return System.Array.Empty<INamedTypeSymbol>();
    if (_annotated.TryGetValue(attributeMetadataName, out var cached)) { Hits++; return cached; }
    Misses++;

    var result = new List<INamedTypeSymbol>();
    foreach (var type in AllTypes(compilation.Assembly.GlobalNamespace))
      foreach (var attribute in Attributes(type))
        if (attribute.AttributeClass is not null &&
            MetadataName(attribute.AttributeClass) == attributeMetadataName) {
          result.Add(type);
          break;
        }

    _annotated[attributeMetadataName] = result;
    return result;
  }

  private static IEnumerable<INamedTypeSymbol> AllTypes(INamespaceOrTypeSymbol root) {
    foreach (var member in root.GetMembers()) {
      switch (member) {
        case INamespaceSymbol ns:
          foreach (var nested in AllTypes(ns)) yield return nested;
          break;
        case INamedTypeSymbol type:
          yield return type;
          foreach (var nested in AllTypes(type)) yield return nested;
          break;
      }
    }
  }
  public int Hits { get; private set; }
  public int Misses { get; private set; }

  public string Qualified(ISymbol symbol) {
    if (symbol is null) return null;
    if (_qualified.TryGetValue(symbol, out var cached)) { Hits++; return cached; }
    Misses++;
    var value = symbol.ToDisplayString(QualifiedFormat);
    _qualified[symbol] = value;
    return value;
  }

  public string MetadataName(ISymbol symbol) {
    if (symbol is null) return null;
    if (_metadata.TryGetValue(symbol, out var cached)) { Hits++; return cached; }
    Misses++;
    var value = symbol.ToDisplayString(MetadataFormat);
    _metadata[symbol] = value;
    return value;
  }

  public IReadOnlyList<AttributeData> Attributes(ISymbol symbol) {
    if (symbol is null) return System.Array.Empty<AttributeData>();
    if (_attributes.TryGetValue(symbol, out var cached)) { Hits++; return cached; }
    Misses++;
    var value = symbol.GetAttributes();
    _attributes.Add(symbol, value);
    return value;
  }

  public bool Implements(ITypeSymbol symbol, string metadataName) {
    if (symbol is null || metadataName is null) return false;
    var key = ((ISymbol)symbol, metadataName);
    if (_implements.TryGetValue(key, out var cached)) { Hits++; return cached; }
    Misses++;
    var result = MetadataName(symbol) == metadataName || InheritsFrom(symbol, metadataName);
    if (!result)
      foreach (var iface in symbol.AllInterfaces)
        if (MetadataName(iface) == metadataName) { result = true; break; }
    _implements[key] = result;
    return result;
  }

  public bool InheritsFrom(ITypeSymbol symbol, string metadataName) {
    if (symbol is null || metadataName is null) return false;
    var key = ((ISymbol)symbol, metadataName);
    if (_inherits.TryGetValue(key, out var cached)) { Hits++; return cached; }
    Misses++;
    var result = false;
    for (var current = symbol.BaseType; current is not null; current = current.BaseType)
      if (MetadataName(current) == metadataName) { result = true; break; }
    _inherits[key] = result;
    return result;
  }

  public TypeRef Ref(ITypeSymbol symbol) => symbol is null ? default : new TypeRef(symbol, this);

  public PureTypeRef Detach(ITypeSymbol symbol) {
    if (symbol is null) return default;
    if (_detached.TryGetValue(symbol, out var cached)) { Hits++; return cached; }
    Misses++;

    var facts = TypeFacts.None;
    if (symbol.TypeKind == TypeKind.Class) facts |= TypeFacts.Class;
    if (symbol.TypeKind == TypeKind.Enum) facts |= TypeFacts.Enum;
    if (symbol.TypeKind == TypeKind.Interface) facts |= TypeFacts.Interface;
    if (symbol.IsValueType) facts |= TypeFacts.ValueType;
    if (IsPrimitive(symbol)) facts |= TypeFacts.Primitive;
    if (symbol is IPointerTypeSymbol) facts |= TypeFacts.Pointer;
    if (ContainsPointer(symbol)) facts |= TypeFacts.ContainsPointer;
    if (symbol is INamedTypeSymbol { IsGenericType: true }) facts |= TypeFacts.Generic;

    var arguments = ImmutableArray<PureTypeRef>.Empty;
    if (symbol is INamedTypeSymbol { TypeArguments.Length: > 0 } named) {
      var builder = ImmutableArray.CreateBuilder<PureTypeRef>(named.TypeArguments.Length);
      foreach (var argument in named.TypeArguments) builder.Add(Detach(argument));
      arguments = builder.MoveToImmutable();
    } else if (symbol is IArrayTypeSymbol array) {
      arguments = ImmutableArray.Create(Detach(array.ElementType));
    }

    var result = new PureTypeRef(Pool, Pool.Intern(Qualified(symbol)), Pool.Intern(MetadataName(symbol)),
                                 facts, arguments);
    _detached[symbol] = result;
    return result;
  }

  public static bool IsPrimitive(ITypeSymbol symbol) => symbol?.SpecialType switch {
    SpecialType.System_Boolean or SpecialType.System_Byte or SpecialType.System_SByte or
    SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or
    SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64 or
    SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Char or
    SpecialType.System_IntPtr or SpecialType.System_UIntPtr => true,
    _ => false,
  };

  public static bool ContainsPointer(ITypeSymbol symbol) {
    switch (symbol) {
      case null: return false;
      case IPointerTypeSymbol: return true;
      case IArrayTypeSymbol array: return ContainsPointer(array.ElementType);
      case INamedTypeSymbol named:
        foreach (var argument in named.TypeArguments)
          if (ContainsPointer(argument)) return true;
        if (named.TypeKind != TypeKind.Struct) return false;
        foreach (var member in named.GetMembers())
          if (member is IFieldSymbol { IsStatic: false } field &&
              !SymbolEqualityComparer.Default.Equals(field.Type, named) && ContainsPointer(field.Type))
            return true;
        return false;
      default: return false;
    }
  }
}
