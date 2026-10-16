#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices {
  // Required by netstandard2.0 for `init` accessors and records. Public, so every authoring
  // project that references the weave surface gets them without its own copy.
  public static class IsExternalInit { }
}
#endif
