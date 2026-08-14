using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Method)]
  public class MixinMethodAttribute : Attribute {
    public readonly string target;
    public readonly int order;

    public MixinMethodAttribute() { }

    public MixinMethodAttribute(string target) {
      this.target = target;
      order = 0;
    }

    public MixinMethodAttribute(string target, int order) {
      this.target = target;
      this.order = order;
    }
  }

  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class AttributeMixinMethodProxyAttribute : Attribute {
    public Type target;
    public string method;

    public AttributeMixinMethodProxyAttribute(Type target, string method) {
      this.target = target;
      this.method = method;
    }
  }


  [AttributeUsage(AttributeTargets.Parameter)]
  public class MixinInjectAttribute : Attribute {
    public readonly MixinInject type;
    public readonly string name;

    public MixinInjectAttribute() {
      type = MixinInject.Target;
    }

    public MixinInjectAttribute(MixinInject type) {
      this.type = type;
      name = null;
    }

    public MixinInjectAttribute(MixinInject type, string name) {
      this.type = type;
      this.name = name;
    }
  }

  public enum MixinInject { This, Target, Attribute, Delegate, ReturnValue }

  [AttributeUsage(AttributeTargets.Property)]
  public class MixinPropertyAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Interface)]
  public class MixinDeclareVariableAttribute : Attribute {
    public Type Type { get; set; }
    public string Name { get; set; }
  }

  [AttributeUsage(AttributeTargets.Interface)]
  public class MixinAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  public class EnableMixinsAttribute : Attribute { }

  [Mixin, EnableMixins]
  public interface IMixin { }

  /// <summary>
  /// Default mixin targets.
  /// </summary>
  /// <remarks>
  /// These default mixin targets may be implicitly referenced using On[MemberName]
  /// </remarks>
  public static class MixinOn {
    public const string Init = "$Init"; // Automatic lifecycle hook
    public const string Dispose = "$Dispose"; // Automatic lifecycle hook

    public const string MonoAwake = "Awake";
    public const string MonoStart = "Start";
    public const string MonoReset = "Reset";
    public const string MonoUpdate = "Update";
    public const string MonoFixedUpdate = "FixedUpdate";
    public const string MonoLateUpdate = "LateUpdate";
    public const string MonoDestroy = "Destroy";
    public const string MonoEnable = "OnEnable";
    public const string MonoDisable = "OnDisable";
    public const string MonoValidate = "OnValidate";
  }
}