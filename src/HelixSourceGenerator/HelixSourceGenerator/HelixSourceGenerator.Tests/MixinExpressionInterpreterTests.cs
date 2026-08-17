using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.SourceGen.Expressions;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinExpressionInterpreterTests {
  private readonly MixinExpressionInterpreter _interpreter = new();

  [Fact]
  public void InterpolatesReferencesAndStoredValues() {
    var variables = new Dictionary<string, string>();
    var result = _interpreter.Execute("""
      @LOCAL<kind> handler
      @VAR<count> one
      @CODE @this:name @local#kind @@ @var#count
      """, new StubContext(), variables);

    Assert.True(result.Success, result.Error);
    Assert.Equal("Demo handler @ one", Assert.Single(result.Outputs).Text);
    Assert.Equal("one", variables["count"]);
  }

  [Fact]
  public void MatchSkipsToTheNextScope() {
    var result = _interpreter.Execute("""
      @SCOPE<first>
      @MATCH @arg#0:?ref
      @CODE wrong
      @RETURN
      @SCOPE<second>
      @MATCH @arg#0:?argument
      @CODE selected
      @RETURN
      """, new StubContext(argumentIsRef: false));

    Assert.True(result.Success, result.Error);
    Assert.Equal("selected", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void LabeledMatchJumpsToTheNamedScopeWhenFalse() {
    var result = _interpreter.Execute("""
      @MATCH<selected> @arg#0:?ref
      @CODE wrong
      @RETURN
      @SCOPE<unrelated>
      @CODE also-wrong
      @RETURN
      @SCOPE<selected>
      @CODE selected
      """, new StubContext(argumentIsRef: false));

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
    var variables = new Dictionary<string, string> { ["value"] = "before" };
    var result = _interpreter.Execute("""
      @VAR<value> after
      @CODE buffered
      @ASSERT @arg#0:?ref
      """, new StubContext(argumentIsRef: false), variables);

    Assert.False(result.Success);
    Assert.Empty(result.Outputs);
    Assert.Equal("before", variables["value"]);
  }

  [Fact]
  public void FailedConditionsProduceUserFacingMessages() {
    var equality = _interpreter.Execute("""
      @VAR<IsComponent> false
      @ASSERT @var#IsComponent:?eq<true>
      """, new StubContext());
    var inheritance = _interpreter.Execute(
      "@ASSERT @target:type:?is<global::IComponent>", new StubContext()
    );

    Assert.Equal("Variable IsComponent is not true", equality.Error);
    Assert.Equal("Target is not of type global::IComponent", inheritance.Error);
  }

  [Fact]
  public void StoredEqualityRetriesAfterUnwrappingAndTreatsMissingAsNull() {
    var unwrapped = _interpreter.Execute("""
      @VAR<kind> "Component"
      @ASSERT @var#kind:?eq<Component>
      """, new StubContext());
    var missingIsNull = _interpreter.Execute(
      "@ASSERT @var#missing:?eq<null>", new StubContext()
    );
    var invertedNull = _interpreter.Execute(
      "@ASSERT @var#missing:!?eq<null>", new StubContext()
    );

    Assert.True(unwrapped.Success, unwrapped.Error);
    Assert.True(missingIsNull.Success, missingIsNull.Error);
    Assert.False(invertedNull.Success);
    Assert.Equal("Variable missing is null", invertedNull.Error);
  }

  [Fact]
  public void SupportsAllCodeTargets() {
    var result = _interpreter.Execute("""
      @CODE target
      @CODE<CLASS> class
      @CODE<FILE> file
      @CODE<IMPLEMENTS> global::IFeature
      @CODE<ANNOTATION> global::Generated
      @USING System.Collections.Generic
      @CODE<DisposeHook> dispose
      """, new StubContext());

    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {
      MixinExpressionOutputTarget.Target,
      MixinExpressionOutputTarget.Class,
      MixinExpressionOutputTarget.File,
      MixinExpressionOutputTarget.Implements,
      MixinExpressionOutputTarget.Annotation,
      MixinExpressionOutputTarget.Using,
      MixinExpressionOutputTarget.Injection
    }, result.Outputs.Select(item => item.Target));
    Assert.Equal("DisposeHook", result.Outputs[6].InjectionTarget);
  }

  [Fact]
  public void ParsesParenthesizedAndInvertedReferences() {
    Assert.True(_interpreter.TryParseReference(
      "@(arg#name:type:!?is<global::IEvent>)", out var reference, out var error
    ), error);

    Assert.Equal("arg", reference.Root);
    Assert.Equal("name", reference.Member);
    Assert.Equal("type", reference.Properties[0].Name);
    Assert.True(reference.Properties[1].Negated);
    Assert.Equal("global::IEvent", reference.Properties[1].Argument);
  }

  [Fact]
  public void ParsesTypeArgumentPaths() {
    Assert.True(_interpreter.TryParseReference(
      "@target:type#0:type#T", out var reference, out var error
    ), error);

    Assert.Equal(new[] { "type", "path", "type", "path" },
      reference.Properties.Select(item => item.Name));
    Assert.Equal("0", reference.Properties[1].Argument);
    Assert.Equal("T", reference.Properties[3].Argument);
  }

  [Fact]
  public void StoredValuesSupportTruthinessAndExistence() {
    var result = _interpreter.Execute("""
      @LOCAL<disabled> false
      @SCOPE<disabled>
      @MATCH @local#disabled
      @FAIL
      @SCOPE<missing>
      @MATCH @local#missing:!?exists
      @CODE selected
      """, new StubContext());

    Assert.True(result.Success, result.Error);
    Assert.Equal("selected", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void GotoLoopsAreBounded() {
    var result = _interpreter.Execute("""
      @SCOPE<again>
      @GOTO<again>
      """, new StubContext());

    Assert.False(result.Success);
    Assert.Contains("execution limit", result.Error);
  }

  [Fact]
  public void GotoContinuesAtTheNamedScope() {
    var result = _interpreter.Execute("""
      @GOTO<selected>
      @CODE wrong
      @SCOPE<selected>
      @CODE right
      """, new StubContext());

    Assert.True(result.Success, result.Error);
    Assert.Equal("right", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void CallsPreparedFunctionsWithSharedLocalsAndVariables() {
    var variables = new Dictionary<string, string>();
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
    Assert.Equal(new[] { "function prepared value", "caller prepared value" },
      result.Outputs.Select(item => item.Text));
    Assert.Equal("prepared", variables["prefix"]);
  }

  [Fact]
  public void PreparedStateIsReusableAndGlobalInitializersAreNotReevaluated() {
    var prepared = _interpreter.PrepareGlobals(new[] {
      "@VAR<prefix> prepared\n" +
      "@FUNC<emit>\n" +
      "@CODE @var#prefix @this:name\n" +
      "@END"
    });
    var firstVariables = new Dictionary<string, string>();
    var first = _interpreter.Execute(
      "@CALL<emit>\n@VAR<prefix> changed", new StubContext(), firstVariables, prepared
    );
    var second = _interpreter.Execute(
      "@CALL<emit>", new StubContext(), new Dictionary<string, string>(), prepared
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
      "@CODE reached\n@RETURN\n@UNKNOWN never-parsed", new StubContext()
    );

    Assert.True(result.Success, result.Error);
    Assert.Equal("reached", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void PreparedStaticLogsAreCapturedOnceAndNotReplayed() {
    var prepared = _interpreter.PrepareGlobals(new[] {
      "@VAR<name> global\n@LOG prepared @var#name"
    });

    var first = _interpreter.Execute("@CODE first", new StubContext(), null, prepared);
    var second = _interpreter.Execute("@CODE second", new StubContext(), null, prepared);

    Assert.Equal("prepared global", Assert.Single(prepared.Logs).Text);
    Assert.Equal(2, prepared.ExecutedOperations);
    Assert.Empty(first.Logs);
    Assert.Empty(second.Logs);
  }

  [Fact]
  public void PreparedDumpsRunOnceAndStateDumpsExposeOperationCounters() {
    var prepared = _interpreter.PrepareGlobals(new[] {
      "@VAR<name> global\n@DUMP<STATE>\n@DUMP<AST>"
    });

    var result = _interpreter.Execute("@DUMP<STATE>", new StubContext(), null, prepared);

    Assert.Equal(3, prepared.ExecutedOperations);
    Assert.Equal(2, prepared.Logs.Count);
    Assert.Contains("operations=3", prepared.Logs[0].Text);
    Assert.Contains("preparedOperations=3", prepared.Logs[0].Text);
    var runtimeState = Assert.Single(result.Logs).Text;
    Assert.Contains("operations=1", runtimeState);
    Assert.Contains("preparedOperations=3", runtimeState);
  }

  [Fact]
  public void DumpAstIncludesPreparedAndCurrentlyAvailableLocalNodes() {
    var prepared = _interpreter.PrepareGlobals(new[] { "@VAR<name> global" });
    var result = _interpreter.Execute(
      "@CODE first\n@DUMP<AST>\n@RETURN\n@CODE unreachable",
      new StubContext(), null, prepared
    );

    Assert.True(result.Success, result.Error);
    var dump = Assert.Single(result.Logs).Text;
    Assert.Contains("AST GLOBAL", dump);
    Assert.Contains("@VAR<name> global", dump);
    Assert.Contains("AST LOCAL", dump);
    Assert.Contains("@DUMP<AST>", dump);
    Assert.DoesNotContain("unreachable", dump);
    Assert.DoesNotContain("\n", dump);
  }

  [Fact]
  public void FunctionScopesAndFunctionEndAreBalanced() {
    var result = _interpreter.Execute("""
      @FUNC<select>
      @SCOPE<first>
      @MATCH @arg#0:?ref
      @CODE wrong
      @END
      @CODE selected
      @END
      @CALL<select>
      @CODE caller
      """, new StubContext(argumentIsRef: false));

    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] { "selected", "caller" }, result.Outputs.Select(item => item.Text));
  }

  [Fact]
  public void RejectsNestedFunctions() {
    var result = _interpreter.Execute("""
      @FUNC<outer>
      @FUNC<inner>
      @END
      @END
      """, new StubContext());

    Assert.False(result.Success);
    Assert.Contains("may not be nested", result.Error);
  }

  [Fact]
  public void LogsAndDumpsStateAndBufferedOutputs() {
    var result = _interpreter.Execute("""
      @LOCAL<kind> handler
      @VAR<count> one
      @CODE first
      @LOG processing @this:name
      @DUMP<STATE>
      @DUMP<BUFFER>
      @FAIL
      """, new StubContext());

    Assert.False(result.Success);
    Assert.Equal(3, result.Logs.Count);
    Assert.Equal("processing Demo", result.Logs[0].Text);
    Assert.Contains("locals={kind=handler}", result.Logs[1].Text);
    Assert.Contains("variables={count=one}", result.Logs[1].Text);
    Assert.Contains("Target: first", result.Logs[2].Text);
  }

  [Fact]
  public void PreparedSyntaxValidationRejectsUnknownDumpKinds() {
    var result = _interpreter.ValidateSyntax("@DUMP<UNKNOWN>");

    Assert.False(result.Success);
    Assert.Contains("STATE, BUFFER or AST", result.Error);
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

    internal StubContext(bool argumentIsRef = false) => _argumentIsRef = argumentIsRef;

    public bool TryResolve(MixinExpressionReference reference, out string value, out string error) {
      error = null;
      value = reference.Root == "this" && reference.Properties.Any(item => item.Name == "name")
        ? "Demo"
        : reference.Root;
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
