using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hix.Runtime;
using K = Hix.HixValueKind;
using static Hix.Runtime.HixCoroutine;

namespace Hix.Functions;

public static class Builtins {
  private static IHixValue Convert(HixCoroutine thread, IHixValue value, HixPattern target) =>
    target is KindHixPattern {ValueKind: not HixValueKind.Pattern} primitive ?
      KindDefinitions.Get(primitive.ValueKind).Convert(thread, value) : thread.Coerce(value, target);

  public static void Register(FunctionSignatureRegistryBuilder definitions) {
    definitions.AddLibrary("""
      %description("Returns a non-error value unchanged, or null for an error.")
      pure func <catch>(any arg0) -> any external;
      %description("Returns a non-error value unchanged, or null for an error.")
      pure func <catch>(any arg0, function arg1) -> any external;

      %description("Explicitly selects and invokes a function using runtime argument values.")
      pure func <call>(function target, %variadic any arguments) -> any external;
      func <var>(any key, any value) -> any external;
      func <tar>(any key, any value) -> any external;
      func <local>(any key, any value) -> any external;

      %description("Leaves the current block if any supplied condition is false.")
      pure func <match>(%variadic any arg0) -> null external;

      %description("Appends a rendered value to the execution log.")
      func <log>(any arg0) -> null external;

      %description("Logs the kind and rendered value, then returns the value.")
      func <dump>(any arg0) -> any external;

      %requiresPrelude()
      %description("Applies derivation mixins to a tuple of records during the prelude and returns the resulting records.")
      func <derive>(tuple arg0) -> tuple external;

      %description("Invokes a callback for each tuple element and returns the resulting tuple.")
      pure func <map>(tuple arg0, function arg1) -> tuple external;

      %description("Returns tuple elements for which the callback is truthy.")
      pure func <where>(tuple arg0, function arg1) -> tuple external;

      %description("Tests whether any tuple element satisfies the callback.")
      pure func <any>(tuple arg0, function arg1) -> bool external;

      %description("Tests whether every tuple element satisfies the callback.")
      pure func <all>(tuple arg0, function arg1) -> bool external;

      %description("Combines tuple elements through a callback and accumulator.")
      pure func <reduce>(tuple arg0, function arg1, any arg2) -> any external;

      %description("Returns the backend-defined name of a value.")
      pure func <name>(any arg0) -> string external;

      %description("Unwraps a host value to its underlying Hix value.")
      pure func <unwrap>(any arg0) -> any external;
      """);
    definitions.AddLibrary("""
      %description("Returns the primitive pattern of a value.")
      pure func pattern(any value) -> pattern external;
      %description("Returns the logarithm of a number in the supplied base.")
      pure func <log>(number value, number base) -> number external builtinLogarithm;
      %description("Constructs the default value of a pattern, or converts the supplied value to that pattern.")
      pure func new(pattern arg0) -> any external;
      %description("Constructs the default value of a pattern, or converts the supplied value to that pattern.")
      pure func new(pattern arg0, any arg1) -> any external;
      %description("Converts a value to the specified pattern. Returns an error when conversion is not supported.")
      pure func cast(any value, pattern target) -> any external;
      %description("Casts to the supplied pattern, returning null when conversion or validation fails.")
      pure func as(any value, pattern target) -> any external;
      %description("Tests whether a value matches a pattern, including checked errors.")
      pure func is(any arg0, pattern arg1) -> bool external;
      %description("Keeps the value when it has the specified pattern; otherwise returns the fallback.")
      pure func if(any arg0, pattern arg1, any arg2) -> any external;
      %description("Keeps the value when it does not have the specified pattern; otherwise returns the fallback.")
      pure func ifNot(any arg0, pattern arg1, any arg2) -> any external;
      %description("Returns true for values other than missing, null, and errors.")
      pure func exists(any arg0) -> bool external;
      %description("Negates the truthiness of a value.")
      pure func not(any arg0) -> bool external;
      %description("Compares two values for equality, including tuple and table contents.")
      pure func eq(any arg0, any arg1) -> bool external;
      %description("Returns true when two values are not equal.")
      pure func neq(any arg0, any arg1) -> bool external;
      %description("Returns true when all supplied values are truthy.")
      pure func and(any arg0, any arg1, %variadic any arg2) -> bool external;
      %description("Returns true when at least one supplied value is truthy.")
      pure func or(any arg0, any arg1, %variadic any arg2) -> bool external;
      %description("Fails execution if any supplied condition is false.")
      pure func assert(%variadic any arg0) -> null external;
      %description("Produces an execution error with an optional message.")
      pure func fail() -> error external;
      %description("Produces an execution error with an optional message.")
      pure func fail(any arg0) -> error external;
      %description("Rounds a number to the nearest integer.")
      pure func round(number arg0) -> number external;
      %description("Rounds a number down to the nearest integer.")
      pure func floor(number arg0) -> number external;
      %description("Rounds a number up to the nearest integer.")
      pure func ceil(number arg0) -> number external;
      %description("Returns the absolute value of a number.")
      pure func abs(number arg0) -> number external;
      %description("Returns the square root of a number.")
      pure func sqrt(number arg0) -> number external;
      %description("Returns the smaller number.")
      pure func min(number arg0, number arg1) -> number external;
      %description("Returns the larger number.")
      pure func max(number arg0, number arg1) -> number external;
      %description("Raises the first number to the power of the second.")
      pure func pow(number arg0, number arg1) -> number external;
      %description("Subtracts the second number from the first, or negates a single number.")
      pure func minus(number arg0, number arg1) -> number external;
      %description("Adds two numbers.")
      pure func plus(number arg0, number arg1) -> number external;
      %description("Multiplies two numbers.")
      pure func mult(number arg0, number arg1) -> number external;
      %description("Divides the first number by the second.")
      pure func div(number arg0, number arg1) -> number external;
      %description("Returns the remainder after division.")
      pure func mod(number arg0, number arg1) -> number external;
      %description("Clamps a number to the supplied minimum and maximum.")
      pure func clamp(number arg0, number arg1, number arg2) -> number external;
      %description("Tests whether a number has no fractional part.")
      pure func isInt(number arg0) -> bool external;
      %description("Numeric comparison operators with relative equality tolerance.")
      type ComparisonOperator = %enum<lt><gt><lte><gte><eq><neq> string
      %description("Compares numbers using a ComparisonOperator, with relative equality tolerance.")
      pure func compare(number arg0, ComparisonOperator arg1, number arg2) -> bool external;
      %description("Extracts text between zero-based start (inclusive) and end (exclusive) offsets.")
      pure func substring(string arg0, number arg1, number arg2) -> string external;
      %description("Splits text by a delimiter and returns a tuple of strings.")
      pure func split(string arg0, string arg1) -> tuple external;
      %description("Tests whether text starts with a prefix.")
      pure func startsWith(string arg0, string arg1) -> bool external;
      %description("Tests whether text ends with a suffix.")
      pure func endsWith(string arg0, string arg1) -> bool external;
      %description("Tests text against a regular expression.")
      pure func matches(string arg0, string arg1) -> bool external;
      %description("Converts text to uppercase.")
      pure func uppercase(string arg0) -> string external;
      %description("Converts text to lowercase.")
      pure func lowercase(string arg0) -> string external;
      %description("Removes leading and trailing whitespace.")
      pure func trim(string arg0) -> string external;
      %description("Removes leading whitespace.")
      pure func trimStart(string arg0) -> string external;
      %description("Removes trailing whitespace.")
      pure func trimEnd(string arg0) -> string external;
      %description("Replaces every occurrence of the specified text.")
      pure func replaceAll(string arg0, string arg1, string arg2) -> string external;
      %description("Replaces the first occurrence of the specified text.")
      pure func replaceFirst(string arg0, string arg1, string arg2) -> string external;
      %description("Replaces the last occurrence of the specified text.")
      pure func replaceLast(string arg0, string arg1, string arg2) -> string external;
      %description("Replaces all regular-expression matches.")
      pure func regexReplaceAll(string arg0, string arg1, string arg2) -> string external;
      %description("Replaces the first regular-expression match.")
      pure func regexReplaceFirst(string arg0, string arg1, string arg2) -> string external;
      %description("Returns the character count of text or the number of tuple elements or table entries.")
      pure func length(string arg0) -> number external;
      %description("Returns the character count of text or the number of tuple elements or table entries.")
      pure func length(tuple arg0) -> number external;
      %description("Returns the character count of text or the number of tuple elements or table entries.")
      pure func length(table arg0) -> number external;
      %description("Tests whether text contains a substring or a tuple contains a value.")
      pure func contains(string arg0, string arg1) -> bool external;
      %description("Tests whether text contains a substring or a tuple contains a value.")
      pure func contains(tuple arg0, any arg1) -> bool external;
      %description("Tests whether text contains a substring or a tuple contains a value.")
      pure func contains(table arg0, any arg1) -> bool external;
      %description("Tests whether a table contains a key, including keys whose value is null.")
      pure func has(tuple arg0, number arg1) -> bool external;
      %description("Tests whether a table contains a key, including keys whose value is null.")
      pure func has(table arg0, string arg1) -> bool external;
      %description("Reads a tuple index or table key; returns the fallback when absent.")
      pure func get(tuple arg0, number arg1) -> any external;
      %description("Reads a tuple index or table key; returns the fallback when absent.")
      pure func get(tuple arg0, number arg1, any arg2) -> any external;
      %description("Reads a tuple index or table key; returns the fallback when absent.")
      pure func get(table arg0, string arg1) -> any external;
      %description("Reads a tuple index or table key; returns the fallback when absent.")
      pure func get(table arg0, string arg1, any arg2) -> any external;
      %description("Returns the index of a substring or tuple value, or -1 when absent.")
      pure func indexOf(tuple arg0, any arg1) -> number external;
      %description("Returns a tuple with the supplied value appended.")
      pure func push(tuple arg0, any arg1) -> tuple external;
      %description("Returns a tuple with its final element removed.")
      pure func pop(tuple arg0) -> tuple external;
      %description("Returns a tuple containing the table keys.")
      pure func keys(table arg0) -> tuple external;
      %description("Returns a tuple containing the table values.")
      pure func values(table arg0) -> tuple external;
      %description("Returns table entries as a tuple of key/value pairs.")
      pure func entries(table arg0) -> tuple external;
      %description("Returns a table without the specified key.")
      pure func remove(table arg0, string arg1) -> table external;
      %description("Returns a table with the specified key set to a value.")
      pure func put(table arg0, string arg1, any arg2) -> table external;
      %description("Returns a table with the specified key set to a value.")
      pure func put(table arg0, table arg1) -> table external;
      %description("Renders tuple elements and joins them with a separator. Nested collections require explicit rendering.")
      pure func join(tuple arg0, string arg1) -> string external;
      %description("Renders tuple elements and joins them with a separator. Nested collections require explicit rendering.")
      pure func join(table arg0, string arg1, string arg2) -> string external;
      """);
    static IHixValue NumberResult(HixCoroutine execution, double value) =>
      double.IsNaN(value) || double.IsInfinity(value)
        ? execution.Error("invalid numeric operation")
        : new NumberHixValue(value);

    definitions.Add(new SimpleFunction("pattern", (_, arguments) => HixPatterns.Primitive(arguments[0].Kind)));
    definitions.Add(
      new SimpleFunction("var", (e, a) => e.Context.Call(e, "var", a)),
      new SimpleFunction("tar", (e, a) => e.Context.Call(e, "tar", a)),
      new SimpleFunction("local", (e, a) => e.Context.Call(e, "local", a))
    );
    definitions.Add(
      new SimpleFunction("new", (e, a) => {
          var target = ((PatternHixValue)a[0]).Pattern;
          if (a.Length == 2) return Convert(e, a[1], target);
          return target is KindHixPattern kind ? KindDefinitions.Get(kind.ValueKind).New(e) :
            e.Error("new requires a primitive pattern; use the declared constructor for structured patterns");
        })
    );
    definitions.Add(
      new SimpleFunction("cast", (e, a) => Convert(e, a[0], ((PatternHixValue)a[1]).Pattern)),
      new SimpleFunction("as", (e, a) => {
        var result = Convert(e, a[0], ((PatternHixValue)a[1]).Pattern);
        return result is ErrorHixValue ? NullHixValue.Instance : result;
      }),
      new SimpleFunction("is", (e, a) => Bool(e.Matches(((PatternHixValue)a[1]).Pattern, a[0], out _))),
      new SimpleFunction("if", (e, a) => e.Matches(((PatternHixValue)a[1]).Pattern, a[0], out _) ? a[0] : a[2]),
      new SimpleFunction("ifNot", (e, a) => !e.Matches(((PatternHixValue)a[1]).Pattern, a[0], out _) ? a[0] : a[2]),
      new SimpleFunction("exists", (_, a) => Bool(a[0] is not (MissingHixValue or NullHixValue or ErrorHixValue))),
      new SimpleFunction("not", (e, a) => Bool(!a[0].IsTruthy(e))),
      new SimpleFunction("eq", (e, a) => Bool(e.Equal(a[0], a[1]))),
      new SimpleFunction("neq", (e, a) => Bool(!e.Equal(a[0], a[1]))),
      new SimpleFunction("and", (e, a) => Bool(a.All(value => value.IsTruthy(e)))),
      new SimpleFunction("or", (e, a) => Bool(a.Any(value => value.IsTruthy(e)))),
      new SimpleFunction("assert", (e, a) => a.All(value => value.IsTruthy(e))
          ? NullHixValue.Instance
          : e.Error("assertion failed")),
      new SimpleFunction("fail", (e, a) => {
          var rendered = a.Length == 1 ? e.RenderText(a[0]) : String("execution failed");
          return rendered is ErrorHixValue ? rendered : e.Error(e.Text(rendered));
        })
    );
    definitions.Add(
      new CatchFunction(), new CallbackFunction("call"),
      new MatchFunction(), new LogFunction("log", false), new LogFunction("dump", true), new DeriveFunction()
    );
    definitions.Add(
      new SimpleFunction("round", (e, a) => NumberResult(e, Math.Round(((NumberHixValue)a[0]).Value))),
      new SimpleFunction("floor", (e, a) => NumberResult(e, Math.Floor(((NumberHixValue)a[0]).Value))),
      new SimpleFunction("ceil", (e, a) => NumberResult(e, Math.Ceiling(((NumberHixValue)a[0]).Value))),
      new SimpleFunction("abs", (e, a) => NumberResult(e, Math.Abs(((NumberHixValue)a[0]).Value))),
      new SimpleFunction("sqrt", (e, a) => NumberResult(e, Math.Sqrt(((NumberHixValue)a[0]).Value))),
      new SimpleFunction("min", (e, a) => NumberResult(e, Math.Min(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value))),
      new SimpleFunction("max", (e, a) => NumberResult(e, Math.Max(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value))),
      new SimpleFunction("pow", (e, a) => NumberResult(e, Math.Pow(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value))),
      new SimpleFunction("builtinLogarithm", (e, a) => NumberResult(e, Math.Log(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value))),
      new SimpleFunction("minus", (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value - ((NumberHixValue)a[1]).Value)),
      new SimpleFunction("plus", (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value + ((NumberHixValue)a[1]).Value)),
      new SimpleFunction("mult", (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value * ((NumberHixValue)a[1]).Value)),
      new SimpleFunction("div", (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value / ((NumberHixValue)a[1]).Value)),
      new SimpleFunction("mod", (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value % ((NumberHixValue)a[1]).Value)),
      new SimpleFunction("clamp", (e, a) => NumberResult(
          e,
          Math.Max(
            ((NumberHixValue)a[1]).Value, Math.Min(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[2]).Value)
          )
        )),
      new SimpleFunction("isInt", (_, a) => {
          var value = ((NumberHixValue)a[0]).Value;
          return Bool(!double.IsNaN(value) && !double.IsInfinity(value) && value == Math.Truncate(value));
        }),
      new SimpleFunction("compare", (e, a) => {
          var value = ((NumberHixValue)a[0]).Value;
          var other = ((NumberHixValue)a[2]).Value;
          var equal = Math.Abs(value - other) <= 1e-9 * Math.Max(1, Math.Max(Math.Abs(value), Math.Abs(other)));
          return e.ResolveText(a[1]) switch {
            "lt" => Bool(value < other && !equal), "gt" => Bool(value > other && !equal),
            "lte" => Bool(value <= other || equal), "gte" => Bool(value >= other || equal), "eq" => Bool(equal),
            "neq" => Bool(!equal), _ => e.Error("unknown comparison")
          };
        })
    );
    definitions.Add(
      new SimpleFunction("substring", (e, a) => {
          var value = e.ResolveText(a[0]);
          var start = ((NumberHixValue)a[1]).Value;
          var end = ((NumberHixValue)a[2]).Value;
          return start != Math.Truncate(start) || end != Math.Truncate(end) || start < 0 || end < start ||
            end > value.Length
              ? e.Error("invalid substring range")
              : String(value.Substring((int)start, (int)(end - start)));
        }),
      new SimpleFunction("split", (e, a) => new TupleHixValue(
          e.ResolveText(a[0]).Split([e.ResolveText(a[1])], StringSplitOptions.None).Select(text => (IHixValue)String(text))
            .ToArray()
        )),
      new SimpleFunction("startsWith", (e, a) => Bool(e.ResolveText(a[0]).StartsWith(e.ResolveText(a[1]), StringComparison.Ordinal))),
      new SimpleFunction("endsWith", (e, a) => Bool(e.ResolveText(a[0]).EndsWith(e.ResolveText(a[1]), StringComparison.Ordinal))),
      new SimpleFunction("matches", (e, a) => Bool(
          new Regex(e.ResolveText(a[1]), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).IsMatch(e.ResolveText(a[0]))
        )),
      new SimpleFunction("uppercase", (e, a) => String(e.ResolveText(a[0]).ToUpperInvariant())),
      new SimpleFunction("lowercase", (e, a) => String(e.ResolveText(a[0]).ToLowerInvariant())),
      new SimpleFunction("trim", (e, a) => String(e.ResolveText(a[0]).Trim())),
      new SimpleFunction("trimStart", (e, a) => String(e.ResolveText(a[0]).TrimStart())),
      new SimpleFunction("trimEnd", (e, a) => String(e.ResolveText(a[0]).TrimEnd())),
      new SimpleFunction("replaceAll", (e, a) => String(e.ResolveText(a[0]).Replace(e.ResolveText(a[1]), e.ResolveText(a[2])))),
      new SimpleFunction("replaceFirst", (e, a) => {
          var value = e.ResolveText(a[0]);
          var search = e.ResolveText(a[1]);
          if (search.Length == 0) return e.Error("replacement search string cannot be empty");
          var index = value.IndexOf(search, StringComparison.Ordinal);
          return String(
            index < 0 ? value : value.Substring(0, index) + e.ResolveText(a[2]) + value.Substring(index + search.Length)
          );
        }),
      new SimpleFunction("replaceLast", (e, a) => {
          var value = e.ResolveText(a[0]);
          var search = e.ResolveText(a[1]);
          if (search.Length == 0) return e.Error("replacement search string cannot be empty");
          var index = value.LastIndexOf(search, StringComparison.Ordinal);
          return String(
            index < 0 ? value : value.Substring(0, index) + e.ResolveText(a[2]) + value.Substring(index + search.Length)
          );
        }),
      new SimpleFunction("regexReplaceAll", (e, a) => String(
          new Regex(e.ResolveText(a[1]), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Replace(
            e.ResolveText(a[0]), e.ResolveText(a[2])
          )
        )),
      new SimpleFunction("regexReplaceFirst", (e, a) => String(
          new Regex(e.ResolveText(a[1]), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Replace(
            e.ResolveText(a[0]), e.ResolveText(a[2]), 1
          )
        ))
    );
    definitions.Add(
      new SimpleFunction("length", (e, a) => new NumberHixValue(
          a[0] switch {
            LiteralHixValue => e.ResolveText(a[0]).Length, TupleHixValue tuple => tuple.Values.Count,
            HixTableValue table => table.Entries.Count, _ => 0
          }
        )),
      new SimpleFunction("contains", (e, a) => a[0] is LiteralHixValue
          ? Bool(e.ResolveText(a[0]).Contains(e.ResolveText(a[1])))
          : Bool(
            (a[0] is TupleHixValue tuple
              ? tuple.Values
              : ((HixTableValue)a[0]).Entries.Select(entry => entry.Value))
            .Any(value => e.Equal(value, a[1]))
          )),
      new SimpleFunction("has", (e, a) => CollectionFunctions.Has(e, a)),
      new SimpleFunction("get", (e, a) => CollectionFunctions.Get(e, a))
    );
    definitions.Add(
      new CollectionTransformFunction("map", CollectionFunctions.TransformKind.Map),
      new CollectionTransformFunction("where", CollectionFunctions.TransformKind.Where),
      new CollectionTransformFunction("any", CollectionFunctions.TransformKind.Any),
      new CollectionTransformFunction("all", CollectionFunctions.TransformKind.All),
      new CollectionTransformFunction("reduce", CollectionFunctions.TransformKind.Reduce)
    );
    definitions.Add(
      new SimpleFunction("indexOf", (e, a) => {
          var values = ((TupleHixValue)a[0]).Values;
          for (var index = 0; index < values.Count; index++)
            if (e.Equal(values[index], a[1]))
              return new NumberHixValue(index);
          return new NumberHixValue(-1);
        }),
      new SimpleFunction("push", (_, a) => ((TupleHixValue)a[0]).Append(a[1])),
      new SimpleFunction("pop", (_, a) => ((TupleHixValue)a[0]).RemoveLast()),
      new SimpleFunction("keys", (e, a) => new TupleHixValue(
          ((HixTableValue)a[0]).Entries.Select(entry => (IHixValue)String(entry.Key.Resolve(e.Strings)))
          .ToArray()
        )),
      new SimpleFunction("values", (_, a) => new TupleHixValue(((HixTableValue)a[0]).Entries.Select(entry => entry.Value).ToArray())),
      new SimpleFunction("entries", (e, a) => KindDefinitions.Get(K.Tuple).Convert(e, a[0])),
      new SimpleFunction("remove", (e, a) => ((HixTableValue)a[0]).Remove(e, e.Text(a[1])))
    );
    definitions.Add(
      new SimpleFunction("put", (e, a) => {
          var table = (HixTableValue)a[0];
          if (a.Length == 3) return CollectionFunctions.Put(e, table, e.ResolveText(a[1]), a[2]);
          foreach (var entry in ((HixTableValue)a[1]).Entries)
            table = CollectionFunctions.Put(e, table, entry.Key.Resolve(e.Strings), entry.Value);
          return table;
        }),
      new SimpleFunction("join", (e, a) => {
          var values = a[0] is TupleHixValue tuple ? tuple.Values : ((HixTableValue)a[0]).Entries.Select(entry => entry.Value).ToArray();
          var rendered = new List<string>();
          foreach (var value in values) {
            var text = e.RenderText(value);
            if (text is ErrorHixValue) return text;
            rendered.Add(e.ResolveText(text));
          }
          return String(a[0] is TupleHixValue
            ? string.Join(e.ResolveText(a[1]), rendered)
            : string.Join(e.ResolveText(a[2]), ((HixTableValue)a[0]).Entries.Select((entry, index) =>
              entry.Key.Resolve(e.Strings) + e.ResolveText(a[1]) + rendered[index])));
        })
    );
  }
}
