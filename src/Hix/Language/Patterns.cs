using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Hix.Runtime;

namespace Hix;

/// <summary>Immutable predicate over ordinary Hix values. Patterns never change a value's runtime kind.</summary>
public abstract record HixPattern {
  public static readonly HixPattern Any = new AnyHixPattern();
  public abstract string Display { get; }
}

public static class HixPatterns {
  public static HixPattern Named(string name) {
    if (string.IsNullOrEmpty(name) || name == "any") return HixPattern.Any;
    return Enum.TryParse<HixValueKind>(name, true, out var kind) && kind != HixValueKind.Any
      ? new KindHixPattern(kind) : new NamedHixPattern(name);
  }

  public static HixPattern Union(IEnumerable<HixPattern> values) {
    var members = values.Where(value => value != null).SelectMany(value => value is UnionHixPattern union
      ? union.Patterns : [value]).Distinct().ToArray();
    if (members.Any(value => value is AnyHixPattern)) return HixPattern.Any;
    return members.Length switch {0 => HixPattern.Any, 1 => members[0], _ => new UnionHixPattern(members)};
  }
}

public sealed record AnyHixPattern : HixPattern {
  public override string Display => "any";
}

public sealed record KindHixPattern(HixValueKind ValueKind) : HixPattern {
  public override string Display => ValueKind.ToString().ToLowerInvariant();
}

public sealed record NamedHixPattern(string Name) : HixPattern {
  public override string Display => Name;
}

public sealed record UnionHixPattern(IReadOnlyList<HixPattern> Patterns) : HixPattern {
  public override string Display => "%union" + string.Concat(Patterns.Select(value => "<" + value.Display + ">"));
}

public sealed record TupleHixPattern(IReadOnlyList<HixPatternField> Fields) : HixPattern {
  public override string Display => "@[" + string.Join(", ", Fields.Select(field => field.Display)) + "]";
}

public sealed record TableHixPattern(IReadOnlyList<HixPatternField> Fields) : HixPattern {
  public override string Display => "@{" + string.Join(", ", Fields.Select(field => field.Display)) + "}";
}

public sealed record ManyHixPattern(HixPattern Element) : HixPattern {
  public override string Display => "%many " + Element.Display;
}

public sealed record MapHixPattern(HixPattern Key, HixPattern Value) : HixPattern {
  public override string Display => "%map<" + Key.Display + "><" + Value.Display + ">";
}

/// <summary>A self-contained pattern carrying definitions imported from an external schema.</summary>
public sealed record DefinedHixPattern(HixPattern Root, IReadOnlyDictionary<string, HixPattern> Definitions) : HixPattern {
  public override string Display => Root.Display;
}

public sealed record ConstantHixPattern(object Value, HixPattern Underlying) : HixPattern {
  public override string Display => "%const<" + Convert.ToString(Value, CultureInfo.InvariantCulture) + "> " + Underlying.Display;
}

public enum HixPatternConstraintKind { Minimum, Maximum, Length, Matches }

public sealed record ConstrainedHixPattern(HixPattern Underlying, HixPatternConstraintKind Constraint, object Argument)
  : HixPattern {
  public override string Display => "%" + Constraint.ToString().ToLowerInvariant() + "<" +
    Convert.ToString(Argument, CultureInfo.InvariantCulture) + "> " + Underlying.Display;
}

public sealed record DelegateHixPattern(IReadOnlyList<HixPatternField> Parameters, HixPattern Result) : HixPattern {
  public override string Display => "delegate(" + string.Join(", ", Parameters.Select(field => field.Display)) +
    ") -> " + Result.Display;
}

public sealed record HixPatternField(string Name, HixPattern Pattern, bool Optional = false) {
  public string Display => (Optional ? "%optional " : "") + Pattern.Display +
    (string.IsNullOrEmpty(Name) ? "" : " " + Name);
}

/// <summary>A signature is a constant callable pattern and is stored in the bytecode constant pool.</summary>
public sealed record SignatureHixPattern(string Name, IReadOnlyList<HixPatternField> Parameters, HixPattern Result) : HixPattern {
  public override string Display => Name + "(" + string.Join(", ", Parameters.Select(field => field.Display)) + ") -> " + Result.Display;
}

public sealed record PatternHixValue(HixPattern Pattern) : IHixValue {
  public HixValueKind Kind => HixValueKind.Pattern;
  public bool IsTruthy(HixThread context) => true;
  public HixString Render(HixThread context) => HixString.Dynamic(Pattern.Display);
  public void Fingerprint(HixFingerprintBuilder builder, HixThread context) {
    builder.Append(nameof(PatternHixValue)); builder.Append(Pattern.Display);
  }
  public IHixValue Select(HixThread context, HixString member) => NullHixValue.Instance;
  public object Unlink(HixThread context) => this;
  public bool Equals(IHixValue other) => other is PatternHixValue value && Equals(Pattern, value.Pattern);
  public override string ToString() => Pattern?.Display ?? "any";
}

public sealed record HixPatternFailure(string Path, string Expected, string Actual, string Constraint = null) {
  public override string ToString() => (string.IsNullOrEmpty(Path) ? "value" : Path) + " expected " + Expected +
    ", found " + Actual + (Constraint == null ? "" : " (" + Constraint + ")");
}

public static class HixPatternMatcher {
  public static bool Matches(HixPattern pattern, IHixValue value, HixThread context,
    IReadOnlyDictionary<string, HixPattern> definitions, out HixPatternFailure failure) =>
    Matches(pattern ?? HixPattern.Any, value, context, definitions ?? Empty, "value", new HashSet<string>(), out failure);

  private static readonly IReadOnlyDictionary<string, HixPattern> Empty =
    new Dictionary<string, HixPattern>(StringComparer.Ordinal);

  private static bool Matches(HixPattern pattern, IHixValue value, HixThread context,
    IReadOnlyDictionary<string, HixPattern> definitions, string path, ISet<string> active,
    out HixPatternFailure failure) {
    failure = null;
    if (pattern is DefinedHixPattern defined) {
      var merged = (definitions ?? Empty).ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
      foreach (var item in defined.Definitions) merged[item.Key] = item.Value;
      return Matches(defined.Root, value, context, merged, path, active, out failure);
    }
    switch (pattern) {
      case AnyHixPattern: return true;
      case KindHixPattern kind when value.Kind == kind.ValueKind: return true;
      case KindHixPattern kind:
        failure = new(path, kind.Display, value.Kind.ToString().ToLowerInvariant()); return false;
      case NamedHixPattern named:
        if (!definitions.TryGetValue(named.Name, out var resolved)) {
          failure = new(path, named.Name, value.Kind.ToString().ToLowerInvariant(), "unknown pattern"); return false;
        }
        if (!active.Add(named.Name)) return true;
        var matched = Matches(resolved, value, context, definitions, path, active, out failure);
        active.Remove(named.Name); return matched;
      case UnionHixPattern union:
        foreach (var member in union.Patterns)
          if (Matches(member, value, context, definitions, path, new HashSet<string>(active), out _)) return true;
        failure = new(path, union.Display, value.Kind.ToString().ToLowerInvariant()); return false;
      case ManyHixPattern many when value is TupleHixValue tuple:
        for (var i = 0; i < tuple.Values.Count; i++)
          if (!Matches(many.Element, tuple.Values[i], context, definitions, path + "[" + i + "]", active, out failure)) return false;
        return true;
      case MapHixPattern map when value is HixTableValue table:
        foreach (var entry in table.Entries) {
          var key = new LiteralHixValue(entry.Key);
          var name = entry.Key.Resolve(context.Strings);
          if (!Matches(map.Key, key, context, definitions, path + ".<key>", active, out failure) ||
              !Matches(map.Value, entry.Value, context, definitions, path + "." + name, active, out failure)) return false;
        }
        return true;
      case TupleHixPattern expected when value is TupleHixValue tuple:
        var required = expected.Fields.Count(field => !field.Optional);
        if (tuple.Values.Count < required || tuple.Values.Count > expected.Fields.Count) {
          failure = new(path, expected.Display, "tuple", "expected " + required + ".." + expected.Fields.Count + " elements"); return false;
        }
        for (var i = 0; i < tuple.Values.Count; i++)
          if (!Matches(expected.Fields[i].Pattern, tuple.Values[i], context, definitions, path + "[" + i + "]", active, out failure)) return false;
        return true;
      case TableHixPattern expected when value is HixTableValue table:
        foreach (var field in expected.Fields) {
          if (!table.TryGetValue(context, context.ResolveString(field.Name), out var member)) {
            if (field.Optional) continue;
            failure = new(path + "." + field.Name, field.Pattern.Display, "missing"); return false;
          }
          if (!Matches(field.Pattern, member, context, definitions, path + "." + field.Name, active, out failure)) return false;
        }
        return true;
      case DelegateHixPattern when value is NamedFunctionHixValue: return true;
      case SignatureHixPattern when value is NamedFunctionHixValue: return true;
      case ConstantHixPattern constant:
        if (!Matches(constant.Underlying, value, context, definitions, path, active, out failure)) return false;
        if (ConstantEquals(constant.Value, value, context)) return true;
        failure = new(path, constant.Display, value.Render(context).Resolve(context.Strings), "constant mismatch"); return false;
      case ConstrainedHixPattern constrained:
        if (!Matches(constrained.Underlying, value, context, definitions, path, active, out failure)) return false;
        if (ConstraintMatches(constrained, value, context)) return true;
        failure = new(path, constrained.Display, value.Render(context).Resolve(context.Strings), "constraint failed"); return false;
      default:
        failure = new(path, pattern.Display, value.Kind.ToString().ToLowerInvariant()); return false;
    }
  }

  private static bool ConstantEquals(object expected, IHixValue value, HixThread context) => value switch {
    LiteralHixValue text => Equals(Convert.ToString(expected, CultureInfo.InvariantCulture), text.Value.Resolve(context.Strings)),
    NumberHixValue number => double.TryParse(Convert.ToString(expected, CultureInfo.InvariantCulture),
      NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) && parsed.Equals(number.Value),
    BooleanHixValue boolean => expected is bool flag && boolean.Value == flag,
    NullHixValue => expected == null,
    _ => false
  };

  private static bool ConstraintMatches(ConstrainedHixPattern pattern, IHixValue value, HixThread context) {
    var argument = Convert.ToString(pattern.Argument, CultureInfo.InvariantCulture);
    var number = value is NumberHixValue numeric ? numeric.Value : double.NaN;
    var length = value switch {
      LiteralHixValue text => text.Value.Resolve(context.Strings).Length,
      TupleHixValue tuple => tuple.Values.Count,
      HixTableValue table => table.Entries.Count,
      _ => -1
    };
    return pattern.Constraint switch {
      HixPatternConstraintKind.Minimum => double.TryParse(argument, NumberStyles.Float, CultureInfo.InvariantCulture, out var min) && number >= min,
      HixPatternConstraintKind.Maximum => double.TryParse(argument, NumberStyles.Float, CultureInfo.InvariantCulture, out var max) && number <= max,
      HixPatternConstraintKind.Length => int.TryParse(argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) && length == count,
      HixPatternConstraintKind.Matches => value is LiteralHixValue text && Regex.IsMatch(text.Value.Resolve(context.Strings), argument),
      _ => false
    };
  }
}

public enum HixPatternRelation { Never, Maybe, Always }

public static class HixPatternRelations {
  public static HixPatternRelation Relate(HixPattern actual, HixPattern expected,
    IReadOnlyDictionary<string, HixPattern> definitions) {
    actual ??= HixPattern.Any; expected ??= HixPattern.Any;
    if (expected is AnyHixPattern) return HixPatternRelation.Always;
    if (actual is AnyHixPattern) return HixPatternRelation.Maybe;
    if (actual.Equals(expected)) return HixPatternRelation.Always;
    if (actual is DefinedHixPattern definedActual) return Relate(definedActual.Root, expected, definedActual.Definitions);
    if (expected is DefinedHixPattern definedExpected) return Relate(actual, definedExpected.Root, definedExpected.Definitions);
    if (actual is NamedHixPattern namedActual && definitions.TryGetValue(namedActual.Name, out var actualBody))
      return Relate(actualBody, expected, definitions);
    if (expected is NamedHixPattern namedExpected && definitions.TryGetValue(namedExpected.Name, out var expectedBody))
      return Relate(actual, expectedBody, definitions);
    if (actual is UnionHixPattern actualUnion) return Merge(actualUnion.Patterns.Select(item => Relate(item, expected, definitions)));
    if (expected is UnionHixPattern expectedUnion) {
      var relations = expectedUnion.Patterns.Select(item => Relate(actual, item, definitions)).ToArray();
      return relations.Any(item => item == HixPatternRelation.Always) ? HixPatternRelation.Always :
        relations.Any(item => item == HixPatternRelation.Maybe) ? HixPatternRelation.Maybe : HixPatternRelation.Never;
    }
    if (actual is KindHixPattern left && expected is KindHixPattern right)
      return left.ValueKind == right.ValueKind ? HixPatternRelation.Always : HixPatternRelation.Maybe;
    if (actual is TupleHixPattern or ManyHixPattern && expected is KindHixPattern {ValueKind: HixValueKind.Tuple})
      return HixPatternRelation.Always;
    if (actual is KindHixPattern {ValueKind: HixValueKind.Tuple} && expected is ManyHixPattern)
      return HixPatternRelation.Maybe;
    if (actual is TableHixPattern or MapHixPattern && expected is KindHixPattern {ValueKind: HixValueKind.Table})
      return HixPatternRelation.Always;
    if (actual is KindHixPattern {ValueKind: HixValueKind.Table} && expected is MapHixPattern)
      return HixPatternRelation.Maybe;
    if (actual is DelegateHixPattern or SignatureHixPattern && expected is KindHixPattern {ValueKind: HixValueKind.Function})
      return HixPatternRelation.Always;
    if (actual is ConstantHixPattern constant) return Relate(constant.Underlying, expected, definitions);
    if (expected is ConstantHixPattern expectedConstant) {
      var relation = Relate(actual, expectedConstant.Underlying, definitions);
      return relation == HixPatternRelation.Never ? relation : HixPatternRelation.Maybe;
    }
    if (actual is ConstrainedHixPattern constrained) return Relate(constrained.Underlying, expected, definitions);
    if (expected is ConstrainedHixPattern expectedConstrained) {
      var relation = Relate(actual, expectedConstrained.Underlying, definitions);
      return relation == HixPatternRelation.Never ? relation : HixPatternRelation.Maybe;
    }
    if (actual is ManyHixPattern many && expected is ManyHixPattern expectedMany)
      return Relate(many.Element, expectedMany.Element, definitions);
    if (actual is TupleHixPattern actualTuple && expected is ManyHixPattern expectedManyElements)
      return Merge(actualTuple.Fields.Select(field => Relate(field.Pattern, expectedManyElements.Element, definitions)));
    if (actual is MapHixPattern map && expected is MapHixPattern expectedMap)
      return Merge([Relate(map.Key, expectedMap.Key, definitions), Relate(map.Value, expectedMap.Value, definitions)]);
    if (actual is TableHixPattern actualTable && expected is MapHixPattern expectedMapEntries)
      return Merge(actualTable.Fields.Select(field => Relate(field.Pattern, expectedMapEntries.Value, definitions)));
    if (actual is TupleHixPattern tuple && expected is TupleHixPattern expectedTuple) {
      if (tuple.Fields.Count < expectedTuple.Fields.Count(field => !field.Optional) || tuple.Fields.Count > expectedTuple.Fields.Count)
        return HixPatternRelation.Never;
      return Merge(tuple.Fields.Select((field, index) => Relate(field.Pattern, expectedTuple.Fields[index].Pattern, definitions)));
    }
    if (actual is TableHixPattern table && expected is TableHixPattern expectedTable) {
      var fields = table.Fields.ToDictionary(field => field.Name, StringComparer.Ordinal);
      var relations = new List<HixPatternRelation>();
      foreach (var field in expectedTable.Fields) {
        if (!fields.TryGetValue(field.Name, out var supplied)) {
          if (!field.Optional) return HixPatternRelation.Never;
          continue;
        }
        relations.Add(Relate(supplied.Pattern, field.Pattern, definitions));
      }
      return Merge(relations);
    }
    return HixPatternRelation.Never;
  }

  private static HixPatternRelation Merge(IEnumerable<HixPatternRelation> values) {
    var result = HixPatternRelation.Always;
    foreach (var value in values) {
      if (value == HixPatternRelation.Never) return value;
      if (value == HixPatternRelation.Maybe) result = value;
    }
    return result;
  }
}
