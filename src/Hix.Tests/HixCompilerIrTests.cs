using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Compiler.Generated;
using Hix.Compiler.Steps;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixCompilerIrTests {
  [Fact]
  public void AliasedCrossFileReferencesKeepTheirSourceRangeAndNavigateToTheQualifiedFunction() {
    const string source = "%using<paths><p>\n---\npure func imported -> number => p.join(2);";
    var imported = new LanguageAnalysis(source);
    var library = new LanguageAnalysis("pure func paths.join(number value) -> number => $value;");
    var reference = Assert.Single(imported.References.Where(reference => reference.Kind == "Function"));
    Assert.Equal(source.IndexOf("p.join", System.StringComparison.Ordinal), reference.Range.Start);
    Assert.Equal(6, reference.Range.Length);
    var declaration = imported.Resolve(reference, [imported, library], out var owner);
    Assert.Equal("paths.join", declaration.Name);
    Assert.Same(library, owner);
  }

  [Fact]
  public void NamespaceImportsBindAcrossFilesWithoutLeakingIntoOtherFiles() {
    var catalog = HixCompiler.PrepareGlobals([
      "%using<paths>\n---\npure func imported -> number => join(2);",
      "pure func paths.join(number value) -> number => plus($value, 10); " +
        "pure func join(number value) -> number => plus($value, 100); " +
        "pure func unimported -> number => join(2);"
    ]);
    var imported = Assert.Single(catalog.Functions.Where(function => function.Name == "imported"));
    var unimported = Assert.Single(catalog.Functions.Where(function => function.Name == "unimported"));
    var importedCall = Assert.IsType<CallExpressionIr>(Assert.IsType<ControlFlowStatementIr>(
      Assert.Single(imported.Body.Statements)).Values[0]);
    var unimportedCall = Assert.IsType<CallExpressionIr>(Assert.IsType<ControlFlowStatementIr>(
      Assert.Single(unimported.Body.Statements)).Values[0]);
    Assert.Equal("paths.join", importedCall.Binding.Signature.Name);
    Assert.Equal("join", unimportedCall.Binding.Signature.Name);
    Assert.Equal(HixCallKind.Static, importedCall.Binding.Kind);
    Assert.Equal(HixCallKind.Static, unimportedCall.Binding.Kind);
  }

  [Fact]
  public void SemanticPassesBindOwnedNodesWithoutRebuildingThemOrMutatingSource() {
    var unit = AntlrSyntax.Parse("mixin Test { expression { local x = 1; emit(plus($x, 2)) } }");
    var declaration = Assert.Single(unit.Declarations.OfType<MixinDeclarationIr>());
    var expression = Assert.Single(declaration.Declarations.OfType<ExpressionDeclarationIr>());
    var catalog = HixCompiler.PrepareGlobals([unit]);
    var compilation = new HixCompilation(new([], [expression], []), catalog);
    var owned = Assert.Single(compilation.Module.Late);
    var assignment = Assert.IsType<AssignmentStatementIr>(owned.Body.Statements[0]);
    var emit = Assert.IsType<InvocationStatementIr>(owned.Body.Statements[1]).Call;
    var plus = Assert.IsType<CallExpressionIr>(emit.Arguments[0]);
    new PatternBindingStep().Run(compilation);
    new NameValidationStep().Run(compilation);
    Assert.Same(owned, compilation.Module.Late[0]);
    Assert.Equal(HixCallKind.Static, plus.Binding.Kind);
    Assert.Equal("number", plus.InferredPattern.Display);
    Assert.Same(assignment.Symbol, Assert.IsType<RootExpressionIr>(plus.Arguments[0]).Binding.Variable);
    var originalCall = Assert.IsType<InvocationStatementIr>(expression.Body.Statements[1]).Call;
    Assert.Equal(HixCallKind.Unbound, originalCall.Binding.Kind);
    Assert.Same(expression, expression.Body.Parent);
  }

  [Fact]
  public void BranchAssignmentsKeepTheInferredLocalTypeForBinding() {
    var unit = AntlrSyntax.Parse("""
      pure func choose (number value) -> string { return(<number>) }
      pure func choose (string value) -> string { return(<string>) }
      mixin Test { expression {
        local value = 1
        when(false) { $value = <changed> }
        emit(choose(local#value))
      } }
      """);
    var catalog = HixCompiler.PrepareGlobals([unit]);
    var declaration = Assert.Single(unit.Declarations.OfType<MixinDeclarationIr>());
    var module = HixCompiler.PrepareIr(declaration, catalog);
    var emit = Assert.IsType<InvocationStatementIr>(module.Late[0].Body.Statements[2]).Call;
    Assert.Equal(HixCallKind.Static, Assert.IsType<CallExpressionIr>(emit.Arguments[0]).Binding.Kind);
    var result = HixVM.Execute(HixCompiler.Compile(unit, "Test"), new HixCoroutine());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("number", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void PreparingFunctionsPreservesParsedMetadataOwnership() {
    var unit = AntlrSyntax.Parse("%doc<Explains the value> pure func describe(string value) -> string { return($value) }");
    var function = Assert.Single(unit.Declarations.OfType<FunctionDeclarationIr>());
    var metadata = Assert.Single(function.Metadata);
    HixCompiler.PrepareGlobals([unit]);
    HixCompiler.PrepareGlobals([unit]);
    Assert.Same(function, metadata.Parent);
    Assert.Same(metadata, Assert.Single(metadata.Values).Parent);
  }

  [Fact]
  public void LookupDocumentsOnlyCompatibleOverloads() {
    var analysis = new LanguageAnalysis("""
      %doc<Returns a number unchanged.>
      pure func choose(number value) -> number { return($value) }
      %doc<Returns a tuple unchanged.>
      pure func choose(tuple value) -> tuple { return($value) }
      mixin Test { expression { emit(choose(42)) } }
      """);
    var fact = Assert.Single(analysis.TypeFacts.Where(fact => fact.Kind == "Call" &&
      fact.Documentation.Contains("Returns a number unchanged.")));
    Assert.DoesNotContain("Returns a tuple unchanged.", fact.Documentation);
    var builtin = new LanguageAnalysis("mixin Test { expression { emit(length(@[1, 2])) } }");
    var docs = Assert.Single(builtin.TypeFacts.Where(fact => fact.Kind == "Call" && fact.Documentation.StartsWith("length(")));
    Assert.Contains("length(tuple arg0)", docs.Documentation);
    Assert.DoesNotContain("length(string arg0)", docs.Documentation);
    Assert.DoesNotContain("Transforms the", docs.Documentation);
  }

  [Fact]
  public void FrontendRetainsGeneratedTokenTypesIncludingHiddenWhitespace() {
    var unit = AntlrSyntax.Parse("mixin Test { expression { emit(42) } }");
    Assert.Contains(unit.Tokens, token => token.Type == HixLexer.OUTER_WHITESPACE && token.Channel == 1);
    Assert.Contains(unit.Tokens, token => token.Type == HixLexer.BEGIN_PARAMETERS);
    Assert.Contains(unit.Tokens, token => token.Type == HixLexer.NUMBER);
    Assert.Equal(unit.Source, string.Concat(unit.Tokens.Select(token => token.Text)));
  }

  [Fact]
  public void InterpolatedWhenBranchesInferString() {
    var analysis = new LanguageAnalysis("""
      mixin Test { expression {
        local suffix = <Type>
        local datatype = when [true] {
          true -> <global::Example.String>
          else -> <global::Example.Object[$suffix]>
        }
      } }
      """);

    var declaration = Assert.Single(analysis.TypeFacts.Where(fact =>
      fact.Inlay && fact.Documentation == "local datatype: string"));
    Assert.Equal("string", declaration.Type);
  }
}
