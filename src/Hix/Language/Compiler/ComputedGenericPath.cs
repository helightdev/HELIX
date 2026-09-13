using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

/// <summary>Compiler-only JSON-pointer-style projections over argument and generic schemas.</summary>
public static class ComputedGenericPath {
  public static string[] Parse(string path) {
    if (string.IsNullOrEmpty(path) || !path.StartsWith("/", StringComparison.Ordinal))
      throw new ArgumentException("computed generic paths must start with '/'");
    var parts = path.Substring(1).Split('/').Select(part => {
      for (var index = 0; index < part.Length; index++)
        if (part[index] == '~' && (++index == part.Length || part[index] is not ('0' or '1')))
          throw new ArgumentException("invalid computed generic pointer escape; use ~0 or ~1");
      return part.Replace("~1", "/").Replace("~0", "~");
    }).ToArray();
    var start = parts[0] == "invoke" ? 3 : 2;
    if (parts.Length < start || parts[0] is not ("arguments" or "generics" or "invoke") ||
        parts.Take(start).Any(string.IsNullOrEmpty)) throw new ArgumentException("computed generic paths start with /arguments/name, /generics/name, or /invoke/callback/arguments");
    for (var index = start; index < parts.Length; index++) {
      switch (parts[index]) {
        case "fields":
          if (++index == parts.Length) throw new ArgumentException("fields requires a field name");
          break;
        case "typeArguments": case "parameters":
          if (++index == parts.Length || !int.TryParse(parts[index], out var position) || position < 0)
            throw new ArgumentException("typeArguments and parameters require a non-negative index");
          break;
        case "*": case "items": case "keys": case "values": case "result": case "pattern": case "data": case "union": break;
        default: throw new ArgumentException("unknown computed generic selector '" + parts[index] + "'");
      }
    }
    return parts;
  }

  public static void Validate(string path, IReadOnlyList<HixPatternField> inputs, IReadOnlyList<HixTypeParameter> parameters) {
    var parts = Parse(path);
    if (parts[0] == "generics") {
      if (!parameters.Any(parameter => parameter.Name == parts[1])) throw new ArgumentException("unknown computed generic source '" + parts[1] + "'");
    } else foreach (var name in parts.Skip(1).Take(parts[0] == "invoke" ? 2 : 1))
      if (!inputs.Any(field => field.Name == name)) throw new ArgumentException("unknown computed generic argument '" + name + "'");
  }

  public static IReadOnlyDictionary<int, int> Invocations(IReadOnlyList<HixTypeParameter> parameters, IReadOnlyList<HixPatternField> inputs) {
    var result = new Dictionary<int, int>();
    foreach (var parameter in parameters.Where(parameter => parameter.ComputedPath != null)) {
      var parts = Parse(parameter.ComputedPath);
      if (parts[0] != "invoke") continue;
      var callback = inputs.ToList().FindIndex(field => field.Name == parts[1]);
      var arguments = inputs.ToList().FindIndex(field => field.Name == parts[2]);
      if (callback < 0 || arguments < 0) throw new ArgumentException("unknown computed generic invocation argument");
      if (HixGenericTypes.Erase(inputs[callback].Pattern) is not (HixValueKind.Function or HixValueKind.Any) ||
          HixGenericTypes.Erase(inputs[arguments].Pattern) is not (HixValueKind.Tuple or HixValueKind.Any))
        throw new ArgumentException("computed generic invocation requires a callable and an argument tuple");
      if (result.TryGetValue(callback, out var existing) && existing != arguments)
        throw new ArgumentException("conflicting computed generic invocation arguments");
      result[callback] = arguments;
    }
    return result;
  }

  public static void Infer(IReadOnlyList<HixTypeParameter> parameters, IReadOnlyList<HixPatternField> inputs,
    IReadOnlyList<HixPattern> arguments, IDictionary<HixTypeParameter, HixPattern> substitutions,
    IReadOnlyDictionary<string, HixPattern> definitions) {
    var active = new HashSet<HixTypeParameter>();
    var completed = new HashSet<HixTypeParameter>();
    HixPattern Generic(string name) {
      var parameter = parameters.FirstOrDefault(item => item.Name == name);
      if (parameter == null) return HixPattern.Any;
      if (parameter.ComputedPath != null && !completed.Contains(parameter)) Compute(parameter);
      return substitutions.TryGetValue(parameter, out var value) ? value : HixPattern.Any;
    }
    HixPattern Argument(string name) {
      var index = inputs.ToList().FindIndex(field => field.Name == name);
      return index >= 0 && index < arguments.Count ? arguments[index] : HixPattern.Any;
    }
    void Compute(HixTypeParameter parameter) {
      if (!active.Add(parameter)) throw new ArgumentException("cyclic computed generic '" + parameter.Name + "'");
      var parts = Parse(parameter.ComputedPath);
      var source = parts[0] == "generics" ? Generic(parts[1]) : Argument(parts[1]);
      substitutions[parameter] = Select(source, parts, parts[0] == "invoke" ? 3 : 2, definitions,
        substitutions as IReadOnlyDictionary<HixTypeParameter, HixPattern> ?? new Dictionary<HixTypeParameter, HixPattern>(substitutions));
      active.Remove(parameter); completed.Add(parameter);
    }
    foreach (var parameter in parameters.Where(item => item.ComputedPath != null)) Compute(parameter);
  }

  private static HixPattern Select(HixPattern value, string[] parts, int index,
    IReadOnlyDictionary<string, HixPattern> definitions, IReadOnlyDictionary<HixTypeParameter, HixPattern> substitutions) {
    if (index == parts.Length) return value;
    var original = value;
    var seen = new HashSet<string>();
    var seenParameters = new HashSet<HixTypeParameter>();
    while (true) {
      if (value is NamedHixPattern named && seen.Add(named.Name) && definitions.TryGetValue(named.Name, out var resolved)) value = resolved;
      else if (value is DocumentedHixPattern documented) value = documented.Underlying;
      else if (value is TypeParameterHixPattern parameter) {
        if (!seenParameters.Add(parameter.Parameter)) return HixPattern.Any;
        value = HixGenericTypes.Substitute(parameter, substitutions, true);
      }
      else break;
    }
    HixPattern Next(HixPattern selected, int consumed = 1) => Select(selected, parts, index + consumed, definitions, substitutions);
    HixPattern Union(IEnumerable<HixPattern> patterns) => HixPatterns.Union(patterns);
    switch (parts[index]) {
      case "pattern": return Next(value is PatternValueHixPattern witness ? witness.ValuePattern : HixPattern.Any);
      case "data": return Next(HixPatterns.TryTrait(original, definitions, out var trait) ? trait.Underlying : original);
      case "result": return Next(value is DelegateHixPattern callable ? callable.Result : HixPattern.Any);
      case "typeArguments": return Next(value is OpaqueHixPattern opaque && int.Parse(parts[index + 1]) < opaque.Arguments.Count ? opaque.Arguments[int.Parse(parts[index + 1])] : HixPattern.Any, 2);
      case "parameters": return Next(value is DelegateHixPattern function && int.Parse(parts[index + 1]) < function.Parameters.Count ? function.Parameters[int.Parse(parts[index + 1])].Pattern : HixPattern.Any, 2);
      case "fields":
        var fields = value is TableHixPattern table ? table.Fields : value is TupleHixPattern tuple ? tuple.Fields : Array.Empty<HixPatternField>();
        return Next(fields.FirstOrDefault(field => field.Name == parts[index + 1])?.Pattern ?? HixPattern.Any, 2);
      case "items": return Next(value is ManyHixPattern many ? many.Element : value is TupleHixPattern items ? Union(items.Fields.Select(field => field.Pattern)) : HixPattern.Any);
      case "keys": return Next(value is MapHixPattern map ? map.Key : value is TableHixPattern ? new KindHixPattern(HixValueKind.String) : HixPattern.Any);
      case "values": return Next(value is MapHixPattern values ? values.Value : value is TableHixPattern entries ? Union(entries.Fields.Select(field => field.Pattern)) : HixPattern.Any);
      case "union": return Next(value is TupleHixPattern results ? Union(results.Fields.Select(field => field.Pattern)) : value is TableHixPattern records ? Union(records.Fields.Select(field => field.Pattern)) : value is ManyHixPattern elements ? elements.Element : original);
      case "*":
        HixPatternField Field(HixPatternField field) => field with {Pattern = Next(field.Pattern)};
        return value switch {
          TupleHixPattern projectedTuple => new TupleHixPattern(projectedTuple.Fields.Select(Field).ToArray()),
          TableHixPattern projectedTable => new TableHixPattern(projectedTable.Fields.Select(Field).ToArray()),
          ManyHixPattern projectedMany => new ManyHixPattern(Next(projectedMany.Element)),
          MapHixPattern projectedMap => new MapHixPattern(projectedMap.Key, Next(projectedMap.Value)),
          UnionHixPattern union => Union(union.Patterns.Select(NextPattern)),
          _ => HixPattern.Any
        };
        HixPattern NextPattern(HixPattern pattern) => Next(pattern);
      default: return HixPattern.Any;
    }
  }
}
