using System.Diagnostics.CodeAnalysis;
using Hix;
using Hix.Compiler;

// Newtonsoft.Json builds typed lists when reading image arrays and signature fields.
// Native AOT must retain their constructors even when they are only called through reflection.
internal static class AotReflectionRoots {
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(System.Collections.ObjectModel.ReadOnlyCollection<>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Dictionary<,>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<string>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<IHixValue>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<BytecodeFunction>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<BytecodeSignature>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<BytecodeField>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<BytecodeExpression>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<HixPattern>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<HixPatternField>))]
  [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(List<object>))]
  public static int Run(string[] args) => Hix.Standalone.Program.Main(args);
}
