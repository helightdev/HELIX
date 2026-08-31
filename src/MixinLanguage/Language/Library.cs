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
      ["format"] = new FormatFunction(), ["identifier"] = new IdentifierFunction(),
      ["floatTime"] = new FloatTimeFunction(), ["table"] = new AsTableFunction(), ["put"] = new PutFunction(),
      ["remove"] = new RemoveFunction(), ["push"] = new PushFunction(), ["pop"] = new PopFunction(),
      ["joinKeys"] = new JoinKeysFunction(), ["joinValues"] = new JoinValuesFunction(),
      ["join"] = new JoinEntriesFunction(), ["mapValues"] = new MapValuesFunction(), ["map"] = new MapFunction(),
      ["filter"] = new FilterFunction(), ["reduce"] = new ReduceFunction(), ["attributes"] = new AttributesFunction(),
      ["derive"] = new DeriveFunction(), ["attributesOf"] = new AttributesOfFunction(),
      ["attributesOfExact"] = new AttributesOfExactFunction(), ["attributeOf"] = new AttributeOfFunction(),
      ["wire"] = new WireFunction(), ["signature"] = new SignaturePredicate(), ["wireable"] = new WireablePredicate(),
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
    ConfigureLanguageSignatures(definitions);
    ValidateMetadata(definitions.Values);
    return definitions;
  }

  public static bool TryGet(string name, out FunctionDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }

  public static bool TryResolve(string name, bool predicate, out FunctionDefinition definition) {
    if (!predicate) return TryGet(name, out definition);
    definition = Definitions.Values.FirstOrDefault(item => item.MatchesPredicate(name));
    return definition is not null;
  }

  public static bool IsPredicate(string name) {
    return TryGet(name, out var definition) && definition.IsPredicate;
  }

  public static IEnumerable<FunctionDefinition> Enumerate() => Definitions.Values;

  private static void ValidateMetadata(IEnumerable<FunctionDefinition> definitions) {
    foreach (var definition in definitions) {
      if (definition.Metadata is null)
        throw new InvalidOperationException("Function ':" + definition.Name + "' must provide language metadata.");
      if (definition.ArgumentTypes.Count < definition.MinimumArguments ||
        definition.MaximumArguments != int.MaxValue && definition.ArgumentTypes.Count != definition.MaximumArguments)
        throw new InvalidOperationException(
          "Function ':" + definition.Name + "' has an incomplete argument descriptor."
        );
    }
  }

  private static void ConfigureLanguageSignatures(IDictionary<string, FunctionDefinition> definitions) {
    SignatureAny(
      definitions,
      [
        "argument", "async", "class", "concrete", "containsPointer", "csharpLiteral", "enum", "eq", "equatableSelf",
        "event", "exists", "exposed", "field", "generic", "genericMethod", "has", "hashCode", "in", "inout", "is",
        "isSelf", "method", "nonEmptyStringConstant", "nullable", "objectEquals", "ordinaryTypedEqualsSelf", "out",
        "parameter", "parameterDefault", "partial", "pointer", "primitive", "property", "public", "ref",
        "referenceType", "signature", "static", "struct", "top", "typeSymbol", "typedEqualsSelf", "unwrap", "valueType",
        "wire", "wireable"
      ]
    );

    Signature(
      definitions, "and", MixinLanguageValueKind.Boolean, MixinLanguageValueKind.Boolean,
      [MixinLanguageValueKind.Boolean]
    );
    Signature(
      definitions, "or", MixinLanguageValueKind.Boolean, MixinLanguageValueKind.Boolean,
      [MixinLanguageValueKind.Boolean]
    );

    Signature(definitions, "name", MixinLanguageValueKind.Any, MixinLanguageValueKind.Text, []);
    Signature(
      definitions, "path", MixinLanguageValueKind.Any, MixinLanguageValueKind.Any,
      [MixinLanguageValueKind.Text]
    );
    Signature(
      definitions, "switch", MixinLanguageValueKind.Boolean, MixinLanguageValueKind.Any,
      [MixinLanguageValueKind.Any, MixinLanguageValueKind.Any]
    );
    Signature(definitions, "size", MixinLanguageValueKind.Any, MixinLanguageValueKind.Text, []);
    Signature(definitions, "table", MixinLanguageValueKind.Any, MixinLanguageValueKind.Table, []);

    Signature(
      definitions, "put", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table,
      [MixinLanguageValueKind.Any, MixinLanguageValueKind.Any]
    );
    Signature(
      definitions, "remove", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table,
      [MixinLanguageValueKind.Any]
    );
    Signature(
      definitions, "push", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table,
      [MixinLanguageValueKind.Any]
    );
    Signature(definitions, "pop", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table, []);
    foreach (var name in new[] { "joinKeys", "joinValues" })
      Signature(
        definitions, name, MixinLanguageValueKind.Table, MixinLanguageValueKind.Text,
        [MixinLanguageValueKind.Text]
      );
    Signature(
      definitions, "join", MixinLanguageValueKind.Table, MixinLanguageValueKind.Text,
      [MixinLanguageValueKind.Text, MixinLanguageValueKind.Text]
    );
    foreach (var name in new[] { "mapValues", "map", "filter" })
      Signature(
        definitions, name, MixinLanguageValueKind.Table, MixinLanguageValueKind.Table,
        [MixinLanguageValueKind.Function]
      );
    Signature(
      definitions, "reduce", MixinLanguageValueKind.Table, MixinLanguageValueKind.Any,
      [MixinLanguageValueKind.Any, MixinLanguageValueKind.Function]
    );
    Signature(definitions, "derive", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table, []);

    Signature(definitions, "type", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Type, []);
    Signature(definitions, "fullName", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Text, []);
    Signature(definitions, "visibility", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Text, []);
    Signature(definitions, "members", MixinLanguageValueKind.Type, MixinLanguageValueKind.Table, []);
    Signature(definitions, "parameters", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Table, []);
    Signature(definitions, "nullableType", MixinLanguageValueKind.Type, MixinLanguageValueKind.Type, []);
    Signature(
      definitions, "makeGeneric", MixinLanguageValueKind.Type, MixinLanguageValueKind.Type,
      [MixinLanguageValueKind.CSharpType]
    );
    Signature(definitions, "attributes", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Table, []);
    foreach (var name in new[] { "attributesOf", "attributesOfExact" })
      Signature(
        definitions, name, MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Table,
        [MixinLanguageValueKind.CSharpType]
      );
    Signature(
      definitions, "attributeOf", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Any,
      [MixinLanguageValueKind.CSharpType]
    );

    foreach (var name in new[] { "replace", "replaceFirst" })
      Signature(
        definitions, name, MixinLanguageValueKind.Text, MixinLanguageValueKind.Text,
        [MixinLanguageValueKind.Text, MixinLanguageValueKind.Text]
      );
    Signature(
      definitions, "format", MixinLanguageValueKind.Text, MixinLanguageValueKind.Text,
      [MixinLanguageValueKind.Any, MixinLanguageValueKind.Any]
    );
    foreach (var name in new[] { "identifier", "floatTime" })
      Signature(definitions, name, MixinLanguageValueKind.Text, MixinLanguageValueKind.Text, []);
    Signature(
      definitions, "matches", MixinLanguageValueKind.Text, MixinLanguageValueKind.Boolean,
      [MixinLanguageValueKind.Text]
    );

    Configure(
      definitions, ["containsPointer"], MixinLanguageValueKind.Type,
      MixinLanguageValueKind.Boolean
    );
  }

  private static void Signature(
    IDictionary<string, FunctionDefinition> definitions, string name,
    MixinLanguageValueKind receiver, MixinLanguageValueKind result,
    IReadOnlyList<MixinLanguageValueKind> arguments
  ) {
    if (definitions.TryGetValue(name, out var definition))
      definition.WithLanguageSignature(
        receiver, definition.IsPredicate ? MixinLanguageValueKind.Boolean : result,
        arguments, definition.IsPredicate ? "Tests the current value." : "Transforms the current value."
      );
  }

  private static void SignatureAny(
    IDictionary<string, FunctionDefinition> definitions, IEnumerable<string> names
  ) {
    foreach (var name in names) {
      if (!definitions.TryGetValue(name, out var definition)) continue;
      definition.WithLanguageSignature(
        MixinLanguageValueKind.Any,
        definition.IsPredicate ? MixinLanguageValueKind.Boolean : MixinLanguageValueKind.Any,
        Enumerable.Repeat(
          MixinLanguageValueKind.Any, Math.Min(definition.MaximumArguments, 16)
        ).ToArray(),
        definition.IsPredicate ? "Tests the current value." : "Transforms the current value."
      );
    }
  }

  private static void Configure(
    IDictionary<string, FunctionDefinition> definitions,
    IEnumerable<string> names, MixinLanguageValueKind receiver, MixinLanguageValueKind result
  ) {
    foreach (var name in names)
      if (definitions.TryGetValue(name, out var definition))
        definition.WithLanguageSignature(
          receiver, definition.IsPredicate ? MixinLanguageValueKind.Boolean : result,
          definition.ArgumentTypes, definition.Documentation
        );
  }

  internal static void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var name in Definitions.Keys) pool.Intern(name);
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
      ["SCOPE"] = Define(
        "SCOPE", 0, 1, DirectiveOperandKind.None,
        [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Declaration)],
        MixinDirectiveSyntaxForm.Scope
      ),
      ["LABEL"] = Define(
        "LABEL", 1, 1, DirectiveOperandKind.None,
        [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Declaration)],
        MixinDirectiveSyntaxForm.Label
      ),
      ["FUNC"] = Define(
        "FUNC", 1, 1, DirectiveOperandKind.None,
        [Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Declaration)],
        MixinDirectiveSyntaxForm.Function
      ),
      ["CALL"] = Define(
        "CALL", 1, 2, DirectiveOperandKind.Value,
        [
          Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Local, MixinSymbolUsage.Declaration),
          Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Reference)
        ],
        MixinDirectiveSyntaxForm.Call,
        new Dictionary<int, IReadOnlyList<MixinDirectiveArgumentMetadata>> {
          [1] = [Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Reference)]
        }
      ),
      ["INLINE"] = Define(
        "INLINE", 1, 1, DirectiveOperandKind.None,
        [Arg(MixinLanguageValueKind.Function, MixinSymbolKind.Function, MixinSymbolUsage.Reference)],
        MixinDirectiveSyntaxForm.Inline
      ),
      ["END"] = Define("END", 0, 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.End), ["MATCH"] = Define(
        "MATCH", 0, 1, DirectiveOperandKind.Boolean,
        [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Reference)],
        MixinDirectiveSyntaxForm.Match
      ),
      ["ASSERT"] = Define("ASSERT", 0, 0, DirectiveOperandKind.Boolean, [], MixinDirectiveSyntaxForm.Assert),
      ["CODE"] = Define(
        "CODE", 0, 1, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.OutputTarget)],
        MixinDirectiveSyntaxForm.Code
      ),
      ["MIXIN"] = Define(
        "MIXIN", 1, 2, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.Any), Arg(MixinLanguageValueKind.Any)],
        MixinDirectiveSyntaxForm.Mixin
      ),
      ["USING"] = Define("USING", 0, 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Using),
      ["LOG"] = Define("LOG", 0, 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Log),
      ["LOCAL"] = NamedValue("LOCAL", MixinSymbolKind.Local, MixinDirectiveSyntaxForm.Local),
      ["VAR"] = NamedValue("VAR", MixinSymbolKind.Variable, MixinDirectiveSyntaxForm.Variable),
      ["TAR"] = NamedValue("TAR", MixinSymbolKind.TargetVariable, MixinDirectiveSyntaxForm.TargetVariable),
      ["CARRY"] = NamedValue("CARRY", MixinSymbolKind.Carry, MixinDirectiveSyntaxForm.Carry),
      ["RETURN"] = Define("RETURN", 0, 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Return),
      ["GOTO"] = Define(
        "GOTO", 1, 1, DirectiveOperandKind.None,
        [Arg(MixinLanguageValueKind.Label, MixinSymbolKind.Label, MixinSymbolUsage.Reference)],
        MixinDirectiveSyntaxForm.Goto
      ),
      ["SKIP"] = Define("SKIP", 0, 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.Skip),
      ["FAIL"] = Define("FAIL", 0, 0, DirectiveOperandKind.Value, [], MixinDirectiveSyntaxForm.Fail),
      ["ANNOTATION"] = Define(
        "ANNOTATION", 1, 1, DirectiveOperandKind.None,
        [
          Arg(
            MixinLanguageValueKind.CSharpType, MixinSymbolKind.Annotation,
            MixinSymbolUsage.Declaration | MixinSymbolUsage.Reference
          )
        ], MixinDirectiveSyntaxForm.Annotation
      ),
      ["DERIVATION"] = Define(
        "DERIVATION", 1, 1, DirectiveOperandKind.None,
        [
          Arg(
            MixinLanguageValueKind.CSharpType, MixinSymbolKind.Derivation,
            MixinSymbolUsage.Declaration | MixinSymbolUsage.Reference
          )
        ]
      ),
      ["PRELUDE"] = Define("PRELUDE", 0, 0, DirectiveOperandKind.None, [], MixinDirectiveSyntaxForm.Prelude),
      ["DEFINE_TARGET"] = Define(
        "DEFINE_TARGET", 2, 2, DirectiveOperandKind.None,
        [
          Arg(MixinLanguageValueKind.Identifier, MixinSymbolKind.Target, MixinSymbolUsage.Declaration),
          Arg(MixinLanguageValueKind.Any)
        ],
        MixinDirectiveSyntaxForm.DefineTarget
      ),
      ["CONFIG"] = Define("CONFIG", 1, 1, DirectiveOperandKind.Value, [Arg(MixinLanguageValueKind.Identifier)])
    };
    ValidateMetadata(definitions.Values);
    return definitions;
  }

  public static bool TryGet(string name, out DirectiveDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }

  public static IEnumerable<DirectiveDefinition> Enumerate() => Definitions.Values;
  public static IEnumerable<DirectiveDefinition> EnumerateLanguageDefinitions() => Definitions.Values;

  private static DirectiveDefinition Define(
    string name, int minimum, int maximum,
    DirectiveOperandKind operand, IReadOnlyList<MixinDirectiveArgumentMetadata> arguments,
    MixinDirectiveSyntaxForm syntaxForm = MixinDirectiveSyntaxForm.Invocation,
    IReadOnlyDictionary<int, IReadOnlyList<MixinDirectiveArgumentMetadata>> arityArguments = null
  ) {
    return new SyntaxDirectiveDefinition(name, operand, maximum, minimum, syntaxForm).WithLanguageSignature(
      arguments, documentation: "@" + name + " directive.", arityArguments: arityArguments
    );
  }

  private static DirectiveDefinition NamedValue(
    string name, MixinSymbolKind symbol,
    MixinDirectiveSyntaxForm syntaxForm
  ) => Define(
    name, 1, 1, DirectiveOperandKind.Value,
    [Arg(MixinLanguageValueKind.Identifier, symbol, MixinSymbolUsage.Declaration)], syntaxForm
  );

  private static DirectiveDefinition FunctionDirective(
    FunctionDefinition function, IReadOnlyList<MixinDirectiveArgumentMetadata> arguments,
    int hoistedLocalArgumentIndex = -1, string documentation = null
  ) {
    function.WithLanguageSignature(
      MixinLanguageValueKind.Any, MixinLanguageValueKind.Any,
      arguments.Select(argument => argument.ValueKind).ToArray(), documentation
    );
    return new DirectiveDefinition(
        function.Name, DirectiveOperandKind.Value,
        function.MaximumArguments, function.MinimumArguments
      ).WithLanguageSignature(arguments, documentation: documentation)
      .WithFunction(function, hoistedLocalArgumentIndex);
  }

  private static MixinDirectiveArgumentMetadata Arg(
    MixinLanguageValueKind kind,
    MixinSymbolKind symbol = MixinSymbolKind.None, MixinSymbolUsage usage = MixinSymbolUsage.None
  ) => new(kind, symbol, usage);

  private static void ValidateMetadata(IEnumerable<DirectiveDefinition> definitions) {
    foreach (var definition in definitions) {
      if (definition.Metadata is null)
        throw new InvalidOperationException("Directive '" + definition.Name + "' must provide language metadata.");
      if (definition.ArgumentTypes.Count < definition.MinimumArguments ||
        definition.MaximumArguments != int.MaxValue && definition.ArgumentTypes.Count != definition.MaximumArguments)
        throw new InvalidOperationException(
          "Directive '" + definition.Name + "' has an incomplete argument descriptor."
        );
    }
  }
}

public static class MixinRootLibrary {
  private static readonly IReadOnlyList<MixinRootDefinition> Definitions = [
    new("target", MixinExpressionRoot.Target, "The target symbol currently being generated."),
    new("this", MixinExpressionRoot.This, "The current declaring type or symbol."),
    new("attr", MixinExpressionRoot.Attribute, "The attribute driving the current mixin."),
    new("arg", MixinExpressionRoot.Argument, "A named argument of the current attribute."),
    new("var", MixinExpressionRoot.Variable, "A named mixin variable."),
    new("tar", MixinExpressionRoot.TargetVariable, "A named variable stored on the current target."),
    new("local", MixinExpressionRoot.Local, "A compiler-generated local mixin value."),
    new("true", MixinExpressionRoot.True, "The Boolean true value."),
    new("false", MixinExpressionRoot.False, "The Boolean false value."),
    new("null", MixinExpressionRoot.Null, "The null mixin value."),
    new("table", MixinExpressionRoot.Table, "A new empty table value."),
    new("param", MixinExpressionRoot.Parameter, "A named parameter of the current function scope."),
    new("carry", MixinExpressionRoot.Carry, "A value carried into an expanded expression scope.")
  ];
  private static readonly IReadOnlyDictionary<string, MixinRootDefinition> ByName =
    Definitions.ToDictionary(item => item.Name, StringComparer.Ordinal);
  private static readonly IReadOnlyDictionary<MixinExpressionRoot, MixinRootDefinition> ByRoot =
    Definitions.ToDictionary(item => item.Root);

  public static bool TryGet(string name, out MixinRootDefinition definition) =>
    ByName.TryGetValue(name ?? "", out definition);

  public static bool TryGet(MixinExpressionRoot root, out MixinRootDefinition definition) =>
    ByRoot.TryGetValue(root, out definition);

  public static IEnumerable<MixinRootDefinition> Enumerate() => Definitions;
}