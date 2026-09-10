using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hix.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using K = Hix.HixValueKind;

namespace Hix.Standalone;

internal static class JsonFunctions {
  public static void Register(FunctionSignatureRegistryBuilder functions) {
    functions.Add(
      new SimpleFunction("parseJson", [new FunctionSignature(K.Any, [K.String]),
          new FunctionSignature(K.Any, [K.String, K.Pattern]), new FunctionSignature(K.Any, [K.String, K.Kind])],
        (thread, args) => Parse(thread, args), documentation: "Parses JSON into Hix null, boolean, number, string, tuple, and table values, optionally validating a pattern or kind."),
      new SimpleFunction("writeJson", [new FunctionSignature(K.String, [K.Any]),
          new FunctionSignature(K.String, [K.Any, K.Pattern]), new FunctionSignature(K.String, [K.Any, K.Kind])],
        (thread, args) => Write(thread, args), acceptsErrors: true,
        documentation: "Writes a Hix value as compact JSON, optionally validating a pattern or kind."),
      new SimpleFunction("generateJsonSchema", [new FunctionSignature(K.String, [K.Pattern])],
        (thread, args) => GenerateSchema(thread, (PatternHixValue)args[0]),
        documentation: "Generates a JSON Schema for a Hix pattern."),
      new SimpleFunction("loadJsonSchema", [new FunctionSignature(K.Pattern, [K.String])],
        (thread, args) => LoadSchema(thread, thread.ResolveText(args[0])),
        documentation: "Imports a JSON Schema as a Hix pattern.")
    );
  }

  private static IHixValue Parse(HixThread thread, IHixValue[] args) {
    try {
      var token = JToken.Parse(thread.ResolveText(args[0]), new JsonLoadSettings {
        DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
        CommentHandling = CommentHandling.Ignore,
        LineInfoHandling = LineInfoHandling.Ignore
      });
      var value = FromJson(token);
      return args.Length == 1 ? value : Validate(thread, value, ToPattern(args[1]));
    } catch (JsonException exception) { return thread.Error("invalid JSON: " + exception.Message); }
  }

  private static IHixValue Write(HixThread thread, IHixValue[] args) {
    var value = args[0];
    if (args.Length == 2 && Validate(thread, value, ToPattern(args[1])) is ErrorHixValue error) return error;
    try { return new LiteralHixValue(HixString.Dynamic(ToJson(value, thread).ToString(Formatting.None))); }
    catch (InvalidOperationException exception) { return thread.Error(exception.Message); }
  }

  private static HixPattern ToPattern(IHixValue value) => value switch {
    PatternHixValue pattern => pattern.Pattern,
    KindHixValue kind => new KindHixPattern(kind.ValueKind),
    _ => HixPattern.Any
  };

  private static IHixValue Validate(HixThread thread, IHixValue value, HixPattern pattern) =>
    thread.Matches(pattern, value, out var failure) ? value : thread.Error("JSON value does not match pattern: " + failure);

  private static IHixValue FromJson(JToken token) => token.Type switch {
    JTokenType.Null or JTokenType.Undefined => NullHixValue.Instance,
    JTokenType.Boolean => BooleanHixValue.From(token.Value<bool>()),
    JTokenType.Integer or JTokenType.Float => new NumberHixValue(token.Value<double>()),
    JTokenType.String => new LiteralHixValue(HixString.Dynamic(token.Value<string>())),
    JTokenType.Array => new TupleHixValue(token.Children().Select(FromJson).ToArray()),
    JTokenType.Object => new HixTableValue(((JObject)token).Properties().Select(property =>
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(property.Name), FromJson(property.Value)))),
    _ => throw new JsonReaderException("unsupported JSON token " + token.Type)
  };

  private static JToken ToJson(IHixValue value, HixThread thread) => value switch {
    NullHixValue => JValue.CreateNull(),
    BooleanHixValue boolean => new JValue(boolean.Value),
    NumberHixValue number when !double.IsNaN(number.Value) && !double.IsInfinity(number.Value) &&
      number.Value == Math.Truncate(number.Value) && number.Value >= long.MinValue && number.Value <= long.MaxValue =>
      new JValue((long)number.Value),
    NumberHixValue number when !double.IsNaN(number.Value) && !double.IsInfinity(number.Value) => new JValue(number.Value),
    NumberHixValue => throw new InvalidOperationException("cannot write a non-finite number as JSON"),
    LiteralHixValue text => new JValue(text.Value.Resolve(thread.Strings)),
    TupleHixValue tuple => new JArray(tuple.Values.Select(item => ToJson(item, thread))),
    HixTableValue table => new JObject(table.Entries.Select(item =>
      new JProperty(item.Key.Resolve(thread.Strings), ToJson(item.Value, thread)))),
    ErrorHixValue error => throw new InvalidOperationException("cannot write an error as JSON: " + error.Message.Resolve(thread.Strings)),
    _ => throw new InvalidOperationException("cannot write Hix " + value.Kind.ToString().ToLowerInvariant() + " values as JSON")
  };

  private static IHixValue GenerateSchema(HixThread thread, PatternHixValue value) {
    try {
      var definitions = new JObject();
      var visiting = new HashSet<string>(StringComparer.Ordinal);
      var root = SchemaFor(value.Pattern, thread.PatternDefinitions, definitions, visiting);
      root["$schema"] = "https://json-schema.org/draft/2020-12/schema";
      if (definitions.HasValues) root["$defs"] = definitions;
      return new LiteralHixValue(HixString.Dynamic(root.ToString(Formatting.None)));
    } catch (InvalidOperationException exception) { return thread.Error(exception.Message); }
  }

  private static JObject SchemaFor(HixPattern pattern, IReadOnlyDictionary<string, HixPattern> known,
    JObject definitions, ISet<string> visiting) {
    if (pattern is DefinedHixPattern defined) {
      var merged = known.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
      foreach (var item in defined.Definitions) merged[item.Key] = item.Value;
      return SchemaFor(defined.Root, merged, definitions, visiting);
    }
    if (pattern is NamedHixPattern named) {
      if (!known.TryGetValue(named.Name, out var body)) throw new InvalidOperationException("unknown pattern '" + named.Name + "'");
      if (!definitions.ContainsKey(named.Name) && visiting.Add(named.Name)) {
        definitions[named.Name] = SchemaFor(body, known, definitions, visiting);
        visiting.Remove(named.Name);
      }
      return new JObject { ["$ref"] = "#/$defs/" + EscapePointer(named.Name) };
    }
    switch (pattern) {
      case AnyHixPattern: return new JObject();
      case KindHixPattern kind: return KindSchema(kind.ValueKind);
      case UnionHixPattern union: return new JObject { ["anyOf"] = new JArray(union.Patterns.Select(item => SchemaFor(item, known, definitions, visiting))) };
      case ManyHixPattern many: return new JObject { ["type"] = "array", ["items"] = SchemaFor(many.Element, known, definitions, visiting) };
      case MapHixPattern map: return new JObject { ["type"] = "object", ["propertyNames"] = SchemaFor(map.Key, known, definitions, visiting), ["additionalProperties"] = SchemaFor(map.Value, known, definitions, visiting) };
      case TupleHixPattern tuple: {
        var required = tuple.Fields.Count(field => !field.Optional);
        return new JObject { ["type"] = "array",
          ["prefixItems"] = new JArray(tuple.Fields.Select(field => SchemaFor(field.Pattern, known, definitions, visiting))),
          ["minItems"] = required, ["maxItems"] = tuple.Fields.Count };
      }
      case TableHixPattern table: {
        var properties = new JObject(table.Fields.Select(field => new JProperty(field.Name,
          SchemaFor(field.Pattern, known, definitions, visiting))));
        var schema = new JObject { ["type"] = "object", ["properties"] = properties };
        var required = table.Fields.Where(field => !field.Optional).Select(field => field.Name).ToArray();
        if (required.Length != 0) schema["required"] = new JArray(required);
        return schema;
      }
      case ConstantHixPattern constant: {
        var schema = SchemaFor(constant.Underlying, known, definitions, visiting);
        schema["const"] = constant.Value == null ? JValue.CreateNull() : JToken.FromObject(constant.Value);
        return schema;
      }
      case ConstrainedHixPattern constrained: {
        var schema = SchemaFor(constrained.Underlying, known, definitions, visiting);
        var argument = JToken.FromObject(constrained.Argument);
        if (constrained.Constraint == HixPatternConstraintKind.Minimum) schema["minimum"] = argument;
        else if (constrained.Constraint == HixPatternConstraintKind.Maximum) schema["maximum"] = argument;
        else if (constrained.Constraint == HixPatternConstraintKind.Matches) schema["pattern"] = Convert.ToString(constrained.Argument, CultureInfo.InvariantCulture);
        else ApplyLength(schema, argument);
        return schema;
      }
      default: throw new InvalidOperationException("cannot generate JSON Schema for " + pattern.Display);
    }
  }

  private static JObject KindSchema(K kind) => kind switch {
    K.Any => new JObject(), K.Null => new JObject { ["type"] = "null" },
    K.String => new JObject { ["type"] = "string" }, K.Bool => new JObject { ["type"] = "boolean" },
    K.Number => new JObject { ["type"] = "number" }, K.Tuple => new JObject { ["type"] = "array" },
    K.Table => new JObject { ["type"] = "object" },
    _ => throw new InvalidOperationException("Hix " + kind.ToString().ToLowerInvariant() + " values have no JSON representation")
  };

  private static void ApplyLength(JObject schema, JToken value) {
    var type = (string)schema["type"];
    var prefix = type == "string" ? "Length" : type == "object" ? "Properties" : "Items";
    schema["min" + prefix] = value.DeepClone(); schema["max" + prefix] = value;
  }

  private static IHixValue LoadSchema(HixThread thread, string json) {
    try {
      var root = JObject.Parse(json);
      var definitions = new Dictionary<string, HixPattern>(StringComparer.Ordinal);
      var sourceDefinitions = (JObject)(root["$defs"] ?? root["definitions"]);
      if (sourceDefinitions != null)
        foreach (var property in sourceDefinitions.Properties()) definitions[property.Name] = PatternFor((JObject)property.Value);
      return new PatternHixValue(new DefinedHixPattern(PatternFor(root), definitions));
    } catch (JsonException exception) { return thread.Error("invalid JSON Schema: " + exception.Message); }
      catch (InvalidOperationException exception) { return thread.Error("unsupported JSON Schema: " + exception.Message); }
  }

  private static HixPattern PatternFor(JObject schema) {
    HixPattern pattern;
    if (schema["$ref"] is JValue reference) {
      var path = reference.Value<string>();
      const string prefix = "#/$defs/"; const string oldPrefix = "#/definitions/";
      var encoded = path.StartsWith(prefix, StringComparison.Ordinal) ? path.Substring(prefix.Length) :
        path.StartsWith(oldPrefix, StringComparison.Ordinal) ? path.Substring(oldPrefix.Length) : null;
      if (encoded == null) throw new InvalidOperationException("only local $defs references are supported");
      pattern = new NamedHixPattern(UnescapePointer(encoded));
    } else if (schema["anyOf"] is JArray alternatives) {
      pattern = HixPatterns.Union(alternatives.Cast<JObject>().Select(PatternFor));
    } else if (schema["const"] is { } constant) {
      pattern = new ConstantHixPattern(ConstantValue(constant), PatternWithout(schema, "const"));
    } else {
      var type = (string)schema["type"];
      pattern = type switch {
        null => HixPattern.Any,
        "null" => new KindHixPattern(K.Null), "string" => new KindHixPattern(K.String),
        "boolean" => new KindHixPattern(K.Bool), "number" or "integer" => new KindHixPattern(K.Number),
        "array" => ArrayPattern(schema), "object" => ObjectPattern(schema),
        _ => throw new InvalidOperationException("unknown schema type '" + type + "'")
      };
    }
    if (schema["minimum"] is JValue minimum) pattern = new ConstrainedHixPattern(pattern, HixPatternConstraintKind.Minimum, minimum.Value);
    if (schema["maximum"] is JValue maximum) pattern = new ConstrainedHixPattern(pattern, HixPatternConstraintKind.Maximum, maximum.Value);
    if (schema["pattern"] is JValue regex) pattern = new ConstrainedHixPattern(pattern, HixPatternConstraintKind.Matches, regex.Value<string>());
    var exactLength = ExactLength(schema);
    if (exactLength != null) pattern = new ConstrainedHixPattern(pattern, HixPatternConstraintKind.Length, exactLength.Value);
    return pattern;
  }

  private static HixPattern PatternWithout(JObject schema, string property) {
    var copy = (JObject)schema.DeepClone(); copy.Remove(property); return PatternFor(copy);
  }
  private static HixPattern ArrayPattern(JObject schema) => schema["prefixItems"] is JArray tuple
    ? new TupleHixPattern(tuple.Cast<JObject>().Select((item, index) => new HixPatternField(index.ToString(), PatternFor(item),
      index >= (schema.Value<int?>("minItems") ?? tuple.Count))).ToArray())
    : new ManyHixPattern(schema["items"] is JObject items ? PatternFor(items) : HixPattern.Any);
  private static HixPattern ObjectPattern(JObject schema) {
    if (schema["properties"] is JObject properties) {
      var required = new HashSet<string>((schema["required"] as JArray)?.Values<string>() ?? [], StringComparer.Ordinal);
      return new TableHixPattern(properties.Properties().Select(property =>
        new HixPatternField(property.Name, PatternFor((JObject)property.Value), !required.Contains(property.Name))).ToArray());
    }
    return new MapHixPattern(schema["propertyNames"] is JObject keys ? PatternFor(keys) : new KindHixPattern(K.String),
      schema["additionalProperties"] is JObject values ? PatternFor(values) : HixPattern.Any);
  }
  private static int? ExactLength(JObject schema) {
    foreach (var suffix in new[] {"Length", "Items", "Properties"}) {
      var min = schema.Value<int?>("min" + suffix); var max = schema.Value<int?>("max" + suffix);
      if (min != null && min == max) return min;
    }
    return null;
  }
  private static object ConstantValue(JToken token) => token.Type switch {
    JTokenType.Null => null, JTokenType.Boolean => token.Value<bool>(),
    JTokenType.Integer or JTokenType.Float => token.Value<double>(), JTokenType.String => token.Value<string>(),
    _ => throw new InvalidOperationException("only scalar const values are supported")
  };
  private static string EscapePointer(string value) => value.Replace("~", "~0").Replace("/", "~1");
  private static string UnescapePointer(string value) => value.Replace("~1", "/").Replace("~0", "~");
}
