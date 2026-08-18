using System;

namespace HELIX.Context {
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
  public class MixinPrepareGlobalAttribute : Attribute {
    public readonly string content;

    public MixinPrepareGlobalAttribute(string content) {
      this.content = content;
    }
  }

  [AttributeUsage(AttributeTargets.Method)]
  [MixinExpression("@CALL<MixinCallbackImpl>")]
  public class MixinMethodAttribute : Attribute {
    public readonly string target;
    public readonly int order;

    public MixinMethodAttribute(string target = null, int order = 0) {
      this.target = target;
      this.order = order;
    }
  }

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
  public class MixinDefineTargetAttribute : Attribute {
    public readonly string key;
    public readonly string target;

    public MixinDefineTargetAttribute(string key, string target) {
      this.key = key;
      this.target = target;
    }
  }

  // Must be put on an attribute or mixin interface.
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
  public class MixinExpressionAttribute : Attribute {
    public readonly string[] target;
    public readonly int[] order;
    public readonly string expression;

    public MixinExpressionAttribute(string target, int order, string expression) {
      this.target = new[] { target };
      this.order = new[] { order };
      this.expression = expression;
    }

    public MixinExpressionAttribute(string[] target, int[] order, string expression) {
      this.target = target;
      this.order = order;
      this.expression = expression;
    }

    public MixinExpressionAttribute(string expression) {
      this.expression = expression;
    }
  }

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
  public class RequireMixinAttribute : Attribute {
    public readonly Type target;
    public readonly bool declareImplicit;

    public RequireMixinAttribute(Type target, bool declareImplicit = false) {
      this.target = target;
      this.declareImplicit = declareImplicit;
    }
  }

  [AttributeUsage(AttributeTargets.Interface)]
  public class MixinAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  public class EnableMixinsAttribute : Attribute { }

  [Mixin] public interface IMixin { }

  /// <summary>
  /// Default mixin targets.
  /// </summary>
  /// <remarks>
  /// These default mixin targets may be implicitly referenced using On[MemberName]
  /// </remarks>
  public static class MixinOn {
    public const string Init = "$Init"; // Automatic lifecycle hook
    public const string Dispose = "$Dispose"; // Automatic lifecycle hook
    public const string ConfigureComponent = "$ConfigureComponent";

    public const string ComponentLoad = "$LoadComponent";
    public const string ComponentLoadLate = "$LoadComponentLate";
    public const string ComponentUnload = "^UnloadComponent";

    public const string RegistrationConfiguratorDelegate = "^*~HELIX.Context.RegistrationConfigurator";
    public const string ComponentLoadDelegate = "^LoadComponent:HELIX.Context.ComponentLoadMethod";
    public const string ComponentLoadLateDelegate = "^LoadComponentLate:HELIX.Context.ComponentLoadMethod";

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

  [AttributeUsage(AttributeTargets.Assembly)]
  public class MixinConfigurationAttribute : Attribute {
    public MixinConfigurationAttribute(
      Type[] candidateTypes = null
    ) { }
  }
}
