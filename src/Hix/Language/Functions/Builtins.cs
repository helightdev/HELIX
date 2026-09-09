using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hix.Runtime;
using K = Hix.HixValueKind;
using static Hix.Runtime.LanguageExecution;

namespace Hix.Functions;

internal static class Builtins {
  internal static void Register(FunctionSignatureRegistryBuilder definitions) {
    static IHixValue NumberResult(LanguageExecution execution, double value) =>
      double.IsNaN(value) || double.IsInfinity(value)
        ? execution.Context.Error("invalid numeric operation")
        : new NumberHixValue(value);

    foreach (var kind in KindDefinitions.Enumerate()) {
      definitions.Add(
        new SimpleFunction(
          kind.Kind.Name,
          [new FunctionSignature(kind.ValueKind, []), new FunctionSignature(kind.ValueKind, [K.Any])],
          (execution, arguments) => arguments.Length == 0
            ? kind.New(execution)
            : kind.Convert(execution, arguments[0]),
          acceptsErrors: kind.ValueKind is K.String or K.Bool or K.Kind
        )
      );
    }
    definitions.Add(
      new SimpleFunction(
        "new", [new FunctionSignature(K.Any, [K.Kind]), new FunctionSignature(K.Any, [K.Kind, K.Any])],
        (e, a) => {
          var kind = KindDefinitions.Get((KindHixValue)a[0]);
          return a.Length == 1 ? kind.New(e) : kind.Convert(e, a[1]);
        }
      )
    );
    definitions.Add(
      new SimpleFunction(
        "as", [new FunctionSignature(K.Any, [K.Any, K.Kind])],
        (e, a) => KindDefinitions.Get((KindHixValue)a[1]).Convert(e, a[0])
      ),
      new SimpleFunction(
        "is", [new FunctionSignature(K.Bool, [K.Any, K.Kind])],
        (_, a) => Bool(KindDefinitions.Get((KindHixValue)a[1]).Is(a[0])), acceptsErrors: true
      ),
      new SimpleFunction(
        "if", [new FunctionSignature(K.Any, [K.Any, K.Kind, K.Any])],
        (_, a) => KindDefinitions.Get((KindHixValue)a[1]).Is(a[0]) ? a[0] : a[2]
      ),
      new SimpleFunction(
        "ifNot", [new FunctionSignature(K.Any, [K.Any, K.Kind, K.Any])],
        (_, a) => !KindDefinitions.Get((KindHixValue)a[1]).Is(a[0]) ? a[0] : a[2]
      ),
      new SimpleFunction(
        "exists", [new FunctionSignature(K.Bool, [K.Any])],
        (_, a) => Bool(a[0] is not (NullHixValue or ErrorHixValue)), acceptsErrors: true
      ),
      new SimpleFunction(
        "not", [new FunctionSignature(K.Bool, [K.Any])],
        (e, a) => Bool(!a[0].IsTruthy(e.Context)), acceptsErrors: true
      ),
      new SimpleFunction(
        "eq", [new FunctionSignature(K.Bool, [K.Any, K.Any])],
        (e, a) => Bool(e.Equal(a[0], a[1]))
      ),
      new SimpleFunction(
        "neq", [new FunctionSignature(K.Bool, [K.Any, K.Any])],
        (e, a) => Bool(!e.Equal(a[0], a[1]))
      ),
      new SimpleFunction(
        "and", [new FunctionSignature(K.Bool, [K.Any, K.Any, K.Any], true)],
        (e, a) => Bool(a.All(value => value.IsTruthy(e.Context)))
      ),
      new SimpleFunction(
        "or", [new FunctionSignature(K.Bool, [K.Any, K.Any, K.Any], true)],
        (e, a) => Bool(a.Any(value => value.IsTruthy(e.Context)))
      ),
      new SimpleFunction(
        "assert", [new FunctionSignature(K.Null, [K.Any], true)],
        (e, a) => a.All(value => value.IsTruthy(e.Context))
          ? NullHixValue.Instance
          : e.Context.Error("assertion failed")
      ),
      new SimpleFunction(
        "fail", [new FunctionSignature(K.Error, []), new FunctionSignature(K.Error, [K.Any])],
        (e, a) => {
          var rendered = a.Length == 1 ? e.RenderText(a[0]) : String("execution failed");
          return rendered is ErrorHixValue ? rendered : e.Context.Error(e.Text(rendered));
        }
      ),
      new SimpleFunction(
        "emit", [new FunctionSignature(K.Null, [K.Any]), new FunctionSignature(K.Null, [K.String, K.Any])],
        (e, a) => {
          var target = a.Length == 1 ? "TARGET" : e.Text(a[0]);
          var rendered = e.RenderText(a[a.Length - 1]);
          if (rendered is ErrorHixValue) return rendered;
          var value = e.Text(rendered);
          if (Enum.TryParse<HixEmissionTarget>(target, true, out var output))
            e.Outputs.Add(new HixExpressionOutput(output, value));
          else
            e.Outputs.Add(
              new HixExpressionOutput(
                HixEmissionTarget.Mixin, value, e.Context.ResolveInjectionTarget(target)
              )
            );
          return NullHixValue.Instance;
        }, effects: true
      )
    );
    definitions.Add(
      new CatchFunction(), new CallbackFunction("call"), new CallbackFunction("inline"),
      new MatchFunction(), new LogFunction("log", false), new LogFunction("dump", true), new DeriveFunction()
    );
    definitions.Add(
      new SimpleFunction(
        "round", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Round(((NumberHixValue)a[0]).Value))
      ),
      new SimpleFunction(
        "floor", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Floor(((NumberHixValue)a[0]).Value))
      ),
      new SimpleFunction(
        "ceil", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Ceiling(((NumberHixValue)a[0]).Value))
      ),
      new SimpleFunction(
        "abs", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Abs(((NumberHixValue)a[0]).Value))
      ),
      new SimpleFunction(
        "sqrt", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Sqrt(((NumberHixValue)a[0]).Value))
      ),
      new SimpleFunction(
        "min", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Min(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value))
      ),
      new SimpleFunction(
        "max", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Max(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value))
      ),
      new SimpleFunction(
        "pow", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Pow(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value))
      ),
      new SimpleFunction(
        "log", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Log(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[1]).Value))
      ),
      new SimpleFunction(
        "minus", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value - ((NumberHixValue)a[1]).Value)
      ),
      new SimpleFunction(
        "plus", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value + ((NumberHixValue)a[1]).Value)
      ),
      new SimpleFunction(
        "mult", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value * ((NumberHixValue)a[1]).Value)
      ),
      new SimpleFunction(
        "div", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value / ((NumberHixValue)a[1]).Value)
      ),
      new SimpleFunction(
        "mod", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberHixValue)a[0]).Value % ((NumberHixValue)a[1]).Value)
      ),
      new SimpleFunction(
        "clamp", [new FunctionSignature(K.Number, [K.Number, K.Number, K.Number])],
        (e, a) => NumberResult(
          e,
          Math.Max(
            ((NumberHixValue)a[1]).Value, Math.Min(((NumberHixValue)a[0]).Value, ((NumberHixValue)a[2]).Value)
          )
        )
      ),
      new SimpleFunction(
        "isInt", [new FunctionSignature(K.Bool, [K.Number])], (_, a) => {
          var value = ((NumberHixValue)a[0]).Value;
          return Bool(!double.IsNaN(value) && !double.IsInfinity(value) && value == Math.Truncate(value));
        }
      ),
      new SimpleFunction(
        "compare", [new FunctionSignature(K.Bool, [K.Number, K.String, K.Number])], (e, a) => {
          var value = ((NumberHixValue)a[0]).Value;
          var other = ((NumberHixValue)a[2]).Value;
          var equal = Math.Abs(value - other) <= 1e-9 * Math.Max(1, Math.Max(Math.Abs(value), Math.Abs(other)));
          return e.Text(a[1]) switch {
            "lt" => Bool(value < other && !equal), "gt" => Bool(value > other && !equal),
            "lte" => Bool(value <= other || equal), "gte" => Bool(value >= other || equal), "eq" => Bool(equal),
            "neq" => Bool(!equal), _ => e.Context.Error("unknown comparison")
          };
        }
      )
    );
    definitions.Add(
      new SimpleFunction(
        "substring", [new FunctionSignature(K.String, [K.String, K.Number, K.Number])], (e, a) => {
          var value = e.Text(a[0]);
          var start = ((NumberHixValue)a[1]).Value;
          var end = ((NumberHixValue)a[2]).Value;
          return start != Math.Truncate(start) || end != Math.Truncate(end) || start < 0 || end < start ||
            end > value.Length
              ? e.Context.Error("invalid substring range")
              : String(value.Substring((int)start, (int)(end - start)));
        }
      ),
      new SimpleFunction(
        "split", [new FunctionSignature(K.Tuple, [K.String, K.String])],
        (e, a) => new TupleHixValue(
          e.Text(a[0]).Split([e.Text(a[1])], StringSplitOptions.None).Select(text => (IHixValue)String(text))
            .ToArray()
        )
      ),
      new SimpleFunction(
        "startsWith", [new FunctionSignature(K.Bool, [K.String, K.String])],
        (e, a) => Bool(e.Text(a[0]).StartsWith(e.Text(a[1]), StringComparison.Ordinal))
      ),
      new SimpleFunction(
        "endsWith", [new FunctionSignature(K.Bool, [K.String, K.String])],
        (e, a) => Bool(e.Text(a[0]).EndsWith(e.Text(a[1]), StringComparison.Ordinal))
      ),
      new SimpleFunction(
        "matches", [new FunctionSignature(K.Bool, [K.String, K.String])],
        (e, a) => Bool(
          new Regex(e.Text(a[1]), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).IsMatch(e.Text(a[0]))
        )
      ),
      new SimpleFunction(
        "uppercase", [new FunctionSignature(K.String, [K.String])],
        (e, a) => String(e.Text(a[0]).ToUpperInvariant())
      ),
      new SimpleFunction(
        "lowercase", [new FunctionSignature(K.String, [K.String])],
        (e, a) => String(e.Text(a[0]).ToLowerInvariant())
      ),
      new SimpleFunction(
        "trim", [new FunctionSignature(K.String, [K.String])], (e, a) => String(e.Text(a[0]).Trim())
      ),
      new SimpleFunction(
        "trimStart", [new FunctionSignature(K.String, [K.String])],
        (e, a) => String(e.Text(a[0]).TrimStart())
      ),
      new SimpleFunction(
        "trimEnd", [new FunctionSignature(K.String, [K.String])],
        (e, a) => String(e.Text(a[0]).TrimEnd())
      ),
      new SimpleFunction(
        "replaceAll", [new FunctionSignature(K.String, [K.String, K.String, K.String])],
        (e, a) => String(e.Text(a[0]).Replace(e.Text(a[1]), e.Text(a[2])))
      ),
      new SimpleFunction(
        "replaceFirst", [new FunctionSignature(K.String, [K.String, K.String, K.String])], (e, a) => {
          var value = e.Text(a[0]);
          var search = e.Text(a[1]);
          if (search.Length == 0) return e.Context.Error("replacement search string cannot be empty");
          var index = value.IndexOf(search, StringComparison.Ordinal);
          return String(
            index < 0 ? value : value.Substring(0, index) + e.Text(a[2]) + value.Substring(index + search.Length)
          );
        }
      ),
      new SimpleFunction(
        "replaceLast", [new FunctionSignature(K.String, [K.String, K.String, K.String])], (e, a) => {
          var value = e.Text(a[0]);
          var search = e.Text(a[1]);
          if (search.Length == 0) return e.Context.Error("replacement search string cannot be empty");
          var index = value.LastIndexOf(search, StringComparison.Ordinal);
          return String(
            index < 0 ? value : value.Substring(0, index) + e.Text(a[2]) + value.Substring(index + search.Length)
          );
        }
      ),
      new SimpleFunction(
        "regexReplaceAll", [new FunctionSignature(K.String, [K.String, K.String, K.String])],
        (e, a) => String(
          new Regex(e.Text(a[1]), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Replace(
            e.Text(a[0]), e.Text(a[2])
          )
        )
      ),
      new SimpleFunction(
        "regexReplaceFirst", [new FunctionSignature(K.String, [K.String, K.String, K.String])],
        (e, a) => String(
          new Regex(e.Text(a[1]), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Replace(
            e.Text(a[0]), e.Text(a[2]), 1
          )
        )
      )
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
            LiteralHixValue => e.Text(a[0]).Length, TupleHixValue tuple => tuple.Values.Count,
            HixTableValue table => table.Entries.Count, _ => 0
          }
        )
      ),
      new SimpleFunction(
        "contains",
        [
          new FunctionSignature(K.Bool, [K.String, K.String]), new FunctionSignature(K.Bool, [K.Tuple, K.Any]),
          new FunctionSignature(K.Bool, [K.Table, K.Any])
        ],
        (e, a) => a[0] is LiteralHixValue
          ? Bool(e.Text(a[0]).Contains(e.Text(a[1])))
          : Bool(
            (a[0] is TupleHixValue tuple
              ? tuple.Values
              : ((HixTableValue)a[0]).Entries.Select(entry => entry.Value))
            .Any(value => e.Equal(value, a[1]))
          )
      ),
      new SimpleFunction(
        "has",
        [new FunctionSignature(K.Bool, [K.Tuple, K.Number]), new FunctionSignature(K.Bool, [K.Table, K.String])],
        (e, a) => CollectionFunctions.Has(e, a)
      ),
      new SimpleFunction(
        "get",
        [
          new FunctionSignature(K.Any, [K.Tuple, K.Number]), new FunctionSignature(K.Any, [K.Tuple, K.Number, K.Any]),
          new FunctionSignature(K.Any, [K.Table, K.String]), new FunctionSignature(K.Any, [K.Table, K.String, K.Any])
        ],
        (e, a) => CollectionFunctions.Get(e, a)
      )
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
        }
      ),
      new SimpleFunction(
        "push", [new FunctionSignature(K.Tuple, [K.Tuple, K.Any])],
        (_, a) => ((TupleHixValue)a[0]).Append(a[1])
      ),
      new SimpleFunction(
        "pop", [new FunctionSignature(K.Tuple, [K.Tuple])], (_, a) => ((TupleHixValue)a[0]).RemoveLast()
      ),
      new SimpleFunction(
        "keys", [new FunctionSignature(K.Tuple, [K.Table])],
        (e, a) => new TupleHixValue(
          ((HixTableValue)a[0]).Entries.Select(entry => (IHixValue)String(entry.Key.Resolve(e.Context.Strings)))
          .ToArray()
        )
      ),
      new SimpleFunction(
        "values", [new FunctionSignature(K.Tuple, [K.Table])],
        (_, a) => new TupleHixValue(((HixTableValue)a[0]).Entries.Select(entry => entry.Value).ToArray())
      ),
      new SimpleFunction(
        "entries", [new FunctionSignature(K.Tuple, [K.Table])],
        (e, a) => KindDefinitions.Get(K.Tuple).Convert(e, a[0])
      ),
      new SimpleFunction(
        "remove", [new FunctionSignature(K.Table, [K.Table, K.String])],
        (e, a) => ((HixTableValue)a[0]).Remove(e.Context, e.Context.ResolveString(e.Text(a[1])))
      )
    );
    definitions.Add(
      new SimpleFunction(
        "put",
        [new FunctionSignature(K.Table, [K.Table, K.String, K.Any]), new FunctionSignature(K.Table, [K.Table, K.Table])],
        (e, a) => {
          var table = (HixTableValue)a[0];
          if (a.Length == 3) return CollectionFunctions.Put(e.Context, table, e.Text(a[1]), a[2]);
          foreach (var entry in ((HixTableValue)a[1]).Entries)
            table = CollectionFunctions.Put(e.Context, table, entry.Key.Resolve(e.Context.Strings), entry.Value);
          return table;
        }
      ),
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
            rendered.Add(e.Text(text));
          }
          return String(a[0] is TupleHixValue
            ? string.Join(e.Text(a[1]), rendered)
            : string.Join(e.Text(a[2]), ((HixTableValue)a[0]).Entries.Select((entry, index) =>
              entry.Key.Resolve(e.Context.Strings) + e.Text(a[1]) + rendered[index])));
        }
      )
    );
  }
}
