using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Helix.Weave;

/// <summary>
/// Symbol-backed type reference. Available to the prelude pass only: it holds a live
/// <see cref="ITypeSymbol"/> and therefore cannot cross the purity boundary.
/// Converts implicitly, one way, to <see cref="PureTypeRef"/>.
/// </summary>
public readonly struct TypeRef {
  private readonly SymbolFacts _facts;

  internal TypeRef(ITypeSymbol symbol, SymbolFacts facts) {
    Symbol = symbol;
    _facts = facts;
  }

  public ITypeSymbol Symbol { get; }
  public bool Exists => Symbol is not null;
  public string Qualified => _facts?.Qualified(Symbol);
  public string MetadataName => Symbol?.ToDisplayString(SymbolFacts.MetadataFormat);

  public bool IsClass => Symbol is { TypeKind: TypeKind.Class };
  public bool IsEnum => Symbol is { TypeKind: TypeKind.Enum };
  public bool IsPrimitive => SymbolFacts.IsPrimitive(Symbol);
  public bool IsPointer => Symbol is IPointerTypeSymbol;
  public bool ContainsPointer => SymbolFacts.ContainsPointer(Symbol);

  public bool Implements(string metadataName) => _facts is not null && _facts.Implements(Symbol, metadataName);
  public bool InheritsFrom(string metadataName) => _facts is not null && _facts.InheritsFrom(Symbol, metadataName);

  /// <summary>The type argument at <paramref name="index"/>, or a non-existent ref.</summary>
  public TypeRef Argument(int index) {
    if (Symbol is INamedTypeSymbol named && index < named.TypeArguments.Length)
      return new TypeRef(named.TypeArguments[index], _facts);
    if (Symbol is IArrayTypeSymbol array && index == 0) return new TypeRef(array.ElementType, _facts);
    return default;
  }

  public PureTypeRef Pure() => _facts is null ? default : _facts.Detach(Symbol);
  public static implicit operator PureTypeRef(TypeRef value) => value.Pure();
  public override string ToString() => Qualified ?? "";
}

/// <summary>
/// Detached type reference: pooled strings plus the facts the mixins query. Safe in a carry
/// record, safe as an incremental cache key, renders as its qualified name.
/// The pool is deliberately excluded from equality — only the handles and facts identify it.
/// </summary>
public readonly struct PureTypeRef : System.IEquatable<PureTypeRef> {
  internal PureTypeRef(string qualified, string metadataName, TypeFacts facts,
                       ImmutableArray<PureTypeRef> typeArguments) {
    Qualified = qualified;
    MetadataName = metadataName;
    Facts = facts;
    TypeArguments = typeArguments.IsDefault ? ImmutableArray<PureTypeRef>.Empty : typeArguments;
  }

  /// <summary>
  /// Plain strings rather than handles into a string pool: a detached ref travels in carry
  /// records that the incremental pipeline caches across runs, and a pool only lives for one.
  /// </summary>
  public string Qualified { get; }
  public string MetadataName { get; }
  public TypeFacts Facts { get; }
  public ImmutableArray<PureTypeRef> TypeArguments { get; }

  public bool Exists => Qualified is not null;
  public bool IsClass => (Facts & TypeFacts.Class) != 0;
  public bool IsEnum => (Facts & TypeFacts.Enum) != 0;
  public bool IsPrimitive => (Facts & TypeFacts.Primitive) != 0;
  public bool IsPointer => (Facts & TypeFacts.Pointer) != 0;
  public bool ContainsPointer => (Facts & TypeFacts.ContainsPointer) != 0;

  public override string ToString() => Qualified ?? "";

  public bool Equals(PureTypeRef other) {
    if (!string.Equals(Qualified, other.Qualified, System.StringComparison.Ordinal) ||
        !string.Equals(MetadataName, other.MetadataName, System.StringComparison.Ordinal) ||
        Facts != other.Facts || TypeArguments.Length != other.TypeArguments.Length) return false;
    for (var i = 0; i < TypeArguments.Length; i++)
      if (!TypeArguments[i].Equals(other.TypeArguments[i])) return false;
    return true;
  }

  public override bool Equals(object obj) => obj is PureTypeRef other && Equals(other);

  public override int GetHashCode() {
    var hash = (Qualified?.GetHashCode() ?? 0) * 397 ^ (int)Facts;
    for (var i = 0; i < TypeArguments.Length; i++) hash = hash * 397 ^ TypeArguments[i].GetHashCode();
    return hash;
  }

  public static bool operator ==(PureTypeRef a, PureTypeRef b) => a.Equals(b);
  public static bool operator !=(PureTypeRef a, PureTypeRef b) => !a.Equals(b);
}

[System.Flags]
public enum TypeFacts {
  None = 0,
  Class = 1 << 0,
  Enum = 1 << 1,
  Primitive = 1 << 2,
  Pointer = 1 << 3,
  ContainsPointer = 1 << 4,
  Interface = 1 << 5,
  ValueType = 1 << 6,
  Generic = 1 << 7,
}
