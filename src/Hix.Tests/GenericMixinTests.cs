using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class GenericMixinTests {
  [Fact]
  public void DerivationRecursionUsesExistingFramesAndThreadCanBeReused() {
    var recursive = HixCompiler.Compile("""
      derivation mixin Provider {
        expression { local shared = [param#value]; return(local#shared) }
        expression { return(derive(@[@{value=[local#shared]}])) }
      }
      mixin Report {
        prelude expression { emit(derive(@[@{value=<hello>}])) }
      }
      """, "Report");
    var normal = HixCompiler.Compile("""
      derivation mixin Provider {
        expression { local shared = [param#value]; return(local#shared) }
        expression { return(<[local#shared]:derived>) }
      }
      mixin Report {
        prelude expression {
          local rows = derive(@[@{value=<hello>}])
          emit(local#rows#0#value)
        }
      }
      """, "Report");
    var thread = HixCoreBackend.Instance.CreateCoroutine();
    for (var index = 0; index < 2; index++) {
      var failure = HixVM.Execute(recursive, thread);
      Assert.False(failure.Success);
      Assert.Contains("recursive derivation", failure.Error.Resolve(failure.Strings));
      var success = HixVM.Execute(normal, thread);
      Assert.True(success.Success, success.Error.Resolve(success.Strings));
      Assert.Equal("hello:derived", Assert.Single(success.Outputs).ReadText());
    }
    Assert.Null(typeof(HixCoroutine).GetMethod("Derive"));
    Assert.DoesNotContain(typeof(HixCoroutine).GetFields(
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
      field => field.FieldType == typeof(System.Collections.Generic.HashSet<BytecodeDerivation>));
  }

  [Fact]
  public void CoreMixinsDerivePlainRecordsAndEmitOpaqueDestinations() {
    var program = HixCompiler.Compile("""
      derivation mixin Decorate {
        expression { return(<[param#value]:derived>) }
      }
      mixin Report {
        prelude expression {
          carry local rows = derive(@[@{value=<hello>, label=<kept>}])
        }
        expression {
          emit(<custom/channel>, local#rows#0#value)
          emit(local#rows#0#label)
        }
      }
      """, "Report");
    var result = HixVM.Execute(program, HixCoreBackend.Instance.CreateCoroutine());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"hello:derived", "kept"}, result.Outputs.Select(output => output.ReadText()));
    Assert.Equal(new[] {"custom/channel", ""}, result.Outputs.Select(output => output.Target.Resolve(result.Strings)));
    Assert.Empty(HixCoreBackend.Instance.Functions.Resolve("inject", 2));
    Assert.Empty(HixCoreBackend.Instance.Functions.Resolve("using", 1));
    Assert.Empty(HixCoreBackend.Instance.Functions.Resolve("defineTarget", 2));
    Assert.DoesNotContain(typeof(HixVM).Assembly.GetReferencedAssemblies(), reference => reference.Name.Contains("CodeAnalysis"));
  }
}
