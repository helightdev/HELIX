namespace HelixSourceGenerator.Language.Functions;

/// <summary>An executable boolean function in the unified function registry.</summary>
internal abstract class PredicateFunctionDefinition : FunctionDefinition {
  internal PredicateFunctionDefinition(string name, int arguments) : base(name, arguments, arguments) { }

  internal override bool IsPredicate => true;

  internal abstract bool Evaluate(
    IMixinValue value, FunctionInvocation invocation, out bool result, out string error
  );
}

internal sealed class ExistsPredicate : PredicateFunctionDefinition {
  internal ExistsPredicate() : base("exists", 0) { }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    result = value.Exists;
    error = null;
    return true;
  }
}

internal sealed class IsPredicate : PredicateFunctionDefinition {
  internal IsPredicate() : base("is", 1) { }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    if (string.IsNullOrWhiteSpace(invocation.Argument)) {
      result = false;
      error = ":?is requires a type";
      return false;
    }
    result = value.Is(invocation.Argument);
    error = null;
    return true;
  }
}

internal sealed class HasPredicate : PredicateFunctionDefinition {
  internal HasPredicate() : base("has", 1) { }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    if (string.IsNullOrWhiteSpace(invocation.Argument)) {
      result = false;
      error = ":?has requires a member";
      return false;
    }
    var property = (MixinExpressionProperty)invocation;
    result = value.Has(property.Values[0]);
    error = null;
    return true;
  }
}

internal sealed class EqualPredicate : PredicateFunctionDefinition {
  internal EqualPredicate() : base("eq", 1) { }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    var property = (MixinExpressionProperty)invocation;
    result = value.EqualsTo(property.Values[0]);
    error = null;
    return true;
  }
}

internal sealed class MatchesPredicate : PredicateFunctionDefinition {
  internal MatchesPredicate() : base("matches", 1) { }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    result = value.Matches(invocation.Argument, out error);
    return error is null;
  }
}

internal sealed class SignaturePredicate : PredicateFunctionDefinition {
  internal SignaturePredicate() : base("signature", 1) { }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    result = value.HasSameSignature(invocation.Argument);
    error = null;
    return true;
  }
}

internal sealed class WireablePredicate : PredicateFunctionDefinition {
  internal WireablePredicate() : base("wireable", 2) { }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    result = value.IsWireable(invocation.Arguments[0], invocation.Arguments[1]);
    error = null;
    return true;
  }
}

internal sealed class TraitPredicate : PredicateFunctionDefinition {
  private readonly MixinValueTrait _trait;

  internal TraitPredicate(string name, MixinValueTrait trait) : base(name, 0) {
    _trait = trait;
  }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    result = value.HasTrait(_trait);
    error = null;
    return true;
  }
}

internal enum PropStructPredicateKind { HasEquality, NoArguments, Augmenting }

internal sealed class PropStructPredicate : PredicateFunctionDefinition {
  private readonly PropStructPredicateKind _kind;

  internal PropStructPredicate(string name, PropStructPredicateKind kind) : base(name, 0) {
    _kind = kind;
  }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    if (value.Value is not MixinPropStructHandle propStruct) {
      result = false;
      error = ":" + Name + " must be called on a prop struct handle";
      return false;
    }
    result = _kind switch {
      PropStructPredicateKind.HasEquality => propStruct.Model.Equality.HasMembers,
      PropStructPredicateKind.NoArguments => propStruct.Model.ParameterParts.Count == 0,
      PropStructPredicateKind.Augmenting => propStruct.Augmenting,
      _ => false
    };
    error = null;
    return true;
  }
}