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

internal abstract class TraitPredicate(string name) : PredicateFunctionDefinition(name, 0) {
  protected abstract MixinValueTrait Trait { get; }

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    result = value.HasTrait(Trait);
    error = null;
    return true;
  }
}

internal sealed class SelfPredicate() : TraitPredicate("isSelf") { protected override MixinValueTrait Trait => MixinValueTrait.Self; }
internal sealed class RefPredicate() : TraitPredicate("ref") { protected override MixinValueTrait Trait => MixinValueTrait.Ref; }
internal sealed class InPredicate() : TraitPredicate("in") { protected override MixinValueTrait Trait => MixinValueTrait.In; }
internal sealed class OutPredicate() : TraitPredicate("out") { protected override MixinValueTrait Trait => MixinValueTrait.Out; }
internal sealed class InOutPredicate() : TraitPredicate("inout") { protected override MixinValueTrait Trait => MixinValueTrait.InOut; }
internal sealed class ArgumentPredicate() : TraitPredicate("argument") { protected override MixinValueTrait Trait => MixinValueTrait.Argument; }
internal sealed class StaticPredicate() : TraitPredicate("static") { protected override MixinValueTrait Trait => MixinValueTrait.Static; }
internal sealed class AsyncPredicate() : TraitPredicate("async") { protected override MixinValueTrait Trait => MixinValueTrait.Async; }
internal sealed class PublicPredicate() : TraitPredicate("public") { protected override MixinValueTrait Trait => MixinValueTrait.Public; }
internal sealed class ExposedPredicate() : TraitPredicate("exposed") { protected override MixinValueTrait Trait => MixinValueTrait.Exposed; }
internal sealed class TopPredicate() : TraitPredicate("top") { protected override MixinValueTrait Trait => MixinValueTrait.Top; }
internal sealed class ConcretePredicate() : TraitPredicate("concrete") { protected override MixinValueTrait Trait => MixinValueTrait.Concrete; }
internal sealed class PartialPredicate() : TraitPredicate("partial") { protected override MixinValueTrait Trait => MixinValueTrait.Partial; }
internal sealed class GenericPredicate() : TraitPredicate("generic") { protected override MixinValueTrait Trait => MixinValueTrait.Generic; }
internal sealed class StructPredicate() : TraitPredicate("struct") { protected override MixinValueTrait Trait => MixinValueTrait.Struct; }
internal sealed class ClassPredicate() : TraitPredicate("class") { protected override MixinValueTrait Trait => MixinValueTrait.Class; }

internal abstract class PropStructPredicate(string name) : PredicateFunctionDefinition(name, 0) {

  internal override bool Evaluate(IMixinValue value, FunctionInvocation invocation, out bool result, out string error) {
    if (value.Value is not MixinPropStructHandle propStruct) {
      result = false;
      error = ":" + Name + " must be called on a prop struct handle";
      return false;
    }
    result = Evaluate(propStruct);
    error = null;
    return true;
  }

  protected abstract bool Evaluate(MixinPropStructHandle propStruct);
}

internal sealed class StructHasEqualityPredicate() : PropStructPredicate("structHasEquality") {
  protected override bool Evaluate(MixinPropStructHandle propStruct) => propStruct.Model.Equality.HasMembers;
}

internal sealed class StructNoArgsPredicate() : PropStructPredicate("structNoArgs") {
  protected override bool Evaluate(MixinPropStructHandle propStruct) => propStruct.Model.ParameterParts.Count == 0;
}

internal sealed class StructAugmentPredicate() : PropStructPredicate("structAugment") {
  protected override bool Evaluate(MixinPropStructHandle propStruct) => propStruct.Augmenting;
}
