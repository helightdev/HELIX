using System;

namespace HELIX.Context {
  public interface IHelixModule {
    void Discover(ComponentRegistrations registration) {}
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class HelixModuleAttribute : Attribute {
    public HelixModuleAttribute(
      string name = null, // Name of the module upper camel case
      string filter = null, // Namespace filter, otherwise everything in the assembly is considered
      Type[] import = null, // Other module-like types to import verbatim
      Type[] stereotypes = null // Overrides the stereotypes used to discover components
    ) { }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class HelixApplicationAttribute : Attribute { // Special version of the module meant as a root entrypoint
    public HelixApplicationAttribute(
      string name = null, // Name of the module upper camel case
      string filter = null, // Namespace filter, otherwise everything in the assembly is considered
      Type[] import = null, // Other module-like types to import verbatim
      Type[] stereotypes = null // Overrides the stereotypes used to discover components
    ) { }
  }
}