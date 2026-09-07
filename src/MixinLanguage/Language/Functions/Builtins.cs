using System;
using System.Linq;
using K = Mixins.MixinValueKind;

namespace Mixins.Functions;

internal static class Builtins {
  internal static void Register(MixinSignatureRegistryBuilder<FunctionDefinition> definitions) {
    foreach (var kind in Enum.GetValues(typeof(MixinValueKind)).Cast<MixinValueKind>().Where(kind => kind != K.Any)) {
      var name = kind.ToString().ToLowerInvariant();
      definitions.Add(new KindFunctionDefinition(name, 0), new KindFunctionDefinition(name, 1));
    }
    definitions.Add(
      new CoreFunctionDefinition("new", K.Any, false, false, K.Kind),
      new CoreFunctionDefinition("new", K.Any, false, false, K.Kind, K.Any),
      new CoreFunctionDefinition("as", K.Any, false, false, K.Any, K.Kind),
      new CoreFunctionDefinition("is", K.Bool, false, false, K.Any, K.Kind),
      new CoreFunctionDefinition("if", K.Any, false, false, K.Any, K.Kind, K.Any),
      new CoreFunctionDefinition("ifNot", K.Any, false, false, K.Any, K.Kind, K.Any),
      new CoreFunctionDefinition("exists", K.Bool, false, false, K.Any),
      new CoreFunctionDefinition("not", K.Bool, false, false, K.Any),
      new CoreFunctionDefinition("eq", K.Bool, false, false, K.Any, K.Any),
      new CoreFunctionDefinition("neq", K.Bool, false, false, K.Any, K.Any),
      new CoreFunctionDefinition("and", K.Bool, false, true, K.Any, K.Any, K.Any),
      new CoreFunctionDefinition("or", K.Bool, false, true, K.Any, K.Any, K.Any),
      new CoreFunctionDefinition("catch", K.Any, false, false, K.Any),
      new CoreFunctionDefinition("catch", K.Any, false, false, K.Any, K.Function),
      new CoreFunctionDefinition("call", K.Any, false, false, K.Function),
      new CoreFunctionDefinition("call", K.Any, false, false, K.Function, K.Any),
      new CoreFunctionDefinition("inline", K.Any, false, false, K.Function),
      new CoreFunctionDefinition("inline", K.Any, false, false, K.Function, K.Any),
      new CoreFunctionDefinition("assert", K.Null, false, true, K.Any),
      new CoreFunctionDefinition("match", K.Null, false, true, K.Any),
      new CoreFunctionDefinition("fail", K.Error, false, false),
      new CoreFunctionDefinition("fail", K.Error, false, false, K.Any),
      new CoreFunctionDefinition("emit", K.Null, true, false, K.Any),
      new CoreFunctionDefinition("emit", K.Null, true, false, K.String, K.Any),
      new CoreFunctionDefinition("using", K.Null, true, false, K.String),
      new CoreFunctionDefinition("inject", K.Null, true, false, K.String, K.Any),
      new CoreFunctionDefinition("inject", K.Null, true, false, K.String, K.Number, K.Any),
      new CoreFunctionDefinition("log", K.Null, true, false, K.Any),
      new CoreFunctionDefinition("dump", K.Any, true, false, K.Any),
      new CoreFunctionDefinition("derive", K.Tuple, true, false, K.Tuple),
      new CoreFunctionDefinition("resolveMixin", K.Any, true, false, K.String, K.Any),
      new CoreFunctionDefinition("defineTarget", K.Null, true, false, K.String, K.String),
      new CoreFunctionDefinition("config", K.Null, true, false, K.String, K.Any));
    foreach (var name in new[] {"round", "floor", "ceil", "abs", "sqrt"})
      definitions.Add(new NumberFunctionDefinition(name, K.Number, K.Number));
    foreach (var name in new[] {"min", "max", "pow", "log", "minus", "plus", "mult", "div", "mod"})
      definitions.Add(new NumberFunctionDefinition(name, K.Number, K.Number, K.Number));
    definitions.Add(new NumberFunctionDefinition("clamp", K.Number, K.Number, K.Number, K.Number),
      new NumberFunctionDefinition("isInt", K.Bool, K.Number), new NumberFunctionDefinition("compare", K.Bool, K.Number, K.String, K.Number));
    definitions.Add(new StringFunctionDefinition("length", K.Number, K.String),
      new StringFunctionDefinition("substring", K.String, K.String, K.Number, K.Number),
      new StringFunctionDefinition("split", K.Tuple, K.String, K.String));
    foreach (var name in new[] {"contains", "startsWith", "endsWith", "matches"})
      definitions.Add(new StringFunctionDefinition(name, K.Bool, K.String, K.String));
    foreach (var name in new[] {"uppercase", "lowercase", "trim", "trimStart", "trimEnd"})
      definitions.Add(new StringFunctionDefinition(name, K.String, K.String));
    foreach (var name in new[] {"replaceAll", "replaceFirst", "replaceLast", "regexReplaceAll", "regexReplaceFirst"})
      definitions.Add(new StringFunctionDefinition(name, K.String, K.String, K.String, K.String));
    foreach (var kind in new[] {K.Tuple, K.Table}) {
      var key = kind == K.Tuple ? K.Number : K.String;
      definitions.Add(new CollectionFunctionDefinition("length", K.Number, kind),
        new CollectionFunctionDefinition("contains", K.Bool, kind, K.Any),
        new CollectionFunctionDefinition("has", K.Bool, kind, key),
        new CollectionFunctionDefinition("get", K.Any, kind, key),
        new CollectionFunctionDefinition("get", K.Any, kind, key, K.Any),
        new CollectionFunctionDefinition("map", kind, kind, K.Function),
        new CollectionFunctionDefinition("where", kind, kind, K.Function),
        new CollectionFunctionDefinition("any", K.Bool, kind, K.Function),
        new CollectionFunctionDefinition("all", K.Bool, kind, K.Function),
        new CollectionFunctionDefinition("reduce", K.Any, kind, K.Function, K.Any));
    }
    definitions.Add(new CollectionFunctionDefinition("indexOf", K.Number, K.Tuple, K.Any),
      new CollectionFunctionDefinition("push", K.Tuple, K.Tuple, K.Any), new CollectionFunctionDefinition("pop", K.Tuple, K.Tuple),
      new CollectionFunctionDefinition("keys", K.Tuple, K.Table), new CollectionFunctionDefinition("values", K.Tuple, K.Table),
      new CollectionFunctionDefinition("entries", K.Tuple, K.Table), new CollectionFunctionDefinition("remove", K.Table, K.Table, K.String),
      new CollectionFunctionDefinition("put", K.Table, K.Table, K.String, K.Any), new CollectionFunctionDefinition("put", K.Table, K.Table, K.Table),
      new CollectionFunctionDefinition("join", K.String, K.Tuple, K.String), new CollectionFunctionDefinition("join", K.String, K.Table, K.String, K.String));
  }
}
