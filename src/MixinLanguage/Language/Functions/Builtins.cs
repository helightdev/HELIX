using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mixins.Runtime;
using K = Mixins.MixinValueKind;
using static Mixins.Runtime.LanguageExecution;

namespace Mixins.Functions;

internal static class Builtins {
  internal static void Register(FunctionSignatureRegistryBuilder definitions) {
    static IMixinValue NumberResult(LanguageExecution execution, double value) =>
      double.IsNaN(value) || double.IsInfinity(value)
        ? execution.Context.Error("invalid numeric operation")
        : new NumberMixinValue(value);

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
          var kind = KindDefinitions.Get((KindMixinValue)a[0]);
          return a.Length == 1 ? kind.New(e) : kind.Convert(e, a[1]);
        }
      )
    );
    definitions.Add(
      new SimpleFunction(
        "as", [new FunctionSignature(K.Any, [K.Any, K.Kind])],
        (e, a) => KindDefinitions.Get((KindMixinValue)a[1]).Convert(e, a[0])
      ),
      new SimpleFunction(
        "is", [new FunctionSignature(K.Bool, [K.Any, K.Kind])],
        (_, a) => Bool(KindDefinitions.Get((KindMixinValue)a[1]).Is(a[0])), acceptsErrors: true
      ),
      new SimpleFunction(
        "if", [new FunctionSignature(K.Any, [K.Any, K.Kind, K.Any])],
        (_, a) => KindDefinitions.Get((KindMixinValue)a[1]).Is(a[0]) ? a[0] : a[2]
      ),
      new SimpleFunction(
        "ifNot", [new FunctionSignature(K.Any, [K.Any, K.Kind, K.Any])],
        (_, a) => !KindDefinitions.Get((KindMixinValue)a[1]).Is(a[0]) ? a[0] : a[2]
      ),
      new SimpleFunction(
        "exists", [new FunctionSignature(K.Bool, [K.Any])],
        (_, a) => Bool(a[0] is not (NullMixinValue or ErrorMixinValue)), acceptsErrors: true
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
          ? NullMixinValue.Instance
          : e.Context.Error("assertion failed")
      ),
      new SimpleFunction(
        "fail", [new FunctionSignature(K.Error, []), new FunctionSignature(K.Error, [K.Any])],
        (e, a) => {
          var rendered = a.Length == 1 ? e.RenderText(a[0]) : String("execution failed");
          return rendered is ErrorMixinValue ? rendered : e.Context.Error(e.Text(rendered));
        }
      ),
      new SimpleFunction(
        "emit", [new FunctionSignature(K.Null, [K.Any]), new FunctionSignature(K.Null, [K.String, K.Any])],
        (e, a) => {
          var target = a.Length == 1 ? "TARGET" : e.Text(a[0]);
          var rendered = e.RenderText(a[a.Length - 1]);
          if (rendered is ErrorMixinValue) return rendered;
          var value = e.Text(rendered);
          if (Enum.TryParse<MixinEmissionTarget>(target, true, out var output))
            e.Outputs.Add(new MixinExpressionOutput(output, value));
          else
            e.Outputs.Add(
              new MixinExpressionOutput(
                MixinEmissionTarget.Mixin, value, e.Context.ResolveInjectionTarget(target)
              )
            );
          return NullMixinValue.Instance;
        }, effects: true
      ),
      new SimpleFunction(
        "using", [new FunctionSignature(K.Null, [K.String])], (e, a) => {
          e.Outputs.Add(new MixinExpressionOutput(MixinEmissionTarget.Using, e.Text(a[0])));
          return NullMixinValue.Instance;
        }, effects: true
      ),
      new SimpleFunction(
        "inject",
        [new FunctionSignature(K.Null, [K.String, K.Any]), new FunctionSignature(K.Null, [K.String, K.Number, K.Any])],
        (e, a) => {
          var rendered = e.RenderText(a[a.Length - 1]);
          if (rendered is ErrorMixinValue) return rendered;
          e.Outputs.Add(
            new MixinExpressionOutput(
              MixinEmissionTarget.Mixin, e.Text(rendered),
              e.Context.ResolveInjectionTarget(e.Text(a[0])),
              a.Length == 3 ? checked((int)((NumberMixinValue)a[1]).Value) : 0
            )
          );
          return NullMixinValue.Instance;
        }, effects: true
      ),
      new SimpleFunction(
        "resolveMixin", [new FunctionSignature(K.Any, [K.String, K.Any])],
        (e, a) =>
          e.IsPrelude
            ? e.Context.ResolveMixin(a[0].Render(e.Context), a[1])
            : e.Context.Error("resolveMixin requires the prelude pass"), effects: true
      ),
      new SimpleFunction(
        "defineTarget", [new FunctionSignature(K.Null, [K.String, K.String])],
        (e, a) =>
          e.IsPrelude
            ? e.Context.DefineTarget(e.Text(a[0]), e.Text(a[1]))
            : e.Context.Error("defineTarget requires the prelude pass"), effects: true
      ),
      new SimpleFunction(
        "config", [new FunctionSignature(K.Null, [K.String, K.Any])],
        (e, a) =>
          e.IsPrelude ? e.Context.Configure(e.Text(a[0]), a[1]) : e.Context.Error("config requires the prelude pass"),
        effects: true
      )
    );
    definitions.Add(
      new CatchFunction(), new CallbackFunction("call"), new CallbackFunction("inline"),
      new MatchFunction(), new LogFunction("log", false), new LogFunction("dump", true), new DeriveFunction()
    );
    definitions.Add(
      new SimpleFunction(
        "round", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Round(((NumberMixinValue)a[0]).Value))
      ),
      new SimpleFunction(
        "floor", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Floor(((NumberMixinValue)a[0]).Value))
      ),
      new SimpleFunction(
        "ceil", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Ceiling(((NumberMixinValue)a[0]).Value))
      ),
      new SimpleFunction(
        "abs", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Abs(((NumberMixinValue)a[0]).Value))
      ),
      new SimpleFunction(
        "sqrt", [new FunctionSignature(K.Number, [K.Number])],
        (e, a) => NumberResult(e, Math.Sqrt(((NumberMixinValue)a[0]).Value))
      ),
      new SimpleFunction(
        "min", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Min(((NumberMixinValue)a[0]).Value, ((NumberMixinValue)a[1]).Value))
      ),
      new SimpleFunction(
        "max", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Max(((NumberMixinValue)a[0]).Value, ((NumberMixinValue)a[1]).Value))
      ),
      new SimpleFunction(
        "pow", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Pow(((NumberMixinValue)a[0]).Value, ((NumberMixinValue)a[1]).Value))
      ),
      new SimpleFunction(
        "log", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, Math.Log(((NumberMixinValue)a[0]).Value, ((NumberMixinValue)a[1]).Value))
      ),
      new SimpleFunction(
        "minus", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberMixinValue)a[0]).Value - ((NumberMixinValue)a[1]).Value)
      ),
      new SimpleFunction(
        "plus", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberMixinValue)a[0]).Value + ((NumberMixinValue)a[1]).Value)
      ),
      new SimpleFunction(
        "mult", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberMixinValue)a[0]).Value * ((NumberMixinValue)a[1]).Value)
      ),
      new SimpleFunction(
        "div", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberMixinValue)a[0]).Value / ((NumberMixinValue)a[1]).Value)
      ),
      new SimpleFunction(
        "mod", [new FunctionSignature(K.Number, [K.Number, K.Number])],
        (e, a) => NumberResult(e, ((NumberMixinValue)a[0]).Value % ((NumberMixinValue)a[1]).Value)
      ),
      new SimpleFunction(
        "clamp", [new FunctionSignature(K.Number, [K.Number, K.Number, K.Number])],
        (e, a) => NumberResult(
          e,
          Math.Max(
            ((NumberMixinValue)a[1]).Value, Math.Min(((NumberMixinValue)a[0]).Value, ((NumberMixinValue)a[2]).Value)
          )
        )
      ),
      new SimpleFunction(
        "isInt", [new FunctionSignature(K.Bool, [K.Number])], (_, a) => {
          var value = ((NumberMixinValue)a[0]).Value;
          return Bool(!double.IsNaN(value) && !double.IsInfinity(value) && value == Math.Truncate(value));
        }
      ),
      new SimpleFunction(
        "compare", [new FunctionSignature(K.Bool, [K.Number, K.String, K.Number])], (e, a) => {
          var value = ((NumberMixinValue)a[0]).Value;
          var other = ((NumberMixinValue)a[2]).Value;
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
          var start = ((NumberMixinValue)a[1]).Value;
          var end = ((NumberMixinValue)a[2]).Value;
          return start != Math.Truncate(start) || end != Math.Truncate(end) || start < 0 || end < start ||
            end > value.Length
              ? e.Context.Error("invalid substring range")
              : String(value.Substring((int)start, (int)(end - start)));
        }
      ),
      new SimpleFunction(
        "split", [new FunctionSignature(K.Tuple, [K.String, K.String])],
        (e, a) => new TupleMixinValue(
          e.Text(a[0]).Split([e.Text(a[1])], StringSplitOptions.None).Select(text => (IMixinValue)String(text))
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
        (e, a) => new NumberMixinValue(
          a[0] switch {
            LiteralMixinValue => e.Text(a[0]).Length, TupleMixinValue tuple => tuple.Values.Count,
            MixinTableValue table => table.Entries.Count, _ => 0
          }
        )
      ),
      new SimpleFunction(
        "contains",
        [
          new FunctionSignature(K.Bool, [K.String, K.String]), new FunctionSignature(K.Bool, [K.Tuple, K.Any]),
          new FunctionSignature(K.Bool, [K.Table, K.Any])
        ],
        (e, a) => a[0] is LiteralMixinValue
          ? Bool(e.Text(a[0]).Contains(e.Text(a[1])))
          : Bool(
            (a[0] is TupleMixinValue tuple
              ? tuple.Values
              : ((MixinTableValue)a[0]).Entries.Select(entry => entry.Value))
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
          var values = ((TupleMixinValue)a[0]).Values;
          for (var index = 0; index < values.Count; index++)
            if (e.Equal(values[index], a[1]))
              return new NumberMixinValue(index);
          return new NumberMixinValue(-1);
        }
      ),
      new SimpleFunction(
        "push", [new FunctionSignature(K.Tuple, [K.Tuple, K.Any])],
        (_, a) => new TupleMixinValue(((TupleMixinValue)a[0]).Values.Concat([a[1]]).ToArray())
      ),
      new SimpleFunction(
        "pop", [new FunctionSignature(K.Tuple, [K.Tuple])], (_, a) => {
          var values = ((TupleMixinValue)a[0]).Values;
          return new TupleMixinValue(values.Take(Math.Max(0, values.Count - 1)).ToArray());
        }
      ),
      new SimpleFunction(
        "keys", [new FunctionSignature(K.Tuple, [K.Table])],
        (e, a) => new TupleMixinValue(
          ((MixinTableValue)a[0]).Entries.Select(entry => (IMixinValue)String(entry.Key.Resolve(e.Context.Strings)))
          .ToArray()
        )
      ),
      new SimpleFunction(
        "values", [new FunctionSignature(K.Tuple, [K.Table])],
        (_, a) => new TupleMixinValue(((MixinTableValue)a[0]).Entries.Select(entry => entry.Value).ToArray())
      ),
      new SimpleFunction(
        "entries", [new FunctionSignature(K.Tuple, [K.Table])],
        (e, a) => KindDefinitions.Get(K.Tuple).Convert(e, a[0])
      ),
      new SimpleFunction(
        "remove", [new FunctionSignature(K.Table, [K.Table, K.String])],
        (e, a) => new MixinTableValue(
          ((MixinTableValue)a[0]).Entries.Where(entry => entry.Key.Resolve(e.Context.Strings) != e.Text(a[1])).ToArray()
        )
      )
    );
    definitions.Add(
      new SimpleFunction(
        "put",
        [new FunctionSignature(K.Table, [K.Table, K.String, K.Any]), new FunctionSignature(K.Table, [K.Table, K.Table])],
        (e, a) => {
          var table = (MixinTableValue)a[0];
          if (a.Length == 3) return CollectionFunctions.Put(e.Context, table, e.Text(a[1]), a[2]);
          foreach (var entry in ((MixinTableValue)a[1]).Entries)
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
          var values = a[0] is TupleMixinValue tuple ? tuple.Values : ((MixinTableValue)a[0]).Entries.Select(entry => entry.Value).ToArray();
          var rendered = new List<string>();
          foreach (var value in values) {
            var text = e.RenderText(value);
            if (text is ErrorMixinValue) return text;
            rendered.Add(e.Text(text));
          }
          return String(a[0] is TupleMixinValue
            ? string.Join(e.Text(a[1]), rendered)
            : string.Join(e.Text(a[2]), ((MixinTableValue)a[0]).Entries.Select((entry, index) =>
              entry.Key.Resolve(e.Context.Strings) + e.Text(a[1]) + rendered[index])));
        }
      )
    );
  }
}