using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

/// <summary>Resolves declarative composition to concrete serializable pattern requirements.</summary>
public static class PatternComposition {
  public static IReadOnlyDictionary<string, HixPattern> ResolveDefinitions(IReadOnlyDictionary<string, HixPattern> definitions) {
    var resolved = new Dictionary<string, HixPattern>(StringComparer.Ordinal);
    var active = new HashSet<string>(StringComparer.Ordinal);
    HixPattern Definition(string name) {
      if (resolved.TryGetValue(name, out var result)) return result;
      if (!definitions.TryGetValue(name, out var source)) throw new ArgumentException("unknown pattern '" + name + "'");
      if (!active.Add(name)) throw new ArgumentException("cyclic pattern composition '" + name + "'");
      result = Resolve(source, Definition);
      active.Remove(name);
      return resolved[name] = result;
    }
    foreach (var name in definitions.Keys) Definition(name);
    return resolved;
  }

  public static HixPattern Resolve(HixPattern pattern, IReadOnlyDictionary<string, HixPattern> definitions) =>
    Resolve(pattern, name => definitions.TryGetValue(name, out var value) ? value :
      throw new ArgumentException("unknown pattern '" + name + "'"));

  public static IReadOnlyList<HixIrNode> ResolveDeclarations(IEnumerable<HixIrNode> declarations,
    IReadOnlyDictionary<string, HixPattern> definitions) => declarations.Select(new CompositionRewriter(definitions).Rewrite).ToArray();

  private sealed class CompositionRewriter(IReadOnlyDictionary<string, HixPattern> definitions) : HixIrRewriter {
    private HixPattern Pattern(HixPattern pattern) => pattern == null ? null : Resolve(pattern, definitions);
    private SignatureField Field(SignatureField field) => new(field.Name, Pattern(field.Pattern), field.Variadic,
      field.Metadata.Select(Rewrite).ToArray(), field.Optional, Rewrite(field.DefaultValue));
    private HixIrNode Assignment(AssignmentStatementIr assignment) {
      var result = new AssignmentStatementIr(assignment.Storage, assignment.Name, Rewrite(assignment.Value),
        assignment.IsCarried, assignment.IsDeclaration, Pattern(assignment.DeclaredPattern));
      result.SetMetadata(assignment.Metadata.Select(Rewrite).ToArray());
      return CopyLocation(assignment, result);
    }
    protected override HixIrNode RewriteNode(HixIrNode node) {
      return node switch {
        TypeDeclarationIr type => CopyLocation(type, new TypeDeclarationIr(type.Name, definitions[type.Name], type.Metadata.Select(Rewrite).ToArray())),
        FunctionDeclarationIr function => CopyLocation(function, new FunctionDeclarationIr(function.Name, function.IsPure,
          function.IsNoinline, function.Signatures.Select(signature => new FunctionSignature(Pattern(signature.InputPattern), signature.Inputs?.Select(Field).ToArray(),
            Pattern(signature.OutputPattern), signature.Outputs?.Select(Field).ToArray(), signature.IsPatternSyntax) { TypeParameters = signature.TypeParameters }).ToArray(), Rewrite(function.Body), function.Metadata.Select(Rewrite).ToArray(), function.TypeParameters, function.ExternalName)),
        MixinDeclarationIr mixin => CopyLocation(mixin, new MixinDeclarationIr(mixin.Name, mixin.IsDerivation,
          mixin.Declarations.Select(Rewrite).ToArray(), mixin.Metadata.Select(Rewrite).ToArray(), mixin.Parameters.Select(Field).ToArray())),
        AssignmentStatementIr assignment => Assignment(assignment),
        InlineExpressionIr inline => CopyLocation(inline, new InlineExpressionIr(Rewrite(inline.Body), inline.ResultLocal, Pattern(inline.ResultPattern))),
        _ => base.RewriteNode(node)
      };
    }
  }

  private static HixPattern Resolve(HixPattern pattern, Func<string, HixPattern> definition) {
    HixPattern Apply(HixPattern value) => Resolve(value, definition);
    HixPattern Dereference(HixPattern value) {
      var active = new HashSet<string>();
      while (value is NamedHixPattern named) {
        if (!active.Add(named.Name)) throw new ArgumentException("cyclic pattern composition '" + named.Name + "'");
        value = definition(named.Name);
      }
      return value switch {
        DocumentedHixPattern documented => documented with { Underlying = Dereference(documented.Underlying) },
        ConstrainedHixPattern constrained => constrained with { Underlying = Dereference(constrained.Underlying) },
        TaggedHixPattern tagged => tagged with { Underlying = Dereference(tagged.Underlying) },
        _ => Apply(value)
      };
    }
    HixPatternField Field(HixPatternField field) => field with { Pattern = Apply(field.Pattern) };
    switch (pattern) {
      case InheritedHixPattern inherited:
        var parent = Dereference(inherited.Parent);
        if (parent is TraitHixPattern) throw new ArgumentException("traits must be used with %trait, not %inherit");
        return Merge(Dereference(inherited.Underlying), parent, definition);
      case TraitUsageHixPattern usage:
        var trait = Dereference(usage.Trait) as TraitHixPattern ??
          throw new ArgumentException("%trait requires a trait type");
        return AddTrait(Dereference(usage.Underlying), trait);
      case TraitHixPattern declaredTrait:
        var data = Dereference(declaredTrait.Underlying);
        if (!HixPatterns.TryStructural(data, null, out var traitData) || traitData is not TableHixPattern) throw new ArgumentException("trait type must be a table pattern");
        return declaredTrait with { Underlying = data };
      case TableHixPattern tablePattern: return tablePattern with { Fields = tablePattern.Fields.Select(Field).ToArray() };
      case TupleHixPattern tuple: return tuple with { Fields = tuple.Fields.Select(Field).ToArray() };
      case UnionHixPattern union: return HixPatterns.Union(union.Patterns.Select(Apply));
      case ManyHixPattern many: return many with { Element = Apply(many.Element) };
      case MapHixPattern map: return map with { Key = Apply(map.Key), Value = Apply(map.Value) };
      case TaggedHixPattern tagged: return tagged with { Underlying = Apply(tagged.Underlying) };
      case DocumentedHixPattern documented: return documented with { Underlying = Apply(documented.Underlying) };
      case ConstrainedHixPattern constrained: return constrained with { Underlying = Apply(constrained.Underlying) };
      case ConstantHixPattern constant: return constant with { Underlying = Apply(constant.Underlying) };
      case EnumHixPattern enumeration: return enumeration with { Underlying = Apply(enumeration.Underlying) };
      case IntersectionHixPattern intersection: return intersection with { Left = Apply(intersection.Left), Right = Apply(intersection.Right) };
      case DefinedHixPattern defined: return defined with { Root = Apply(defined.Root) };
      case DelegateHixPattern callable: return callable with { Parameters = callable.Parameters.Select(Field).ToArray(), Result = Apply(callable.Result) };
      default: return pattern;
    }
  }

  private static HixPattern AddTrait(HixPattern pattern, TraitHixPattern trait) {
    if (pattern is DocumentedHixPattern documented) return documented with { Underlying = AddTrait(documented.Underlying, trait) };
    if (pattern is TaggedHixPattern tagged) return tagged with { Underlying = AddTrait(tagged.Underlying, trait) };
    if (pattern is ConstrainedHixPattern constrained) return constrained with { Underlying = AddTrait(constrained.Underlying, trait) };
    if (pattern is not TableHixPattern table) throw new ArgumentException("%trait only applies to table patterns");
    if (table.Fields.Any(field => field.Name == trait.FieldName)) throw new ArgumentException("duplicate trait component '" + trait.FieldName + "'");
    return table with { Fields = table.Fields.Concat(new[] { new HixPatternField(trait.FieldName, trait.Underlying) }).ToArray() };
  }

  private static HixPattern Merge(HixPattern left, HixPattern right, Func<string, HixPattern> definition) {
    if (left is DocumentedHixPattern documented) return documented with { Underlying = Merge(documented.Underlying, right, definition) };
    if (right is DocumentedHixPattern parentDocumentation) return Merge(left, parentDocumentation.Underlying, definition);
    if (left is TaggedHixPattern leftTag && right is TaggedHixPattern rightTag) {
      if (leftTag.Discriminator != rightTag.Discriminator) throw new ArgumentException("conflicting inherited tagged discriminators");
      return leftTag with { Underlying = Merge(leftTag.Underlying, rightTag.Underlying, definition) };
    }
    if (right is TaggedHixPattern parentTag) return parentTag with { Underlying = Merge(left, parentTag.Underlying, definition) };
    if (left is ConstrainedHixPattern leftConstraint) return leftConstraint with { Underlying = Merge(leftConstraint.Underlying, right, definition) };
    if (right is ConstrainedHixPattern rightConstraint) return rightConstraint with { Underlying = Merge(left, rightConstraint.Underlying, definition) };
    if (left is TaggedHixPattern tagged) return tagged with { Underlying = Merge(tagged.Underlying, right, definition) };
    if (left is TableHixPattern table && right is TableHixPattern parent) {
      var fields = parent.Fields.ToList();
      foreach (var field in table.Fields) {
        var index = fields.FindIndex(existing => existing.Name == field.Name);
        if (index < 0) fields.Add(field); else fields[index] = MergeField(fields[index], field, definition);
      }
      return new TableHixPattern(fields.ToArray());
    }
    if (left is TupleHixPattern tuple && right is TupleHixPattern parentTuple) {
      if (tuple.Fields.Count != parentTuple.Fields.Count) throw new ArgumentException("inherited tuple patterns must have equal lengths");
      return new TupleHixPattern(tuple.Fields.Zip(parentTuple.Fields, (leftField, rightField) => MergeField(leftField, rightField, definition)).ToArray());
    }
    throw new ArgumentException("%inherit requires two table patterns or two tuple patterns");
  }

  private static HixPatternField MergeField(HixPatternField left, HixPatternField right, Func<string, HixPattern> definition) {
    if (left.HasDefault && right.HasDefault && !Equals(left.DefaultValue, right.DefaultValue))
      throw new ArgumentException("conflicting inherited defaults for field '" + left.Name + "'");
    if (left.Graph != null && right.Graph != null && left.Graph != right.Graph)
      throw new ArgumentException("conflicting inherited graph metadata for field '" + left.Name + "'");
    var pattern = Intersect(left.Pattern, right.Pattern, definition);
    var required = !left.AllowsMissing || !right.AllowsMissing;
    // A default makes omission legal, so it cannot survive an intersected required requirement.
    var hasDefault = !required && (left.HasDefault || right.HasDefault);
    return left with { Pattern = pattern, Optional = !required && !hasDefault,
      HasDefault = hasDefault, DefaultValue = hasDefault ? left.HasDefault ? left.DefaultValue : right.DefaultValue : null,
      Graph = left.Graph ?? right.Graph, Metadata = left.Metadata.Concat(right.Metadata).Distinct().ToArray() };
  }

  private static HixPattern Intersect(HixPattern left, HixPattern right, Func<string, HixPattern> definition) {
    HixPattern Dereference(HixPattern value) {
      var active = new HashSet<string>();
      while (value is NamedHixPattern named && active.Add(named.Name)) value = definition(named.Name);
      return value;
    }
    if (left is NamedHixPattern) left = Dereference(left);
    if (right is NamedHixPattern) right = Dereference(right);
    if (Equals(left, right)) return left;
    if (left is AnyHixPattern) return right;
    if (right is AnyHixPattern) return left;
    if (left is KindHixPattern a && right is KindHixPattern b && a.ValueKind != b.ValueKind)
      throw new ArgumentException("impossible inherited field intersection: " + left.Display + " and " + right.Display);
    if (left is TableHixPattern && right is TableHixPattern || left is TupleHixPattern && right is TupleHixPattern)
      return Merge(left, right, definition);
    var leftKind = HixGenericTypes.Erase(left);
    var rightKind = HixGenericTypes.Erase(right);
    if (leftKind != HixValueKind.Any && rightKind != HixValueKind.Any && leftKind != rightKind)
      throw new ArgumentException("impossible inherited field intersection: " + left.Display + " and " + right.Display);
    return new IntersectionHixPattern(left, right);
  }
}
