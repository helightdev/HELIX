using System;

namespace HELIX {

  [AttributeUsage(AttributeTargets.All)]
  public class ExperimentalAttribute : Attribute {
    public ExperimentalAttribute(string message) => Message = message;

    public ExperimentalAttribute() {
      Message = "This API is experimental and may change or be removed in future versions.";
    }
    public string Message { get; }
  }

  [AttributeUsage(AttributeTargets.All)]
  public class InternalApiAttribute : Attribute {
    public InternalApiAttribute() { }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
  public class PropAttribute : Attribute {
    public object defaultValue;
    public PropInit defaultInit;

    public bool Equatable { get; set; } = true;
    public string EqualitySyntax { get; set; } = "{0} == {1}";
    public string HashCodeSyntax { get; set; } = "{0}";
    public string Datatype { get; set; }

    public string ProxyFunction { get; set; }
    public string ProxySetter { get; set; }
    public string ProxyGetter { get; set; }
    public bool ProxyEquality { get; set; }

    public PropAttribute(
      object defaultValue,
      PropInit defaultInit = PropInit.Literal
    ) {
      this.defaultValue = defaultValue;
      this.defaultInit = defaultInit;
    }

    public PropAttribute() {
      defaultValue = null;
      defaultInit = PropInit.None;
    }
  }

  [AttributeUsage(AttributeTargets.Struct)]
  public class PropStructAttribute : Attribute {
    public PropStructAttribute(bool datatype = false) { }
  }

  public enum PropInit {
    /// <summary>The literal value is used as the constructor parameter initializer.</summary>
    Literal,

    /// <summary>The string content is inserted as a constant initializer expression.</summary>
    Constant,

    /// <summary>
    /// The parameter is nullable and defaults to null; the string content is used when it is null.
    /// </summary>
    Deferred,

    None,
  }
}
