using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HELIX.SourceGen.Expressions;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinExpressionInterpreterTests {
  private readonly MixinExpressionInterpreter _interpreter = new();

  [Fact]
  public void TablesMutateWhileOpenAndCopyAfterAssignmentClosesThem() {
    var variables = new Dictionary<string, object>();
    var created = _interpreter.Execute(
      "@VAR<table> @table:put<first><one>:put<second><two>",
      new StubContext(), variables
    );
    Assert.True(created.Success, created.Error);
    var original = Assert.IsType<MixinExpressionTable>(variables["table"]);
    Assert.True(original.IsClosed);
    Assert.Equal(2, original.Count);

    var changed = _interpreter.Execute(
      "@VAR<table> @var#table:put<third><three>",
      new StubContext(), variables
    );
    Assert.True(changed.Success, changed.Error);
    var replacement = Assert.IsType<MixinExpressionTable>(variables["table"]);
    Assert.NotSame(original, replacement);
    Assert.True(replacement.IsClosed);
    Assert.Equal(2, original.Count);
    Assert.Equal(3, replacement.Count);
  }

  [Fact]
  public void TableFunctionUsesScalarAndNullConversionSemantics() {
    var result = _interpreter.Execute(
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
  public void CallsCanReturnValuesIntoLocals() {
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    Assert.IsType<MixinExpressionTable>(variables["data"]);
    Assert.Null(variables["nothing"]);
    Assert.Equal(new[] { "Ada", "3", "1", "Ada" }, result.Outputs.Select(item => item.Text));
  }

  [Fact]
  public void FunctionParametersAreTypedAndNestedCallsRestoreTheCallerParameter() {
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
      "@VAR<time> " + time + "\n@CODE @var#time:floatTime", new StubContext()
    );

    Assert.False(result.Success);
    Assert.Contains("as a float time", result.Error);
  }

  [Fact]
  public void RegexAndLogicalBooleanFunctionsCanBeUsedAsValuesOrConditions() {
    var result = _interpreter.Execute(
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
  [InlineData("@ASSERT @local#props:?structNoArgs<true>", ":structNoArgs requires 0 arguments")]
  [InlineData("@CODE @local#props:structParams<first><second>", ":structParams accepts at most 1 argument")]
  public void FunctionGrammarRejectsAmbiguousOrInvalidCalls(string expression, string expected) {
    var validation = _interpreter.ValidateSyntax(expression);

    Assert.False(validation.Success);
    Assert.Contains(expected, validation.Error);
  }

  [Theory]
  [InlineData("@PROP_STRUCT<Props><props><unknown> @target", "unknown PROP_STRUCT flag 'unknown'")]
  [InlineData("@PROP_STRUCT<Props><props><datatype><DATATYPE> @target", "specified more than once")]
  [InlineData("@PROP_STRUCT<Props><props><datatype><noGenerate><extra> @target", "accepts at most 4 arguments")]
  public void PropStructDirectiveRejectsInvalidFlags(string expression, string expected) {
    var validation = _interpreter.ValidateSyntax(expression);

    Assert.False(validation.Success);
    Assert.Contains(expected, validation.Error);
  }

  [Theory]
  [InlineData("@PROP_STRUCT<Props><props> @target")]
  [InlineData("@PROP_STRUCT<Props><props><datatype> @target")]
  [InlineData("@PROP_STRUCT<Props><props><noGenerate><datatype> @target")]
  public void PropStructDirectiveAcceptsOptionalFlags(string expression) {
    var validation = _interpreter.ValidateSyntax(expression);

    Assert.True(validation.Success, validation.Error);
  }

  [Fact]
  public void InterpolatesReferencesAndStoredValues() {
    var variables = new Dictionary<string, object>();
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
  public void FailSupportsAnInterpolatedMessageAndKeepsTheBareForm() {
    var custom = _interpreter.Execute("@FAIL invalid @this:name", new StubContext());
    var bare = _interpreter.Execute("@FAIL", new StubContext());

    Assert.False(custom.Success);
    Assert.Equal("invalid Demo", custom.Error);
    Assert.False(bare.Success);
    Assert.Equal("expression requested failure", bare.Error);
  }

  [Fact]
  public void FailedExecutionDoesNotCommitCodeOrVariables() {
    var variables = new Dictionary<string, object> { ["value"] = "before" };
    var result = _interpreter.Execute(
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
    var equality = _interpreter.Execute(
      """
      @VAR<IsComponent> false
      @ASSERT @var#IsComponent:?eq<true>
      """,
      new StubContext()
    );
    var inheritance = _interpreter.Execute(
      "@ASSERT @target:type:?is<global::IComponent>",
      new StubContext()
    );

    Assert.Equal("Variable IsComponent is not true", equality.Error);
    Assert.Equal("Target is not of type global::IComponent", inheritance.Error);
  }

  [Fact]
  public void StoredEqualityRetriesAfterUnwrappingAndTreatsMissingAsNull() {
    var unwrapped = _interpreter.Execute(
      """
      @VAR<kind> "Component"
      @ASSERT @var#kind:?eq<Component>
      """,
      new StubContext()
    );
    var missingIsNull = _interpreter.Execute(
      "@ASSERT @var#missing:?eq<null>",
      new StubContext()
    );
    var invertedNull = _interpreter.Execute(
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
    var result = _interpreter.Execute(
      """
      @CODE target
      @CODE<CLASS> class
      @CODE<FILE> file
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
        MixinExpressionOutputTarget.Implements,
        MixinExpressionOutputTarget.Annotation,
        MixinExpressionOutputTarget.Using,
        MixinExpressionOutputTarget.Injection
      },
      result.Outputs.Select(item => item.Target)
    );
    Assert.Equal("DisposeHook", result.Outputs[6].InjectionTarget);
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

    var validation = _interpreter.ValidateSyntax(expression);
    var result = _interpreter.Execute(expression, new StubContext());

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
    var result = _interpreter.ValidateSyntax("@+orphan");

    Assert.False(result.Success);
    Assert.Equal(1, result.ErrorLine);
    Assert.Contains("continuation requires an immediately preceding directive", result.Error);
  }

  [Fact]
  public void ParsesParenthesizedAndInvertedReferences() {
    Assert.True(
      _interpreter.TryParseReference(
        "@(arg#name:type:!?is<global::IEvent>)",
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
  }

  [Fact]
  public void RewritesOnlyTargetReferenceRootsAsThis() {
    var rewritten = MixinExpressionCompiler.RewriteTargetAsThis(
      "@target:name | @(target:type) | @targeted:name | @var#target"
    );

    Assert.Equal("@this:name | @(this:type) | @targeted:name | @var#target", rewritten);
  }

  [Fact]
  public void ParsesTypeArgumentPaths() {
    Assert.True(
      _interpreter.TryParseReference(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var explicitJump = _interpreter.Execute(
      "@CODE runtime",
      new StubContext(),
      null,
      new[] { "@GOTO<next>", "@SCOPE<next>\n@CODE escaped" }
    );
    var implicitJump = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var prepared = _interpreter.PrepareGlobals(
      new[] {
        "@VAR<prefix> prepared\n" +
        "@FUNC<emit>\n" +
        "@CODE @var#prefix @this:name\n" +
        "@END"
      }
    );
    var firstVariables = new Dictionary<string, object>();
    var first = _interpreter.Execute(
      "@CALL<emit>\n@VAR<prefix> changed",
      new StubContext(),
      firstVariables,
      prepared
    );
    var second = _interpreter.Execute(
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
      _interpreter.PrepareGlobals(new[] { "@VAR<invalid> @target:name" })
    );

    Assert.Contains("only reference an existing @var", error.Message);
  }

  [Fact]
  public void RuntimeProgramAstIsBuiltOnlyAsExecutionReachesIt() {
    var result = _interpreter.Execute(
      "@CODE reached\n@RETURN\n@UNKNOWN never-parsed",
      new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("reached", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void CachedProgramsCanBeEvaluatedConcurrently() {
    const string expression = "@CODE @this:name\n@RETURN\n@UNKNOWN unreachable";
    var failures = new ConcurrentQueue<string>();

    Parallel.For(0, 64, _ => {
      var result = new MixinExpressionInterpreter().Execute(expression, new StubContext());
      if (!result.Success || result.Outputs.Count != 1 || result.Outputs[0].Text != "Demo") {
        failures.Enqueue(result.Error ?? "unexpected output");
      }
    });

    Assert.Empty(failures);
  }

  [Fact]
  public void PreparedStaticLogsAreCapturedOnceAndNotReplayed() {
    var prepared = _interpreter.PrepareGlobals(
      new[] {
        "@VAR<name> global\n@LOG prepared @var#name"
      }
    );

    var first = _interpreter.Execute("@CODE first", new StubContext(), null, prepared);
    var second = _interpreter.Execute("@CODE second", new StubContext(), null, prepared);

    Assert.Equal("prepared global", Assert.Single(prepared.Logs).Text);
    Assert.Equal(2, prepared.ExecutedOperations);
    Assert.Empty(first.Logs.Where(item => !item.IsHint));
    Assert.Empty(second.Logs.Where(item => !item.IsHint));
  }

  [Fact]
  public void DumpIsNotAnExpressionDirective() {
    var result = _interpreter.ValidateSyntax("@DUMP<STATE>");

    Assert.False(result.Success);
    Assert.Contains("unknown directive", result.Error);
  }

  [Fact]
  public void FunctionScopesAndFunctionEndAreBalanced() {
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute(
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
    var result = _interpreter.Execute("@SKIP", new StubContext());

    Assert.False(result.Success);
    Assert.Contains("no following scope", result.Error);
  }

  [Fact]
  public void UnknownDirectivesReportTheirLine() {
    var result = _interpreter.Execute("\n@UNKNOWN", new StubContext());

    Assert.False(result.Success);
    Assert.Equal(2, result.ErrorLine);
    Assert.Contains("unknown directive", result.Error);
  }

  private sealed class StubContext : IMixinExpressionContext {
    private readonly bool _argumentIsRef;

    internal StubContext(bool argumentIsRef = false) {
      _argumentIsRef = argumentIsRef;
    }

    public bool TryResolve(MixinExpressionReference reference, out string value, out string error) {
      error = null;
      value = reference.Root == MixinExpressionRoot.This &&
        reference.Properties.Any(item => item.Name == "name")
        ? "Demo"
        : reference.Root switch {
          MixinExpressionRoot.Attribute => "attr",
          MixinExpressionRoot.Argument => "arg",
          MixinExpressionRoot.Variable => "var",
          MixinExpressionRoot.Parameter => "param",
          _ => reference.Root.ToString().ToLowerInvariant()
        };
      return true;
    }

    public bool TryEvaluate(MixinExpressionReference reference, out bool value, out string error) {
      error = null;
      var predicate = reference.Properties.Last();
      value = predicate.Name switch {
        "ref" => _argumentIsRef,
        "argument" => !_argumentIsRef,
        _ => false
      };
      if (predicate.Negated) value = !value;
      return true;
    }
  }

}
