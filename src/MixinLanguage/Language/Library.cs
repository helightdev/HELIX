using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler;
using Mixins.Env;
using Mixins.Functions;

namespace Mixins;

public static class FunctionLibrary {
  private static readonly IReadOnlyDictionary<string, FunctionDefinition> Definitions =
    CreateDefinitions();

  private static IReadOnlyDictionary<string, FunctionDefinition> CreateDefinitions() {
    using var profile = MixinProfiler.Measure("static.function_library");
    var definitions = new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal) {
      ["name"] = new NameFunction(), ["type"] = new TypeFunction(), ["fullName"] = new FullNameFunction(),
      ["members"] = new MembersFunction(), ["parameters"] = new ParametersFunction(),
      ["nullableType"] = new NullableTypeFunction(), ["csharpLiteral"] = new CSharpLiteralFunction(),
      ["makeGeneric"] = new MakeGenericFunction(), ["visibility"] = new VisibilityFunction(),
      ["path"] = new PathFunction(), ["unwrap"] = new UnwrapFunction(), ["switch"] = new SwitchFunction(),
      ["size"] = new SizeFunction(), ["replace"] = new ReplaceFunction(), ["replaceFirst"] = new ReplaceFirstFunction(),
      ["format/1"] = new FormatFunction(1), ["format/2"] = new FormatFunction(2),
      ["identifier"] = new IdentifierFunction(), ["floatTime"] = new FloatTimeFunction(),
      ["table"] = new AsTableFunction(), ["put"] = new PutFunction(), ["remove"] = new RemoveFunction(),
      ["push"] = new PushFunction(), ["pop"] = new PopFunction(), ["joinKeys"] = new JoinKeysFunction(),
      ["joinValues"] = new JoinValuesFunction(), ["join"] = new JoinEntriesFunction(),
      ["mapValues"] = new MapValuesFunction(), ["map"] = new MapFunction(), ["filter"] = new FilterFunction(),
      ["reduce"] = new ReduceFunction(), ["attributes"] = new AttributesFunction(), ["derive"] = new DeriveFunction(),
      ["attributesOf"] = new AttributesOfFunction(), ["attributesOfExact"] = new AttributesOfExactFunction(),
      ["attributeOf"] = new AttributeOfFunction(), ["wire"] = new WireFunction(),
      ["signature"] = new SignaturePredicate(), ["wireable"] = new WireablePredicate(),
      ["exists"] = new ExistsPredicate(), ["and"] = new AndPredicate(), ["or"] = new OrPredicate(),
      ["is"] = new IsPredicate(), ["has"] = new HasPredicate(), ["eq"] = new EqualPredicate(),
      ["matches"] = new MatchesPredicate(), ["isSelf"] = new TraitPredicate("isSelf"),
      ["ref"] = new TraitPredicate("ref"), ["in"] = new TraitPredicate("in"), ["out"] = new TraitPredicate("out"),
      ["inout"] = new TraitPredicate("inout"), ["argument"] = new TraitPredicate("argument"),
      ["static"] = new TraitPredicate("static"), ["async"] = new TraitPredicate("async"),
      ["public"] = new TraitPredicate("public"), ["exposed"] = new TraitPredicate("exposed"),
      ["top"] = new TraitPredicate("top"), ["concrete"] = new TraitPredicate("concrete"),
      ["partial"] = new TraitPredicate("partial"), ["generic"] = new TraitPredicate("generic"),
      ["genericMethod"] = new TraitPredicate("genericMethod"), ["struct"] = new TraitPredicate("struct"),
      ["class"] = new TraitPredicate("class"), ["field"] = new TraitPredicate("field"),
      ["property"] = new TraitPredicate("property"), ["method"] = new TraitPredicate("method"),
      ["event"] = new TraitPredicate("event"), ["parameter"] = new TraitPredicate("parameter"),
      ["typeSymbol"] = new TraitPredicate("typeSymbol"), ["referenceType"] = new TraitPredicate("referenceType"),
      ["valueType"] = new TraitPredicate("valueType"), ["nullable"] = new TraitPredicate("nullable"),
      ["pointer"] = new TraitPredicate("pointer"), ["containsPointer"] = new TraitPredicate("containsPointer"),
      ["enum"] = new TraitPredicate("enum"), ["primitive"] = new TraitPredicate("primitive"),
      ["parameterDefault"] = new TraitPredicate("parameterDefault"),
      ["nonEmptyStringConstant"] = new TraitPredicate("nonEmptyStringConstant"),
      ["equatableSelf"] = new TraitPredicate("equatableSelf"),
      ["typedEqualsSelf"] = new TraitPredicate("typedEqualsSelf"),
      ["ordinaryTypedEqualsSelf"] = new TraitPredicate("ordinaryTypedEqualsSelf"),
      ["objectEquals"] = new TraitPredicate("objectEquals"), ["hashCode"] = new TraitPredicate("hashCode")
    };
    foreach (var directive in DirectiveLibrary.Enumerate().Where(item => item.Function is not null))
      definitions.Add(directive.Function.Name, directive.Function);
    definitions["typeSymbol"].WithPredicateAlias("type");
    ValidateMetadata(definitions.Values);
    return definitions;
  }

  public static bool TryGet(string name, int argumentCount, out FunctionDefinition definition) {
    definition = Definitions.Values.FirstOrDefault(item =>
      string.Equals(item.Name, name, StringComparison.Ordinal) && item.MatchesArgumentCount(argumentCount)
    );
    return definition is not null;
  }

  public static bool TryResolve(string name, bool predicate, int argumentCount, out FunctionDefinition definition) {
    definition = Definitions.Values.FirstOrDefault(item => item.MatchesArgumentCount(argumentCount) &&
      (predicate ? item.MatchesPredicate(name) : string.Equals(item.Name, name, StringComparison.Ordinal))
    );
    return definition is not null;
  }

  public static MixinLanguageValueKind GetArgumentType(string name, bool predicate, int index) {
    var candidates = Definitions.Values.Where(item =>
      predicate ? item.MatchesPredicate(name) : string.Equals(item.Name, name, StringComparison.Ordinal)
    ).Select(item => item.GetArgumentType(index)).Distinct().ToArray();
    return candidates.Length == 1 ? candidates[0] : MixinLanguageValueKind.Any;
  }

  public static bool IsPredicate(string name) {
    return Definitions.Values.Any(item => item.IsPredicate &&
      string.Equals(item.Name, name, StringComparison.Ordinal)
    );
  }

  public static IEnumerable<FunctionDefinition> Enumerate() => Definitions.Values;

  private static void ValidateMetadata(IEnumerable<FunctionDefinition> definitions) {
    foreach (var definition in definitions) {
      if (definition.ArgumentTypes is null)
        throw new InvalidOperationException("Function ':" + definition.Name + "' must provide language metadata.");
    }
  }

  internal static void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var name in Definitions.Values.Select(item => item.Name).Distinct()) pool.Intern(name);
  }
}

public static class DirectiveLibrary {
  private static readonly IReadOnlyDictionary<string, DirectiveDefinition> Definitions = CreateDefinitions();

  private static IReadOnlyDictionary<string, DirectiveDefinition> CreateDefinitions() {
    using var profile = MixinProfiler.Measure("static.directive_library");
    var definitions = new Dictionary<string, DirectiveDefinition>(StringComparer.Ordinal) {
      ["RESOLVE_MIXIN"] = FunctionDirective(
        new ResolveMixinDirectiveFunction(), [Arg(MixinLanguageValueKind.Text)], 0,
        "Resolves and applies a named mixin."
      ),
      ["PUSH"] = FunctionDirective(
        new PushDirectiveFunction(),
        [Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Local, MixinSymbolUsage.Reference)],
        documentation: "Appends the operand to a local table."
      ),
      ["PUT"] = FunctionDirective(
        new PutDirectiveFunction(),
        [
          Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Local, MixinSymbolUsage.Reference),
          Arg(MixinLanguageValueKind.Any)
        ], documentation: "Stores the operand under a key in a local table."
      ),
      ["SCOPE/0"] = Define("SCOPE", 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.Scope), ["SCOPE/1"] =
        Define(
          "SCOPE", 1, DirectiveOperandKind.None,
          [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Declaration)],
          MixinDirectiveSyntaxForm.Scope
        ),
      ["LABEL"] = Define(
        "LABEL", 1, DirectiveOperandKind.None,
        [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Declaration)],
        MixinDirectiveSyntaxForm.Label
      ),
      ["FUNC"] = Define(
        "FUNC", 1, DirectiveOperandKind.None,
        [Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Declaration)],
        MixinDirectiveSyntaxForm.Function
      ),
      ["CALL/1"] = Define(
        "CALL", 1, DirectiveOperandKind.Value,
        [Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Reference)],
        MixinDirectiveSyntaxForm.Call
      ),
      ["CALL/2"] = Define(
        "CALL", 2, DirectiveOperandKind.Value,
        [
          Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Local, MixinSymbolUsage.Declaration),
          Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Reference)
        ], MixinDirectiveSyntaxForm.Call
      ),
      ["INLINE"] = Define(
        "INLINE", 1, DirectiveOperandKind.None,
        [Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Reference)],
        MixinDirectiveSyntaxForm.Inline
      ),
      ["END"] = Define("END", 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.End),
      ["MATCH/0"] = Define("MATCH", 0, DirectiveOperandKind.Boolean, [], MixinDirectiveSyntaxForm.Match), ["MATCH/1"] =
        Define(
          "MATCH", 1, DirectiveOperandKind.Boolean,
          [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Reference)],
          MixinDirectiveSyntaxForm.Match
        ),
      ["ASSERT"] = Define("ASSERT", 0, DirectiveOperandKind.Boolean, [], MixinDirectiveSyntaxForm.Assert),
      ["CODE/0"] = Define("CODE", 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Code), ["CODE/1"] =
        Define(
          "CODE", 1, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.OutputTarget)],
          MixinDirectiveSyntaxForm.Code
        ),
      ["MIXIN/1"] = Define(
        "MIXIN", 1, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.Any)],
        MixinDirectiveSyntaxForm.Mixin
      ),
      ["MIXIN/2"] = Define(
        "MIXIN", 2, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.Any), Arg(MixinLanguageValueKind.Any)],
        MixinDirectiveSyntaxForm.Mixin
      ),
      ["USING"] = Define("USING", 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Using),
      ["LOG"] = Define("LOG", 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Log),
      ["LOCAL"] = NamedValue("LOCAL", MixinSymbolKind.Local, MixinDirectiveSyntaxForm.Local),
      ["VAR"] = NamedValue("VAR", MixinSymbolKind.Variable, MixinDirectiveSyntaxForm.Variable),
      ["TAR"] = NamedValue("TAR", MixinSymbolKind.TargetVariable, MixinDirectiveSyntaxForm.TargetVariable),
      ["CARRY"] = NamedValue("CARRY", MixinSymbolKind.Carry, MixinDirectiveSyntaxForm.Carry),
      ["RETURN"] = Define("RETURN", 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Return), ["GOTO"] =
        Define(
          "GOTO", 1, DirectiveOperandKind.None,
          [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Reference)],
          MixinDirectiveSyntaxForm.Goto
        ),
      ["SKIP"] = Define("SKIP", 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.Skip),
      ["FAIL"] = Define("FAIL", 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Fail), ["ANNOTATION"] =
        Define(
          "ANNOTATION", 1, DirectiveOperandKind.None,
          [
            Arg(
              MixinLanguageValueKind.CSharpType, MixinSymbolKind.Annotation,
              MixinSymbolUsage.Declaration | MixinSymbolUsage.Reference
            )
          ], MixinDirectiveSyntaxForm.Annotation
        ),
      ["DERIVATION"] = Define(
        "DERIVATION", 1, DirectiveOperandKind.None,
        [
          Arg(
            MixinLanguageValueKind.CSharpType, MixinSymbolKind.Derivation,
            MixinSymbolUsage.Declaration | MixinSymbolUsage.Reference
          )
        ]
      ),
      ["PRELUDE"] = Define("PRELUDE", 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.Prelude),
      ["DEFINE_TARGET"] = Define(
        "DEFINE_TARGET", 2, DirectiveOperandKind.None,
        [
          Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Target, MixinSymbolUsage.Declaration),
          Arg(MixinLanguageValueKind.Any)
        ],
        MixinDirectiveSyntaxForm.DefineTarget
      ),
      ["CONFIG"] = Define("CONFIG", 1, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.Identifier)])
    };
    ValidateMetadata(definitions.Values);
    return definitions;
  }

  public static bool TryGet(string name, int argumentCount, out DirectiveDefinition definition) {
    definition = Definitions.Values.FirstOrDefault(item =>
      string.Equals(item.Name, name, StringComparison.Ordinal) && item.ArgumentCount == argumentCount
    );
    return definition is not null;
  }

  public static IEnumerable<DirectiveDefinition> Enumerate() => Definitions.Values;

  private static DirectiveDefinition Define(
    string name, int argumentCount,
    DirectiveOperandKind operand, IReadOnlyList<MixinDirectiveArgumentMetadata> arguments,
    MixinDirectiveSyntaxForm syntaxForm = MixinDirectiveSyntaxForm.Invocation
  ) {
    return new SyntaxDirectiveDefinition(name, operand, argumentCount, syntaxForm).WithLanguageSignature(
      arguments, documentation: "@" + name + " directive."
    );
  }

  private static DirectiveDefinition NamedValue(
    string name, MixinSymbolKind symbol,
    MixinDirectiveSyntaxForm syntaxForm
  ) => Define(
    name, 1, DirectiveOperandKind.Value,
    [Arg(MixinLanguageValueKind.Identifier, symbol, MixinSymbolUsage.Declaration)],
    syntaxForm
  );

  private static DirectiveDefinition FunctionDirective(
    FunctionDefinition function, IReadOnlyList<MixinDirectiveArgumentMetadata> arguments,
    int hoistedLocalArgumentIndex = -1, string documentation = null
  ) => new DirectiveDefinition(function.Name, DirectiveOperandKind.Value, function.ArgumentCount)
    .WithLanguageSignature(arguments, documentation: documentation)
    .WithFunction(function, hoistedLocalArgumentIndex);

  private static MixinDirectiveArgumentMetadata Arg(
    MixinLanguageValueKind kind,
    MixinSymbolKind symbol = MixinSymbolKind.None, MixinSymbolUsage usage = MixinSymbolUsage.None
  ) => new(kind, symbol, usage);

  private static void ValidateMetadata(IEnumerable<DirectiveDefinition> definitions) {
    foreach (var definition in definitions) {
      if (definition.Arguments is null)
        throw new InvalidOperationException("Directive '" + definition.Name + "' must provide language metadata.");
    }
  }
}

public static class MixinRootLibrary {
  private static readonly IReadOnlyList<MixinRootDefinition> Definitions = [
    new(
      Name: "target", Root: MixinExpressionRoot.Target, Kind: MixinRootKind.Roslyn,
      Documentation: "The target symbol currently being generated."
    ),
    new(
      Name: "this", Root: MixinExpressionRoot.This, Kind: MixinRootKind.Roslyn,
      Documentation: "The current declaring type or symbol."
    ),
    new(
      Name: "attr", Root: MixinExpressionRoot.Attribute, Kind: MixinRootKind.Roslyn,
      Documentation: "The attribute driving the current mixin."
    ),
    new(
      Name: "arg", Root: MixinExpressionRoot.Argument, Kind: MixinRootKind.Roslyn,
      Documentation: "A named argument of the current attribute."
    ),
    new(
      Name: "true", Root: MixinExpressionRoot.True, Kind: MixinRootKind.Constant,
      Documentation: "The Boolean true value."
    ),
    new(
      Name: "false", Root: MixinExpressionRoot.False, Kind: MixinRootKind.Constant,
      Documentation: "The Boolean false value."
    ),
    new(
      Name: "null", Root: MixinExpressionRoot.Null, Kind: MixinRootKind.Constant,
      Documentation: "The null mixin value."
    ),
    new(
      Name: "table", Root: MixinExpressionRoot.Table, Kind: MixinRootKind.Constant,
      Documentation: "A new empty table value."
    ),
    new(
      Name: "var", Root: MixinExpressionRoot.Variable, Kind: MixinRootKind.Variable,
      Documentation: "A named mixin variable."
    ),
    new(
      Name: "tar", Root: MixinExpressionRoot.TargetVariable, Kind: MixinRootKind.Variable,
      Documentation: "A named variable stored on the current target."
    ),
    new(
      Name: "local", Root: MixinExpressionRoot.Local, Kind: MixinRootKind.Variable,
      Documentation: "A compiler-generated local mixin value."
    ),
    new(
      Name: "param", Root: MixinExpressionRoot.Parameter, Kind: MixinRootKind.Variable,
      Documentation: "A named parameter of the current function scope."
    ),
    new(
      Name: "carry", Root: MixinExpressionRoot.Carry, Kind: MixinRootKind.Variable,
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