using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Hix.Hal;
using Hix.Runtime;
using K = Hix.HixValueKind;

namespace Hix.Standalone;

internal static class HalFunctions {
  public static void Register(FunctionSignatureRegistryBuilder functions) => functions.Add(
    new SimpleFunction("parseHal", [new FunctionSignature(K.Any, [K.String]), new FunctionSignature(K.Any, [K.String, K.String])],
      (thread, args) => Parse(thread, args), documentation: "Parses a Hal asset and returns its primary or named local section."),
    new SimpleFunction("writeHal", [new FunctionSignature(K.String, [K.Any]), new FunctionSignature(K.String, [K.Any, K.Pattern]),
        new FunctionSignature(K.String, [K.Any, K.Kind])], (thread, args) => Write(thread, args), acceptsErrors: true,
      documentation: "Writes a Hix value as a canonical single-section Hal asset."));

  private static IHixValue Parse(HixThread thread, IHixValue[] args) {
    try {
      var document = HalDocumentSyntax.Parse(thread.ResolveText(args[0]));
      if (document.Diagnostics.Count != 0) throw new HalException(string.Join("; ", document.Diagnostics.Select(
        diagnostic => "line " + diagnostic.Line + ": " + diagnostic.Message)));
      return new Asset(document.Sections).Resolve(thread, args.Length == 1 ? "0" : thread.ResolveText(args[1]));
    } catch (HalException exception) { return thread.Error("invalid Hal: " + exception.Message); }
  }

  private static IHixValue Write(HixThread thread, IHixValue[] args) {
    var pattern = args.Length == 1 ? Infer(args[0]) : args[1] switch {
      PatternHixValue value => value.Pattern, KindHixValue kind => new KindHixPattern(kind.ValueKind), _ => HixPattern.Any
    };
    if (!thread.Matches(pattern, args[0], out var failure)) return thread.Error("Hal value does not match pattern: " + failure);
    var text = new StringBuilder("--- ").Append(pattern is NamedHixPattern named ? named.Name : pattern.Display).AppendLine();
    if (args[0] is HixTableValue table) WriteEntries(text, table, thread, 0); else WriteValue(text, args[0], thread, 0);
    return HixThread.String(text.ToString().TrimEnd() + "\n");
  }

  private static HixPattern Infer(IHixValue value) => new KindHixPattern(value.Kind);
  private static void WriteEntries(StringBuilder text, HixTableValue table, HixThread thread, int depth) {
    foreach (var entry in table.Entries) {
      text.Append(' ', depth * 2).Append(entry.Key.Resolve(thread.Strings)).Append(" = ");
      WriteValue(text, entry.Value, thread, depth); text.AppendLine();
    }
  }
  private static void WriteValue(StringBuilder text, IHixValue value, HixThread thread, int depth) {
    switch (value) {
      case NullHixValue: text.Append("null"); break;
      case BooleanHixValue flag: text.Append(flag.Value ? "true" : "false"); break;
      case NumberHixValue number: text.Append(number.Value.ToString("R", CultureInfo.InvariantCulture)); break;
      case LiteralHixValue literal: text.Append('"').Append(Escape(literal.Value.Resolve(thread.Strings))).Append('"'); break;
      case TupleHixValue tuple:
        text.Append('['); if (tuple.Values.Count != 0) text.AppendLine();
        foreach (var item in tuple.Values) { text.Append(' ', (depth + 1) * 2); WriteValue(text, item, thread, depth + 1); text.AppendLine(); }
        text.Append(' ', depth * 2).Append(']'); break;
      case HixTableValue table:
        text.Append('{'); if (table.Count != 0) text.AppendLine(); WriteEntries(text, table, thread, depth + 1);
        text.Append(' ', depth * 2).Append('}'); break;
      default: throw new HalException("cannot write Hix " + value.Kind.ToString().ToLowerInvariant() + " values");
    }
  }
  private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"")
    .Replace("\r", "\\r").Replace("\n", "\\n");

  private sealed class Asset {
    private readonly IReadOnlyDictionary<string, HalSectionSyntax> sections;
    private readonly Dictionary<string, IHixValue> values = new(StringComparer.Ordinal);
    private readonly HashSet<string> active = new(StringComparer.Ordinal);
    public Asset(IReadOnlyList<HalSectionSyntax> source) {
      if (source.Count == 0) throw new HalException("expected at least one section");
      if (source.Skip(1).Any(section => section.Id == null)) throw new HalException("only the first section may omit its id");
      if (source.GroupBy(section => section.Id, StringComparer.Ordinal).Any(group => group.Count() > 1)) throw new HalException("duplicate section id");
      sections = source.ToDictionary(section => section.Id, StringComparer.Ordinal);
    }
    public IHixValue Resolve(HixThread thread, string id) {
      if (values.TryGetValue(id, out var cached)) return cached;
      if (!sections.TryGetValue(id, out var section)) throw new HalException("unknown section '" + id + "'");
      if (!active.Add(id)) throw new HalException("cyclic section reference '" + id + "'");
      var value = Construct(thread, section.Type, Evaluate(thread, new HalTableSyntax(section.Fields)));
      active.Remove(id); return values[id] = value;
    }
    private IHixValue Evaluate(HixThread thread, HalValueSyntax node) => node switch {
      HalScalarSyntax scalar => Host(scalar.Value),
      HalListSyntax sequence => new TupleHixValue(sequence.Values.Select(item => Evaluate(thread, item)).ToArray()),
      HalTableSyntax table => new HixTableValue(table.Fields.Select(item =>
        new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(item.Name), Evaluate(thread, item.Value)))),
      HalTypedSyntax typed => Construct(thread, typed.Type, Evaluate(thread, typed.Value)),
      HalCallSyntax {Name: "ref", Arguments.Count: 1} reference => Resolve(thread, ScalarText(reference.Arguments[0].Value)),
      HalSelectionSyntax selection => selection.Members.Aggregate(Evaluate(thread, selection.Receiver),
        (current, member) => current.Select(thread, HixString.Dynamic(member))),
      HalCallSyntax expression => throw new HalException("unsupported data expression '" + expression.Name + "'"),
      _ => throw new HalException("unsupported value")
    };
    private static string ScalarText(HalValueSyntax node) => node is HalScalarSyntax {Value: { } value}
      ? Convert.ToString(value, CultureInfo.InvariantCulture) : throw new HalException("reference id must be a scalar");
    private static IHixValue Construct(HixThread thread, string type, IHixValue value) {
      var pattern = HixPatterns.Named(type);
      if (pattern is NamedHixPattern named && !thread.PatternDefinitions.TryGetValue(named.Name, out pattern))
        throw new HalException("unknown pattern '" + type + "'");
      value = ApplyDefaults(thread, value, pattern, thread.PatternDefinitions);
      if (!thread.Matches(pattern, value, out var failure)) throw new HalException("value does not match '" + type + "': " + failure);
      return value;
    }
  }

  private static IHixValue ApplyDefaults(HixThread thread, IHixValue value, HixPattern pattern,
    IReadOnlyDictionary<string, HixPattern> definitions) {
    if (pattern is NamedHixPattern named && definitions.TryGetValue(named.Name, out var resolved)) return ApplyDefaults(thread, value, resolved, definitions);
    if (pattern is DefinedHixPattern defined) return ApplyDefaults(thread, value, defined.Root, defined.Definitions);
    if (pattern is TaggedHixPattern tagged && value is HixTableValue taggedTable) {
      var key = HixString.Dynamic(TaggedHixPattern.FieldName);
      var result = taggedTable.TryGetValue(thread, key, out _) ? taggedTable : taggedTable.Put(thread, key, HixThread.String(tagged.Discriminator));
      return ApplyDefaults(thread, result, tagged.Underlying, definitions);
    }
    if (pattern is DocumentedHixPattern documented) return ApplyDefaults(thread, value, documented.Underlying, definitions);
    if (pattern is ConstrainedHixPattern constrained) return ApplyDefaults(thread, value, constrained.Underlying, definitions);
    if (pattern is ConstantHixPattern constant) return ApplyDefaults(thread, value, constant.Underlying, definitions);
    if (pattern is EnumHixPattern enumeration) return ApplyDefaults(thread, value, enumeration.Underlying, definitions);
    if (pattern is TableHixPattern table && value is HixTableValue objectValue) {
      var result = objectValue;
      foreach (var field in table.Fields) {
        var key = HixString.Dynamic(field.Name);
        if (!result.TryGetValue(thread, key, out var member)) { if (field.HasDefault) result = result.Put(thread, key, Host(field.DefaultValue)); }
        else result = result.Put(thread, key, ApplyDefaults(thread, member, field.Pattern, definitions));
      }
      return result;
    }
    if (pattern is ManyHixPattern many && value is TupleHixValue sequence)
      return new TupleHixValue(sequence.Values.Select(item => ApplyDefaults(thread, item, many.Element, definitions)).ToArray());
    if (pattern is TupleHixPattern tuple && value is TupleHixValue tupleValue)
      return new TupleHixValue(tupleValue.Values.Select((item, position) => position < tuple.Fields.Count
        ? ApplyDefaults(thread, item, tuple.Fields[position].Pattern, definitions) : item).ToArray());
    return value;
  }
  private static IHixValue Host(object value) => value switch {
    null => NullHixValue.Instance, bool flag => BooleanHixValue.From(flag), double number => new NumberHixValue(number),
    string text => HixThread.String(text), _ => throw new HalException("unsupported scalar")
  };
  private sealed class HalException(string message) : Exception(message);
}
