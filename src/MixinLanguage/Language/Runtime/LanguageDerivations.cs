using System.Collections.Generic;
using System.Linq;

namespace Mixins.Runtime;

internal sealed partial class LanguageExecution {
  internal IMixinValue Derive(IMixinValue value, int line) {
    if (value is not TupleMixinValue tuple) return context.Error("derive requires a tuple of records");
    var savedVariables = variables.ToArray();
    var savedTargets = targetVariables.ToArray();
    var savedCarries = carries.ToArray();
    var previousParameter = parameter;
    var previousLocals = locals;
    var previousScope = scope;
    var outputCount = outputs.Count;
    var result = new List<IMixinValue>(tuple.Values.Count);
    var index = 0;
    BytecodeDerivation provider = null;
    IMixinValue Fail(IMixinValue error, int errorLine, bool control = false) {
      Restore(variables, savedVariables);
      Restore(targetVariables, savedTargets);
      Restore(carries, savedCarries);
      outputs.RemoveRange(outputCount, outputs.Count - outputCount);
      return control ? error : context.Error("derive entry " + index + (provider == null ? "" : " in '" + provider.Name + "'") +
        " at line " + errorLine + ": " + Text(error));
    }
    try {
      for (; index < tuple.Values.Count; index++) {
        if (tuple.Values[index] is not MixinTableValue record)
          return Fail(context.Error("record must be a table"), line);
        var symbol = record.Select(context, context.ResolveString("symbol"));
        if (symbol.Kind != MixinValueKind.Symbol)
          return Fail(context.Error("record must contain a semantic symbol"), line);
        if (!record.Entries.Any(entry => entry.Key.Resolve(context.Strings) == "value"))
          return Fail(context.Error("record must contain value"), line);
        var current = record.Select(context, context.ResolveString("value"));
        foreach (var declaration in derivations) {
          provider = declaration;
          if (!activeDerivations.Add(provider))
            return Fail(context.Error("recursive derivation"), provider.Line);
          locals = new Dictionary<string, IMixinValue>(System.StringComparer.Ordinal);
          scope = provider.Scope;
          try {
            foreach (var expression in provider.Expressions) {
              parameter = Functions.CollectionFunctions.Put(context, record, "value", current);
              var completion = Run(expression.Body);
              if (completion.Kind == BytecodeFlow.Error) return Fail(completion.Value, completion.Line);
              if (completion.Kind == BytecodeFlow.Normal)
                return Fail(context.Error("derivation must explicitly return a value"), expression.Line);
              if (completion.Kind != BytecodeFlow.Return)
                return Fail(context.Error("control flow cannot cross derivation boundaries: " + completion.Kind), completion.Line, true);
              current = completion.Value;
              if (current is ErrorMixinValue) return Fail(current, expression.Line);
            }
          } finally { activeDerivations.Remove(provider); }
        }
        result.Add(Functions.CollectionFunctions.Put(context, record, "value", current));
        provider = null;
      }
      return new TupleMixinValue(result.ToArray());
    } finally {
      parameter = previousParameter;
      locals = previousLocals;
      scope = previousScope;
    }
  }
}
