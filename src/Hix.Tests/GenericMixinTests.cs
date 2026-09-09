using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class GenericMixinTests {
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
    var result = HixVM.Execute(program, HixCoreBackend.Instance.CreateThread());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"hello:derived", "kept"}, result.Outputs.Select(output => output.ReadText()));
    Assert.Equal(new[] {"custom/channel", ""}, result.Outputs.Select(output => output.Target.Resolve(result.Strings)));
    Assert.Empty(HixCoreBackend.Instance.Functions.Resolve("inject", 2));
    Assert.Empty(HixCoreBackend.Instance.Functions.Resolve("using", 1));
    Assert.Empty(HixCoreBackend.Instance.Functions.Resolve("defineTarget", 2));
    Assert.DoesNotContain(typeof(HixVM).Assembly.GetReferencedAssemblies(), reference => reference.Name.Contains("CodeAnalysis"));
  }
}
