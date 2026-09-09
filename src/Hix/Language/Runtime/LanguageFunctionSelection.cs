using Hix.Env;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Runtime;

internal sealed partial class LanguageExecution {
  private readonly record struct FunctionMatch(BytecodeFunction Function, BytecodeSignature Signature,
    IHixValue Parameter, LanguageFunctionScope Owner);

  // Only declaration metadata is reused. Conversion success can depend on argument values,
  // so a previous winner is never cached by argument kind alone.
  private IHixValue SelectFunction(string name, IHixValue[] arguments, LanguageFunctionScope binding,
    out FunctionMatch match) {
    using var profile = HixProfiler.Measure("vm.select_function");
    match = default;
    var candidates = binding.Candidates(name);
    if (candidates.Count == 0) return context.Error("unknown function '" + name + "'");
    LanguageFunctionCandidate best = null;
    IHixValue[] bestArguments = null;
    IHixValue bestParameter = null;
    var bestScore = int.MinValue;
    var ambiguous = false;
    Dictionary<string, int> namedIndices = null;

    foreach (var candidate in candidates) {
      // Conversion penalties can only reduce this score; equal scores must still
      // be examined to preserve ambiguity detection.
      if (candidate.BaseScore < bestScore) continue;
      var signature = candidate.Signature;
      var score = candidate.BaseScore;
      var convertedArguments = arguments;
      IHixValue suppliedParameter = null;
      if (signature is {Inputs: null}) {
        if (arguments.Length != 1 || !TryConvert(arguments[0], signature.InputKind, out suppliedParameter, out var count)) continue;
        score -= count;
      } else if (signature != null) {
        var fields = signature.Inputs;
        var namedMatch = false;
        if (arguments.Length == 1 && arguments[0] is HixTableValue supplied && !candidate.Variadic) {
          if (namedIndices == null) {
            namedIndices = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < supplied.Entries.Count; i++) {
              var key = supplied.Entries[i].Key.Resolve(context.Strings);
              if (!namedIndices.ContainsKey(key)) namedIndices.Add(key, i);
            }
          }
          KeyValuePair<HixString, IHixValue>[] convertedEntries = null;
          namedMatch = true;
          foreach (var field in fields) {
            if (!namedIndices.TryGetValue(field.Name, out var index) ||
                !TryConvert(convertedEntries == null ? supplied.Entries[index].Value : convertedEntries[index].Value,
                  field.Kind, out var converted, out var count)) { namedMatch = false; break; }
            if (count != 0) {
              convertedEntries ??= supplied.Entries.ToArray();
              convertedEntries[index] = new(convertedEntries[index].Key, converted);
              score -= count;
            }
          }
          if (namedMatch) suppliedParameter = convertedEntries == null ? supplied : new HixTableValue(convertedEntries);
        }
        if (!namedMatch) {
          if (arguments.Length < candidate.FixedCount || !candidate.Variadic && arguments.Length != candidate.FixedCount) continue;
          score = candidate.BaseScore;
          var valid = true;
          for (var i = 0; i < arguments.Length; i++) {
            if (!TryConvert(arguments[i], fields[Math.Min(i, fields.Count - 1)].Kind, out var converted, out var count)) {
              valid = false; break;
            }
            if (count != 0) {
              if (ReferenceEquals(convertedArguments, arguments)) convertedArguments = (IHixValue[])arguments.Clone();
              convertedArguments[i] = converted;
              score -= count;
            }
          }
          if (!valid) continue;
        }
      }
      if (score < bestScore) continue;
      if (score == bestScore) { ambiguous = true; continue; }
      best = candidate;
      bestScore = score;
      bestArguments = convertedArguments;
      bestParameter = suppliedParameter;
      ambiguous = false;
    }
    if (best == null || ambiguous) return context.Error(best == null
      ? "no matching signature for '" + name + "'" : "ambiguous signature for '" + name + "'");

    // Allocate the parameter container only for the selected signature.
    if (bestParameter == null) {
      if (best.Signature == null) bestParameter = Pack(arguments);
      else {
        var fields = best.Signature.Inputs;
        var entries = new KeyValuePair<HixString, IHixValue>[fields.Count];
        for (var i = 0; i < fields.Count; i++) {
          IHixValue value;
          if (fields[i].Variadic) {
            var tail = new IHixValue[bestArguments.Length - i];
            Array.Copy(bestArguments, i, tail, 0, tail.Length);
            value = new TupleHixValue(tail);
          } else value = bestArguments[i];
          entries[i] = new(context.ResolveString(fields[i].Name), value);
        }
        bestParameter = new HixTableValue(entries);
      }
    }
    match = new(best.Function, best.Signature, bestParameter, best.Owner);
    return null;
  }
}
