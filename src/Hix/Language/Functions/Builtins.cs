using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hix.Runtime;
using K = Hix.HixValueKind;
using static Hix.Runtime.HixThread;

namespace Hix.Functions;

public static class Builtins {
  public static void Register(FunctionSignatureRegistryBuilder definitions) {
    static IHixValue NumberResult(HixThread execution, double value) =>
      double.IsNaN(value) || double.IsInfinity(value)
        ? execution.Error("invalid numeric operation")
        : new NumberHixValue(value);

    foreach (var kind in KindDefinitions.Enumerate()) {
      definitions.Add(
        new SimpleFunction(
          kind.Kind.Name.Resolve(null),
          [new FunctionSignature(kind.ValueKind, []), new FunctionSignature(kind.ValueKind, [K.Any])],
          (execution, arguments) => arguments.Length == 0
            ? kind.New(execution)
            : kind.Convert(execution, arguments[0]),
          acceptsErrors: kind.ValueKind is K.String or K.Bool or K.Kind,
          documentation: "Constructs the default value or converts a value to the " + kind.Kind.Name.Resolve(null) + " kind.")
      );
    }
    definitions.Add(
      new SimpleFunction(
        "new", [new FunctionSignature(K.Any, [K.Kind]), new FunctionSignature(K.Any, [K.Kind, K.Any])],
        (e, a) => {
          var kind = KindDefinitions.Get((KindHixValue)a[0]);
          return a.Length == 1 ? kind.New(e) : kind.Convert(e, a[1]);
        },
        documentation: "Constructs the default value of a kind, or converts the supplied value to that kind.")
    );
    definitions.Add(
      new SimpleFunction(
        "as", [new FunctionSignature(K.Any, [K.Any, K.Kind])],
        (e, a) => KindDefinitions.Get((KindHixValue)a[1]).Convert(e, a[0]),
        documentation: "Converts a value to the specified kind. Returns an error when conversion is not supported."),
      new SimpleFunction(
        "is", [new FunctionSignature(K.Bool, [K.Any, K.Kind])],
        (_, a) => Bool(KindDefinitions.Get((KindHixValue)a[1]).Is(a[0])), acceptsErrors: true,
        documentation: "Tests whether a value has the specified runtime kind, including checked errors."),
      new SimpleFunction(
        "if", [new FunctionSignature(K.Any, [K.Any, K.Kind, K.Any])],
        (_, a) => KindDefinitions.Get((KindHixValue)a[1]).Is(a[0]) ? a[0] : a[2],
        documentation: "Keeps the value when it has the specified kind; otherwise returns the fallback."),
      new SimpleFunction(
        "ifNot", [new FunctionSignature(K.Any, [K.Any, K.Kind, K.Any])],
        (_, a) => !KindDefinitions.Get((KindHixValue)a[1]).Is(a[0]) ? a[0] : a[2],
        documentation: "Keeps the value when it does not have the specified kind; otherwise returns the fallback."),
      new SimpleFunction(
        "exists", [new FunctionSignature(K.Bool, [K.Any])],
        (_, a) => Bool(a[0] is not (MissingHixValue or NullHixValue or ErrorHixValue)), acceptsErrors: true,
        documentation: "Returns true for values other than missing, null, and errors."),
      new SimpleFunction(
        "not", [new FunctionSignature(K.Bool, [K.Any])],
        (e, a) => Bool(!a[0].IsTruthy(e)), acceptsErrors: true,
        documentation: "Negates the truthiness of a value."),
      new SimpleFunction(
        "eq", [new FunctionSignature(K.Bool, [K.Any, K.Any])],
        (e, a) => Bool(e.Equal(a[0], a[1])),
        documentation: "Compares two values for equality, including tuple and table contents."),
      new SimpleFunction(
        "neq", [new FunctionSignature(K.Bool, [K.Any, K.Any])],
        (e, a) => Bool(!e.Equal(a[0], a[1])),
        documentation: "Returns true when two values are not equal."),
      new SimpleFunction(
        "and", [new FunctionSignature(K.Bool, [K.Any, K.Any, K.Any], true)],
        (e, a) => Bool(a.All(value => value.IsTruthy(e))),
        documentation: "Returns true when all supplied values are truthy."),
      new SimpleFunction(
        "or", [new FunctionSignature(K.Bool, [K.Any, K.Any, K.Any], true)],
        (e, a) => Bool(a.Any(value => value.IsTruthy(e))),
        documentation: "Returns true when at least one supplied value is truthy."),
      new SimpleFunction(
        "assert", [new FunctionSignature(K.Null, [K.Any], true)],
        (e, a) => a.All(value => value.IsTruthy(e))
          ? NullHixValue.Instance
          : e.Error("assertion failed"),
        documentation: "Fails execution if any supplied condition is false."),
      new SimpleFunction(
        "fail", [new FunctionSignature(K.Error, []), new FunctionSignature(K.Error, [K.Any])],
        (e, a) => {
          var rendered = a.Length == 1 ? e.RenderText(a[0]) : String("execution failed");
          return rendered is ErrorHixValue ? rendered : e.Error(e.Text(rendered));
        },
        documentation: "Produces an execution error with an optional message.")
    );
    definitions.Add(
      new CatchFunction(), new CallbackFunction("call"), new CallbackFunction("inline"),
      new MatchFunction(), new LogFunction("log", false), new LogFunction("dump", true), new DeriveFunction()
    );
    definitions.Add(
      new SimpleFunction(
        "round", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Round(((NumberHixValue)a[0]).Value)),
        documentation: "Rounds a number to the nearest integer."),
      new SimpleFunction(
        "floor", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Floor(((NumberHixValue)a[0]).Value)),
        documentation: "Rounds a number down to the nearest integer."),
      new SimpleFunction(
        "ceil", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Ceiling(((NumberHixValue)a[0]).Value)),
        documentation: "Rounds a number up to the nearest integer."),
      new SimpleFunction(
        "abs", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Abs(((NumberHixValue)a[0]).Value)),
        documentation: "Returns the absolute value of a number."),
      new SimpleFunction(
        "sqrt", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Sqrt(((NumberHixValue)a[0]).Value)),
        documentation: "Returns the square root of a number."),
      new SimpleFunction(
        "min", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Min(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value)),
        documentation: "Returns the smaller number."),
      new SimpleFunction(
        "max", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Max(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value)),
        documentation: "Returns the larger number."),
      new SimpleFunction(
        "pow", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Pow(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value)),
        documentation: "Raises the first number to the power of the second."),
      new SimpleFunction(
        "log", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Log(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value)),
        documentation: "Returns the natural logarithm of a number."),
      new SimpleFunction(
        "minus", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value - ((NumberHixValue)a[1]).Value),
        documentation: "Subtracts the second number from the first, or negates a single number."),
      new SimpleFunction(
        "plus", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value + ((NumberHixValue)a[1]).Value),
        documentation: "Adds two numbers."),
      new SimpleFunction(
        "mult", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value * ((NumberHixValue)a[1]).Value),
        documentation: "Multiplies two numbers."),
      new SimpleFunction(
        "div", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value / ((NumberHixValue)a[1]).Value),
        documentation: "Divides the first number by the second."),
      new SimpleFunction(
        "mod", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value % ((NumberHixValue)a[1]).Value),
        documentation: "Returns the remainder after division."),
      new SimpleFunction(
        "clamp", [new FunctionSignature(K.Number, [K.Number, K.Number, K.Number])],
        (e, a) => NumberResult(
          e,
          Math.Max(
            ((NumberHixValue)a[1]).Value, Math.Min(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[2]).Value)
          )
        ),
        documentation: "Clamps a number to the supplied minimum and maximum."),
      new SimpleFunction(
        "isInt", [new FunctionSignature(K.Bool, [K.Number])], (_, a) => {
          var value = ((NumberHixValue)a[0]).Value;
          return Bool(!double.IsNaN(value) && !double.IsInfinity(value) && value == Math.Truncate(value));
        },
        documentation: "Tests whether a number has no fractional part."),
      new SimpleFunction(
        "compare", [new FunctionSignature(K.Bool, [K.Number, K.String, K.Number])], (e, a) => {
          var value = ((NumberHixValue)a[0]).Value;
          var other = ((NumberHixValue)a[2]).Value;
          var equal = Math.Abs(value - other) <= 1e-9 * Math.Max(1, Math.Max(Math.Abs(value), Math.Abs(other)));
          return e.ResolveText(a[1]) switch {
            "lt" => Bool(value < other && !equal), "gt" => Bool(value > other && !equal),
            "lte" => Bool(value <= other || equal), "gte" => Bool(value >= other || equal), "eq" => Bool(equal),
            "neq" => Bool(!equal), _ => e.Error("unknown comparison")
          };
        },
        documentation: "Compares numbers using lt, gt, lte, gte, eq, or neq, with relative equality tolerance.")
    );
    definitions.Add(
      new SimpleFunction(
        "substring", [new FunctionSignature(K.String, [K.String, K.Number, K.Number])], (e, a) => {
          var value = e.ResolveText(a[0]);
          var start = ((NumberHixValue)a[1]).Value;
          var end = ((NumberHixValue)a[2]).Value;
          return start != Math.Truncate(start) || end != Math.Truncate(end) || start < 0 || end < start ||
            end > value.Length
              ? e.Error("invalid substring range")
              : String(value.Substring((int)start, (int)(end - start)));
        },
        documentation: "Extracts text between zero-based start (inclusive) and end (exclusive) offsets."),
      new SimpleFunction(
        "split", [new FunctionSignature(K.Tuple, [K.String, K.String])],
        (e, a) => new TupleHixValue(
          e.ResolveText(a[0]).Split([e.ResolveText(a[1])], StringSplitOptions.None).Select(text => (IHixValue)String(text))
            .ToArray()
        ),
        documentation: "Splits text by a delimiter and returns a tuple of strings."),
      new SimpleFunction(
        "startsWith", [new FunctionSignature(K.Bool, [K.String, K.String])],
        (e, a) => Bool(e.ResolveText(a[0]).StartsWith(e.ResolveText(a[1]), StringComparison.Ordinal)),
        documentation: "Tests whether text starts with a prefix."),
      new SimpleFunction(
        "endsWith", [new FunctionSignature(K.Bool, [K.String, K.String])],
        (e, a) => Bool(e.ResolveText(a[0]).EndsWith(e.ResolveText(a[1]), StringComparison.Ordinal)),
        documentation: "Tests whether text ends with a suffix."),
      new SimpleFunction(
        "matches", [new FunctionSignature(K.Bool, [K.String, K.String])],
        (e, a) => Bool(
          new Regex(e.ResolveText(a[1]), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).IsMatch(e.ResolveText(a[0]))
        ),
        documentation: "Tests text against a regular expression."),
      new SimpleFunction(
        "uppercase", [new FunctionSignature(K.String, [K.String])],
        (e, a) => String(e.ResolveText(a[0]).ToUpperInvariant()),
        documentation: "Converts text to uppercase."),
      new SimpleFunction(
        "lowercase", [new FunctionSignature(K.String, [K.String])],
        (e, a) => String(e.ResolveText(a[0]).ToLowerInvariant()),
        documentation: "Converts text to lowercase."),
      new SimpleFunction(
        "trim", [new FunctionSignature(K.String, [K.String])], (e, a) => String(e.ResolveText(a[0]).Trim()),
        documentation: "Removes leading and trailing whitespace."),
      new SimpleFunction(
        "trimStart", [new FunctionSignature(K.String, [K.String])],
        (e, a) => String(e.ResolveText(a[0]).TrimStart()),
        documentation: "Removes leading whitespace."),
      new SimpleFunction(
        "trimEnd", [new FunctionSignature(K.String, [K.String])],
        (e, a) => String(e.ResolveText(a[0]).TrimEnd()),
        documentation: "Removes trailing whitespace."),
      new SimpleFunction(
        "replaceAll", [new FunctionSignature(K.String, [K.String, K.String, K.String])],
        (e, a) => String(e.ResolveText(a[0]).Replace(e.ResolveText(a[1]), e.ResolveText(a[2]))),
        documentation: "Replaces every occurrence of the specified text."),
      new SimpleFunction(
        "replaceFirst", [new FunctionSignature(K.String, [K.String, K.String, K.String])], (e, a) => {
          var value = e.ResolveText(a[0]);
          var search = e.ResolveText(a[1]);
          if (search.Length == 0) return e.Error("replacement search string cannot be empty");
          var index = value.IndexOf(search, StringComparison.Ordinal);
          return String(
            index < 0 ? value : value.Substring(0, index) + e.ResolveText(a[2]) + value.Substring(index + search.Length)
          );
        },
        documentation: "Replaces the first occurrence of the specified text."),
      new SimpleFunction(
        "replaceLast", [new FunctionSignature(K.String, [K.String, K.String, K.String])], (e, a) => {
          var value = e.ResolveText(a[0]);
          var search = e.ResolveText(a[1]);
          if (search.Length == 0) return e.Error("replacement search string cannot be empty");
          var index = value.LastIndexOf(search, StringComparison.Ordinal);
          return String(
            index < 0 ? value : value.Substring(0, index) + e.ResolveText(a[2]) + value.Substring(index + search.Length)
          );
        },
        documentation: "Replaces the last occurrence of the specified text."),
      new SimpleFunction(
        "regexReplaceAll", [new FunctionSignature(K.String, [K.String, K.String, K.String])],
        (e, a) => String(
          new Regex(e.ResolveText(a[1]), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Replace(
            e.ResolveText(a[0]), e.ResolveText(a[2])
          )
        ),
        documentation: "Replaces all regular-expression matches."),
      new SimpleFunction(
        "regexReplaceFirst", [new FunctionSignature(K.String, [K.String, K.String, K.String])],
        (e, a) => String(
          new Regex(e.ResolveText(a[1]), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Replace(
            e.ResolveText(a[0]), e.ResolveText(a[2]), 1
          )
        ),
        documentation: "Replaces the first regular-expression match.")
    );
    definitions.Add(
      new SimpleFunction(
        "length",
        [
          new FunctionSignature(K.Number, [K.String]), new FunctionSignature(K.Number, [K.Tuple]),
          new FunctionSignature(K.Number, [K.Table])
        ],
        (e, a) => new NumberHixValue(
          a[0] switch {
            LiteralHixValue => e.ResolveText(a[0]).Length, TupleHixValue tuple => tuple.Values.Count,
            HixTableValue table => table.Entries.Count, _ => 0
          }
        ),
        documentation: "Returns the character count of text or the number of tuple elements or table entries."),
      new SimpleFunction(
        "contains",
        [
          new FunctionSignature(K.Bool, [K.String, K.String]), new FunctionSignature(K.Bool, [K.Tuple, K.Any]),
          new FunctionSignature(K.Bool, [K.Table, K.Any])
        ],
        (e, a) => a[0] is LiteralHixValue
          ? Bool(e.ResolveText(a[0]).Contains(e.ResolveText(a[1])))
          : Bool(
            (a[0] is TupleHixValue tuple
              ? tuple.Values
              : ((HixTableValue)a[0]).Entries.Select(entry => entry.Value))
            .Any(value => e.Equal(value, a[1]))
          ),
        documentation: "Tests whether text contains a substring or a tuple contains a value."),
      new SimpleFunction(
        "has",
        [new FunctionSignature(K.Bool, [K.Tuple, K.Number]), new FunctionSignature(K.Bool, [K.Table, K.String])],
        (e, a) => CollectionFunctions.Has(e, a),
        documentation: "Tests whether a table contains a key, including keys whose value is null."),
      new SimpleFunction(
        "get",
        [
          new FunctionSignature(K.Any, [K.Tuple, K.Number]), new FunctionSignature(K.Any, [K.Tuple, K.Number, K.Any]),
          new FunctionSignature(K.Any, [K.Table, K.String]), new FunctionSignature(K.Any, [K.Table, K.String, K.Any])
        ],
        (e, a) => CollectionFunctions.Get(e, a),
        documentation: "Reads a tuple index or table key; returns the fallback when absent.")
    );
    definitions.Add(
      new CollectionTransformFunction("map", CollectionFunctions.TransformKind.Map),
      new CollectionTransformFunction("where", CollectionFunctions.TransformKind.Where),
      new CollectionTransformFunction("any", CollectionFunctions.TransformKind.Any),
      new CollectionTransformFunction("all", CollectionFunctions.TransformKind.All),
      new CollectionTransformFunction("reduce", CollectionFunctions.TransformKind.Reduce, true)
    );
    definitions.Add(
      new SimpleFunction(
        "indexOf", [new FunctionSignature(K.Number, [K.Tuple, K.Any])], (e, a) => {
          var values = ((TupleHixValue)a[0]).Values;
          for (var index = 0; index < values.Count; index++)
            if (e.Equal(values[index], a[1]))
              return new NumberHixValue(index);
          return new NumberHixValue(-1);
        },
        documentation: "Returns the index of a substring or tuple value, or -1 when absent."),
      new SimpleFunction(
        "push", [new FunctionSignature(K.Tuple, [K.Tuple, K.Any])],
        (_, a) => ((TupleHixValue)a[0]).Append(a[1]),
        documentation: "Returns a tuple with the supplied value appended."),
      new SimpleFunction(
        "pop", [new FunctionSignature(K.Tuple, [K.Tuple])], (_, a) => ((TupleHixValue)a[0]).RemoveLast(),
        documentation: "Returns a tuple with its final element removed."),
      new SimpleFunction(
        "keys", [new FunctionSignature(K.Tuple, [K.Table])],
        (e, a) => new TupleHixValue(
          ((HixTableValue)a[0]).Entries.Select(entry => (IHixValue)String(entry.Key.Resolve(e.Strings)))
          .ToArray()
        ),
        documentation: "Returns a tuple containing the table keys."),
      new SimpleFunction(
        "values", [new FunctionSignature(K.Tuple, [K.Table])],
        (_, a) => new TupleHixValue(((HixTableValue)a[0]).Entries.Select(entry => entry.Value).ToArray()),
        documentation: "Returns a tuple containing the table values."),
      new SimpleFunction(
        "entries", [new FunctionSignature(K.Tuple, [K.Table])],
        (e, a) => KindDefinitions.Get(K.Tuple).Convert(e, a[0]),
        documentation: "Returns table entries as a tuple of key/value pairs."),
      new SimpleFunction(
        "remove", [new FunctionSignature(K.Table, [K.Table, K.String])],
        (e, a) => ((HixTableValue)a[0]).Remove(e, e.Text(a[1])),
        documentation: "Returns a table without the specified key.")
    );
    definitions.Add(
      new SimpleFunction(
        "put",
        [new FunctionSignature(K.Table, [K.Table, K.String, K.Any]), new FunctionSignature(K.Table, [K.Table, K.Table])],
        (e, a) => {
          var table = (HixTableValue)a[0];
          if (a.Length == 3) return CollectionFunctions.Put(e, table, e.ResolveText(a[1]), a[2]);
          foreach (var entry in ((HixTableValue)a[1]).Entries)
            table = CollectionFunctions.Put(e, table, entry.Key.Resolve(e.Strings), entry.Value);
          return table;
        },
        documentation: "Returns a table with the specified key set to a value."),
      new SimpleFunction(
        "join",
        [
          new FunctionSignature(K.String, [K.Tuple, K.String]),
          new FunctionSignature(K.String, [K.Table, K.String, K.String])
        ],
        (e, a) => {
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
        },
        documentation: "Renders tuple elements and joins them with a separator. Nested collections require explicit rendering.")
    );
  }
}
