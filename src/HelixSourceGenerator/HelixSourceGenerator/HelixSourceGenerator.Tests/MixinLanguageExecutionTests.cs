using System;
using System.Collections.Generic;
using System.Linq;
using Mixins;
using Mixins.Compiler;
using Mixins.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinLanguageExecutionTests {
  private static MixinExpressionResult Run(string statements, string functions = "", string prelude = "",
    IDictionary<string, object> variables = null, Mixins.Runtime.ExecutionContext context = null) {
    var unit = AntlrSyntax.Parse(functions + "\nmixin Example {\nprelude expression {\n" + prelude +
      "\n}\nexpression {\n" + statements + "\n}\n}");
    Assert.Empty(unit.Diagnostics);
    return MixinVirtualMachine.Execute(HixCompiler.Compile(unit, "Example"), context ?? new Context(), variables);
  }

  [Theory]
  [InlineData("emit(<hello>)", "hello")]
  [InlineData("emit @> Hello\n@+  world\n", "Hello world")]
  [InlineData("emit @> Hello\n@> world\n", "Hello\nworld")]
  [InlineData("emit(<a[null]b>)", "ab")]
  [InlineData("emit(true)", "true")]
  [InlineData("emit(false)", "false")]
  [InlineData("emit(plus(number<2>, number<3>))", "5")]
  [InlineData("emit(<A[b()]>)", "Ab", "pure func b { return(<b>) }")]
  public void RendersTypedValues(string statements, string expected, string functions = "") {
    var result = Run(statements, functions);
    Assert.True(result.Success, result.Error);
    Assert.Equal(expected, Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void TuplesDoNotSpreadAndTablesRemainKeyed() {
    var result = Run("""
      local items = @[<a>, @[<b>, <c>]]
      local record = @{second=null, first=<one>}
      emit(length(local#items))
      emit(length(local#items#1))
      emit(has(local#record, <second>))
      emit(get(local#record, <second>, <fallback>))
      emit(get(local#record, <missing>, <fallback>))
      local replaced = put(local#record, <second>, <two>)
      emit(join(keys(local#replaced), <,>))
      emit(eq(local#replaced, @{first=<one>, second=<two>}))
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"2", "2", "true", "", "fallback", "second,first", "true"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void FunctionsHavePrivateLocalsAndPreserveTheCallersParameter() {
    var result = Run("local value = <caller>\nemit(outer(<input>))\nemit(local#value)", """
      pure func inner { local value = <callee>; return(param) }
      pure func outer {
        local value = <outer>
        local nested = inner(<nested>)
        return(<[param]:[local#value]:[local#nested]>)
      }
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"input:outer:nested", "caller"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void NamedSignaturesBindPositionalArgumentsAndValidateReturns() {
    var result = Run("emit(describe(<Ada>, number<3>))", """
      pure func describe sig @{name=string, count=number} -> string {
        return(<[$name]:[$count]>)
      }
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal("Ada:3", Assert.Single(result.Outputs).Text);
    var invalid = Run("emit(describe(number<3>, <Ada>))", "pure func describe sig @{name=string, count=number} -> string { return(<ok>) }");
    Assert.False(invalid.Success);
    Assert.Contains("signature", invalid.Error);
  }

  [Fact]
  public void SmartLookupIsLimitedToParametersAndTheCurrentLocalFrame() {
    var result = Run("""
      local visible = <local>
      emit($visible)
      emit(read(<parameter>))
      """, "pure func read { return($0) }");
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"local", "parameter"}, result.Outputs.Select(output => output.Text));

    var variable = Run("var hidden = <variable>\nemit($hidden)");
    Assert.False(variable.Success);
    Assert.Contains("unknown local or parameter 'hidden'", variable.Error);
    var target = Run("target var hidden = <target>\nemit($hidden)");
    Assert.False(target.Success);
    Assert.Contains("unknown local or parameter 'hidden'", target.Error);
  }

  [Fact]
  public void NamedSignatureSmartParametersBindByPosition() {
    var result = Run("emit(format(<left>, <right>))", """
      pure func format sig @{first=string, second=string} -> string {
        return(<[$1]:[$first]>)
      }
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal("right:left", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void SignatureMatchingAppliesOneDirectImplicitCoercion() {
    var declared = Run("emit(describe(12))", """
      pure func describe sig string -> string { return(param) }
      """);
    Assert.True(declared.Success, declared.Error);
    Assert.Equal("12", Assert.Single(declared.Outputs).Text);

    var builtin = Run("emit(identifier(34))");
    Assert.True(builtin.Success, builtin.Error);
    Assert.Equal("34", Assert.Single(builtin.Outputs).Text);

    var fromString = Run("emit(plus(<2>, 3))");
    Assert.True(fromString.Success, fromString.Error);
    Assert.Equal("5", Assert.Single(fromString.Outputs).Text);

    var exact = Run("emit(choose(56))", """
      pure func choose sig string -> string { return(<string>) }
      pure func choose sig number -> string { return(<number>) }
      """);
    Assert.True(exact.Success, exact.Error);
    Assert.Equal("number", Assert.Single(exact.Outputs).Text);
  }

  [Fact]
  public void CheckedFailuresCanBeHandledButElvisDoesNotCatchErrors() {
    var result = Run("local x = [error<bad>?]\nemit(kind(local#x))\nemit(catch(local#x) ?: <fallback>)");
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"error", "fallback"}, result.Outputs.Select(output => output.Text));
    Assert.False(Run("emit(error<bad> ?: <fallback>)").Success);
    Assert.False(Run("emit(catch(error<bad>))").Success);
  }

  [Fact]
  public void WhenChainsSelectValuesLazilyAndIndependentGuardsContinue() {
    var result = Run("""
      local suffix = when {
        (false, error<unreachable>) -> error<unreachable>
        (true) -> <, value>
        else -> error<unreachable>
      }
      when(true) { emit @> first{{local#suffix}}
      }
      when(false) { fail<unreachable> }
      when(true) { emit @> second
      }
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"first, value", "second"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void SelectionAndFallbackDoNotEvaluateUnselectedBranches() {
    var result = Run("""
      local selected = when <yes> {
        <yes> -> <selected>
        else -> error<unreachable>
      }
      emit(local#selected)
      emit([<present>] ?: error<unreachable>)
      when(false, error<unreachable>) { fail<unreachable> }
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"selected", "present"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void CollectionCallbacksSupportPositionalSmartAccess() {
    var result = Run("""
      local changed = map(@[number<2>, number<3>], describe)
      emit(join(local#changed, <,>))
      emit(reduce(@[number<2>, number<3>], sum, number<0>))
      emit(any(@[], yes))
      emit(all(@[], yes))
      """, """
      pure func describe { return(<value:[$0]>) }
      pure func sum { return(plus($0, $1)) }
      pure func yes { return(true) }
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"value:2,value:3", "5", "false", "true"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void AnonymousFunctionsSupportBlockAndArrowBodies() {
    const string statements = """
      emit(join(map(@[<a>, <b>], func => <[$0]!>), <,>))
      emit(join(map(@[<a>, <b>], func { return(<[$0]?>) }), <,>))
      emit(apply(<global>))
      """;
    var result = Run(statements, "pure func apply { return(call(func => <[$0]~>, $0)) }");
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"a!,b!", "a?,b?", "global~"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void PositionalSmartAccessPreservesTableValuedArguments() {
    var result = Run("""
      local items = @[@{value=@{parameter=<first>}}, @{value=@{parameter=<second>}}]
      emit(local#items:map(func => $0#value#parameter):join<,>)
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"first,second"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void SmartItAliasesTheCompleteParameterValue() {
    var result = Run("emit(single(<value>))\nemit(join(pair(<left>, <right>), <,>))", """
      pure func single { return($it) }
      pure func pair { return($it) }
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"value", "left,right"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void SmartNamesDoNotSelectFieldsFromTableParameters() {
    var result = Run("emit(read(@{name=<Ada>}))", "pure func read { return($name) }");
    Assert.False(result.Success);
    Assert.Contains("unknown local or parameter 'name'", result.Error);
  }


  [Fact]
  public void AnonymousFunctionsAreAlwaysPure() {
    var unit = AntlrSyntax.Parse("mixin Example { expression { local callback = func => emit(<bad>) } }");
    Assert.Contains(unit.Diagnostics, diagnostic => diagnostic.Message.Contains("pure functions cannot perform 'emit'"));
  }

  [Fact]
  public void NamedFunctionsSupportArrowBodiesIncludingEffects() {
    var result = Run("emit(answer())", """
      pure func answer => 42;
      func announce => emit(<announced>);
      """, "announce()");
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"announced", "42"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void CarriesCrossPhasesButPreludeLocalsDoNot() {
    var result = Run("emit(local#value)\nemit(local#hidden)", prelude: "local hidden = <private>\ncarry local value = <durable>");
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"durable", ""}, result.Outputs.Select(output => output.Text));
    Assert.True(Run("emit(this:name)").Success);
    var updated = Run("emit(local#value)", prelude: "carry local value = <ok>\nlocal value = <updated>");
    Assert.True(updated.Success, updated.Error);
    Assert.Equal("updated", Assert.Single(updated.Outputs).Text);
  }

  [Fact]
  public void CarriedLocalsBelongToTheUnitAndAreNotInheritedByFunctions() {
    var result = Run("emit(read())\nemit(local#value)",
      "pure func read { return(local#value) }", "carry local value = <unit>");
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"", "unit"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void CompilerExpandsInlineFunctionsBeforeHoistingHostValues() {
    var result = Run("emit(host())", "inline func host { return(this) }", context: new Context(new TestSymbol()));
    Assert.True(result.Success, result.Error);
    Assert.Equal("TestSymbol", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void InlineRewritePreservesLocalsAndEarlyReturns() {
    var result = Run("emit(choose(<selected>, true))", """
      inline func choose {
        local copy @= $0;
        when($1) { return(local#copy) }
        return(<fallback>)
      }
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal("selected", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void ExplicitInlineCallExpandsAnInlineableFunction() {
    var result = Run("emit(inline(decorate, <value>))", """
      func decorate {
        local copy @= $0;
        return(<[local#copy]!>)
      }
      """);
    Assert.True(result.Success, result.Error);
    Assert.Equal("value!", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void StrictExpressionDisablesAutomaticHoisting() {
    var unit = AntlrSyntax.Parse("mixin Example { strict expression { emit(this) } }");
    Assert.Empty(unit.Diagnostics);
    var result = MixinVirtualMachine.Execute(HixCompiler.Compile(unit, "Example"), new Context(new TestSymbol()));
    Assert.False(result.Success);
    Assert.Contains("prelude", result.Error);
  }

  [Fact]
  public void FailedCalleesRollBackWritesAndOutput() {
    var result = Run("emit(var#value)", """
      func broken { var value = <bad>; emit<discarded>; fail<bad> }
      """, "var value = <good>\nlocal failure = [broken()?]\nassert(is(local#failure, error))");
    Assert.True(result.Success, result.Error);
    Assert.Equal("good", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void CheckedControlFlowFailuresRollBackTheCallee() {
    var result = Run("emit(var#value)", """
      func broken { var value = <bad>; emit<discarded>; goto outside }
      """, "var value = <good>\nlocal failure = [broken()?]\nassert(is(local#failure, error))");
    Assert.True(result.Success, result.Error);
    Assert.Equal("good", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void SnapshotRoundTripPreservesCheckedErrorKinds() {
    var variables = new Dictionary<string, object>();
    Assert.True(Run("var failure = [error<saved>?]", variables: variables).Success);
    var result = Run("emit(kind(var#failure))\nemit(catch(var#failure) ?: <handled>)", variables: variables);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"error", "handled"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void LocalOverloadsShadowOnlyTheMatchingSignature() {
    var unit = AntlrSyntax.Parse("""
      pure func choose sig string -> string { return(<global string>) }
      pure func choose sig number -> string { return(<global number>) }
      mixin Example {
        pure func choose sig string -> string { return(<local string>) }
        expression { emit(choose(<text>)); emit(choose(number<2>)) }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = MixinVirtualMachine.Execute(HixCompiler.Compile(unit, "Example"), new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"local string", "global number"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void FunctionBindingsKeepTheirDeclarationScopeAcrossCalls() {
    var unit = AntlrSyntax.Parse("""
      pure func choose { return(<global>) }
      pure func exported { return(choose) }
      pure func invoke { return(call(param)) }
      mixin Example {
        pure func choose { return(<local>) }
        expression {
          emit(choose())
          emit(invoke(choose))
          emit(call(exported()))
        }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = MixinVirtualMachine.Execute(HixCompiler.Compile(unit, "Example"), new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"local", "local", "global"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void DerivationsUseTuplesAndPreserveRecordFieldsInProviderOrder() {
    var unit = AntlrSyntax.Parse("""
      derivation mixin First {
        expression { local original = [param#value]; return(<[local#original]:first>) }
        expression { return(<[param#value]:[local#original]>) }
      }
      derivation mixin Second {
        expression { return(<[param#value]:second>) }
      }
      mixin Example {
        prelude expression {
          local records = derive(@[@{symbol=this, value=<initial>, extra=<kept>}])
          carry local result = [local#records]
        }
        expression {
          emit(local#result#0#value)
          emit(local#result#0#extra)
        }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = MixinVirtualMachine.Execute(HixCompiler.Compile(unit, "Example"), new Context(new TestSymbol()));
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"initial:first:initial:second", "kept"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void CheckedDerivationFailureRollsBackEarlierProviders() {
    var unit = AntlrSyntax.Parse("""
      derivation mixin First {
        expression { var changed = <discarded>; emit<discarded>; return(null) }
      }
      derivation mixin Second {
        expression { assert(is(param#value, null)); fail<provider failed> }
      }
      mixin Example {
        prelude expression {
          var changed = <saved>
          local failure = [derive(@[@{symbol=this, value=<initial>}])?]
          assert(is(local#failure, error))
          emit(var#changed)
        }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = MixinVirtualMachine.Execute(HixCompiler.Compile(unit, "Example"), new Context(new TestSymbol()));
    Assert.True(result.Success, result.Error);
    Assert.Equal("saved", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void FailedExpressionsPreserveEarlierCommits() {
    var unit = AntlrSyntax.Parse("""
      mixin Example {
        expression { var value = <committed>; emit<first> }
        expression { var value = <discarded>; emit<discarded>; fail<stop> }
        expression { emit<unreachable> }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var variables = new Dictionary<string, object>();
    var result = MixinVirtualMachine.Execute(HixCompiler.Compile(unit, "Example"), new Context(), variables);
    Assert.False(result.Success);
    Assert.Equal("stop", result.Error);
    Assert.Equal("first", Assert.Single(result.Outputs).Text);
    Assert.Equal("committed", variables["value"]);
    Assert.Equal("committed", result.Variables["value"]);
  }

  [Fact]
  public void TargetStoragePersistsAcrossExecutionsButFailedWritesDoNot() {
    var context = new Context();
    MixinExpressionResult Execute(string body) {
      var unit = AntlrSyntax.Parse("mixin Example { prelude expression { " + body + " } }");
      Assert.Empty(unit.Diagnostics);
      return MixinVirtualMachine.Execute(HixCompiler.Compile(unit, "Example"), context);
    }
    Assert.True(Execute("target var value = <saved>").Success);
    Assert.False(Execute("target var value = <discarded>; fail<stop>").Success);
    var read = Execute("emit(tar#value)");
    Assert.True(read.Success, read.Error);
    Assert.Equal("saved", Assert.Single(read.Outputs).Text);
  }

  [Fact]
  public void BlocksAndBackwardJumpsRespectExecutionLimits() {
    var result = Run("{ emit<one>; break; emit<unreachable> }\nemit<two>");
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"one", "two"}, result.Outputs.Select(output => output.Text));
    var loop = Run(":again\ngoto again");
    Assert.False(loop.Success);
    Assert.Contains("limit", loop.Error);
  }

  private sealed class Context(IMixinValue symbol = null) : Mixins.Runtime.ExecutionContext(new MixinStringPoolBuilder().Freeze()) {
    protected override IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member) => symbol ?? NullMixinValue.Instance;
  }

  private sealed record TestSymbol : IMixinValue {
    public MixinValueKind Kind => MixinValueKind.Symbol;
    public bool IsTruthy(Mixins.Runtime.ExecutionContext context) => true;
    public MixinString Render(Mixins.Runtime.ExecutionContext context) => MixinString.Dynamic("TestSymbol");
    public void Fingerprint(MixinFingerprintBuilder builder, Mixins.Runtime.ExecutionContext context) => builder.Append("TestSymbol");
    public IMixinValue Select(Mixins.Runtime.ExecutionContext context, MixinString member) => NullMixinValue.Instance;
    public object Unlink(Mixins.Runtime.ExecutionContext context) => this;
    public bool Equals(IMixinValue other) => other is TestSymbol;
  }
}
