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

  internal virtual void CollectConstants(MixinStringPoolBuilder pool) => pool.Intern(Name);

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
      ["path"] = new PathFunction(), ["unwrap"] = new UnwrapFunction(), ["replace"] = new ReplaceFunction(),
      ["replaceFirst"] = new ReplaceFirstFunction(), ["switch"] = new SwitchFunction(),
      ["table"] = new AsTableFunction(), ["put"] = new PutFunction(), ["wire"] = new WireFunction(),
      ["remove"] = new RemoveFunction(), ["push"] = new PushFunction(),
      ["structParams"] = new PropStructFunction("structParams", 0, 1),
      ["structArgs"] = new PropStructFunction("structArgs", 0, 1),
      ["propStructCall"] = new PropStructFunction("propStructCall", 2, 2), ["pop"] = new PopFunction(),
      ["size"] = new SizeFunction(), ["floatTime"] = new FloatTimeFunction(), ["joinKeys"] = new JoinKeysFunction(),
      ["joinValues"] = new JoinValuesFunction(), ["join"] = new JoinEntriesFunction(),
      ["mapValues"] = new MapValuesFunction(), ["map"] = new MapFunction(), ["filter"] = new FilterFunction(),
      ["attributes"] = new AttributesFunction(), ["attributesOf"] = new AttributesOfFunction(),
      ["attributesOfExact"] = new AttributesOfExactFunction(), ["attributeOf"] = new AttributeOfFunction(),

      // Predicates
      ["and"] = new LogicalFunctionDefinition("and"), ["or"] = new LogicalFunctionDefinition("or"),
      ["is"] = new IsPredicate(), ["has"] = new HasPredicate(), ["eq"] = new EqualPredicate(),
      ["matches"] = new MatchesPredicate(), ["signature"] = new SignaturePredicate(),
      ["wireable"] = new WireablePredicate(), ["exists"] = new ExistsPredicate(), ["isSelf"] = new SelfPredicate(),
      ["ref"] = new RefPredicate(), ["in"] = new InPredicate(), ["out"] = new OutPredicate(),
      ["inout"] = new InOutPredicate(), ["argument"] = new ArgumentPredicate(), ["static"] = new StaticPredicate(),
      ["async"] = new AsyncPredicate(), ["public"] = new PublicPredicate(), ["exposed"] = new ExposedPredicate(),
      ["top"] = new TopPredicate(), ["concrete"] = new ConcretePredicate(), ["partial"] = new PartialPredicate(),
      ["generic"] = new GenericPredicate(), ["struct"] = new StructPredicate(), ["class"] = new ClassPredicate(),
      ["structHasEquality"] = new StructHasEqualityPredicate(), ["structNoArgs"] = new StructNoArgsPredicate(),
      ["structAugment"] = new StructAugmentPredicate()
    };

  internal static bool TryGet(string name, out FunctionDefinition definition) =>
    Definitions.TryGetValue(name ?? "", out definition);

  internal static bool IsPredicate(string name) => TryGet(name, out var definition) && definition.IsPredicate;

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