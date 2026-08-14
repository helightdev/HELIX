using System;

namespace HELIX.Context {
  // TODO

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class InjectAttribute : Attribute {
    public readonly Source source;
    public readonly string qualifier;

    public InjectAttribute() {
      source = Source.Container;
      qualifier = null;
    }

    public InjectAttribute(string qualifier) {
      source = Source.Container;
      this.qualifier = qualifier;
    }

    public InjectAttribute(Source source) {
      this.source = source;
      qualifier = null;
    }

    public InjectAttribute(Source source, string qualifier) {
      this.source = source;
      this.qualifier = qualifier;
    }
  }

  public enum Source { Container, Components, Addressables, Resources }
}