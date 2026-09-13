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
    IDictionary<string, object> variables = null, Hix.Runtime.HixCoroutine context = null) {
    var unit = AntlrSyntax.Parse(functions + "\nmixin Example {\nprelude expression {\n" + prelude +
      "\n}\nexpression {\n" + statements + "\n}\n}");
    Assert.Empty(unit.Diagnostics);
    return HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), context ?? new HixCoroutine(new Context()), variables);
  }

  [Theory]
  [InlineData("emit(<hello>)", "hello")]
  [InlineData("emit @> Hello\n@+  world\n", "Hello world")]
  [InlineData("emit @> Hello\n@> world\n", "Hello\nworld")]
  [InlineData("emit(<a[null]b>)", "ab")]
  [InlineData("emit(true)", "true")]
  [InlineData("emit(false)", "false")]
  [InlineData("emit(plus(cast(<2>, number), cast(<3>, number)))", "5")]
  [InlineData("emit(<A[b()]>)", "Ab", "pure func b() -> string { return(<b>) }")]
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
      emit(length(cast(local#items#1, tuple)))
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
      pure func inner(any __arg0) -> any { local value = <callee>; return($0) }
      pure func outer(any __arg0) -> string {
        local value = <outer>
        local nested = inner(<nested>)
        return(<[$0]:[local#value]:[local#nested]>)
      }
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"input:outer:nested", "caller"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void NamedSignaturesBindPositionalArgumentsAndValidateReturns() {
    var result = Run("emit(describe(<Ada>, cast(<3>, number)))", """
      pure func describe (string name, number count) -> string {
        return(<[$name]:[$count]>)
      }
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("Ada:3", Assert.Single(result.Outputs).ReadText());
    var invalid = Run("emit(describe(cast(<3>, number), <Ada>))", "pure func describe (string name, number count) -> string { return(<ok>) }");
    Assert.False(invalid.Success);
    Assert.Contains("signature", invalid.Error.Resolve(invalid.Strings));
  }

  [Fact]
  public void SmartLookupIsLimitedToParametersAndTheCurrentLocalFrame() {
    var result = Run("""
      local visible = <local>
      emit($visible)
      emit(read(<parameter>))
      """, "pure func read(any __arg0) -> any { return($0) }");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"local", "parameter"}, result.Outputs.Select(output => output.ReadText()));

    var variable = Run("var<hidden> @= <variable>;\nemit($hidden)");
    Assert.False(variable.Success);
    Assert.Contains("unknown local or parameter 'hidden'", variable.Error.Resolve(variable.Strings));
    var target = Run("tar<hidden> @= <target>;\nemit($hidden)");
    Assert.False(target.Success);
    Assert.Contains("unknown local or parameter 'hidden'", target.Error.Resolve(target.Strings));
  }

  [Fact]
  public void NamedSignatureSmartParametersBindByPosition() {
    var result = Run("emit(format(<left>, <right>))", """
      pure func format (string first, string second) -> string {
        return(<[$1]:[$first]>)
      }
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("right:left", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void SignatureMatchingAppliesOneDirectImplicitCoercion() {
    var declared = Run("emit(describe(12))", """
      pure func describe (string value) -> string { return($value) }
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
      pure func choose (string value) -> string { return(<string>) }
      pure func choose (number value) -> string { return(<number>) }
      """);
    Assert.True(exact.Success, exact.Error.Resolve(exact.Strings));
    Assert.Equal("number", Assert.Single(exact.Outputs).ReadText());
  }

  [Fact]
  public void CheckedFailuresCanBeHandledButElvisDoesNotCatchErrors() {
    var result = Run("local x = [cast(<bad>, error)?]\nemit(pattern(local#x))\nemit(catch(local#x) ?: <fallback>)");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"error", "fallback"}, result.Outputs.Select(output => output.ReadText()));
    Assert.False(Run("emit(cast(<bad>, error) ?: <fallback>)").Success);
    Assert.False(Run("emit(catch(cast(<bad>, error)))").Success);
  }

  [Fact]
  public void WhenChainsSelectValuesLazilyAndIndependentGuardsContinue() {
    var result = Run("""
      local suffix = when {
        (false, cast(<unreachable>, error)) -> cast(<unreachable>, error)
        (true) -> <, value>
        else -> cast(<unreachable>, error)
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
        else -> cast(<unreachable>, error)
      }
      emit(local#selected)
      emit([<present>] ?: cast(<unreachable>, error))
      when(false, cast(<unreachable>, error)) { fail<unreachable> }
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"selected", "present"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void CollectionCallbacksSupportPositionalSmartAccess() {
    var result = Run("""
      local changed = map(@[cast(<2>, number), cast(<3>, number)], describe)
      emit(join(local#changed, <,>))
      emit(reduce(@[cast(<2>, number), cast(<3>, number)], sum, cast(<0>, number)))
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
    var result = Run(statements, "pure func apply(any __arg0) -> any { return(call(func => <[$0]~>, $0)) }");
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
      pure func single [any] -> any { return($it) }
      pure func pair(any first, any second) -> tuple { return(@[$it#first, $it#second]) }
      """);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"value", "left,right"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void SmartNamesDoNotSelectFieldsFromTableParameters() {
    var result = Run("emit(read(@{name=<Ada>}))", "pure func read(any __arg0) -> any { return($name) }");
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
      pure func answer() -> number => 42;
      func announce() -> null => emit(<announced>);
      """, "announce()");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"announced", "42"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void CarriesCrossPhasesButPreludeLocalsDoNot() {
    var result = Run("emit(local#value)\nemit(local#hidden)", prelude: "local hidden = <private>\ncarry local value = <durable>");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"durable", "missing"}, result.Outputs.Select(output => output.ReadText()));
    Assert.True(Run("emit(this:name)").Success);
    var updated = Run("emit(local#value)", prelude: "carry local value = <ok>\n$value = <updated>");
    Assert.True(updated.Success, updated.Error.Resolve(updated.Strings));
    Assert.Equal("updated", Assert.Single(updated.Outputs).ReadText());
  }

  [Fact]
  public void CarriedLocalsBelongToTheUnitAndAreNotInheritedByFunctions() {
    var result = Run("emit(read())\nemit(local#value)",
      "pure func read() -> any { return(local#value) }", "carry local value = <unit>");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"missing", "unit"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void CompilerExpandsInlineFunctionsBeforeHoistingHostValues() {
    var result = Run("%inline emit(host())", "func host() -> any { return(this) }", context: new HixCoroutine(new Context(new TestSymbol())));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("TestSymbol", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void InlineRewritePreservesLocalsAndEarlyReturns() {
    var result = Run("%inline emit(choose(<selected>, true))", """
      func choose(any __arg0, any __arg1) -> any {
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
    var result = Run("%inline emit(decorate(<value>))", """
      func decorate(string value) -> string {
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
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context(new TestSymbol())));
    Assert.False(result.Success);
    Assert.Contains("prelude", result.Error.Resolve(result.Strings));
  }

  [Fact]
  public void FailedCalleesRollBackWritesAndOutput() {
    var result = Run("emit(var#value)", """
      func broken() -> any { var<value> @= <bad>; emit<discarded>; fail<bad> }
      """, "var<value> @= <good>;\nlocal failure = [broken()?]\nassert(is(local#failure, error))");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("good", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void CheckedControlFlowFailuresRollBackTheCallee() {
    var result = Run("emit(var#value)", """
      func broken() -> any { var<value> @= <bad>; emit<discarded>; goto outside }
      """, "var<value> @= <good>;\nlocal failure = [broken()?]\nassert(is(local#failure, error))");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("good", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void SnapshotRoundTripPreservesCheckedErrorKinds() {
    var variables = new Dictionary<string, object>();
    Assert.True(Run("var<failure> @= [cast(<saved>, error)?];", variables: variables).Success);
    var result = Run("emit(pattern(var#failure))\nemit(catch(var#failure) ?: <handled>)", variables: variables);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"error", "handled"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void LocalOverloadsShadowOnlyTheMatchingSignature() {
    var unit = AntlrSyntax.Parse("""
      pure func choose (string value) -> string { return(<global string>) }
      pure func choose (number value) -> string { return(<global number>) }
      mixin Example {
        pure func choose (string value) -> string { return(<local string>) }
        expression { emit(choose(<text>)); emit(choose(cast(<2>, number))) }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"local string", "global number"}, result.Outputs.Select(output => output.ReadText()));
  }

  private static IEnumerable<HixIrNode> DescendantsAndSelf(HixIrNode node) {
    yield return node;
    foreach (var child in node.Children)
      foreach (var descendant in DescendantsAndSelf(child)) yield return descendant;
  }

  [Fact]
  public void FunctionBindingsKeepTheirDeclarationScopeAcrossCalls() {
    var unit = AntlrSyntax.Parse("""
      pure func choose() -> string { return(<global>) }
      pure func exported() -> any { return(choose) }
      pure func invoke(any __arg0) -> any { return(call($0)) }
      mixin Example {
        pure func choose() -> string { return(<local>) }
        expression {
          emit(choose())
          emit(invoke(choose))
          emit(call(exported()))
        }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context()));
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
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context(new TestSymbol())));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"initial:first:initial:second", "kept"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void CheckedDerivationFailureRollsBackEarlierProviders() {
    var unit = AntlrSyntax.Parse("""
      derivation mixin First {
        expression { var<changed> @= <discarded>; emit<discarded>; return(null) }
      }
      derivation mixin Second {
        expression { assert(is(param#value, null)); fail<provider failed> }
      }
      mixin Example {
        prelude expression {
          var<changed> @= <saved>;
          local failure = [derive(@[@{symbol=this, value=<initial>}])?]
          assert(is(local#failure, error))
          emit(var#changed)
        }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context(new TestSymbol())));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("saved", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void DerivationBatchFailureRestoresCallerAndAllowsAnotherBatch() {
    var unit = AntlrSyntax.Parse("""
      derivation mixin Provider {
        expression {
          var<changed> @= <provider>;
          emit<provider>
          return(<[param#value]:derived>)
        }
      }
      mixin Example {
        prelude expression {
          local caller = <caller>
          var<changed> @= <saved>;
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
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example"), new HixCoroutine(new Context(new TestSymbol())));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"saved", "caller", "provider", "retry:derived", "caller"},
      result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void FailedExpressionsPreserveEarlierCommits() {
    var unit = AntlrSyntax.Parse("""
      mixin Example {
        expression { var<value> @= <committed>; emit<first> }
        expression { var<value> @= <discarded>; emit<discarded>; fail<stop> }
        expression { emit<unreachable> }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var variables = new Dictionary<string, object>();
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context()), variables);
    Assert.False(result.Success);
    Assert.Equal("stop", result.Error.Resolve(result.Strings));
    Assert.Equal("first", Assert.Single(result.Outputs).ReadText());
    Assert.Equal("committed", variables["value"]);
    Assert.Equal("committed", Assert.IsType<LiteralHixValue>(result.Variables[HixString.Dynamic("value")]).Value.Resolve(result.Strings));
  }

  [Fact]
  public void TargetStoragePersistsAcrossExecutionsButFailedWritesDoNot() {
    var context = new HixCoroutine(new Context());
    HixExecutionResult Execute(string body) {
      var unit = AntlrSyntax.Parse("mixin Example { prelude expression { " + body + " } }");
      Assert.Empty(unit.Diagnostics);
      return HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), context);
    }
    Assert.True(Execute("tar<value> @= <saved>;").Success);
    Assert.False(Execute("tar<value> @= <discarded>; fail<stop>").Success);
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
        local person = cast(@{name=<Ada>, aliases=@[<A>, <B>], attributes=@{role=<admin>}}, Person)
        local nullable = cast(null, NullableString)
        emit(local#person#name)
        emit(local#nullable)
        emit(cast(2, Positive))
      } }
      """;
    var unit = AntlrSyntax.Parse(source);
    Assert.Empty(unit.Diagnostics);
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"Ada", "", "2"}, result.Outputs.Select(output => output.ReadText()));

    var rejected = AntlrSyntax.Parse("type Positive = %min<1> number\nmixin Example { expression { emit(cast(0, Positive)) } }");
    Assert.Empty(rejected.Diagnostics);
    var failure = HixVM.Execute(TestCompiler.Compile(rejected, "Example", TestBackend.Instance), new HixCoroutine(new Context()));
    Assert.False(failure.Success);
    Assert.Contains("constraint failed", failure.Error.Resolve(failure.Strings));
  }

  [Fact]
  public void TableAndTuplePatternsProvideOrderedAndNamedConstructors() {
    var source = """
      type Person = @{string name, number age = [18], %optional string note}
      type Pair = @[string left, number right]
      %tagged type Pet = @{string name}
      mixin Example { expression {
        local named = Person(note = <ready>, name = <Ada>)
        local positional = Person(<Grace>, 21)
        local pair = Pair(<left>, 2)
        local namedPair = Pair(right = 4, left = <named>)
        local identity = cast(@{name=<Existing>, age=30}, Person)
        local pet = Pet(name = <Milo>)
        emit(local#named#name)
        emit(local#named#age)
        emit(local#named#note)
        emit(local#positional#age)
        emit(local#pair#0)
        emit(local#namedPair#1)
        emit(local#identity#name)
        emit(get(local#pet, <$type>))
      } }
      """;
    var unit = AntlrSyntax.Parse(source);
    Assert.Empty(unit.Diagnostics);
    var program = TestCompiler.Compile(unit, "Example", TestBackend.Instance);
    var vm = new HixVM([program]);
    // Constructor methods belong to the image and must work across independent invocations.
    for (var run = 0; run < 2; run++) {
      var result = vm.Run(program, new HixCoroutine(new Context()));
      Assert.True(result.Success, result.Error.Resolve(result.Strings));
      Assert.Equal(new[] {"Ada", "18", "ready", "21", "left", "4", "Existing", "Pet"},
        result.Outputs.Select(output => output.ReadText()));
    }
  }

  [Fact]
  public void PatternConstructorDiagnosticsDescribeTheClosestSignatureAndMissingFields() {
    var unit = AntlrSyntax.Parse("""
      type Person = @{string name, %optional number age}
      mixin Example { expression { local person = Person(age = 3) } }
      """);

    Assert.Throws<ArgumentException>(() => TestCompiler.Compile(unit, "Example", TestBackend.Instance));
    var diagnostic = Assert.Single(unit.Diagnostics.Where(item => item.Message.Contains("no constructor of 'Person'")));
    Assert.Contains("missing required argument 'name'", diagnostic.Message);
    Assert.Contains("Person(name: string, age: number?)", diagnostic.Message);
  }

  [Fact]
  public void InferredCallsUseStaticSignaturesAndCastsNarrowVariables() {
    var unit = AntlrSyntax.Parse("""
      pure func choose(string value) -> string { return(<string>) }
      pure func choose(number value) -> string { return(<number>) }
      mixin Example { expression {
        local known = choose(<value>)
        var<dynamic> @= <value>;
        local unknown = choose(cast(var#dynamic, string))
        emit(local#known)
        emit(local#unknown)
      } }
      """);
    Assert.Empty(unit.Diagnostics);
    var program = TestCompiler.Compile(unit, "Example", TestBackend.Instance);
    var dump = program.Disassemble();
    Assert.Contains("CALL c", dump);
    Assert.All(program.ConstantPool.OfType<FunctionReferenceHixValue>().Where(reference => reference.Signature.Name == "choose"),
      reference => Assert.Equal("string", reference.Signature.Parameters[0].Pattern.Display));
    Assert.Contains("static emit(any", dump);
    var result = HixVM.Execute(program, new HixCoroutine(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"string", "string"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void NamedCallsLowerToPositionalSlotsAndApplyFunctionDefaults() {
    var unit = AntlrSyntax.Parse("""
      pure func choose(string first, string second = [<default>]) -> string { return($second) }
      mixin Example { expression {
        emit(choose(first = <one>))
        emit(choose(second = <named>, first = <one>))
      } }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"default", "named"}, result.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void OptionalPresenceAndNullableValuesAreDistinct() {
    var unit = AntlrSyntax.Parse("""
      pure func absentIsMissing(%optional string value) -> bool { return($value:eq(missing)) }
      pure func absentExists(%optional string value) -> bool { return($value:exists) }
      pure func nullable(string? value) -> bool { return($value:eq(null)) }
      pure func acceptsNull(null value) -> bool { return($value:eq(null)) }
      mixin Example { expression {
        emit(absentIsMissing())
        emit(absentExists())
        emit(nullable(null))
        emit(acceptsNull(missing))
      } }
      """);
    Assert.Empty(unit.Diagnostics);
    Assert.Equal(2, unit.Declarations.SelectMany(DescendantsAndSelf).Count(node => node is MissingExpressionIr));
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"true", "false", "true", "true"}, result.Outputs.Select(output => output.ReadText()));

    var rejected = AntlrSyntax.Parse("""
      pure func absent(%optional string value) -> null { return }
      mixin Example { expression { absent(null) } }
      """);
    Assert.Empty(rejected.Diagnostics);
    var failure = HixVM.Execute(TestCompiler.Compile(rejected, "Example", TestBackend.Instance), new HixCoroutine(new Context()));
    Assert.False(failure.Success);
    Assert.Contains("no matching signature", failure.Error.Resolve(failure.Strings));
  }

  [Fact]
  public void MixinParametersResolveThroughSmartSlotsLazily() {
    var unit = AntlrSyntax.Parse("""
      mixin Example(string used = [<ready>], number unused) {
        expression { emit($used) }
      }
      """);
    Assert.Empty(unit.Diagnostics);
    var result = HixVM.Execute(TestCompiler.Compile(unit, "Example", TestBackend.Instance), new HixCoroutine(new Context()));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("ready", Assert.Single(result.Outputs).ReadText());
  }

  [Fact]
  public void BackendCallsInsideGlobalFunctionsAndDerivationsArePreparedStatically() {
    var unit = AntlrSyntax.Parse("""
      func inspect(opaque value) -> string { return(param#value:name) }
      derivation mixin Base { expression { local inspected @= param:name; } }
      mixin Example { expression { } }
      """, TestBackend.Instance);
    Assert.Empty(unit.Diagnostics);

    var program = TestCompiler.Compile(unit, "Example", TestBackend.Instance);
    var dump = program.Disassemble();

    Assert.Contains("static name(any arg0) -> string", dump);
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
    var result = HixVM.Execute(program, new HixCoroutine(new Context()));
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
      mixin Example { expression { var<value> @= <x>; local result = call(choose, var#value) } }
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
  public void DynamicArrowFunctionsAcceptAnyArgumentShape() {
    var result = Run("emit(call(inspect, <left>, 2))", """
      pure func inspect -> string { return(<[pattern($it)]-[param#0]-[param#1]>) }
      """);

    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("tuple-left-2", Assert.Single(result.Outputs).ReadText());
  }

  [Theory]
  [InlineData("local value = 1; local value = 2", "already declared")]
  [InlineData("$value = 2", "undeclared local")]
  [InlineData("local(<value>, 2)", "locals cannot be modified")]
  public void InvalidLocalMutationFormsProduceDiagnostics(string statements, string message) {
    var unit = AntlrSyntax.Parse("mixin Example { expression { " + statements + " } }", TestBackend.Instance);

    Assert.Contains(unit.Diagnostics, diagnostic => diagnostic.Message.Contains(message));
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

  private sealed class Context(IHixValue symbol = null) : HixExpressionContext(TestBackend.Instance, new HixStringPoolBuilder().Freeze()) {
    public override IHixValue ResolveHost(HixCoroutine thread, HixExpressionRoot root, HixString member) => symbol ?? NullHixValue.Instance;
  }

  private sealed record TestSymbol : IHixValue {
    public HixValueKind Kind => HixValueKind.Opaque;
    public bool IsTruthy(Hix.Runtime.HixCoroutine context) => true;
    public HixString Render(Hix.Runtime.HixCoroutine context) => HixString.Dynamic("TestSymbol");
    public void Fingerprint(HixFingerprintBuilder builder, Hix.Runtime.HixCoroutine context) => builder.Append("TestSymbol");
    public IHixValue Select(Hix.Runtime.HixCoroutine context, HixString member) => NullHixValue.Instance;
    public object Unlink(Hix.Runtime.HixCoroutine context) => this;
    public bool Equals(IHixValue other) => other is TestSymbol;
  }
}
