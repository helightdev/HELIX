using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Runtime;

namespace Hix.Compiler;

/// <summary>Converts retained annotations to immutable, syntax-free descriptor data.</summary>
public static class HixMetadataCompiler {
  public static MetadataIr Synthetic() => new("synthetic", []) { Retention = HixMetadataRetention.Runtime };

  public static IReadOnlyList<BytecodeMetadata> Compile(IEnumerable<MetadataIr> metadata) => Array.AsReadOnly(metadata
    .Where(item => item.Name != null && item.Retention != HixMetadataRetention.Compiler)
    .Select(item => new BytecodeMetadata(item.Name, Array.AsReadOnly(item.Values.Select(Constant).ToArray()), item.Retention)).ToArray());

  public static IHixValue Constant(ExpressionIr value) => value switch {
    StringExpressionIr text => new LiteralHixValue(HixString.Dynamic(text.Value)),
    NumberExpressionIr number => new NumberHixValue(number.Value),
    BooleanExpressionIr boolean => BooleanHixValue.From(boolean.Value),
    NullExpressionIr => NullHixValue.Instance,
    MissingExpressionIr => MissingHixValue.Instance,
    RootExpressionIr {IsSmart: false} root => new PatternHixValue(HixPatterns.Named(root.Name)),
    TupleExpressionIr tuple => new TupleHixValue(tuple.Values.Select(Constant).ToArray()),
    TableExpressionIr table => new HixTableValue(table.Entries.Select(entry =>
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(entry.Key), Constant(entry.Value)))),
    _ => throw new ArgumentException("retained metadata arguments must be constant values")
  };

  public static IReadOnlyList<BytecodeMetadata> Runtime(IReadOnlyList<BytecodeMetadata> metadata) =>
    Array.AsReadOnly(metadata.Where(item => item.Retention == HixMetadataRetention.Runtime)
      .Select(item => item with {Values = Array.AsReadOnly(item.Values.Select(RuntimeValue).ToArray())}).ToArray());

  private static IHixValue RuntimeValue(IHixValue value) => value switch {
    PatternHixValue pattern => new PatternHixValue(Runtime(pattern.Pattern)),
    TupleHixValue tuple => new TupleHixValue(tuple.Values.Select(RuntimeValue).ToArray()),
    HixTableValue table => new HixTableValue(table.Entries.Select(entry =>
      new KeyValuePair<HixString, IHixValue>(entry.Key, RuntimeValue(entry.Value)))),
    _ => value
  };

  public static BytecodeField Runtime(BytecodeField field) => field with {
    Pattern = Runtime(field.Pattern), Metadata = Runtime(field.Metadata)
  };
  public static BytecodeFunction Runtime(BytecodeFunction function) => function with {
    Metadata = Runtime(function.Metadata),
    Signatures = function.Signatures.Select(signature => signature with {
      InputPattern = Runtime(signature.InputPattern), OutputPattern = Runtime(signature.OutputPattern),
      Inputs = signature.Inputs?.Select(Runtime).ToArray(), Outputs = signature.Outputs?.Select(Runtime).ToArray()
    }).ToArray()
  };

  public static HixPattern Runtime(HixPattern pattern) {
    HixPatternField Field(HixPatternField field) => field with {Pattern = Runtime(field.Pattern), Metadata = Runtime(field.Metadata)};
    return pattern switch {
      PatternValueHixPattern => new KindHixPattern(HixValueKind.Pattern),
      TableHixPattern table => table with {Fields = table.Fields.Select(Field).ToArray()},
      TupleHixPattern tuple => tuple with {Fields = tuple.Fields.Select(Field).ToArray()},
      DelegateHixPattern callable => callable with {Parameters = callable.Parameters.Select(Field).ToArray(), Result = Runtime(callable.Result)},
      SignatureHixPattern signature => signature with {Parameters = signature.Parameters.Select(Field).ToArray(), Result = Runtime(signature.Result)},
      OpaqueHixPattern opaque => new OpaqueHixPattern(opaque.Definition, opaque.Arguments.Select(Runtime).ToArray()),
      UnionHixPattern union => union with {Patterns = union.Patterns.Select(Runtime).ToArray()},
      ManyHixPattern many => many with {Element = Runtime(many.Element)},
      MapHixPattern map => map with {Key = Runtime(map.Key), Value = Runtime(map.Value)},
      TraitHixPattern trait => trait with {Underlying = Runtime(trait.Underlying)},
      IntersectionHixPattern intersection => intersection with {Left = Runtime(intersection.Left), Right = Runtime(intersection.Right)},
      TaggedHixPattern tagged => tagged with {Underlying = Runtime(tagged.Underlying)},
      DocumentedHixPattern documented => documented with {Underlying = Runtime(documented.Underlying)},
      ConstrainedHixPattern constrained => constrained with {Underlying = Runtime(constrained.Underlying)},
      DefinedHixPattern defined => defined with {Root = Runtime(defined.Root), Definitions = defined.Definitions.ToDictionary(item => item.Key, item => Runtime(item.Value))},
      _ => pattern
    };
  }

  internal static void Fingerprint(HixFingerprintBuilder builder, IReadOnlyList<BytecodeMetadata> metadata) {
    builder.Append(metadata.Count);
    foreach (var item in metadata) {
      builder.Append(item.Name); builder.Append((int)item.Retention); builder.Append(item.Values.Count);
      foreach (var value in item.Values) FingerprintValue(builder, value);
    }
  }
  private static void FingerprintValue(HixFingerprintBuilder builder, IHixValue value) {
    builder.Append((int)value.Kind);
    switch (value) {
      case LiteralHixValue text: builder.Append(text.Value.Resolve(null)); break;
      case NumberHixValue number: builder.Append(number.Value); break;
      case BooleanHixValue boolean: builder.Append(boolean.Value); break;
      case TupleHixValue tuple:
        builder.Append(tuple.Values.Count);
        foreach (var item in tuple.Values) FingerprintValue(builder, item);
        break;
      case HixTableValue table:
        builder.Append(table.Count);
        foreach (var item in table.Entries.OrderBy(item => item.Key.Resolve(null), StringComparer.Ordinal)) {
          builder.Append(item.Key.Resolve(null)); FingerprintValue(builder, item.Value);
        }
        break;
      case PatternHixValue pattern:
        builder.Append(HixGenericTypes.PatternIdentity(pattern.Pattern)); FingerprintPattern(builder, pattern.Pattern); break;
      case NullHixValue or MissingHixValue: break;
      default: throw new ArgumentException("unsupported retained metadata value");
    }
  }

  internal static void FingerprintPattern(HixFingerprintBuilder builder, HixPattern pattern) {
    if (pattern == null) return;
    builder.Append(HixGenericTypes.PatternIdentity(pattern));
    void Fields(IEnumerable<HixPatternField> fields) {
      foreach (var field in fields) { Fingerprint(builder, field.Metadata); FingerprintPattern(builder, field.Pattern); }
    }
    switch (pattern) {
      case TableHixPattern table: Fields(table.Fields); break;
      case TupleHixPattern tuple: Fields(tuple.Fields); break;
      case DelegateHixPattern callable: Fields(callable.Parameters); FingerprintPattern(builder, callable.Result); break;
      case SignatureHixPattern signature: Fields(signature.Parameters); FingerprintPattern(builder, signature.Result); break;
      case OpaqueHixPattern opaque: foreach (var child in opaque.Arguments) FingerprintPattern(builder, child); break;
      case UnionHixPattern union: foreach (var child in union.Patterns) FingerprintPattern(builder, child); break;
      case ManyHixPattern many: FingerprintPattern(builder, many.Element); break;
      case MapHixPattern map: FingerprintPattern(builder, map.Key); FingerprintPattern(builder, map.Value); break;
      case TraitHixPattern trait: FingerprintPattern(builder, trait.Underlying); break;
      case IntersectionHixPattern intersection: FingerprintPattern(builder, intersection.Left); FingerprintPattern(builder, intersection.Right); break;
      case TaggedHixPattern tagged: FingerprintPattern(builder, tagged.Underlying); break;
      case DocumentedHixPattern documented: FingerprintPattern(builder, documented.Underlying); break;
      case ConstrainedHixPattern constrained: FingerprintPattern(builder, constrained.Underlying); break;
      case DefinedHixPattern defined:
        FingerprintPattern(builder, defined.Root);
        foreach (var child in defined.Definitions.OrderBy(item => item.Key, StringComparer.Ordinal)) FingerprintPattern(builder, child.Value);
        break;
    }
  }

}
