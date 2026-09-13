using Hix.Compiler;
using Hix.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hix.Standalone;

/// <summary>Versioned, syntax-free storage for standalone executable images.</summary>
public static class HixImageFile {
  private sealed class Document {
    public string Format { get; set; } = "hix-bytecode";
    public int Version { get; set; } = 1;
    public byte[] Bytecode { get; set; }
    public string[] Strings { get; set; }
    public IHixValue[] Constants { get; set; }
    public Dictionary<int, int> SourceLines { get; set; }
    public BytecodeFunction[] Globals { get; set; }
    public BytecodeFunction[] Functions { get; set; }
    public Expression[] Expressions { get; set; }
    public Derivation[] Derivations { get; set; }
    public Dictionary<string, HixPattern> Patterns { get; set; }
    public BytecodeField[] Parameters { get; set; }
    public Dictionary<string, BytecodeMarker> Markers { get; set; }
    public string Entry { get; set; }
  }
  private sealed class Derivation {
    public string Name { get; set; }
    public int Line { get; set; }
    public Expression[] Expressions { get; set; }
    public BytecodeFunction[] Functions { get; set; }
  }
  // Keep the version-one storage layout; raw body offsets belong to the file format, not runtime entries.
  private sealed class Expression {
    public int Body { get; set; }
    public bool IsPrelude { get; set; }
    public int Line { get; set; }
    public static Expression From(BytecodeExpression entry) => new() {
      Body = entry.Function.Body, IsPrelude = entry.IsPrelude, Line = entry.Line
    };
    public BytecodeExpression ToEntry() => new(new BytecodeFunction("", false, [], Body), IsPrelude, Line);
  }
  private static JsonSerializer Serializer(HixStringPool strings = null) => JsonSerializer.Create(new JsonSerializerSettings {
    Formatting = Formatting.Indented, MaxDepth = 256,
    Converters = { new PatternConverter(), new ValueConverter(strings) }
  });
  public static void Write(string path, HixProgramImage image, string entry = "main") {
    var document = new Document {
      Bytecode = image.Bytecode.ToArray(), Strings = Enumerable.Range(0, image.StringPool.Count).Select(i => image.StringPool[i]).ToArray(),
      Constants = image.ConstantPool.ToArray(), SourceLines = image.SourceLines.ToDictionary(p => p.Key, p => p.Value),
      Globals = image.Scope.DisassemblyFunctions("mixin").Where(p => p.Scope == "global").Select(p => p.Function).ToArray(),
      Functions = image.Scope.DisassemblyFunctions("mixin", false).Select(p => p.Function).ToArray(),
      Expressions = image.Expressions.Select(Expression.From).ToArray(), Parameters = image.Parameters.ToArray(), Entry = entry,
      Patterns = image.Patterns.ToDictionary(p => p.Key, p => p.Value), Markers = image.Markers.ToDictionary(p => p.Key, p => p.Value),
      Derivations = image.Derivations.Select(d => new Derivation { Name = d.Name, Line = d.Line, Expressions = d.Expressions.Select(Expression.From).ToArray(),
        Functions = d.Scope.DisassemblyFunctions("derivation", false).Select(p => p.Function).ToArray() }).ToArray()
    };
    // Serialize completely before opening the destination: unsupported constants cannot truncate a file.
    var json = JToken.FromObject(document, Serializer(image.StringPool)).ToString(Formatting.Indented);
    File.WriteAllText(path, json);
  }
  public static HixProgramImage Read(string path, HixBackend backend, out string entry) {
    try {
      using var reader = new JsonTextReader(new StringReader(File.ReadAllText(path))) { MaxDepth = 256, DateParseHandling = DateParseHandling.None };
      var json = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
      if (reader.Read()) throw new ArgumentException("Trailing data in bytecode image");
      if ((string)json["Format"] != "hix-bytecode" || (int?)json["Version"] != 1)
        throw new ArgumentException("Unsupported bytecode image format or version");
      var d = json.ToObject<Document>(Serializer());
      if (d.Bytecode == null || d.Strings == null || d.Strings.Any(s => s == null) || d.Constants == null || d.Constants.Any(c => c == null) ||
          d.Globals == null || d.Functions == null || d.Expressions == null || d.Derivations == null || d.Patterns == null ||
          d.SourceLines == null || d.Parameters == null || d.Markers == null || string.IsNullOrEmpty(d.Entry))
        throw new ArgumentException("Incomplete bytecode image");
      var ids = d.Strings.Select((s, i) => new { s, i }).ToDictionary(p => p.s, p => p.i, StringComparer.Ordinal);
      var strings = new HixStringPool(d.Strings, ids);
      var globals = new LanguageFunctionScope(d.Globals);
      var scope = new LanguageFunctionScope(d.Functions, globals);
      var derivations = d.Derivations.Select(x => new BytecodeDerivation(x.Name, x.Line, x.Expressions.Select(expression => expression.ToEntry()).ToArray(),
        new LanguageFunctionScope(x.Functions, globals))).ToArray();
      var image = new HixProgramImage(d.Bytecode, d.SourceLines, d.Constants, strings, d.Expressions.Select(expression => expression.ToEntry()).ToArray(), derivations, scope, d.Patterns, backend, d.Parameters, d.Markers);
      // Validate instruction boundaries, references, stack reconstruction and declaration bodies before use.
      HixDecompiler.Decompile(image);
      entry = d.Entry;
      return image;
    } catch (Exception exception) when (exception is JsonException or InvalidOperationException or NullReferenceException or IndexOutOfRangeException or InvalidCastException or FormatException or OverflowException or System.Reflection.TargetInvocationException) {
      throw new ArgumentException("Invalid bytecode image: " + exception.Message, exception);
    }
  }

  // Only known data records are constructible; image files never select arbitrary CLR types.
  private sealed class PatternConverter : JsonConverter {
    private static readonly Type[] Types = {
      typeof(AnyHixPattern), typeof(KindHixPattern), typeof(NamedHixPattern), typeof(UnionHixPattern), typeof(TupleHixPattern), typeof(TableHixPattern),
      typeof(ManyHixPattern), typeof(MapHixPattern), typeof(DefinedHixPattern), typeof(TaggedHixPattern), typeof(ConstantHixPattern), typeof(EnumHixPattern),
      typeof(DocumentedHixPattern), typeof(ConstrainedHixPattern), typeof(DelegateHixPattern), typeof(SignatureHixPattern), typeof(HixPatternField),
      typeof(OpaqueHixPattern), typeof(HixOpaqueTypeDefinition), typeof(TypeParameterHixPattern), typeof(HixTypeParameter)
    };
    public override bool CanConvert(Type type) => typeof(HixPattern).IsAssignableFrom(type) ||
      type == typeof(HixPatternField) || type == typeof(HixOpaqueTypeDefinition) || type == typeof(HixTypeParameter);
    private static System.Reflection.ConstructorInfo Constructor(Type type) => type.GetConstructors().Single(c =>
      c.GetParameters().All(p => type.GetProperty(p.Name) != null));
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      var type = value.GetType();
      if (!Types.Contains(type)) throw new ArgumentException("Unsupported image pattern: " + type.Name);
      writer.WriteStartObject(); writer.WritePropertyName("PatternType"); writer.WriteValue(type.Name);
      foreach (var parameter in Constructor(type).GetParameters()) {
        writer.WritePropertyName(parameter.Name); serializer.Serialize(writer, type.GetProperty(parameter.Name).GetValue(value));
      }
      if (value is HixTypeParameter generic) {
        writer.WritePropertyName("AdditionalBounds"); serializer.Serialize(writer, generic.AdditionalBounds);
      }
      writer.WriteEndObject();
    }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      if (reader.TokenType == JsonToken.Null) return null;
      var json = JObject.Load(reader); var name = (string)json["PatternType"];
      var type = Types.SingleOrDefault(t => t.Name == name);
      if (type == null || !objectType.IsAssignableFrom(type)) throw new ArgumentException("Unsupported image pattern: " + name);
      var result = Constructor(type).Invoke(Constructor(type).GetParameters().Select(p => {
        var token = json[p.Name] ?? throw new ArgumentException("Missing pattern field: " + p.Name);
        return p.ParameterType == typeof(object) ? Untyped(token) : token.ToObject(p.ParameterType, serializer);
      }).ToArray());
      return result is HixTypeParameter generic && json["AdditionalBounds"] is { } bounds
        ? generic with { AdditionalBounds = bounds.ToObject<HixPattern[]>(serializer) } : result;
    }
    private static object Untyped(JToken token) => token switch {
      JValue value => value.Value,
      JArray array => array.Select(Untyped).ToArray(),
      JObject table => table.Properties().ToDictionary(p => p.Name, p => Untyped(p.Value)),
      _ => throw new ArgumentException("Invalid pattern value")
    };
  }
  private sealed class ValueConverter(HixStringPool strings) : JsonConverter {
    public override bool CanConvert(Type type) => typeof(IHixValue).IsAssignableFrom(type);
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      var json = new JObject();
      JToken Token(object item) => item == null ? JValue.CreateNull() : JToken.FromObject(item, serializer);
      switch (value) {
        case MissingHixValue: json["ValueType"] = "missing"; break;
        case NullHixValue: json["ValueType"] = "null"; break;
        case NumberHixValue number: json["ValueType"] = "number"; json["Value"] = number.Value; break;
        case BooleanHixValue boolean: json["ValueType"] = "bool"; json["Value"] = boolean.Value; break;
        case LiteralHixValue literal: json["ValueType"] = "string"; json["Value"] = literal.Value.Resolve(strings); break;
        case TupleHixValue tuple: json["ValueType"] = "tuple"; json["Value"] = Token(tuple.Values); break;
        case HixTableValue table: json["ValueType"] = "table";
          json["Value"] = new JArray(table.Entries.OrderBy(e => e.Key.Resolve(strings), StringComparer.Ordinal).Select(e =>
            new JObject { ["Key"] = e.Key.Resolve(strings), ["Value"] = Token(e.Value) })); break;
        case KindHixValue kind: json["ValueType"] = "kind"; json["Value"] = Token(kind.ValueKind); break;
        case PatternHixValue pattern: json["ValueType"] = "pattern"; json["Value"] = Token(pattern.Pattern); break;
        case FunctionReferenceHixValue function when value.GetType() == typeof(FunctionReferenceHixValue):
          json["ValueType"] = "function"; json["Value"] = Token(function.Signature);
          if (function.DeclarationSignature != null) json["DeclarationSignature"] = function.DeclarationSignature;
          break;
        default: throw new ArgumentException("Unsupported bytecode image constant: " + value.GetType().Name);
      }
      json.WriteTo(writer);
    }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      if (reader.TokenType == JsonToken.Null) return null;
      var json = JObject.Load(reader); var token = json["Value"];
      return (string)json["ValueType"] switch {
        "missing" => MissingHixValue.Instance, "null" => NullHixValue.Instance,
        "number" => new NumberHixValue(token.ToObject<double>()), "bool" => BooleanHixValue.From(token.ToObject<bool>()),
        "string" => new LiteralHixValue(HixString.Dynamic(token.ToObject<string>())),
        "tuple" => new TupleHixValue(token.ToObject<IHixValue[]>(serializer)),
        "table" => new HixTableValue(((JArray)token).Select(e => new KeyValuePair<HixString, IHixValue>(HixString.Dynamic((string)e["Key"]), e["Value"].ToObject<IHixValue>(serializer)))),
        "kind" => new KindHixValue(token.ToObject<HixValueKind>()), "pattern" => new PatternHixValue(token.ToObject<HixPattern>(serializer)),
        "function" => new FunctionReferenceHixValue(token.ToObject<SignatureHixPattern>(serializer)) {
          DeclarationSignature = (string)json["DeclarationSignature"]
        },
        _ => throw new ArgumentException("Unsupported bytecode image constant type")
      };
    }
  }
}
