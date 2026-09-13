using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Compiler;

public sealed record HixCallResolution(SignatureHixPattern Signature,
  IReadOnlyDictionary<HixTypeParameter, HixPattern> Substitutions, ExpressionIr[] Arguments,
  HixCallBinding Binding, int Score, bool Generic) {
  public string DeclarationSignature { get; init; }
}

/// <summary>Shared compiler/analyzer binding and generic inference. Runtime receives erased signatures.</summary>
public static class HixCallResolver {
  public static HixCallResolution Resolve(CallExpressionIr call,
    IReadOnlyDictionary<string, FunctionDeclarationIr[]> functions, HixBackend backend,
    IReadOnlyDictionary<string, HixPattern> patterns, Func<ExpressionIr, HixPattern> infer,
    out bool ambiguous) {
    if (call.UsingImports.Count != 0) {
      foreach (var name in HixUsingImports.Candidates(call.OriginalName, call.UsingImports).Distinct()) {
        var qualified = new CallExpressionIr(name, call.Arguments, call.CoerceBoolean,
          argumentNames: call.ArgumentNames) { TypeArguments = call.TypeArguments };
        var resolution = Resolve(qualified, functions, backend, patterns, infer, out ambiguous);
        if (resolution != null || ambiguous) { call.Name = name; return resolution; }
        if (functions.TryGetValue(name, out var dynamicFunctions) && dynamicFunctions.Any(function =>
            function.Signatures.Count == 0 || function.Signatures.Any(signature => signature.Inputs == null && signature.InputPattern == null))) {
          call.Name = name;
          return null;
        }
      }
      ambiguous = false;
      return null;
    }
    var candidates = new List<HixCallResolution>();
    var languageSignatures = new HashSet<string>(StringComparer.Ordinal);
    if (functions.TryGetValue(call.Name, out var declarations)) {
      foreach (var declaration in declarations)
        foreach (var signature in declaration.Signatures) {
          if (signature.Inputs == null && signature.InputPattern == null) continue;
          var key = HixGenericTypes.SignatureIdentity(signature.Constant(call.Name) with { Result = HixPattern.Any });
          if (!languageSignatures.Add(key)) continue;
          var fields = signature.Constant(call.Name).Parameters;
          var defaults = signature.Inputs?.Select(field => field.DefaultValue ??
            (field.Optional ? new MissingExpressionIr() : null)).ToArray();
          Add(signature.Constant(call.Name), declaration.TypeParameters.Concat(signature.TypeParameters).Distinct().ToArray(), defaults,
            signature.Inputs?.Any(field => field.Variadic) == true, declaration, null, null, null);
        }
    } else foreach (var definition in backend.Functions.Resolve(call.Name))
      foreach (var signature in definition.Signatures)
        Add(new SignatureHixPattern(call.Name, signature.ArgumentPatterns.Select((pattern, index) =>
          new HixPatternField(signature.GetArgumentName(index) ?? index.ToString(), pattern)).ToArray(), signature.ResultPattern),
          signature.TypeParameters, signature.ArgumentDefaults, signature.IsVariadic, null, definition, signature.InferResult,
          signature.CallableArguments);
    ambiguous = false;
    if (candidates.Count == 0) return null;
    var ordered = candidates.OrderByDescending(candidate => candidate.Score).ThenBy(candidate => candidate.Generic).ToArray();
    var best = ordered[0];
    ambiguous = ordered.Skip(1).Any(candidate => candidate.Score == best.Score && candidate.Generic == best.Generic);
    if (ambiguous && best.Score < best.Arguments.Length) {
      ambiguous = false;
      return best with { Binding = HixCallBinding.Dynamic, Signature = best.Signature with { Result = HixPattern.Any } };
    }
    if (declarations != null && declarations.Any(declaration => declaration.Signatures.Count == 0 ||
        declaration.Signatures.Any(signature => signature.Inputs == null && signature.InputPattern == null)) &&
        best.Score < best.Arguments.Length)
      return best with { Binding = HixCallBinding.Dynamic };
    return ambiguous ? null : best;

    void Add(SignatureHixPattern signature, IReadOnlyList<HixTypeParameter> parameters,
      IReadOnlyList<ExpressionIr> defaults, bool variadic, FunctionDeclarationIr language,
      HixFunctionEntry host, Func<IReadOnlyList<HixPattern>, HixPattern> inferResult,
      IReadOnlyDictionary<int, int> callableArguments) {
      if (!Arrange(call, signature.Parameters, defaults, variadic, out var arguments)) return;
      if (call.TypeArguments.Count != 0 && call.TypeArguments.Count != parameters.Count) return;
      var substitutions = new Dictionary<HixTypeParameter, HixPattern>();
      for (var index = 0; index < call.TypeArguments.Count; index++) substitutions[parameters[index]] = call.TypeArguments[index];
      var supplied = arguments.Select(infer).ToArray();
      // Infer data arguments first so callable parameters receive their concrete context.
      for (var index = 0; index < supplied.Length; index++) {
        var expected = ResolvePattern(signature.Parameters[Math.Min(index, signature.Parameters.Count - 1)].Pattern, patterns);
        if (expected is not DelegateHixPattern && !Unify(expected, supplied[index], substitutions, patterns)) return;
      }
      for (var index = 0; index < arguments.Length; index++) {
        var expected = ResolvePattern(signature.Parameters[Math.Min(index, signature.Parameters.Count - 1)].Pattern, patterns);
        if (expected is not DelegateHixPattern callable) continue;
        var context = HixGenericTypes.Substitute(callable, substitutions, true) as DelegateHixPattern;
        TupleHixPattern invocation = null;
        if (callableArguments != null && callableArguments.TryGetValue(index, out var tupleIndex)) {
          invocation = supplied[tupleIndex] as TupleHixPattern;
          if (invocation != null) context = new DelegateHixPattern(invocation.Fields, context.Result);
        }
        supplied[index] = PatternTypeAnalysis.InferCallable(arguments[index], context, functions, patterns, backend,
          out var validCallable, invocationContext: invocation != null) ?? supplied[index];
        if (!validCallable) return;
        if (invocation != null && supplied[index] is DelegateHixPattern actualCallable &&
            (actualCallable.Parameters.Count != invocation.Fields.Count || actualCallable.Parameters.Select((field, position) =>
              HixPatternRelations.Relate(invocation.Fields[position].Pattern, field.Pattern, patterns)).Any(relation => relation == HixPatternRelation.Never))) return;
      }
      for (var index = 0; index < supplied.Length; index++) {
        var expected = signature.Parameters[Math.Min(index, signature.Parameters.Count - 1)].Pattern;
        if (!Unify(expected, supplied[index], substitutions, patterns)) return;
      }
      foreach (var parameter in parameters)
        if (substitutions.TryGetValue(parameter, out var value) &&
            !HixGenericTypes.SatisfiesBounds(value, parameter, substitutions, patterns)) return;
      var instantiated = new SignatureHixPattern(signature.Name, signature.Parameters.Select((field, fieldIndex) => field with {
        Pattern = HixGenericTypes.Substitute(field.Pattern, substitutions, true) }).ToArray(),
        HixGenericTypes.Substitute(signature.Result, substitutions, true));
      if (inferResult != null) instantiated = instantiated with { Result = inferResult(supplied) };
      var relations = supplied.Select((value, index) => HixPatternRelations.Relate(value,
        instantiated.Parameters[Math.Min(index, instantiated.Parameters.Count - 1)].Pattern, patterns)).ToArray();
      if (relations.Any(relation => relation == HixPatternRelation.Never)) return;
      var erased = new SignatureHixPattern(signature.Name, signature.Parameters.Select((field, fieldIndex) => field with {
        Pattern = host != null ? Kind(HixGenericTypes.Erase(field.Pattern)) : HixGenericTypes.ErasePattern(field.Pattern),
        DefaultValue = host != null && defaults != null && fieldIndex < defaults.Count ?
          ConstantData(defaults[fieldIndex]) : field.DefaultValue,
        HasDefault = host == null ? field.HasDefault : defaults != null && fieldIndex < defaults.Count &&
          defaults[fieldIndex] != null
      }).ToArray(), host != null ? Kind(HixGenericTypes.Erase(signature.Result)) : HixGenericTypes.ErasePattern(signature.Result));
      var binding = language != null ? HixCallBinding.Language(erased, language) : HixCallBinding.Host(erased, host);
      candidates.Add(new(instantiated, substitutions, arguments, binding,
        relations.Count(relation => relation == HixPatternRelation.Always), parameters.Count != 0) {
        DeclarationSignature = parameters.Count != 0 || signature.Parameters.Any(field => HixGenericTypes.RequiresErasure(field.Pattern)) ||
          HixGenericTypes.RequiresErasure(signature.Result) ? HixGenericTypes.SignatureIdentity(signature) : null
      });
    }
  }

  private static HixPattern Kind(HixValueKind kind) => kind == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(kind);

  private static HixPattern ResolvePattern(HixPattern pattern, IReadOnlyDictionary<string, HixPattern> definitions) {
    if (pattern is not NamedHixPattern) return pattern;
    var active = new HashSet<string>();
    while (pattern is NamedHixPattern named && active.Add(named.Name) && definitions.TryGetValue(named.Name, out var resolved))
      pattern = resolved;
    return pattern;
  }

  public static bool Unify(HixPattern expected, HixPattern actual,
    IDictionary<HixTypeParameter, HixPattern> substitutions, IReadOnlyDictionary<string, HixPattern> patterns) {
    expected = ResolvePattern(expected, patterns);
    actual = ResolvePattern(actual, patterns);
    if (actual is AnyHixPattern) return true;
    if (expected is TypeParameterHixPattern parameter) {
      if (!substitutions.TryGetValue(parameter.Parameter, out var existing)) {
        substitutions[parameter.Parameter] = actual; return true;
      }
      return HixGenericTypes.Equivalent(existing, actual);
    }
    if (expected is OpaqueHixPattern opaque && actual is OpaqueHixPattern supplied)
      return opaque.Definition.Identity == supplied.Definition.Identity && opaque.Arguments.Count == supplied.Arguments.Count &&
        opaque.Arguments.Zip(supplied.Arguments, (a, b) => Unify(a, b, substitutions, patterns)).All(value => value);
    if (expected is ManyHixPattern many && actual is TupleHixPattern tuple)
      return tuple.Fields.All(field => Unify(many.Element, field.Pattern, substitutions, patterns));
    if (expected is TupleHixPattern expectedTuple && actual is TupleHixPattern actualTuple)
      return expectedTuple.Fields.Count == actualTuple.Fields.Count && expectedTuple.Fields.Zip(actualTuple.Fields,
        (a, b) => Unify(a.Pattern, b.Pattern, substitutions, patterns)).All(value => value);
    if (expected is TableHixPattern expectedTable && actual is TableHixPattern actualTable)
      return expectedTable.Fields.All(field => actualTable.Fields.FirstOrDefault(value => value.Name == field.Name) is { } found ?
        Unify(field.Pattern, found.Pattern, substitutions, patterns) : field.AllowsMissing);
    if (expected is MapHixPattern map && actual is MapHixPattern actualMap)
      return Unify(map.Key, actualMap.Key, substitutions, patterns) && Unify(map.Value, actualMap.Value, substitutions, patterns);
    if (expected is DelegateHixPattern callable && actual is DelegateHixPattern actualCallable)
      return (callable.Parameters.Count == 0 || callable.Parameters.Count == actualCallable.Parameters.Count &&
        callable.Parameters.Zip(actualCallable.Parameters, (a, b) => Unify(a.Pattern, b.Pattern, substitutions, patterns)).All(value => value)) &&
        Unify(callable.Result, actualCallable.Result, substitutions, patterns);
    if (expected is UnionHixPattern union) {
      foreach (var member in union.Patterns) {
        var trial = new Dictionary<HixTypeParameter, HixPattern>(substitutions);
        if (!Unify(member, actual, trial, patterns)) continue;
        foreach (var pair in trial) substitutions[pair.Key] = pair.Value;
        return true;
      }
      return false;
    }
    return HixPatternRelations.Relate(actual, expected, patterns) != HixPatternRelation.Never;
  }

  private static object ConstantData(ExpressionIr value) => value switch {
    null or NullExpressionIr => null,
    MissingExpressionIr => MissingHixValue.Instance,
    StringExpressionIr text => text.Value,
    NumberExpressionIr number => number.Value,
    BooleanExpressionIr boolean => boolean.Value,
    TupleExpressionIr tuple => tuple.Values.Select(ConstantData).ToArray(),
    TableExpressionIr table => table.Entries.ToDictionary(entry => entry.Key, entry => ConstantData(entry.Value)),
    _ => throw new ArgumentException("parameter defaults must be constant values")
  };

  public static bool Arrange(CallExpressionIr call, IReadOnlyList<HixPatternField> fields,
    IReadOnlyList<ExpressionIr> defaults, bool variadic, out ExpressionIr[] arguments) {
    arguments = null;
    if (variadic) {
      if (call.ArgumentNames.Any(name => name != null) || call.Arguments.Count < fields.Count - 1) return false;
      arguments = call.Arguments.ToArray(); return true;
    }
    if (call.Arguments.Count > fields.Count) return false;
    var values = new ExpressionIr[fields.Count];
    var positional = 0; var named = false;
    for (var index = 0; index < call.Arguments.Count; index++) {
      var name = call.ArgumentNames[index];
      var target = name == null ? positional++ : fields.ToList().FindIndex(field => field.Name == name);
      if (name == null && named || target < 0 || target >= fields.Count || values[target] != null) return false;
      named |= name != null;
      values[target] = call.Arguments[index];
    }
    for (var index = 0; index < values.Length; index++) {
      if (values[index] != null) continue;
      if (defaults == null || index >= defaults.Count || defaults[index] == null) return false;
      values[index] = new HixIrRewriter().Rewrite(defaults[index]);
    }
    arguments = values; return true;
  }
}
