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
}