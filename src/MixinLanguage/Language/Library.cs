using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler;
using Mixins.Env;
using Mixins.Functions;

namespace Mixins;

public static class FunctionLibrary {
  private static readonly FunctionSignatureRegistry Definitions = BuildDefinitions();

  private static FunctionSignatureRegistry BuildDefinitions() {
    var definitions = new FunctionSignatureRegistryBuilder();
    using var profile = MixinProfiler.Measure("static.function_library");
    Builtins.Register(definitions);
    definitions.Add(
      new NameFunction(), new TypeFunction(), new FullNameFunction(), new MembersFunction(),
      new ParametersFunction(), new NullableTypeFunction(), new CSharpLiteralFunction(),
      new MakeGenericFunction(), new VisibilityFunction(), new UnwrapFunction(),
      new IdentifierFunction(), new FloatTimeFunction(), new AttributesFunction(),
      new AttributesOfFunction(), new AttributesOfExactFunction(), new AttributeOfFunction(),
      new WireFunction(), new SignatureFunction(), new WireableFunction(), new IsTypeFunction(),
      new HasMemberFunction(), new TraitFunction("isSelf"), new TraitFunction("ref"), new TraitFunction("in"),
      new TraitFunction("out"), new TraitFunction("inout"), new TraitFunction("argument"),
      new TraitFunction("static"), new TraitFunction("async"), new TraitFunction("public"),
      new TraitFunction("exposed"), new TraitFunction("top"), new TraitFunction("concrete"),
      new TraitFunction("partial"), new TraitFunction("generic"), new TraitFunction("genericMethod"),
      new TraitFunction("struct"), new TraitFunction("class"), new TraitFunction("field"),
      new TraitFunction("property"), new TraitFunction("method"), new TraitFunction("event"),
      new TraitFunction("parameter"), new TraitFunction("typeSymbol"), new TraitFunction("referenceType"),
      new TraitFunction("valueType"), new TraitFunction("nullable"), new TraitFunction("pointer"),
      new TraitFunction("containsPointer"), new TraitFunction("enum"), new TraitFunction("primitive"),
      new TraitFunction("parameterDefault"), new TraitFunction("nonEmptyStringConstant"),
      new TraitFunction("equatableSelf"), new TraitFunction("typedEqualsSelf"),
      new TraitFunction("ordinaryTypedEqualsSelf"), new TraitFunction("objectEquals"),
      new TraitFunction("hashCode")
    );
    return definitions.Build(ValidateMetadata);
  }

  public static bool TryGet(string name, int argumentCount, out FunctionDefinition definition) =>
    Definitions.TryGet(name, argumentCount, out definition);

  public static bool TryResolve(string name, int argumentCount, out FunctionDefinition definition) =>
    Definitions.TryGet(name, argumentCount, out definition);

  public static IReadOnlyList<FunctionDefinition> Resolve(string name, int count) => Definitions.Resolve(name, count);

  public static IEnumerable<FunctionDefinition> Enumerate() => Definitions.Enumerate();

  private static void ValidateMetadata(IEnumerable<FunctionDefinition> definitions) {
    foreach (var definition in definitions) {
      if (definition.Signatures is null || definition.Signatures.Count == 0 ||
        definition.Signatures.Any(signature => signature.ArgumentTypes is null))
        throw new InvalidOperationException("Function ':" + definition.Name + "' must provide language metadata.");
    }
  }

  internal static void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var name in Definitions.Enumerate().Select(item => item.Name).Distinct()) pool.Intern(name);
  }
}

public static class MixinRootLibrary {
  private static readonly IReadOnlyList<MixinRootDefinition> Definitions = [
    new(
      Name: "target", Root: MixinExpressionRoot.Target, Kind: MixinValueKind.Symbol,
      Documentation: "The target symbol currently being generated."
    ),
    new(
      Name: "this", Root: MixinExpressionRoot.This, Kind: MixinValueKind.Symbol,
      Documentation: "The current declaring type or symbol."
    ),
    new(
      Name: "attr", Root: MixinExpressionRoot.Attribute, Kind: MixinValueKind.Symbol,
      Documentation: "The attribute driving the current mixin."
    ),
    new(
      Name: "table", Root: MixinExpressionRoot.Table, Kind: MixinValueKind.Kind,
      Documentation: "The table kind. Construct a table with table() or a table literal."
    ),
    new(
      Name: "var", Root: MixinExpressionRoot.Variable, Kind: MixinValueKind.Table,
      Documentation: "A named mixin variable."
    ),
    new(
      Name: "tar", Root: MixinExpressionRoot.TargetVariable, Kind: MixinValueKind.Table,
      Documentation: "A named variable stored on the current target."
    ),
    new(
      Name: "local", Root: MixinExpressionRoot.Local, Kind: MixinValueKind.Table,
      Documentation: "A compiler-generated local mixin value."
    ),
    new(
      Name: "param", Root: MixinExpressionRoot.Parameter, Kind: MixinValueKind.Any,
      Documentation: "A named parameter of the current function scope."
    ),
    new(
      Name: "carry", Root: MixinExpressionRoot.Carry, Kind: MixinValueKind.Table,
      Documentation: "A value carried into an expanded expression scope."
    )
  ];

  private static readonly IReadOnlyDictionary<string, MixinRootDefinition> ByName = Definitions.ToDictionary(
    item => item.Name, StringComparer.Ordinal
  );

  private static readonly IReadOnlyDictionary<MixinExpressionRoot, MixinRootDefinition> ByRoot =
    Definitions.ToDictionary(item => item.Root);

  public static bool TryGet(string name, out MixinRootDefinition definition) => ByName.TryGetValue(
    name ?? "",
    out definition
  );

  public static bool TryGet(MixinExpressionRoot root, out MixinRootDefinition definition) => ByRoot.TryGetValue(
    root,
    out definition
  );

  public static IEnumerable<MixinRootDefinition> Enumerate() => Definitions;
}
