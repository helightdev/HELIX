using System;
using System.Collections.Generic;
using HelixSourceGenerator.Language.Functions;

namespace HelixSourceGenerator.Language;

public abstract class FunctionInvocation(string name, IReadOnlyList<string> arguments, bool negated) {
  public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
  public string Argument => Arguments.Count == 0 ? null : Arguments[0];
  public IReadOnlyList<string> Arguments { get; } = arguments ?? [];
  public bool Negated { get; } = negated;
}

internal abstract class FunctionDefinition(string name, int minimumArguments, int maximumArguments) {
  internal string Name { get; } = name;
  internal int MinimumArguments { get; } = minimumArguments;
  internal int MaximumArguments { get; } = maximumArguments;
  internal virtual bool IsPredicate => false;

  internal virtual void CollectConstants(MixinStringPoolBuilder pool) {
    pool.Intern(Name);
  }

  internal virtual bool Validate(FunctionInvocation invocation, out string error) {
    var count = invocation.Arguments.Count;
    if (count >= MinimumArguments && count <= MaximumArguments) {
      error = null;
      return true;
    }
    if (MinimumArguments == MaximumArguments) {
      error = ":" + Name + " requires " + MinimumArguments +
        (MinimumArguments == 1 ? " argument" : " arguments");
    } else {
      error = ":" + Name + " accepts at most " + MaximumArguments +
        (MaximumArguments == 1 ? " argument" : " arguments");
    }
    return false;
  }

  internal virtual bool Invoke(
    FunctionInvocation invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    error = "function ':" + Name + "' cannot be used as a value transformation";
    return false;
  }
}

internal static class FunctionLibrary {
  private static readonly IReadOnlyDictionary<string, FunctionDefinition> Definitions =
    new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal) {
      ["name"] = new NameFunction(), ["type"] = new TypeFunction(), ["fullName"] = new FullNameFunction(),
      ["makeGeneric"] = new MakeGenericFunction(), ["visibility"] = new VisibilityFunction(),
      ["path"] = new PathFunction(), ["unwrap"] = new UnwrapFunction(),
      ["replace"] = new RegexFunction("replace", false), ["replaceFirst"] = new RegexFunction("replaceFirst", true),
      ["switch"] = new SwitchFunction(), ["table"] = new AsTableFunction(),
      ["put"] = new TableFunction("put", 2, TableFunctionKind.Put), ["wire"] = new WireFunction(),
      ["remove"] = new TableFunction("remove", 1, TableFunctionKind.Remove),
      ["push"] = new TableFunction("push", 1, TableFunctionKind.Push),
      ["structParams"] = new PropStructFunction("structParams", 0, 1),
      ["structArgs"] = new PropStructFunction("structArgs", 0, 1),
      ["propStructCall"] = new PropStructFunction("propStructCall", 2, 2),
      ["pop"] = new TableFunction("pop", 0, TableFunctionKind.Pop), ["size"] = new SizeFunction(),
      ["floatTime"] = new FloatTimeFunction(), ["joinKeys"] = new TableJoinFunction("joinKeys", 1, TableJoinKind.Keys),
      ["joinValues"] = new TableJoinFunction("joinValues", 1, TableJoinKind.Values),
      ["join"] = new TableJoinFunction("join", 2, TableJoinKind.Entries),
      ["mapValues"] = new TableTransformFunction("mapValues", TableTransformKind.MapValues),
      ["map"] = new TableTransformFunction("map", TableTransformKind.Map),
      ["filter"] = new TableTransformFunction("filter", TableTransformKind.Filter),
      ["attributes"] = new AttributeFunction("attributes", 0, AttributeFunctionKind.All),
      ["attributesOf"] = new AttributeFunction("attributesOf", 1, AttributeFunctionKind.Assignable),
      ["attributesOfExact"] = new AttributeFunction("attributesOfExact", 1, AttributeFunctionKind.Exact),
      ["attributeOf"] = new AttributeFunction("attributeOf", 1, AttributeFunctionKind.First),

      // Predicates
      ["and"] = new LogicalFunctionDefinition("and"), ["or"] = new LogicalFunctionDefinition("or"),
      ["is"] = new IsPredicate(), ["has"] = new HasPredicate(), ["eq"] = new EqualPredicate(),
      ["matches"] = new MatchesPredicate(), ["signature"] = new SignaturePredicate(),
      ["wireable"] = new WireablePredicate(), ["exists"] = new ExistsPredicate(),
      ["isSelf"] = new TraitPredicate("isSelf", MixinValueTrait.Self),
      ["ref"] = new TraitPredicate("ref", MixinValueTrait.Ref), ["in"] = new TraitPredicate("in", MixinValueTrait.In),
      ["out"] = new TraitPredicate("out", MixinValueTrait.Out),
      ["inout"] = new TraitPredicate("inout", MixinValueTrait.InOut),
      ["argument"] = new TraitPredicate("argument", MixinValueTrait.Argument),
      ["static"] = new TraitPredicate("static", MixinValueTrait.Static),
      ["async"] = new TraitPredicate("async", MixinValueTrait.Async),
      ["public"] = new TraitPredicate("public", MixinValueTrait.Public),
      ["exposed"] = new TraitPredicate("exposed", MixinValueTrait.Exposed),
      ["top"] = new TraitPredicate("top", MixinValueTrait.Top),
      ["concrete"] = new TraitPredicate("concrete", MixinValueTrait.Concrete),
      ["partial"] = new TraitPredicate("partial", MixinValueTrait.Partial),
      ["generic"] = new TraitPredicate("generic", MixinValueTrait.Generic),
      ["struct"] = new TraitPredicate("struct", MixinValueTrait.Struct),
      ["class"] = new TraitPredicate("class", MixinValueTrait.Class),
      ["structHasEquality"] = new PropStructPredicate("structHasEquality", PropStructPredicateKind.HasEquality),
      ["structNoArgs"] = new PropStructPredicate("structNoArgs", PropStructPredicateKind.NoArguments),
      ["structAugment"] = new PropStructPredicate("structAugment", PropStructPredicateKind.Augmenting)
    };

  internal static bool TryGet(string name, out FunctionDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }

  internal static bool IsPredicate(string name) {
    return TryGet(name, out var definition) && definition.IsPredicate;
  }

  internal static void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var definition in Definitions.Values) definition.CollectConstants(pool);
  }

  internal static bool TryInvoke(
    MixinExpressionProperty invocation, IMixinExpressionContext context,
    string root, string member, ref object value, out string error
  ) {
    if (TryGet(invocation.Name, out var function))
      return function.Invoke(invocation, context, root, member, ref value, out error);
    error = "property '" + invocation.Name + "' is not valid for @" + root;
    return false;
  }
}