using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix;

/// <summary>A compiler-only callable which constructs a pattern; it is not itself a pattern.</summary>
public sealed class HixPatternConstructor {
  private readonly Func<IReadOnlyList<HixPattern>, HixPattern> implementation;

  public HixPatternConstructor(string name, int arity,
    Func<IReadOnlyList<HixPattern>, HixPattern> implementation, string documentation = "") {
    if (string.IsNullOrWhiteSpace(name) || arity < 0 || implementation == null)
      throw new ArgumentException("A pattern constructor requires a name, arity, and implementation.");
    Name = name;
    Arity = arity;
    Documentation = documentation;
    this.implementation = implementation;
  }

  public string Name { get; }
  public int Arity { get; }
  public string Documentation { get; }

  public HixPattern Construct(IReadOnlyList<HixPattern> arguments) {
    if (arguments == null || arguments.Count != Arity || arguments.Any(argument => argument == null))
      throw new ArgumentException("Pattern constructor '" + Name + "' requires " + Arity + " type argument(s).");
    return implementation(arguments) ?? throw new InvalidOperationException("Pattern constructor returned no pattern.");
  }
}

/// <summary>A nominal host-owned type. Its identity and arguments exist only during compilation.</summary>
public sealed record HixOpaqueTypeDefinition(string Identity, string Name, int Arity = 0, string Documentation = "") {
  public OpaqueHixPattern Construct(params HixPattern[] arguments) {
    if (arguments == null || arguments.Length != Arity)
      throw new ArgumentException("Opaque type '" + Name + "' requires " + Arity + " type argument(s).");
    return new OpaqueHixPattern(this, arguments);
  }
}

/// <summary>Scope identity prevents unrelated declarations named T from sharing substitutions.</summary>
public sealed record HixTypeParameter(string Scope, string Name, HixPattern Bound = null) {
  public TypeParameterHixPattern Pattern => new(this);
  public IReadOnlyList<HixPattern> AdditionalBounds { get; init; } = Array.Empty<HixPattern>();
}

public sealed record TypeParameterHixPattern(HixTypeParameter Parameter) : HixPattern {
  public override string Display => Parameter.Name;
}

public sealed record OpaqueHixPattern(HixOpaqueTypeDefinition Definition, IReadOnlyList<HixPattern> Arguments) : HixPattern {
  public IReadOnlyList<HixPattern> Arguments { get; } = Arguments != null && Arguments.Count == Definition.Arity &&
    Arguments.All(argument => argument != null) ? Array.AsReadOnly(Arguments.ToArray()) :
    throw new ArgumentException("Opaque type arguments do not match declared arity.");
  public override string Display => Definition.Name + (Arguments.Count == 0 ? "" : "<" +
    string.Join(", ", Arguments.Select(argument => argument.Display)) + ">");
}

public static class HixGenericTypes {
  public static bool SatisfiesBounds(HixPattern value, HixTypeParameter parameter,
    IReadOnlyDictionary<HixTypeParameter, HixPattern> substitutions,
    IReadOnlyDictionary<string, HixPattern> definitions) =>
    (parameter.Bound == null || SatisfiesBound(value, Substitute(parameter.Bound, substitutions, true), definitions)) &&
    parameter.AdditionalBounds.All(bound => SatisfiesBound(value, Substitute(bound, substitutions, true), definitions));

  public static string SignatureIdentity(SignatureHixPattern signature) => signature.Name + "(" +
    string.Join(",", signature.Parameters.Select(field => Identity(field.Pattern))) + ")->" + Identity(signature.Result);

  private static string Identity(HixPattern pattern) => pattern switch {
    OpaqueHixPattern opaque => opaque.Definition.Identity + "<" + string.Join(",", opaque.Arguments.Select(Identity)) + ">",
    TypeParameterHixPattern parameter => parameter.Parameter.Scope + ":" + parameter.Parameter.Name,
    TupleHixPattern tuple => "tuple[" + string.Join(",", tuple.Fields.Select(field => Identity(field.Pattern))) + "]",
    TableHixPattern table => "table{" + string.Join(",", table.Fields.Select(field => field.Name + ":" + Identity(field.Pattern))) + "}",
    ManyHixPattern many => "many<" + Identity(many.Element) + ">",
    MapHixPattern map => "map<" + Identity(map.Key) + "," + Identity(map.Value) + ">",
    DelegateHixPattern callable => "delegate(" + string.Join(",", callable.Parameters.Select(field => Identity(field.Pattern))) + ")->" + Identity(callable.Result),
    UnionHixPattern union => string.Join("|", union.Patterns.Select(Identity)),
    DocumentedHixPattern documented => Identity(documented.Underlying),
    ConstrainedHixPattern constrained => constrained.Constraint + "<" + constrained.Argument + ">" + Identity(constrained.Underlying),
    TaggedHixPattern tagged => tagged.Discriminator + ":" + Identity(tagged.Underlying),
    ConstantHixPattern constant => constant.Value + ":" + Identity(constant.Underlying),
    EnumHixPattern enumeration => enumeration.Display + ":" + Identity(enumeration.Underlying),
    DefinedHixPattern defined => Identity(defined.Root),
    _ => pattern?.Display ?? "any"
  };
  public static bool RequiresErasure(HixPattern pattern) => pattern switch {
    OpaqueHixPattern or TypeParameterHixPattern => true,
    TupleHixPattern tuple => tuple.Fields.Any(field => RequiresErasure(field.Pattern)),
    TableHixPattern table => table.Fields.Any(field => RequiresErasure(field.Pattern)),
    ManyHixPattern many => RequiresErasure(many.Element),
    MapHixPattern map => RequiresErasure(map.Key) || RequiresErasure(map.Value),
    DelegateHixPattern callable => callable.Parameters.Any(field => RequiresErasure(field.Pattern)) || RequiresErasure(callable.Result),
    UnionHixPattern union => union.Patterns.Any(RequiresErasure),
    DocumentedHixPattern documented => RequiresErasure(documented.Underlying),
    ConstrainedHixPattern constrained => RequiresErasure(constrained.Underlying),
    TaggedHixPattern tagged => RequiresErasure(tagged.Underlying),
    ConstantHixPattern constant => RequiresErasure(constant.Underlying),
    EnumHixPattern enumeration => RequiresErasure(enumeration.Underlying),
    DefinedHixPattern defined => RequiresErasure(defined.Root) || defined.Definitions.Values.Any(RequiresErasure),
    _ => false
  };
  public static HixPattern ErasePattern(HixPattern pattern) {
    if (!RequiresErasure(pattern)) return pattern;
    HixPatternField Field(HixPatternField field) => field with { Pattern = ErasePattern(field.Pattern) };
    return pattern switch {
      OpaqueHixPattern => new KindHixPattern(HixValueKind.Opaque),
      TypeParameterHixPattern parameter => ErasePattern(parameter.Parameter.Bound ?? HixPattern.Any),
      TupleHixPattern tuple => new TupleHixPattern(tuple.Fields.Select(Field).ToArray()),
      TableHixPattern table => new TableHixPattern(table.Fields.Select(Field).ToArray()),
      ManyHixPattern many => new ManyHixPattern(ErasePattern(many.Element)),
      MapHixPattern map => new MapHixPattern(ErasePattern(map.Key), ErasePattern(map.Value)),
      DelegateHixPattern callable => new DelegateHixPattern(callable.Parameters.Select(Field).ToArray(), ErasePattern(callable.Result)),
      UnionHixPattern union => HixPatterns.Union(union.Patterns.Select(ErasePattern)),
      DocumentedHixPattern documented => documented with { Underlying = ErasePattern(documented.Underlying) },
      ConstrainedHixPattern constrained => constrained with { Underlying = ErasePattern(constrained.Underlying) },
      TaggedHixPattern tagged => tagged with { Underlying = ErasePattern(tagged.Underlying) },
      ConstantHixPattern constant => constant with { Underlying = ErasePattern(constant.Underlying) },
      EnumHixPattern enumeration => enumeration with { Underlying = ErasePattern(enumeration.Underlying) },
      DefinedHixPattern defined => new DefinedHixPattern(ErasePattern(defined.Root), defined.Definitions.ToDictionary(pair => pair.Key, pair => ErasePattern(pair.Value))),
      _ => pattern
    };
  }
  public static HixValueKind Erase(HixPattern pattern) => pattern switch {
    KindHixPattern kind => kind.ValueKind,
    OpaqueHixPattern => HixValueKind.Opaque,
    TypeParameterHixPattern parameter => Erase(parameter.Parameter.Bound ?? HixPattern.Any),
    TupleHixPattern or ManyHixPattern => HixValueKind.Tuple,
    TableHixPattern or MapHixPattern => HixValueKind.Table,
    DelegateHixPattern => HixValueKind.Function,
    DocumentedHixPattern documented => Erase(documented.Underlying),
    ConstrainedHixPattern constrained => Erase(constrained.Underlying),
    TaggedHixPattern tagged => Erase(tagged.Underlying),
    ConstantHixPattern constant => Erase(constant.Underlying),
    EnumHixPattern enumeration => Erase(enumeration.Underlying),
    DefinedHixPattern defined => Erase(defined.Root),
    _ => HixValueKind.Any
  };

  public static HixPattern Substitute(HixPattern pattern, IReadOnlyDictionary<HixTypeParameter, HixPattern> substitutions,
    bool eraseUnresolved = false) {
    HixPattern Apply(HixPattern value) => Substitute(value, substitutions, eraseUnresolved);
    HixPatternField Field(HixPatternField field) => field with { Pattern = Apply(field.Pattern) };
    return pattern switch {
      TypeParameterHixPattern parameter => substitutions.TryGetValue(parameter.Parameter, out var resolved) ? resolved :
        eraseUnresolved ? Apply(parameter.Parameter.Bound ?? HixPattern.Any) : parameter,
      OpaqueHixPattern opaque => opaque.Definition.Construct(opaque.Arguments.Select(Apply).ToArray()),
      TupleHixPattern tuple => new TupleHixPattern(tuple.Fields.Select(Field).ToArray()),
      TableHixPattern table => new TableHixPattern(table.Fields.Select(Field).ToArray()),
      ManyHixPattern many => new ManyHixPattern(Apply(many.Element)),
      MapHixPattern map => new MapHixPattern(Apply(map.Key), Apply(map.Value)),
      UnionHixPattern union => HixPatterns.Union(union.Patterns.Select(Apply)),
      DelegateHixPattern callable => new DelegateHixPattern(callable.Parameters.Select(Field).ToArray(), Apply(callable.Result)),
      DocumentedHixPattern documented => documented with { Underlying = Apply(documented.Underlying) },
      ConstrainedHixPattern constrained => constrained with { Underlying = Apply(constrained.Underlying) },
      TaggedHixPattern tagged => tagged with { Underlying = Apply(tagged.Underlying) },
      ConstantHixPattern constant => constant with { Underlying = Apply(constant.Underlying) },
      EnumHixPattern enumeration => enumeration with { Underlying = Apply(enumeration.Underlying) },
      DefinedHixPattern defined => new DefinedHixPattern(Apply(defined.Root), defined.Definitions.ToDictionary(pair => pair.Key, pair => Apply(pair.Value))),
      _ => pattern ?? HixPattern.Any
    };
  }

  public static bool Equivalent(HixPattern left, HixPattern right) {
    if (left is OpaqueHixPattern a) return right is OpaqueHixPattern b && a.Definition.Identity == b.Definition.Identity &&
      a.Arguments.Count == b.Arguments.Count && a.Arguments.Zip(b.Arguments, Equivalent).All(value => value);
    if (left is TypeParameterHixPattern parameter) return right is TypeParameterHixPattern other && parameter.Parameter == other.Parameter;
    if (left is TupleHixPattern tuple) return right is TupleHixPattern other && Fields(tuple.Fields, other.Fields);
    if (left is TableHixPattern table) return right is TableHixPattern other && Fields(table.Fields, other.Fields);
    if (left is ManyHixPattern many) return right is ManyHixPattern other && Equivalent(many.Element, other.Element);
    if (left is MapHixPattern map) return right is MapHixPattern other && Equivalent(map.Key, other.Key) && Equivalent(map.Value, other.Value);
    if (left is DelegateHixPattern callable) return right is DelegateHixPattern other && Fields(callable.Parameters, other.Parameters) &&
      Equivalent(callable.Result, other.Result);
    if (left is UnionHixPattern union) return right is UnionHixPattern other && union.Patterns.Count == other.Patterns.Count &&
      union.Patterns.All(member => other.Patterns.Any(value => Equivalent(member, value)));
    return Equals(left, right);
  }

  private static bool Fields(IReadOnlyList<HixPatternField> left, IReadOnlyList<HixPatternField> right) =>
    left.Count == right.Count && left.Zip(right, (a, b) => a.Name == b.Name && a.AllowsMissing == b.AllowsMissing &&
      Equivalent(a.Pattern, b.Pattern)).All(value => value);

  public static bool SatisfiesBound(HixPattern value, HixPattern bound, IReadOnlyDictionary<string, HixPattern> definitions) {
    if (value is AnyHixPattern || bound is AnyHixPattern) return true;
    if (bound is KindHixPattern kind) return Erase(value) == kind.ValueKind;
    return HixPatternRelations.Relate(value, bound, definitions) != HixPatternRelation.Never;
  }
}
