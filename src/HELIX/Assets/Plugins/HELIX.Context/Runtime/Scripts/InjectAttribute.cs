using System;

namespace HELIX.Context {

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  [MixinImport(typeof(ContextMixinLibrary))]
  public class InjectAttribute : Attribute {
    public InjectAttribute(string qualifier = null, bool required = true) { }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
  [MixinImport(typeof(ContextMixinLibrary))]
  public class BindAttribute : Attribute {
    public BindAttribute(string qualifier = null, bool required = true, bool proxied = false) { }
    public BindAttribute(Type type, string qualifier = null, bool required = true, bool proxied = false) { }
    public BindAttribute(Type[] type, string qualifier = null, bool required = true, bool proxied = false) { }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  [MixinImport(typeof(ContextMixinLibrary))]
  public class ResourceAttribute : Attribute {
    public ResourceAttribute(Source source, string qualifier = null, bool required = true) { }
  }

  public enum Source { Addressables = 2, Resources = 3 }
}
