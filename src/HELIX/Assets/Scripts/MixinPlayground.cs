using System;
using HELIX;

namespace DefaultNamespace {
  [EnableMixins]
  public partial class MixinPlayground {

    // [TextMixin]
    // [Marker("Hello")]
    // [Marker("World")]
    // [Marker("Test")]
    // [Marker("Test")]
    // public void MyMethod() {}

  }

  [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
  public class MarkerAttribute : Attribute {
    public MarkerAttribute(string name) { }
  }

  public class TextMixin : Attribute {

  }
}
