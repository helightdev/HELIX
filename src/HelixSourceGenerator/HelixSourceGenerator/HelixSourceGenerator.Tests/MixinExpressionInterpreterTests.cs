using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mixins;
using Mixins.Compiler;
using Mixins.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinExpressionInterpreterTests {

  [Fact]
  public void FrozenStringPoolResolvesKnownTextWithoutInterningRuntimeText() {
    var builder = new MixinStringPoolBuilder();
    var expected = builder.Intern("known");
    var pool = builder.Freeze();

    var known = pool.Get("known");
    var runtime = pool.Get("runtime-only");

    Assert.Equal(expected, known);
    Assert.True(known.IsInterned);
    Assert.False(runtime.IsInterned);
    Assert.Equal("runtime-only", runtime.DynamicValue);
    Assert.Equal(1, pool.Count);
    Assert.False(pool.TryGetId("runtime-only", out _));
  }

  [Fact]
  public void RuntimeInputsReuseCompiledStringsAndKeepUnknownStringsDynamic() {
    var variables = new Dictionary<string, object> {
      ["known"] = "known",
      ["runtime"] = "runtime-only"
    };
    var context = new StubContext();
    var result = MixinVirtualMachine.Execute(
      "@CODE @var#known\n@CODE @var#runtime", context, variables
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] { "known", "runtime-only" }, result.Outputs.Select(item => item.Text));
    Assert.True(context.Strings.TryGetId("known", out _));
    Assert.False(context.Strings.TryGetId("runtime-only", out _));
  }

  [Fact]
  public void TablesMutateWhileOpenAndCopyAfterAssignmentClosesThem() {
    var variables = new Dictionary<string, object>();
    var created = MixinVirtualMachine.Execute(
      "@VAR<table> @table:put<first><one>:put<second><two>",
      new StubContext(), variables
    );
    Assert.True(created.Success, created.Error);
    var original = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(variables["table"]);
    Assert.Equal(2, original.Count);

    var changed = MixinVirtualMachine.Execute(
      "@VAR<table> @var#table:put<third><three>",
      new StubContext(), variables
    );
    Assert.True(changed.Success, changed.Error);
    var replacement = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(variables["table"]);
    Assert.NotSame(original, replacement);
    Assert.Equal(2, original.Count);
    Assert.Equal(3, replacement.Count);
  }

  [Fact]
  public void TableFunctionUsesScalarAndNullConversionSemantics() {
    var result = MixinVirtualMachine.Execute(
      """
      @CODE @true:table:size
      @CODE @true:table:path<0>
      @CODE @null:table:size
      @CODE @table:put<a><b>:table:size
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] { "1", "true", "0", "1" }, result.Outputs.Select(item => item.Text));
  }

  [Fact]
  public void EmptyReferenceRootIsNullShorthand() {
    var result = MixinVirtualMachine.Execute(
      "@CODE @:table:size\n@CODE @@:literal",
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] { "0", "@:literal" }, result.Outputs.Select(item => item.Text));
  }

  [Fact]
  public void HoistingCarriesCompleteRoslynPredicates() {
    var success = MixinCompiler.TryCompileSyntax(
      MixinParser.Parse(""),
      MixinParser.Parse("@MATCH @this:?is<MonoBehaviour>\n@MATCH @attr#type:?exists"),
      null,
      out var prelude,
      out var lateExpression,
      out var error,
      out var errorLine
    );

    Assert.True(success, $"line {errorLine}: {error}");
    var renderedPrelude = MixinSyntaxRenderer.RenderProgram(prelude);
    var renderedLate = MixinSyntaxRenderer.RenderProgram(lateExpression);
    Assert.Contains("@CARRY<__0> @this:is<MonoBehaviour>", renderedPrelude);
    Assert.Contains("@CARRY<__1> @attr#type:exists", renderedPrelude);
    Assert.Contains("@MATCH @carry#__0", renderedLate);
    Assert.Contains("@MATCH @carry#__1", renderedLate);
  }

  [Fact]
  public void AutomaticHoistingDeduplicatesReferencesAndUsesLeafSnapshots() {
    var success = MixinCompiler.TryCompileSyntax(
      MixinParser.Parse(""),
      MixinParser.Parse(
        "@CODE @target:type\n@CODE @target:type\n@CODE @attr#qualifier\n@CODE @attr#qualifier"
      ),
      null,
      out var prelude,
      out _,
      out var error,
      out var errorLine
    );

    Assert.True(success, $"line {errorLine}: {error}");
    var rendered = MixinSyntaxRenderer.RenderProgram(prelude);
    Assert.Equal(1, rendered.Split(new[] { "@target:type" }, StringSplitOptions.None).Length - 1);
    Assert.Equal(1, rendered.Split(new[] { "@attr#qualifier" }, StringSplitOptions.None).Length - 1);
  }

  [Fact]
  public void CallsCanReturnValuesIntoLocals() {
    var result = MixinVirtualMachine.Execute(
      """
      @FUNC<select>
      @RETURN @param#value
      @END
      @CALL<selected><select> @table:put<value><chosen>
      @CODE @local#selected
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("chosen", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void TablesSupportJoiningMappingAndFiltering() {
    var result = MixinVirtualMachine.Execute(
      """
      @FUNC<bracket>
      @RETURN [@param]
      @END
      @FUNC<decorate>
      @CALL<decorated><bracket> @param
      @RETURN @local#decorated
      @END
      @FUNC<pair>
      @RETURN @param#k=@param#v
      @END
      @FUNC<keep>
      @RETURN @param#v:?eq<two>
      @END
      @VAR<data> @table:put<a><one>:put<b><two>
      @CODE @var#data:joinKeys<,>
      @CODE @var#data:joinValues<|>
      @CODE @var#data:join<:><;>
      @CODE @var#data:mapValues<decorate>:joinValues<,>
      @CODE @var#data:map<pair>:joinValues<,>
      @CODE @var#data:filter<keep>:joinValues<,>
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(
      new[] { "a,b", "one|two", "a:one;b:two", "[one],[two]", "a=one,b=two", "two" },
      result.Outputs.Select(item => item.Text)
    );
  }

  [Fact]
  public void VariablesPreserveImmutableTablesAndTableOperations() {
    var variables = new Dictionary<string, object>();
    var result = MixinVirtualMachine.Execute(
      """
      @VAR<data> @table:put<name><Ada>:push<first>:push<second>
      @ASSERT @var#data:?has<second>
      @CODE @var#data#name
      @CODE @var#data:size
      @VAR<nested> @table:put<child><(@var#data)>
      @VAR<data> @var#data:pop:remove<name>
      @CODE @var#data:size
      @CODE @var#nested#child#name
      @VAR<nothing> @null
      @ASSERT @var#nothing:!?exists
      """,
      new StubContext(), variables
    );

    Assert.True(result.Success, result.Error);
    Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(variables["data"]);
    Assert.Null(variables["nothing"]);
    Assert.Equal(new[] { "Ada", "3", "1", "Ada" }, result.Outputs.Select(item => item.Text));
  }

  [Fact]
  public void FunctionParametersAreTypedAndNestedCallsRestoreTheCallerParameter() {
    var result = MixinVirtualMachine.Execute(
      """
      @FUNC<inner>
      @CODE inner=@param#value
      @RETURN
      @END
      @FUNC<outer>
      @CODE outer=@param#value
      @CALL<inner> @table:put<value><second>
      @CODE restored=@param#value
      @RETURN
      @END
      @CALL<outer> @table:put<value><first>
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(
      new[] { "outer=first", "inner=second", "restored=first" },
      result.Outputs.Select(item => item.Text)
    );
  }

  [Fact]
  public void PutAndPushUpdateImmutableLocalTablesWithTypedValues() {
    var result = MixinVirtualMachine.Execute(
      """
      @PUT<items><name> Ada
      @PUSH<items> first
      @PUSH<items> @table:put<nested><value>
      @CODE @local#items#name
      @CODE @local#items#2#nested
      @CODE @local#items:size
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] { "Ada", "value", "3" }, result.Outputs.Select(item => item.Text));
  }

  [Fact]
  public void MixinSupportsConstantAndDynamicTargetsWithPriorities() {
    var variables = new Dictionary<string, object> {
      ["destination"] = "$Dispose",
      ["priority"] = "12"
    };
    var result = MixinVirtualMachine.Execute(
      "@MIXIN<$Init> First()\n@MIXIN<(@var#destination)><(@var#priority)> Second()",
      new StubContext(), variables
    );

    Assert.True(result.Success, result.Error);
    Assert.Collection(
      result.Outputs,
      output => {
        Assert.Equal(MixinExpressionOutputTarget.Mixin, output.Target);
        Assert.Equal("$Init", output.InjectionTarget);
        Assert.Equal(0, output.InjectionPriority);
        Assert.Equal("First()", output.Text);
      },
      output => {
        Assert.Equal(MixinExpressionOutputTarget.Mixin, output.Target);
        Assert.Equal("$Dispose", output.InjectionTarget);
        Assert.Equal(12, output.InjectionPriority);
        Assert.Equal("Second()", output.Text);
      }
    );
  }

  [Fact]
  public void ValueFunctionsSupportMultipleAndDynamicArguments() {
    var result = MixinVirtualMachine.Execute(
      """
      @VAR<text> alpha-alpha
      @VAR<replacement> omega
      @CODE @var#text:replaceFirst<alpha><(@var#replacement)>
      @CODE @true:switch<yes><no>
      @CODE @false:eq<(@true)>
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(
      new[] { "omega-alpha", "yes", "false" },
      result.Outputs.Select(item => item.Text)
    );
  }

  [Fact]
  public void FloatTimeParsesSecondsMillisecondsTicksMinutesAndFrequency() {
    var variables = new Dictionary<string, object> {
      ["integer"] = 7,
      ["decimal"] = 1.25m,
      ["tiny"] = -0.0000005d,
      ["zero"] = 0
    };
    var result = MixinVirtualMachine.Execute(
      """
      @VAR<seconds> 1.5 seconds
      @VAR<millis> 250ms
      @VAR<frequency> %4
      @VAR<ticks> 3 ticks
      @VAR<minutes> 2 minutes
      @CODE @var#seconds:floatTime
      @CODE @var#millis:floatTime
      @CODE @null:floatTime
      @CODE @var#frequency:floatTime
      @CODE @var#ticks:floatTime
      @CODE @var#minutes:floatTime
      @CODE @var#integer:floatTime
      @CODE @var#decimal:floatTime
      @CODE @var#tiny:floatTime
      @CODE @var#zero:floatTime
      """,
      new StubContext(), variables
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(
      new[] {
        "1.5f", "0.25f", "-0f", "0.25f", "-3f", "120f", "7f", "1.25f", "-0f", "-0f"
      },
      result.Outputs.Select(item => item.Text)
    );
  }

  [Theory]
  [InlineData("1.5 ticks")]
  [InlineData("%0")]
  [InlineData("tomorrow")]
  public void FloatTimeRejectsInvalidFormats(string time) {
    var result = MixinVirtualMachine.Execute(
      "@VAR<time> " + time + "\n@CODE @var#time:floatTime", new StubContext()
    );

    Assert.False(result.Success);
    Assert.Contains("as a float time", result.Error);
  }

  [Fact]
  public void RegexAndLogicalBooleanFunctionsCanBeUsedAsValuesOrConditions() {
    var result = MixinVirtualMachine.Execute(
      """
      @VAR<text> Event42
      @ASSERT @var#text:?matches<^Event[0-9]+$>
      @ASSERT @false:or<(@true)>
      @CODE @var#text:matches<^Event>
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("true", Assert.Single(result.Outputs).Text);
  }

  [Theory]
  [InlineData("@CODE @true:switch<only-one>", ":switch requires 2 arguments")]
  [InlineData("@ASSERT @true:and<@false>", "dynamic boolean expressions")]
  [InlineData("@CODE @true:eq<true>:unwrap", "must be terminal")]
  [InlineData("@CODE @this:makeGeneric", ":makeGeneric requires 1 argument")]
  [InlineData("@CODE @this:visibility<public>", ":visibility requires 0 arguments")]
  public void FunctionGrammarRejectsAmbiguousOrInvalidCalls(string expression, string expected) {
    var validation = MixinCompiler.ValidateSyntax(expression);

    Assert.False(validation.Success);
    Assert.Contains(expected, validation.Error);
  }

  [Fact]
  public void InterpolatesReferencesAndStoredValues() {
    var variables = new Dictionary<string, object>();
    var result = MixinVirtualMachine.Execute(
      """
      @LOCAL<kind> handler
      @VAR<count> one
      @CODE @this:name @local#kind @@ @var#count
      """,
      new StubContext(),
      variables
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("Demo handler @ one", Assert.Single(result.Outputs).Text);
    Assert.Equal("one", variables["count"]);
  }

  [Fact]
  public void MatchSkipsToTheNextScope() {
    var result = MixinVirtualMachine.Execute(
      """
      @SCOPE<first>
      @MATCH @arg#0:?ref
      @CODE wrong
      @RETURN
      @SCOPE<second>
      @MATCH @arg#0:?argument
      @CODE selected
      @RETURN
      """,
      new StubContext(false)
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("selected", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void LabeledMatchJumpsToTheNamedScopeWhenFalse() {
    var result = MixinVirtualMachine.Execute(
      """
      @MATCH<selected> @arg#0:?ref
      @CODE wrong
      @RETURN
      @SCOPE<unrelated>
      @CODE also-wrong
      @RETURN
      @SCOPE<selected>
      @CODE selected
      """,
      new StubContext(false)
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("selected", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void LabelIsAnImmediatelyClosedScopeJumpTarget() {
    var result = MixinVirtualMachine.Execute(
      """
      @GOTO<selected>
      @CODE wrong
      @LABEL<selected>
      @CODE selected
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("selected", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void LabelClosesThePreviousFunctionScopeWithoutOpeningAnother() {
    var result = MixinVirtualMachine.Execute(
      """
      @FUNC<emit>
      @SCOPE<previous>
      @LABEL<selected>
      @RETURN selected
      @END
      @CALL<value><emit>
      @CODE @local#value
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("selected", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void FailSupportsAnInterpolatedMessageAndKeepsTheBareForm() {
    var custom = MixinVirtualMachine.Execute("@FAIL invalid @this:name", new StubContext());
    var bare = MixinVirtualMachine.Execute("@FAIL", new StubContext());

    Assert.False(custom.Success);
    Assert.Equal("invalid Demo", custom.Error);
    Assert.False(bare.Success);
    Assert.Equal("expression requested failure", bare.Error);
  }

  [Fact]
  public void FailedExecutionDoesNotCommitCodeOrVariables() {
    var variables = new Dictionary<string, object> { ["value"] = "before" };
    var result = MixinVirtualMachine.Execute(
      """
      @VAR<value> after
      @CODE buffered
      @ASSERT @arg#0:?ref
      """,
      new StubContext(false),
      variables
    );

    Assert.False(result.Success);
    Assert.Empty(result.Outputs);
    Assert.Equal("before", variables["value"]);
  }

  [Fact]
  public void FailedConditionsProduceUserFacingMessages() {
    var equality = MixinVirtualMachine.Execute(
      """
      @VAR<IsComponent> false
      @ASSERT @var#IsComponent:?eq<true>
      """,
      new StubContext()
    );
    var inheritance = MixinVirtualMachine.Execute(
      "@ASSERT @target:type:?is<global::IComponent>",
      new StubContext()
    );

    Assert.Equal("Variable IsComponent is not true", equality.Error);
    Assert.Equal("Target is not of type global::IComponent", inheritance.Error);
  }

  [Fact]
  public void StoredEqualityRetriesAfterUnwrappingAndTreatsMissingAsNull() {
    var unwrapped = MixinVirtualMachine.Execute(
      """
      @VAR<kind> "Component"
      @ASSERT @var#kind:?eq<Component>
      """,
      new StubContext()
    );
    var missingIsNull = MixinVirtualMachine.Execute(
      "@ASSERT @var#missing:?eq<null>",
      new StubContext()
    );
    var invertedNull = MixinVirtualMachine.Execute(
      "@ASSERT @var#missing:!?eq<null>",
      new StubContext()
    );

    Assert.True(unwrapped.Success, unwrapped.Error);
    Assert.True(missingIsNull.Success, missingIsNull.Error);
    Assert.False(invertedNull.Success);
    Assert.Equal("Variable missing is null", invertedNull.Error);
  }

  [Fact]
  public void SupportsAllCodeTargets() {
    var result = MixinVirtualMachine.Execute(
      """
      @CODE target
      @CODE<CLASS> class
      @CODE<FILE> file
      @CODE<EXTENDS> global::Base
      @CODE<IMPLEMENTS> global::IFeature
      @CODE<ANNOTATION> global::Generated
      @USING System.Collections.Generic
      @CODE<DisposeHook> dispose
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(
      new[] {
        MixinExpressionOutputTarget.Target,
        MixinExpressionOutputTarget.Class,
        MixinExpressionOutputTarget.File,
        MixinExpressionOutputTarget.Extends,
        MixinExpressionOutputTarget.Implements,
        MixinExpressionOutputTarget.Annotation,
        MixinExpressionOutputTarget.Using,
        MixinExpressionOutputTarget.Injection
      },
      result.Outputs.Select(item => item.Target)
    );
    Assert.Equal("DisposeHook", result.Outputs[7].InjectionTarget);
  }

  [Fact]
  public void SupportsNewlineAndDirectContinuationsAndLineComments() {
    const string expression = """
                              @CODE public static void Example(
                              @+int first,
                              @+ int second) {
                              @\  Use(first);
                              @\
                              @\  Use(second);
                              @\}
                              @# @UNKNOWN ignored
                              """;

    var validation = MixinCompiler.ValidateSyntax(expression);
    var result = MixinVirtualMachine.Execute(expression, new StubContext());

    Assert.True(validation.Success, validation.Error);
    Assert.True(result.Success, result.Error);
    Assert.Equal(
      "public static void Example(int first, int second) {\n" +
      "  Use(first);\n\n  Use(second);\n}",
      Assert.Single(result.Outputs).Text
    );
  }

  [Fact]
  public void ContinuationsRequireAnImmediatelyPrecedingDirective() {
    var result = MixinCompiler.ValidateSyntax("@+orphan");

    Assert.False(result.Success);
    Assert.Equal(1, result.ErrorLine);
    Assert.Contains("continuation requires an immediately preceding directive", result.Error);
  }

  [Fact]
  public void ParsesParenthesizedAndInvertedReferences() {
    const string source = "@(arg#name:type:!?is<global::IEvent>)";
    Assert.True(
      MixinCompiler.TryParseReference(
        source,
        out var reference,
        out var error
      ),
      error
    );

    Assert.Equal(MixinExpressionRoot.Argument, reference.Root);
    Assert.Equal("name", reference.Member);
    Assert.Equal("type", reference.Properties[0].Name);
    Assert.True(reference.Properties[1].Negated);
    Assert.Equal("global::IEvent", reference.Properties[1].Argument);
    Assert.Equal(new MixinSourceRange(0, source.Length, 1, 0), reference.SourceRange);
    Assert.Equal(10, reference.Properties[0].SourceRange.Start);
    Assert.Equal(
      "<global::IEvent>",
      source[reference.Properties[1].ParsedArguments[0].SourceRange.Start..
        reference.Properties[1].ParsedArguments[0].SourceRange.End]
    );
  }

  [Fact]
  public void ParserPreservesPhysicalLineRangesIncludingTrivia() {
    const string source = "@LOCAL<Name> @this:name\r\n@# comment\n@+ suffix\r\n";

    var program = MixinParser.Parse(source);

    Assert.Equal(4, program.Instructions.Count);
    Assert.Equal("@LOCAL<Name> @this:name\r\n", Slice(source, program.Instructions[0].SourceRange));
    Assert.Equal("@# comment\n", Slice(source, program.Instructions[1].SourceRange));
    Assert.Equal("@+ suffix\r\n", Slice(source, program.Instructions[2].SourceRange));
    Assert.Equal("", Slice(source, program.Instructions[3].SourceRange));
  }


  private static string Slice(string source, MixinSourceRange range) =>
    source.Substring(range.Start, range.Length);

  [Fact]
  public void RewritesOnlyTargetReferenceRootsAsThis() {
    var rewritten = MixinCompiler.RewriteTargetAsThis(
      MixinParser.Parse(
        "@CODE @target:name | @(target:type) | @targeted:name | @var#target"
      )
    );

    Assert.Equal(
      "@CODE @this:name | @(this:type) | @targeted:name | @var#target",
      MixinSyntaxRenderer.RenderProgram(rewritten)
    );
  }

  [Fact]
  public void ParsesTypeArgumentPaths() {
    Assert.True(
      MixinCompiler.TryParseReference(
        "@target:type#0:type#T",
        out var reference,
        out var error
      ),
      error
    );

    Assert.Equal(
      new[] { "type", "path", "type", "path" },
      reference.Properties.Select(item => item.Name)
    );
    Assert.Equal("0", reference.Properties[1].Argument);
    Assert.Equal("T", reference.Properties[3].Argument);
  }

  [Fact]
  public void StoredValuesSupportTruthinessAndExistence() {
    var result = MixinVirtualMachine.Execute(
      """
      @LOCAL<disabled> false
      @SCOPE<disabled>
      @MATCH @local#disabled
      @FAIL
      @SCOPE<missing>
      @MATCH @local#missing:!?exists
      @CODE selected
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("selected", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void GotoLoopsAreBounded() {
    var result = MixinVirtualMachine.Execute(
      """
      @SCOPE<again>
      @GOTO<again>
      """,
      new StubContext()
    );

    Assert.False(result.Success);
    Assert.Contains("execution limit", result.Error);
  }

  [Fact]
  public void GotoContinuesAtTheNamedScope() {
    var result = MixinVirtualMachine.Execute(
      """
      @GOTO<selected>
      @CODE wrong
      @SCOPE<selected>
      @CODE right
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("right", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void ScopeLabelsAreBoundToTheirFunction() {
    var result = MixinVirtualMachine.Execute(
      """
      @FUNC<emit>
      @GOTO<outside>
      @END
      @SCOPE<outside>
      @CALL<emit>
      """,
      new StubContext()
    );

    Assert.False(result.Success);
    Assert.Equal("unknown scope label 'outside'", result.Error);
  }

  [Fact]
  public void ScopeLabelsMayBeReusedInSeparateControlFlowRegions() {
    var result = MixinVirtualMachine.Execute(
      """
      @SCOPE<start>
      @FUNC<first>
      @SCOPE<start>
      @RETURN
      @END
      @END
      @FUNC<second>
      @SCOPE<start>
      @RETURN
      @END
      @END
      @CALL<first>
      @CALL<second>
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
  }

  [Fact]
  public void ExplicitAndImplicitJumpsCannotCrossPreparedExpressions() {
    var explicitJump = MixinVirtualMachine.Execute(
      "@CODE runtime",
      new StubContext(),
      null,
      new[] { "@GOTO<next>", "@SCOPE<next>\n@CODE escaped" }
    );
    var implicitJump = MixinVirtualMachine.Execute(
      "@CODE runtime",
      new StubContext(),
      null,
      new[] { "@SKIP", "@SCOPE<next>\n@CODE escaped" }
    );

    Assert.False(explicitJump.Success);
    Assert.Equal("unknown scope label 'next'", explicitJump.Error);
    Assert.False(implicitJump.Success);
    Assert.Equal("SKIP has no following scope", implicitJump.Error);
  }

  [Fact]
  public void CallsPreparedFunctionsWithSharedLocalsAndVariables() {
    var variables = new Dictionary<string, object>();
    var result = MixinVirtualMachine.Execute(
      "@CALL<emit>\n@CODE caller @var#prefix @local#shared",
      new StubContext(),
      variables,
      new[] {
        """
        @VAR<prefix> prepared
        @FUNC<emit>
        @LOCAL<shared> value
        @CODE function @var#prefix @local#shared
        @RETURN
        @CODE unreachable
        @END
        """
      }
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(
      new[] { "function prepared value", "caller prepared value" },
      result.Outputs.Select(item => item.Text)
    );
    Assert.Equal("prepared", variables["prefix"]);
  }

  [Fact]
  public void PreparedStateIsReusableAndGlobalInitializersAreNotReevaluated() {
    var prepared = MixinCompiler.PrepareGlobals(
      new[] {
        "@VAR<prefix> prepared\n" +
        "@FUNC<emit>\n" +
        "@CODE @var#prefix @this:name\n" +
        "@END"
      }
    );
    var firstVariables = new Dictionary<string, object>();
    var first = MixinVirtualMachine.Execute(
      "@CALL<emit>\n@VAR<prefix> changed",
      new StubContext(),
      firstVariables,
      prepared
    );
    var second = MixinVirtualMachine.Execute(
      "@CALL<emit>",
      new StubContext(),
      new Dictionary<string, object>(),
      prepared
    );

    Assert.True(first.Success, first.Error);
    Assert.True(second.Success, second.Error);
    Assert.Equal("prepared Demo", Assert.Single(first.Outputs).Text);
    Assert.Equal("prepared Demo", Assert.Single(second.Outputs).Text);
    Assert.Equal("changed", firstVariables["prefix"]);
  }

  [Fact]
  public void PreparedInitializersCannotCaptureRuntimeContext() {
    var error = Assert.Throws<ArgumentException>(() =>
      MixinCompiler.PrepareGlobals(new[] { "@VAR<invalid> @target:name" })
    );

    Assert.Contains("only reference an existing @var", error.Message);
  }

  [Fact]
  public void RuntimeProgramIsFullyCompiledBeforeExecution() {
    var result = MixinVirtualMachine.Execute(
      "@CODE reached\n@RETURN\n@UNKNOWN never-parsed",
      new StubContext()
    );

    Assert.False(result.Success);
    Assert.Equal("unknown directive '@UNKNOWN'", result.Error);
    Assert.Empty(result.Outputs);
  }

  [Fact]
  public void ConcurrentExecutionsAreIndependent() {
    const string expression = "@CODE @this:name\n@RETURN";
    var failures = new ConcurrentQueue<string>();

    Parallel.For(0, 64, _ => {
      var result = MixinVirtualMachine.Execute(expression, new StubContext());
      if (!result.Success || result.Outputs.Count != 1 || result.Outputs[0].Text != "Demo") {
        failures.Enqueue(result.Error ?? "unexpected output");
      }
    });

    Assert.Empty(failures);
  }

  [Fact]
  public void PreparedStaticLogsAreCapturedOnceAndNotReplayed() {
    var prepared = MixinCompiler.PrepareGlobals(
      new[] {
        "@VAR<name> global\n@LOG prepared @var#name"
      }
    );

    var first = MixinVirtualMachine.Execute("@CODE first", new StubContext(), null, prepared);
    var second = MixinVirtualMachine.Execute("@CODE second", new StubContext(), null, prepared);

    Assert.Equal("prepared global", Assert.Single(prepared.Logs).Text);
    Assert.Equal(2, prepared.ExecutedOperations);
    Assert.Empty(first.Logs.Where(item => !item.IsHint));
    Assert.Empty(second.Logs.Where(item => !item.IsHint));
  }

  [Fact]
  public void DumpIsNotAnExpressionDirective() {
    var result = MixinCompiler.ValidateSyntax("@DUMP<STATE>");

    Assert.False(result.Success);
    Assert.Contains("unknown directive", result.Error);
  }

  [Fact]
  public void FunctionScopesAndFunctionEndAreBalanced() {
    var result = MixinVirtualMachine.Execute(
      """
      @FUNC<select>
      @SCOPE<first>
      @MATCH @arg#0:?ref
      @CODE wrong
      @END
      @CODE selected
      @END
      @CALL<select>
      @CODE caller
      """,
      new StubContext(false)
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] { "selected", "caller" }, result.Outputs.Select(item => item.Text));
  }

  [Fact]
  public void RejectsNestedFunctions() {
    var result = MixinVirtualMachine.Execute(
      """
      @FUNC<outer>
      @FUNC<inner>
      @END
      @END
      """,
      new StubContext()
    );

    Assert.False(result.Success);
    Assert.Contains("may not be nested", result.Error);
  }

  [Fact]
  public void LogsArePreservedWhenEvaluationFails() {
    var result = MixinVirtualMachine.Execute(
      """
      @LOCAL<kind> handler
      @VAR<count> one
      @CODE first
      @LOG processing @this:name
      @FAIL
      """,
      new StubContext()
    );

    Assert.False(result.Success);
    Assert.Equal("processing Demo", Assert.Single(result.Logs).Text);
  }

  [Fact]
  public void SkipWithoutAnotherScopeFails() {
    var result = MixinVirtualMachine.Execute("@SKIP", new StubContext());

    Assert.False(result.Success);
    Assert.Contains("no following scope", result.Error);
  }

  [Fact]
  public void UnknownDirectivesReportTheirLine() {
    var result = MixinVirtualMachine.Execute("\n@UNKNOWN", new StubContext());

    Assert.False(result.Success);
    Assert.Equal(2, result.ErrorLine);
    Assert.Contains("unknown directive", result.Error);
  }

  [Fact]
  public void TargetVariablesAndReducePreserveTypedValues() {
    var result = MixinVirtualMachine.Execute(
      """
      @TAR<Model> @table:push<a>:push<b>:push<c>
      @FUNC<Append>
      @RETURN @param#acc@param#key=@param#value;
      @END
      @CODE @tar#Model:reduce<><Append>
      """,
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("0=a;1=b;2=c;", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void ReduceRequiresAValueProducingReturn() {
    var result = MixinVirtualMachine.Execute(
      """
      @FUNC<NoValue>
      @RETURN
      @END
      @CODE @table:push<a>:reduce<><NoValue>
      """,
      new StubContext()
    );

    Assert.False(result.Success);
    Assert.Contains("must return a value", result.Error);
  }

  [Fact]
  public void ReduceReturnsInitialForEmptyTableAndSharesCallbackVariables() {
    var variables = new Dictionary<string, object>();
    var result = MixinVirtualMachine.Execute(
      """
      @VAR<Calls> 0
      @FUNC<Fold>
      @VAR<Calls> @var#Calls@param#value
      @RETURN @param#acc:push<(@param#key)>:push<(@param#value)>
      @END
      @LOCAL<Empty> @table:reduce<(@table:push<initial>)><Fold>
      @LOCAL<Folded> @table:push<a>:push<b>:reduce<(@table)><Fold>
      @CODE @local#Empty#0|@local#Folded#0,@local#Folded#1,@local#Folded#2,@local#Folded#3|@var#Calls
      """,
      new StubContext(), variables
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("initial|0,a,1,b|0ab", Assert.Single(result.Outputs).Text);
    Assert.Equal("0ab", variables["Calls"]);
  }

  [Theory]
  [InlineData("@null:reduce<seed><Fold>", ":reduce requires a table")]
  [InlineData("@table:push<a>:reduce<seed><Missing>", "unknown function")]
  public void ReduceRejectsInvalidReceiverAndMissingCallback(string expression, string expected) {
    var result = MixinVirtualMachine.Execute(
      "@FUNC<Fold>\n@RETURN @param#acc\n@END\n@CODE " + expression,
      new StubContext()
    );

    Assert.False(result.Success);
    Assert.Contains(expected, result.Error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void ReduceAcceptsExplicitNullAndPropagatesCallbackFailures() {
    var nullResult = MixinVirtualMachine.Execute(
      """
      @FUNC<Null>
      @RETURN @null
      @END
      @CODE @table:push<a>:reduce<seed><Null>
      """,
      new StubContext()
    );
    Assert.True(nullResult.Success, nullResult.Error);
    Assert.Equal("null", Assert.Single(nullResult.Outputs).Text);

    var failure = MixinVirtualMachine.Execute(
      """
      @FUNC<Broken>
      @FAIL callback failed
      @END
      @CODE @table:push<a>:reduce<seed><Broken>
      """,
      new StubContext()
    );
    Assert.False(failure.Success);
    Assert.Contains("callback failed", failure.Error);
  }

  private sealed class StubContext : ExecutionContext {
    private readonly bool _argumentIsRef;

    internal StubContext(bool argumentIsRef = false)
      : base(new MixinStringPoolBuilder().Freeze()) {
      _argumentIsRef = argumentIsRef;
    }

    protected override IMixinValue ResolveHost(
      MixinExpressionRoot root, MixinString member
    ) {
      var name = member.Resolve(Strings);
      var value = root == MixinExpressionRoot.This && name == "name"
        ? "Demo"
        : root switch {
          MixinExpressionRoot.Attribute => "attr",
          MixinExpressionRoot.Argument => "arg",
          _ => root.ToString().ToLowerInvariant()
        };
      return new ObjectMixinValue(value);
    }

    public override bool HasTrait(IMixinValue value, MixinString trait) {
      return trait.Resolve(Strings) switch {
        "ref" => _argumentIsRef,
        "argument" => !_argumentIsRef,
        _ => false
      };
    }

    public override MixinString NameOf(IMixinValue value) =>
      ResolveString("Demo");
  }

}
