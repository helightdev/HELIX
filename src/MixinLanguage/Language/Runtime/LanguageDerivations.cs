using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler;

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
    MixinDeclarationAst provider = null;
    try {
      for (; index < tuple.Values.Count; index++) {
        if (tuple.Values[index] is not MixinTableValue record)
          throw new Failure(context.Error("record must be a table"), line);
        var symbol = record.Select(context, context.ResolveString("symbol"));
        if (KindMixinValue.Of(symbol).Name != "symbol")
          throw new Failure(context.Error("record must contain a semantic symbol"), line);
        if (!record.Entries.Any(entry => entry.Key.Resolve(context.Strings) == "value"))
          throw new Failure(context.Error("record must contain value"), line);
        var current = record.Select(context, context.ResolveString("value"));
        foreach (var declaration in derivations) {
          provider = declaration;
          if (!activeDerivations.Add(provider))
            throw new Failure(context.Error("recursive derivation"), provider.Line);
          locals = new Dictionary<string, IMixinValue>(System.StringComparer.Ordinal);
          scope = derivationScopes[provider];
          try {
            foreach (var expression in provider.Declarations.OfType<ExpressionDeclarationAst>()) {
              parameter = Functions.CollectionFunctionDefinition.Put(context, record, "value", current);
              var returned = false;
              try { Block(expression.Body); }
              catch (Flow flow) when (flow.Kind == ControlFlowKind.Return) {
                current = flow.Value;
                returned = true;
              }
              if (!returned) throw new Failure(context.Error("derivation must explicitly return a value"), expression.Line);
              if (current is ErrorMixinValue) throw new Failure(current, expression.Line);
            }
          } finally { activeDerivations.Remove(provider); }
        }
        result.Add(Functions.CollectionFunctionDefinition.Put(context, record, "value", current));
        provider = null;
      }
      return new TupleMixinValue(result.ToArray());
    } catch (Failure failure) {
      Restore(variables, savedVariables);
      Restore(targetVariables, savedTargets);
      Restore(carries, savedCarries);
      outputs.RemoveRange(outputCount, outputs.Count - outputCount);
      return context.Error("derive entry " + index + (provider == null ? "" : " in '" + provider.Name + "'") +
        " at line " + failure.Line + ": " + Text(failure.Value));
    } catch (Flow flow) {
      Restore(variables, savedVariables);
      Restore(targetVariables, savedTargets);
      Restore(carries, savedCarries);
      outputs.RemoveRange(outputCount, outputs.Count - outputCount);
      return context.Error("control flow cannot cross derivation boundaries: " + flow.Kind);
    } finally {
      parameter = previousParameter;
      locals = previousLocals;
      scope = previousScope;
    }
  }
}
