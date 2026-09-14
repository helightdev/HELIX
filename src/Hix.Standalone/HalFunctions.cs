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
    new SimpleFunction("standalone_parseHal", (thread, args) => Parse(thread, args)),
    new SimpleFunction("standalone_writeHal", (thread, args) => Write(thread, args)));

  internal static IReadOnlyDictionary<string, IHixValue> Evaluate(HalDocumentSyntax document, IReadOnlyDictionary<string, HixPattern> definitions,
    HixCoroutine thread, Func<HalAssetReference, IHixValue> external = null) {
    if (document.Diagnostics.Count != 0) throw new ArgumentException(string.Join("; ", document.Diagnostics.Select(d => d.Message)));
    var asset = new Asset(document.Sections, definitions, external);
    return document.Sections.ToDictionary(s => s.Id, s => asset.Resolve(thread, s.Id), StringComparer.Ordinal);
  }
  internal static string Validate(HalDocumentSyntax document, IReadOnlyDictionary<string, HixPattern> definitions, Func<HalAssetReference,IHixValue> external = null) {
    try {
      using var context = new HixCoroutine();
      var asset = new Asset(document.Sections, definitions, external);
      foreach (var section in document.Sections) asset.Resolve(context, section.Id);
      return null;
    } catch (HalException exception) { return exception.Message; }
  }

  private static IHixValue Parse(HixCoroutine thread, IHixValue[] args) {
    try {
      var document = HalDocumentSyntax.Parse(thread.ResolveText(args[0]));
      if (document.Diagnostics.Count != 0) throw new HalException(string.Join("; ", document.Diagnostics.Select(
        diagnostic => "line " + diagnostic.Line + ": " + diagnostic.Message)));
      return new Asset(document.Sections).Resolve(thread, args.Length == 1 ? document.Sections.FirstOrDefault()?.Id ?? "0" : thread.ResolveText(args[1]));
    } catch (HalException exception) { return thread.Error("invalid Hal: " + exception.Message); }
  }

  private static IHixValue Write(HixCoroutine thread, IHixValue[] args) {
    var pattern = args.Length == 1 ? Infer(args[0]) : ((PatternHixValue)args[1]).Pattern;
    if (HixPatterns.TryTrait(pattern, thread.PatternDefinitions, out _)) return thread.Error("traits are not instantiable as Hal sections");
    if (!thread.Matches(pattern, args[0], out var failure)) return thread.Error("Hal value does not match pattern: " + failure);
    var text = new StringBuilder("--- ").Append(pattern is NamedHixPattern named ? named.Name : pattern.Display).AppendLine();
    if (args[0] is HixTableValue table) WriteEntries(text, table, thread, 0); else WriteValue(text, args[0], thread, 0);
    return HixCoroutine.String(text.ToString().TrimEnd() + "\n");
  }

  private static HixPattern Infer(IHixValue value) => new KindHixPattern(value.Kind);
  private static void WriteEntries(StringBuilder text, HixTableValue table, HixCoroutine thread, int depth) {
    foreach (var entry in table.Entries) {
      var name = entry.Key.Resolve(thread.Strings);
      var key = System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z_0-9]*$") ? name : "\"" + Escape(name) + "\"";
      text.Append(' ', depth * 2).Append(key).Append(" = ");
      WriteValue(text, entry.Value, thread, depth); text.AppendLine();
    }
  }
  private static void WriteValue(StringBuilder text, IHixValue value, HixCoroutine thread, int depth) {
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
    private readonly IReadOnlyDictionary<string, HixPattern> definitions;
    private readonly Func<HalAssetReference,IHixValue> external;
    public Asset(IReadOnlyList<HalSectionSyntax> source, IReadOnlyDictionary<string, HixPattern> definitions = null, Func<HalAssetReference,IHixValue> external = null) {
      this.definitions = definitions;
      this.external = external;
      if (source.Count == 0) throw new HalException("expected at least one section");
      if (source.Skip(1).Any(section => section.Id == null)) throw new HalException("only the first section may omit its id");
      if (source.GroupBy(section => section.Id, StringComparer.Ordinal).Any(group => group.Count() > 1)) throw new HalException("duplicate section id");
      sections = source.ToDictionary(section => section.Id, StringComparer.Ordinal);
    }
    public IHixValue Resolve(HixCoroutine thread, string id) {
      if (values.TryGetValue(id, out var cached)) return cached;
      if (!sections.TryGetValue(id, out var section)) throw new HalException("unknown section '" + id + "'");
      if (!active.Add(id)) throw new HalException("cyclic section reference '" + id + "'");
      var value = Construct(thread, section.Type, Evaluate(thread, section.Value));
      active.Remove(id); return values[id] = value;
    }
    private IHixValue Evaluate(HixCoroutine thread, HalValueSyntax node) => node switch {
      HalScalarSyntax scalar => Host(scalar.Value),
      HalListSyntax sequence => new TupleHixValue(sequence.Values.Select(item => Evaluate(thread, item)).ToArray()),
      HalTableSyntax table => new HixTableValue(table.Fields.Select(item =>
        new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(item.Name), Evaluate(thread, item.Value)))),
      HalTypedSyntax typed => Construct(thread, typed.Type, Evaluate(thread, typed.Value)),
      HalCallSyntax {Name: "ref", Arguments.Count: 1} reference => Resolve(thread, ScalarText(reference.Arguments[0].Value)),
      HalCallSyntax {Name: "asset"} reference => external != null
        ? external(HalAssetReference.Parse(reference))
        : throw new HalException("external asset references require an AssetDatabase"),
      HalSelectionSyntax selection => selection.Members.Aggregate(Evaluate(thread, selection.Receiver),
        (current, member) => current.Select(thread, HixString.Dynamic(member))),
      HalCallSyntax expression => throw new HalException("unsupported data expression '" + expression.Name + "'"),
      _ => throw new HalException("unsupported value")
    };
    private static string ScalarText(HalValueSyntax node) => node is HalScalarSyntax {Value: { } value}
      ? Convert.ToString(value, CultureInfo.InvariantCulture) : throw new HalException("reference id must be a scalar");
    private IHixValue Construct(HixCoroutine thread, string type, IHixValue value) {
      var patterns = definitions ?? thread.PatternDefinitions;
      var pattern = HixPatterns.Named(type);
      if (pattern is NamedHixPattern named && !patterns.TryGetValue(named.Name, out pattern))
        throw new HalException("unknown pattern '" + type + "'");
      if (HixPatterns.TryTrait(pattern, patterns, out _)) throw new HalException("traits are not instantiable");
      value = ApplyDefaults(thread, value, pattern, patterns);
      if (!HixPatternMatcher.Matches(pattern, value, thread, patterns, out var failure)) throw new HalException("value does not match '" + type + "': " + failure);
      return value;
    }
  }

  private static IHixValue ApplyDefaults(HixCoroutine thread, IHixValue value, HixPattern pattern,
    IReadOnlyDictionary<string, HixPattern> definitions) {
    if (pattern is NamedHixPattern named && definitions.TryGetValue(named.Name, out var resolved)) return ApplyDefaults(thread, value, resolved, definitions);
    if (pattern is DefinedHixPattern defined) return ApplyDefaults(thread, value, defined.Root, defined.Definitions);
    if (pattern is IntersectionHixPattern intersection) return ApplyDefaults(thread, ApplyDefaults(thread, value, intersection.Left, definitions), intersection.Right, definitions);
    if (pattern is TaggedHixPattern tagged && value is HixTableValue taggedTable) {
      var key = HixString.Dynamic(TaggedHixPattern.FieldName);
      var result = taggedTable.TryGetValue(thread, key, out _) ? taggedTable : taggedTable.Put(thread, key, HixCoroutine.String(tagged.Discriminator));
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
    string text => HixCoroutine.String(text), _ => throw new HalException("unsupported scalar")
  };
  private sealed class HalException(string message) : Exception(message);
}
