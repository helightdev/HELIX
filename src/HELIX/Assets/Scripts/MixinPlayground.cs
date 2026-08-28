using System;
using HELIX;

namespace DefaultNamespace {
  [EnableMixins]
  public partial class MixinPlayground {

    [TextMixin]
    [Marker("Hello")]
    [Marker("World")]
    [Marker("Test")]
    public void MyMethod() {}

  }

  [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
  public class MarkerAttribute : Attribute {
    public MarkerAttribute(string name) { }
  }

  [MixinExpression(@"
@FUNC<LocalMapper>
  @RETURN @param#name:unwrap
@END

@LOCAL<Temp> @target:attributesOf<DefaultNamespace.MarkerAttribute>
@LOCAL<Temp> @local#Temp:mapValues<LocalMapper>:join<=><,>
@LOG @local#Temp
@DUMP<State>
")]
  public class TextMixin : Attribute {

  }
}