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
  public void SupportsAllCodeTargets() {
    var result = _interpreter.Execute("""
      @CODE target
      @CODE<CLASS> class
      @CODE<FILE> file
      @CODE<IMPLEMENTS> global::IFeature
      @CODE<DisposeHook> dispose
      """, new StubContext());

    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {
      MixinExpressionOutputTarget.Target,
      MixinExpressionOutputTarget.Class,
      MixinExpressionOutputTarget.File,
      MixinExpressionOutputTarget.Implements,
      MixinExpressionOutputTarget.Injection
    }, result.Outputs.Select(item => item.Target));
    Assert.Equal("DisposeHook", result.Outputs[4].InjectionTarget);
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
