using System;
using System.Collections.Generic;
using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixThreadTests {
  [Fact]
  public void ContextOwnsVariablesAndCarriesAcrossThreadsAndProgramPools() {
    var context = new HixContext(TestBackend.Instance);
    var first = TestCompiler.Compile("""
      mixin Example { prelude expression {
        var saved = <value>
        carry local carried = null
        local transient = <private>
      } }
      """, "Example");
    Assert.True(HixVM.Execute(first, new HixThread(context)).Success);
    Assert.True(context.Carries.ContainsKey(HixString.Dynamic("carried")));
    Assert.Equal("value", Assert.IsType<LiteralHixValue>(context.Variables[HixString.Dynamic("saved")]).Value.Resolve(null));
    var second = TestCompiler.Compile("""
      mixin Example { expression {
        local carried = <updated>
        emit(var#saved)
        emit(local#carried)
        emit(local#transient)
      } }
      """, "Example");
    var result = HixVM.Execute(second, new HixThread(context));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"value", "updated", ""}, result.Outputs.Select(output => output.ReadText()));
    Assert.Equal("updated", Assert.IsType<LiteralHixValue>(context.Carries[HixString.Dynamic("carried")]).Value.Resolve(null));
    var failing = TestCompiler.Compile("""
      mixin Example { expression { var saved = <discard>; local carried = <discard>; fail<stop> } }
      """, "Example");
    Assert.False(HixVM.Execute(failing, new HixThread(context)).Success);
    Assert.Equal("value", Assert.IsType<LiteralHixValue>(context.Variables[HixString.Dynamic("saved")]).Value.Resolve(null));
    Assert.Equal("updated", Assert.IsType<LiteralHixValue>(context.Carries[HixString.Dynamic("carried")]).Value.Resolve(null));
  }

  [Fact]
  public void ReuseResetsInvocationStateAndPreservesCommittedTargets() {
    var program = TestCompiler.Compile("""
      mixin Example { expression {
        emit(var#input)
        target var saved = [var#input]
      } }
      """, "Example");
    var thread = TestBackend.Instance.CreateThread();
    var first = HixVM.Execute(program, thread, new Dictionary<string, object> { ["input"] = "first" });
    var second = HixVM.Execute(program, thread, new Dictionary<string, object> { ["input"] = "second" });
    Assert.True(first.Success, first.Error.Resolve(first.Strings));
    Assert.True(second.Success, second.Error.Resolve(second.Strings));
    Assert.Equal("first", Assert.Single(first.Outputs).ReadText());
    Assert.Equal("second", Assert.Single(second.Outputs).ReadText());
    Assert.Equal(first.ExecutedOperations, second.ExecutedOperations);
    Assert.Equal("second", thread.Resolve(HixExpressionRoot.TargetVariable, HixString.Dynamic("saved")).Unlink(thread));
    Assert.False(thread.IsRunning);
    Assert.False(thread.IsPrelude);
    Assert.IsType<ErrorHixValue>(thread.Invoke(new NamedFunctionHixValue(HixString.Dynamic("missing")), []));
  }

  [Fact]
  public void IndependentThreadsShareTheCachedVmAcrossDifferentSeedPools() {
    var program = TestCompiler.Compile("mixin Example { expression { emit(var#input) } }", "Example");
    var threads = Enumerable.Range(0, 8).Select(_ => new HixThread(TestBackend.Instance, new HixStringPoolBuilder().Freeze())).ToArray();
    System.Threading.Tasks.Parallel.For(0, threads.Length, index => {
      var result = HixVM.Execute(program, threads[index], new Dictionary<string, object> { ["input"] = index });
      Assert.True(result.Success, result.Error.Resolve(result.Strings));
      Assert.Equal(index.ToString(), Assert.Single(result.Outputs).ReadText());
    });
    Assert.All(threads, thread => Assert.Same(threads[0].Machine, thread.Machine));
  }

  [Fact]
  public void HostCallsSeeCurrentFrameAndCannotRestartTheActiveThread() {
    var backend = new ProbeBackend();
    var program = HixCompiler.CompileFunctions(["""
      pure func identity { return(param) }
      pure func resolved sig number -> number { return(param) }
      pure func main { local current = [param]; return(probe(identity)) }
      """], backend);
    var machine = new HixVM([program]);
    var thread = backend.CreateThread();
    backend.Probe = active => {
      Assert.Same(thread, active);
      var candidate = Assert.Single(program.Scope.Candidates("resolved"));
      var reference = new ResolvedFunctionHixValue(candidate.Signature.Constant("resolved"), candidate);
      Assert.Equal(17, Assert.IsType<NumberHixValue>(active.Invoke(reference, [new NumberHixValue(17)])).Value);
      Assert.True(active.IsRunning);
      Assert.Equal(42d, active.Resolve(HixExpressionRoot.Parameter, HixString.Dynamic("")).Unlink(active));
      Assert.Equal(42d, active.Resolve(HixExpressionRoot.Local, HixString.Dynamic("current")).Unlink(active));
      Assert.Throws<InvalidOperationException>(() => machine.Invoke(program, active));
      var competing = new HixThread(active.Context);
      Assert.Throws<InvalidOperationException>(() => machine.Invoke(program, competing));
      Assert.False(competing.IsRunning);
      Assert.True(active.IsRunning);
    };
    var result = machine.Invoke(program, thread, "main", new NumberHixValue(42));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(42, Assert.IsType<NumberHixValue>(result.Value).Value);
    Assert.False(thread.IsRunning);
    Assert.IsType<NullHixValue>(thread.Resolve(HixExpressionRoot.Local, HixString.Dynamic("current")));
  }

  [Fact]
  public void ContextRetainsTargetDataAcrossFreshThreadsAndFailedRuns() {
    var program = TestCompiler.Compile("""
      mixin Example { prelude expression {
        emit(tar#saved)
        target var saved = [this]
        emit(this)
      } }
      """, "Example");
    var context = new TargetContext("first");
    var first = HixVM.Execute(program, new HixThread(context));
    var second = HixVM.Execute(program, new HixThread(context));
    var other = HixVM.Execute(program, new HixThread(new TargetContext("other")));
    Assert.True(first.Success, first.Error.Resolve(first.Strings));
    Assert.True(second.Success, second.Error.Resolve(second.Strings));
    Assert.True(other.Success, other.Error.Resolve(other.Strings));
    Assert.Equal(new[] {"", "first"}, first.Outputs.Select(output => output.ReadText()));
    Assert.Equal(new[] {"first", "first"}, second.Outputs.Select(output => output.ReadText()));
    Assert.Equal(new[] {"", "other"}, other.Outputs.Select(output => output.ReadText()));

    var failing = TestCompiler.Compile("mixin Example { expression { target var saved = <discarded>; fail<stop> } }", "Example");
    Assert.False(HixVM.Execute(failing, new HixThread(context)).Success);
    Assert.Equal("first", Assert.IsType<LiteralHixValue>(context.TargetVariables[HixString.Dynamic("saved")]).Value.DynamicValue);
  }

  [Fact]
  public void ContextSeedStringsAreDetachedBeforeLoadingAnotherProgramPool() {
    var strings = new HixStringPoolBuilder();
    var key = strings.Intern("seed");
    var value = strings.Intern("host value");
    var context = new HixContext(TestBackend.Instance, strings.Freeze());
    context.TargetVariables.StoreIsolated(key, new LiteralHixValue(value));
    var program = TestCompiler.Compile("mixin Example { expression { emit(tar#seed) } }", "Example");
    for (var i = 0; i < 2; i++) {
      var result = HixVM.Execute(program, new HixThread(context));
      Assert.True(result.Success, result.Error.Resolve(result.Strings));
      Assert.Equal("host value", Assert.Single(result.Outputs).ReadText());
    }
  }

  private sealed class TargetContext(string name) : HixContext(TestBackend.Instance) {
    public override IHixValue ResolveHost(HixThread thread, HixExpressionRoot root, HixString member) =>
      new LiteralHixValue(HixString.Dynamic(name));
  }

  private sealed class ProbeBackend : HixBackend {
    internal Action<HixThread> Probe;
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
      base.RegisterFunctions(functions);
      functions.Add(new ProbeFunction(this));
    }
  }

  private sealed class ProbeFunction(ProbeBackend backend) : FunctionDefinition("probe", 1) {
    public override IHixValue Execute(HixThread thread, IHixValue[] arguments, int line) {
      backend.Probe(thread);
      return thread.Invoke(arguments[0], [thread.Resolve(HixExpressionRoot.Parameter, HixString.Dynamic(""))], line);
    }
  }
}
