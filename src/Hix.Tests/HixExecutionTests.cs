using System;
using System.Collections.Generic;
using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixExecutionTests {
  private static HixExecutionResult Run(string statements, string functions = "", string prelude = "",
    IDictionary<string, object> variables = null, Hix.Runtime.HixThread context = null) {
    var unit = AntlrSyntax.Parse(functions + "\nmixin Example {\nprelude expression {\n" + prelude +
      "\n}\nexpression {\n" + statements + "\n}\n}");
    Assert.Empty(unit.Diagnostics);
    return HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), context ?? new HixThread(new Context()), variables);
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
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(expected, Assert.Single(result.Outputs).ReadText());
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
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    var outputs = result.Outputs.Select(output => output.ReadText()).ToArray();
    Assert.Equal(new[] {"2", "2", "true", "", "fallback"}, outputs.Take(5));
    Assert.Equal(new[] {"first", "second"}, outputs[5].Split(',').OrderBy(key => key));
    Assert.Equal("true", outputs[6]);
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
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"input:outer:nested", "caller"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void NamedSignaturesBindPositionalArgumentsAndValidateReturns() {
    var result = Run("emit(describe(<Ada>, number<3>))", """
      pure func describe sig @{name=string, count=number} -> string {
        return(<[$name]:[$count]>)
      }
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("Ada:3", Assert.Single(result.Outputs).ReadText());
    var invalid = Run("emit(describe(number<3>, <Ada>))", "pure func describe sig @{name=string, count=number} -> string { return(<ok>) }");
    Assert.False(invalid.Success);
    Assert.Contains("signature", invalid.Error.Resolve(invalid.Strings));
  }

  [Fact]
  public void SmartLookupIsLimitedToParametersAndTheCurrentLocalFrame() {
    var result = Run("""
      local visible = <local>
      emit($visible)
      emit(read(<parameter>))
      """, "pure func read { return($0) }");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"local", "parameter"}, result.Outputs.Select(output => output.ReadText()));

    var variable = Run("var hidden = <variable>\nemit($hidden)");
    Assert.False(variable.Success);
    Assert.Contains("unknown local or parameter 'hidden'", variable.Error.Resolve(variable.Strings));
    var target = Run("target var hidden = <target>\nemit($hidden)");
    Assert.False(target.Success);
    Assert.Contains("unknown local or parameter 'hidden'", target.Error.Resolve(target.Strings));
  }

  [Fact]
  public void NamedSignatureSmartParametersBindByPosition() {
    var result = Run("emit(format(<left>, <right>))", """
      pure func format sig @{first=string, second=string} -> string {
        return(<[$1]:[$first]>)
      }
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("right:left", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void SignatureMatchingAppliesOneDirectImplicitCoercion() {
    var declared = Run("emit(describe(12))", """
      pure func describe sig string -> string { return(param) }
      """);
    Assert.True(declared.Success, declared.Error.Resolve(declared.Strings));
    Assert.Equal("12", Assert.Single(declared.Outputs).ReadText());

    var builtin = Run("emit(trim(34))");
    Assert.True(builtin.Success, builtin.Error.Resolve(builtin.Strings));
    Assert.Equal("34", Assert.Single(builtin.Outputs).ReadText());

    var fromString = Run("emit(plus(<2>, 3))");
    Assert.True(fromString.Success, fromString.Error.Resolve(fromString.Strings));
    Assert.Equal("5", Assert.Single(fromString.Outputs).ReadText());

    var exact = Run("emit(choose(56))", """
      pure func choose sig string -> string { return(<string>) }
      pure func choose sig number -> string { return(<number>) }
      """);
    Assert.True(exact.Success, exact.Error.Resolve(exact.Strings));
    Assert.Equal("number", Assert.Single(exact.Outputs).ReadText());
  }

  [Fact]
  public void CheckedFailuresCanBeHandledButElvisDoesNotCatchErrors() {
    var result = Run("local x = [error<bad>?]\nemit(kind(local#x))\nemit(catch(local#x) ?: <fallback>)");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"error", "fallback"}, result.Outputs.Select(output => output.ReadText()));
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
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"first, value", "second"}, result.Outputs.Select(output => output.ReadText()));
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
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"selected", "present"}, result.Outputs.Select(output => output.ReadText()));
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
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"value:2,value:3", "5", "false", "true"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void AnonymousFunctionsSupportBlockAndArrowBodies() {
    const string statements = """
      emit(join(map(@[<a>, <b>], func => <[$0]!>), <,>))
      emit(join(map(@[<a>, <b>], func { return(<[$0]?>) }), <,>))
      emit(apply(<global>))
      """;
    var result = Run(statements, "pure func apply { return(call(func => <[$0]~>, $0)) }");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"a!,b!", "a?,b?", "global~"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void PositionalSmartAccessPreservesTableValuedArguments() {
    var result = Run("""
      local items = @[@{value=@{parameter=<first>}}, @{value=@{parameter=<second>}}]
      emit(local#items:map(func => $0#value#parameter):join<,>)
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"first,second"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void SmartItAliasesTheCompleteParameterValue() {
    var result = Run("emit(single(<value>))\nemit(join(pair(<left>, <right>), <,>))", """
      pure func single { return($it) }
      pure func pair { return($it) }
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"value", "left,right"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void SmartNamesDoNotSelectFieldsFromTableParameters() {
    var result = Run("emit(read(@{name=<Ada>}))", "pure func read { return($name) }");
    Assert.False(result.Success);
    Assert.Contains("unknown local or parameter 'name'", result.Error.Resolve(result.Strings));
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
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"announced", "42"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void CarriesCrossPhasesButPreludeLocalsDoNot() {
    var result = Run("emit(local#value)\nemit(local#hidden)", prelude: "local hidden = <private>\ncarry local value = <durable>");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"durable", ""}, result.Outputs.Select(output => output.ReadText()));
    Assert.True(Run("emit(this:name)").Success);
    var updated = Run("emit(local#value)", prelude: "carry local value = <ok>\nlocal value = <updated>");
    Assert.True(updated.Success, updated.Error.Resolve(updated.Strings));
    Assert.Equal("updated", Assert.Single(updated.Outputs).ReadText());
  }

  [Fact]
  public void CarriedLocalsBelongToTheUnitAndAreNotInheritedByFunctions() {
    var result = Run("emit(read())\nemit(local#value)",
      "pure func read { return(local#value) }", "carry local value = <unit>");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"", "unit"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void CompilerExpandsInlineFunctionsBeforeHoistingHostValues() {
    var result = Run("emit(host())", "inline func host { return(this) }", context: new HixThread(new Context(new TestSymbol())));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("TestSymbol", Assert.Single(result.Outputs).ReadText());
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
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("selected", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void ExplicitInlineCallExpandsAnInlineableFunction() {
    var result = Run("emit(inline(decorate, <value>))", """
      func decorate {
        local copy @= $0;
        return(<[local#copy]!>)
      }
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("value!", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void StrictExpressionDisablesAutomaticHoisting() {
    var unit = AntlrSyntax.Parse("mixin Example { strict expression { emit(this) } }");
    Assert.Empty(unit.Diagnostics);
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixThread(new Context(new TestSymbol())));
    Assert.False(result.Success);
    Assert.Contains("prelude", result.Error.Resolve(result.Strings));
  }

  [Fact]
  public void FailedCalleesRollBackWritesAndOutput() {
    var result = Run("emit(var#value)", """
      func broken { var value = <bad>; emit<discarded>; fail<bad> }
      """, "var value = <good>\nlocal failure = [broken()?]\nassert(is(local#failure, error))");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("good", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void CheckedControlFlowFailuresRollBackTheCallee() {
    var result = Run("emit(var#value)", """
      func broken { var value = <bad>; emit<discarded>; goto outside }
      """, "var value = <good>\nlocal failure = [broken()?]\nassert(is(local#failure, error))");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("good", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void SnapshotRoundTripPreservesCheckedErrorKinds() {
    var variables = new Dictionary<string, object>();
    Assert.True(Run("var failure = [error<saved>?]", variables: variables).Success);
    var result = Run("emit(kind(var#failure))\nemit(catch(var#failure) ?: <handled>)", variables: variables);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"error", "handled"}, result.Outputs.Select(output => output.ReadText()));
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
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixThread(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"local string", "global number"}, result.Outputs.Select(output => output.ReadText()));
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
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixThread(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"local", "local", "global"}, result.Outputs.Select(output => output.ReadText()));
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
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixThread(new Context(new TestSymbol())));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"initial:first:initial:second", "kept"}, result.Outputs.Select(output => output.ReadText()));
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
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixThread(new Context(new TestSymbol())));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("saved", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void DerivationBatchFailureRestoresCallerAndAllowsAnotherBatch() {
    var unit = AntlrSyntax.Parse("""
      derivation mixin Provider {
        expression {
          var changed = <provider>
          emit<provider>
          return(<[param#value]:derived>)
        }
      }
      mixin Example {
        prelude expression {
          local caller = <caller>
          var changed = <saved>
          local failure = [derive(@[
            @{symbol=this, value=<first>},
            @{symbol=null, missing=<invalid>}
          ])?]
          assert(is(local#failure, error))
          emit(var#changed)
          emit(local#caller)
          local recovered = derive(@[@{symbol=this, value=<retry>}])
          emit(local#recovered#0#value)
          emit(local#caller)
        }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example"), new HixThread(new Context(new TestSymbol())));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"saved", "caller", "provider", "retry:derived", "caller"},
      result.Outputs.Select(output => output.ReadText()));
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
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixThread(new Context()), variables);
    Assert.False(result.Success);
    Assert.Equal("stop", result.Error.Resolve(result.Strings));
    Assert.Equal("first", Assert.Single(result.Outputs).ReadText());
    Assert.Equal("committed", variables["value"]);
    Assert.Equal("committed", Assert.IsType<LiteralHixValue>(result.Variables[HixString.Dynamic("value")]).Value.Resolve(result.Strings));
  }

  [Fact]
  public void TargetStoragePersistsAcrossExecutionsButFailedWritesDoNot() {
    var context = new HixThread(new Context());
    HixExecutionResult Execute(string body) {
      var unit = AntlrSyntax.Parse("mixin Example { prelude expression { " + body + " } }");
      Assert.Empty(unit.Diagnostics);
      return HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), context);
    }
    Assert.True(Execute("target var value = <saved>").Success);
    Assert.False(Execute("target var value = <discarded>; fail<stop>").Success);
    var read = Execute("emit(tar#value)");
    Assert.True(read.Success, read.Error.Resolve(read.Strings));
    Assert.Equal("saved", Assert.Single(read.Outputs).ReadText());
  }

  [Fact]
  public void BlocksAndBackwardJumpsRespectExecutionLimits() {
    var result = Run("{ emit<one>; break; emit<unreachable> }\nemit<two>");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"one", "two"}, result.Outputs.Select(output => output.ReadText()));
    var loop = Run(":again\ngoto again");
    Assert.False(loop.Success);
    Assert.Contains("limit", loop.Error.Resolve(loop.Strings));
  }

  [Fact]
  public void PatternsValidateStructuresUnionsCollectionsAndConstraints() {
    var source = """
      type NullableString = %union<string><null>
      type Person = @{string name, %optional number age, %many<string> aliases, %map<string,string> attributes}
      type Positive = %min<1> number
      type Handler = delegate(Person self, string value) -> null
      mixin Example { expression {
        local person = Person(@{name=<Ada>, aliases=@[<A>, <B>], attributes=@{role=<admin>}})
        local nullable = NullableString(null)
        emit(local#person#name)
        emit(local#nullable)
        emit(Positive(2))
      } }
      """;
    var unit = AntlrSyntax.Parse(source);
    Assert.Empty(unit.Diagnostics);
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixThread(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"Ada", "", "2"}, result.Outputs.Select(output => output.ReadText()));

    var rejected = AntlrSyntax.Parse("type Positive = %min<1> number\nmixin Example { expression { emit(Positive(0)) } }");
    Assert.Empty(rejected.Diagnostics);
    var failure = HixVM.Execute(TestCompiler.Compile(rejected, "Example", TestBackend.Instance), new HixThread(new Context()));
    Assert.False(failure.Success);
    Assert.Contains("constraint failed", failure.Error.Resolve(failure.Strings));
  }

  [Fact]
  public void InferredCallsUseStaticSignaturesWhileVariablesRemainDynamic() {
    var unit = AntlrSyntax.Parse("""
      pure func choose(string value) -> string { return(<string>) }
      pure func choose(number value) -> string { return(<number>) }
      mixin Example { expression {
        local known = choose(<value>)
        var dynamic = <value>
        local unknown = choose(var#dynamic)
        emit(local#known)
        emit(local#unknown)
      } }
      """);
    Assert.Empty(unit.Diagnostics);
    var program = TestCompiler.Compile(unit, "Example", TestBackend.Instance);
    var dump = program.Disassemble();
    Assert.Contains("CALL c", dump);
    Assert.Single(HixInstruction.ReadAll(program.Bytecode)
      .Where(item => item.Instruction.Opcode == HixOpcode.CallDynamic));
    Assert.Contains("static emit(any", dump);
    var result = HixVM.Execute(program, new HixThread(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"string", "string"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void BackendCallsInsideGlobalFunctionsAndDerivationsArePreparedStatically() {
    var unit = AntlrSyntax.Parse("""
      func inspect(symbol value) -> string { return(param#value:name) }
      derivation mixin Base { expression { local inspected @= param:name; } }
      mixin Example { expression { } }
      """, TestBackend.Instance);
    Assert.Empty(unit.Diagnostics);

    var program = TestCompiler.Compile(unit, "Example", TestBackend.Instance);
    var dump = program.Disassemble();

    Assert.Contains("static name(any 0) -> string", dump);
    Assert.DoesNotContain("dynamic name(", dump);
  }

  [Fact]
  public void ParameterListsOnlyAcceptPositionalArgumentsNotNamedTables() {
    const string function = "pure func combine(string left, string right) -> string { return(<[$left][$right]>) }\n";
    var invalid = AntlrSyntax.Parse(function +
      "mixin Example { expression { emit(combine(@{left=<a>, right=<b>})) } }");
    Assert.Contains(invalid.Diagnostics, diagnostic =>
      diagnostic.Message == "no overload of 'combine' accepts (@{string left, string right})");

    var valid = AntlrSyntax.Parse(function +
      "mixin Example { expression { emit(combine(<a>, <b>)) } }");
    Assert.Empty(valid.Diagnostics);
    var program = TestCompiler.Compile(valid, "Example", TestBackend.Instance);
    Assert.Contains("static combine(string left, string right) -> string", program.Disassemble());
    var result = HixVM.Execute(program, new HixThread(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("ab", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void AnalyzerChecksBuiltinCallsAndPublishesInferredTypeFacts() {
    var invalid = AntlrSyntax.Parse("mixin Example { expression { replaceAll(true) } }");
    Assert.Contains(invalid.Diagnostics, diagnostic =>
      diagnostic.Message == "no overload of 'replaceAll' accepts 1 argument(s)");

    var analysis = new LanguageAnalysis("mixin Example { expression { local value = lowercase(<TEXT>) } }");
    Assert.Contains(analysis.TypeFacts, fact => fact.Inlay && fact.Type == "string" &&
      fact.Documentation == "local value: string");
    Assert.Contains(analysis.TypeFacts, fact => !fact.Inlay && fact.Type == "string" &&
      fact.Documentation.Contains("lowercase(string) -> string"));

    var coercion = new LanguageAnalysis(
      "mixin Example { expression { local value = replaceAll(true, <r>, <x>) } }");
    Assert.Contains(coercion.TypeFacts, fact => fact.Kind == "Coercion" && fact.Inlay && fact.Type == "string");

    var dynamic = new LanguageAnalysis("""
      pure func choose(string value) -> string { return(<string>) }
      pure func choose(number value) -> string { return(<number>) }
      mixin Example { expression { var value = <x>; local result = choose(var#value) } }
      """);
    Assert.Contains(dynamic.TypeFacts, fact => fact.Kind == "DynamicCall" && fact.Documentation.Contains("runtime"));

    var incompleteHostCall = new LanguageAnalysis("mixin Example { expression { emit() } }");
    Assert.Contains(incompleteHostCall.TypeFacts, fact => fact.Kind == "Call" &&
      fact.Documentation.Contains("emit("));

    var nestedWhen = new LanguageAnalysis("""
      mixin Example { expression {
        when(true) { replaceAll(true) }
        else { lowercase(<TEXT>) }
      } }
      """);
    Assert.Contains(nestedWhen.Program.Diagnostics, diagnostic =>
      diagnostic.Message == "no overload of 'replaceAll' accepts 1 argument(s)");
    Assert.Contains(nestedWhen.TypeFacts, fact => fact.Kind == "Call" &&
      fact.Documentation.Contains("lowercase(string) -> string"));

    var uncertainSelection = new LanguageAnalysis("""
      mixin Example { expression {
        local value = when {
          (true) -> <known>
          else -> unknown()
        }
      } }
      """);
    Assert.Empty(uncertainSelection.Program.Diagnostics);
    Assert.Contains(uncertainSelection.TypeFacts, fact => fact.Inlay && fact.Type == "any" &&
      fact.Documentation == "local value: any");
    Assert.Equal("any", HixPatterns.Union([
      new KindHixPattern(HixValueKind.String), HixPattern.Any
    ]).Display);
  }

  [Fact]
  public void EditorAnalysisRetainsValidDeclarationsAroundIncompleteSyntax() {
    const string source = "type Existing = string\n%";

    var editor = new LanguageAnalysis(source, recoverValidDeclarations: true);
    Assert.NotEmpty(editor.Program.Diagnostics);
    Assert.Contains(editor.Declarations, symbol => symbol is {Kind: "Pattern", Name: "Existing"});

    var compiler = new LanguageAnalysis(source);
    Assert.NotEmpty(compiler.Program.Diagnostics);
    Assert.Empty(compiler.Declarations);
  }

  [Fact]
  public void EditorAnalysisRetainsDeclarationsBelowIncompleteHeaderMetadata() {
    const string source = "%\n---\ntype Existing = string\nmixin Example { expression { emit(<ok>) } }";

    var editor = new LanguageAnalysis(source, recoverValidDeclarations: true);

    Assert.Contains(editor.Declarations, symbol => symbol is {Kind: "Pattern", Name: "Existing"});
    Assert.Contains(editor.Declarations, symbol => symbol is {Kind: "Mixin", Name: "Example"});
  }

  private sealed class Context(IHixValue symbol = null) : HixContext(TestBackend.Instance, new HixStringPoolBuilder().Freeze()) {
    public override IHixValue ResolveHost(HixThread thread, HixExpressionRoot root, HixString member) => symbol ?? NullHixValue.Instance;
  }

  private sealed record TestSymbol : IHixValue {
    public HixValueKind Kind => HixValueKind.Symbol;
    public bool IsTruthy(Hix.Runtime.HixThread context) => true;
    public HixString Render(Hix.Runtime.HixThread context) => HixString.Dynamic("TestSymbol");
    public void Fingerprint(HixFingerprintBuilder builder, Hix.Runtime.HixThread context) => builder.Append("TestSymbol");
    public IHixValue Select(Hix.Runtime.HixThread context, HixString member) => NullHixValue.Instance;
    public object Unlink(Hix.Runtime.HixThread context) => this;
    public bool Equals(IHixValue other) => other is TestSymbol;
  }
}
