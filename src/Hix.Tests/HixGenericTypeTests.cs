using System;
using System.Collections.Generic;
using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;
using HostSignature = Hix.FunctionSignature;
using IrSignature = Hix.Compiler.FunctionSignature;

namespace HELIX.SourceGen.Tests;

public sealed class HixGenericTypeTests {
  private static readonly HixPattern Number = new KindHixPattern(HixValueKind.Number);
  private static readonly HixPattern String = new KindHixPattern(HixValueKind.String);
  private static readonly IReadOnlyDictionary<string, HixPattern> Patterns = new Dictionary<string, HixPattern>();
  private static readonly IReadOnlyDictionary<string, FunctionDeclarationIr[]> Functions = new Dictionary<string, FunctionDeclarationIr[]>();

  [Fact]
  public void PatternConstructorsAreCompilerOnlyCallables() {
    var builder = new FunctionSignatureRegistryBuilder();
    var many = builder.DefinePatternConstructor("many", 1, arguments => new ManyHixPattern(arguments[0]));
    Assert.IsType<ManyHixPattern>(many.Construct([String]));
    Assert.Throws<ArgumentException>(() => many.Construct([]));
    Assert.Throws<ArgumentException>(() => builder.DefinePatternConstructor("many", 1, arguments => arguments[0]));
    var library = new FunctionLibrary(builder);
    Assert.Empty(library.Resolve("many"));
    Assert.Single(library.ResolveMetadata("many", HixMetadataKind.Pattern, 1));
    Assert.Empty(library.ResolveMetadata("many", HixMetadataKind.Pattern, 2));
  }

  [Fact]
  public void MetadataParametersHaveStableAndIndependentSourceScopes() {
    const string first = "%generic<T> pure func first(T value) -> T { return($value) }";
    const string second = "%generic<T> pure func other(T value) -> T { return($value) }";
    HixTypeParameter Parameter(string source) => AntlrSyntax.Parse(source).Declarations
      .OfType<FunctionDeclarationIr>().Single().TypeParameters.Single();
    Assert.Equal(Parameter(first).Scope, Parameter(first).Scope);
    Assert.NotEqual(Parameter(first).Scope, Parameter(second).Scope);
  }

  [Fact]
  public void OpaqueRenamePreservesNumericKindAndRemovesAlias() {
    Assert.Equal(7, (int)HixValueKind.Opaque);
    Assert.False(Enum.TryParse<HixValueKind>("Symbol", out _));
    Assert.IsType<KindHixPattern>(HixPatterns.Named("opaque"));
    Assert.IsType<NamedHixPattern>(HixPatterns.Named("symbol"));
    Assert.Equal(HixValueKind.Opaque, new ObjectHixValue(new object()).Kind);
  }

  [Fact]
  public void NominalIdentityArityAndInvarianceAreEnforced() {
    var first = new HixOpaqueTypeDefinition("one.box", "box", 1);
    var second = new HixOpaqueTypeDefinition("two.box", "box", 1);
    Assert.Throws<ArgumentException>(() => first.Construct());
    Assert.Throws<ArgumentException>(() => new OpaqueHixPattern(first, []));
    Assert.Equal(HixPatternRelation.Never, HixPatternRelations.Relate(first.Construct(Number), second.Construct(Number), Patterns));
    Assert.Equal(HixPatternRelation.Never, HixPatternRelations.Relate(first.Construct(Number), first.Construct(String), Patterns));
    Assert.Equal(HixPatternRelation.Always, HixPatternRelations.Relate(first.Construct(Number), first.Construct(Number), Patterns));
    Assert.Equal(HixValueKind.Opaque, HixGenericTypes.Erase(first.Construct(Number)));
  }

  [Fact]
  public void RegistryRejectsConflictingDefinitions() {
    var builder = new FunctionSignatureRegistryBuilder();
    var type = builder.DefineOpaqueType("test.box", "box", 1);
    Assert.Same(type, builder.DefineOpaqueType("test.box", "box", 1));
    Assert.Throws<ArgumentException>(() => builder.DefineOpaqueType("test.box", "box", 2));
    Assert.Throws<ArgumentException>(() => builder.DefineOpaqueType("", "box"));
    Assert.Single(new FunctionLibrary(builder).OpaqueTypes);
  }

  [Fact]
  public void ScopeIdentityAndNestedSubstitutionArePreserved() {
    var first = new HixTypeParameter("first", "T");
    var second = new HixTypeParameter("second", "T");
    var type = new HixOpaqueTypeDefinition("test.box", "box", 1);
    var substitutions = new Dictionary<HixTypeParameter, HixPattern> { [first] = Number };
    Assert.Equal("box<number>", HixGenericTypes.Substitute(type.Construct(first.Pattern), substitutions).Display);
    Assert.Equal("T", HixGenericTypes.Substitute(second.Pattern, substitutions).Display);
    Assert.Equal("any", HixGenericTypes.Substitute(second.Pattern, substitutions, true).Display);
    Assert.False(HixGenericTypes.Equivalent(first.Pattern, second.Pattern));
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public void GenericHostCallsInferAndEraseTypes(bool explicitArguments) {
    var parameter = new HixTypeParameter("identity", "T");
    var backend = new Backend(new HostDeclaration("identity", [new HostSignature(parameter.Pattern, [parameter.Pattern], typeParameters: [parameter])], (_, values) => values[0]));
    var call = new CallExpressionIr("identity", [new NumberExpressionIr(42)]);
    if (explicitArguments) call.TypeArguments = [Number];
    var resolved = HixCallResolver.Resolve(call, Functions, backend, Patterns, Infer, out var ambiguous);
    Assert.False(ambiguous);
    Assert.Equal("number", resolved.Signature.Result.Display);
    Assert.Equal("any", resolved.Binding.Signature.Result.Display);
    Assert.Equal("number", resolved.Substitutions[parameter].Display);
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public void ConflictingArgumentsAndBoundsRejectCalls(bool bounded) {
    var parameter = new HixTypeParameter("pair", "T", bounded ? Number : null);
    var signature = new HostSignature(parameter.Pattern, bounded ? [parameter.Pattern] : [parameter.Pattern, parameter.Pattern], typeParameters: [parameter]);
    var backend = new Backend(new HostDeclaration("pair", [signature], (_, values) => values[0]));
    var call = new CallExpressionIr("pair", bounded ? [new StringExpressionIr("text")] : [new NumberExpressionIr(1), new StringExpressionIr("text")]);
    Assert.Null(HixCallResolver.Resolve(call, Functions, backend, Patterns, Infer, out _));
  }

  [Fact]
  public void UnknownArgumentsDoNotInventSubstitutions() {
    var parameter = new HixTypeParameter("identity", "T");
    var backend = new Backend(new HostDeclaration("identity", [new HostSignature(parameter.Pattern, [parameter.Pattern], typeParameters: [parameter])], (_, values) => values[0]));
    var resolved = HixCallResolver.Resolve(new("identity", [new RootExpressionIr("dynamic")]), Functions, backend, Patterns, Infer, out _);
    Assert.Empty(resolved.Substitutions);
    Assert.Equal("any", resolved.Signature.Result.Display);
  }

  [Fact]
  public void ConcreteOverloadWinsAndExactGenericTiesAreAmbiguous() {
    var parameter = new HixTypeParameter("choose", "T");
    var generic = new HostSignature(parameter.Pattern, [parameter.Pattern], typeParameters: [parameter]);
    var concrete = new HostSignature(String, [Number]);
    var backend = new Backend(new HostDeclaration("choose", [generic, concrete], (_, values) => values[0]));
    var resolved = HixCallResolver.Resolve(new("choose", [new NumberExpressionIr(1)]), Functions, backend, Patterns, Infer, out var ambiguous);
    Assert.False(ambiguous);
    Assert.Equal("string", resolved.Signature.Result.Display);
    backend = new Backend(new HostDeclaration("choose", [generic, generic], (_, values) => values[0]));
    Assert.Null(HixCallResolver.Resolve(new("choose", [new NumberExpressionIr(1)]), Functions, backend, Patterns, Infer, out ambiguous));
    Assert.True(ambiguous);
  }

  [Fact]
  public void NamedDefaultsAndVariadicArgumentsParticipateInInference() {
    var parameter = new HixTypeParameter("choose", "T");
    var backend = new Backend(new HostDeclaration("choose", [new HostSignature(parameter.Pattern, [parameter.Pattern, parameter.Pattern],
      ArgumentNames: ["first", "second"], ArgumentDefaults: [null, new NumberExpressionIr(2)], typeParameters: [parameter])], (_, values) => values[0]));
    var resolved = HixCallResolver.Resolve(new("choose", [new NumberExpressionIr(1)], argumentNames: ["first"]), Functions, backend, Patterns, Infer, out _);
    Assert.Equal(2, resolved.Arguments.Length);
    Assert.Equal("number", resolved.Signature.Result.Display);
    backend = new Backend(new HostDeclaration("choose", [new HostSignature(parameter.Pattern, [parameter.Pattern], IsVariadic: true, typeParameters: [parameter])], (_, values) => values[0]));
    Assert.NotNull(HixCallResolver.Resolve(new("choose", [new NumberExpressionIr(1), new NumberExpressionIr(2)]), Functions, backend, Patterns, Infer, out _));
    Assert.Null(HixCallResolver.Resolve(new("choose", [new NumberExpressionIr(1), new StringExpressionIr("text")]), Functions, backend, Patterns, Infer, out _));
  }

  [Fact]
  public void GenericIrCompilesOnceAndRunsWithDifferentKinds() {
    var parameter = new HixTypeParameter("identity", "T");
    var function = new FunctionDeclarationIr("identity", true, false, true,
      [new IrSignature(null, [new SignatureField("value", parameter.Pattern)], parameter.Pattern, null) { TypeParameters = [parameter] }],
      new BlockStatementIr([new ControlFlowStatementIr(ControlFlowKind.Return, null, [new RootExpressionIr("0", true)])]));
    var unit = new CompilationUnitIr("", [function], [], []);
    var program = HixCompiler.CompileFunctions([unit]);
    using var vm = new HixVM([program]);
    Assert.Equal(42, Assert.IsType<NumberHixValue>(vm.Invoke(program, new HixCoroutine(), function: "identity", arguments: [new NumberHixValue(42)]).Value).Value);
    Assert.IsType<LiteralHixValue>(vm.Invoke(program, new HixCoroutine(), "identity", arguments: [new LiteralHixValue(HixString.Dynamic("text"))]).Value);
    Assert.Single(program.Scope.Functions());
    Assert.Equal("any", program.Scope.Functions().Single().Function.Signatures[0].Inputs[0].Pattern.Display);
  }

  private static HixPattern Infer(ExpressionIr expression) => expression switch {
    NumberExpressionIr => Number, StringExpressionIr => String, _ => HixPattern.Any
  };

  [Fact]
  public void ContextualLambdaInferenceUsesGenericDataArguments() {
    var element = new HixTypeParameter("apply", "T");
    var result = new HixTypeParameter("apply", "R");
    var signature = new HostSignature(result.Pattern, [element.Pattern,
      new DelegateHixPattern([new("value", element.Pattern)], result.Pattern)], typeParameters: [element, result]);
    var backend = new Backend(new HostDeclaration("apply", [signature], (_, values) => values[0]));
    var lambda = new LambdaExpressionIr(new BlockStatementIr([
      new ControlFlowStatementIr(ControlFlowKind.Return, null, [new RootExpressionIr("0", true)])]));
    var call = new CallExpressionIr("apply", [new NumberExpressionIr(42), lambda]);
    var resolved = HixCallResolver.Resolve(call, Functions, backend, Patterns, Infer, out _);
    Assert.Equal("number", resolved.Signature.Result.Display);
    Assert.Equal("number", resolved.Substitutions[result].Display);
  }

  [Fact]
  public void GenericIrCallablesInferFromInvocationArguments() {
    var element = new HixTypeParameter("identity", "T");
    var result = new HixTypeParameter("launch", "R");
    var function = new FunctionDeclarationIr("identity", true, false, true,
      [new IrSignature(null, [new SignatureField("value", element.Pattern)], element.Pattern, null)],
      new BlockStatementIr([new ControlFlowStatementIr(ControlFlowKind.Return, null, [new RootExpressionIr("0", true)])]),
      typeParameters: [element]);
    var backend = new Backend(new HostDeclaration("launch", [new HostSignature(result.Pattern,
      [new DelegateHixPattern([], result.Pattern), new KindHixPattern(HixValueKind.Tuple)], typeParameters: [result]) {
      CallableArguments = new Dictionary<int, int> { [0] = 1 }
    }], (_, values) => values[0]));
    var arguments = new TupleExpressionIr([new NumberExpressionIr(42)]);
    var call = new CallExpressionIr("launch", [new RootExpressionIr("identity"), arguments]);
    var functions = new Dictionary<string, FunctionDeclarationIr[]> { ["identity"] = [function] };
    var resolved = HixCallResolver.Resolve(call, functions, backend, Patterns, expression => expression == arguments ?
      new TupleHixPattern([new("0", Number)]) : HixPattern.Any, out _);
    Assert.Equal("number", resolved.Signature.Result.Display);
  }

  [Fact]
  public void NominalHostOverloadsKeepIdentityAfterIdenticalErasure() {
    var first = new HixOpaqueTypeDefinition("first.handle", "handle");
    var second = new HixOpaqueTypeDefinition("second.handle", "handle");
    var backend = new Backend(
      new HostDeclaration("first", [new HostSignature(first.Construct(), [])], (_, _) => new ObjectHixValue(new object())),
      new HostDeclaration("second", [new HostSignature(second.Construct(), [])], (_, _) => new ObjectHixValue(new object())),
      new HostDeclaration("route", [new HostSignature(String, [first.Construct()])], (_, _) => new LiteralHixValue(HixString.Dynamic("first"))),
      new HostDeclaration("route", [new HostSignature(String, [second.Construct()])], (_, _) => new LiteralHixValue(HixString.Dynamic("second"))));
    var program = HixCompiler.CompileFunctions(["func main { return(@[route(first()), route(second())]) }"], backend);
    using var vm = new HixVM([program]);
    var result = vm.Invoke(program, backend.CreateCoroutine());
    var values = Assert.IsType<TupleHixValue>(result.Value).Values;
    Assert.Equal(new[] {"first", "second"}, values.Cast<LiteralHixValue>().Select(value => value.Value.Resolve(result.Execution.Strings)));
  }

  private sealed record HostDeclaration(string Name, HostSignature[] Signatures, InlineFunction Handler);

  private sealed class Backend(params HostDeclaration[] definitions) : HixBackend {
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
      base.RegisterFunctions(functions);
      for (var index = 0; index < definitions.Length; index++) {
        var definition = definitions[index];
        var hostName = definition.Name + "_" + index;
        functions.Add(new SimpleFunction(hostName, definition.Handler));
        var declarations = definition.Signatures.Select(signature => new FunctionDeclarationIr(
          definition.Name, true, false, false,
          [new IrSignature(null, signature.ArgumentTypes.Select((_, argument) =>
            new SignatureField(signature.GetArgumentName(argument), signature.GetArgumentPattern(argument),
              signature.IsVariadic && argument == signature.ArgumentTypes.Count - 1,
              defaultValue: signature.GetArgumentDefault(argument))).ToArray(), signature.ResultPattern, null)],
          null, typeParameters: signature.TypeParameters, externalName: hostName)).ToArray();
        functions.AddLibrary(new CompilationUnitIr("", declarations, [], []));
        if (definition.Signatures.Any(signature => signature.InferResult != null || signature.CallableArguments != null))
          functions.DefineInference(definition.Name, signature => signature with {
            InferResult = definition.Signatures[0].InferResult,
            CallableArguments = definition.Signatures[0].CallableArguments
          });
      }
    }
  }
}
