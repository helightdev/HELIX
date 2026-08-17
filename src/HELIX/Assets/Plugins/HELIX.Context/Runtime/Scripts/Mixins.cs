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
  public class MixinCallbackAttribute : Attribute {
    public readonly string target;
    public readonly int order;
    public MixinCallbackAttribute(string target = null, int order = 0) {
      this.target = target;
      this.order = order;
    }
  }


  [AttributeUsage(AttributeTargets.Method)]
  public class MixinMethodAttribute : Attribute {
    public readonly string target;
    public readonly int order;
    public readonly string expression;

    public MixinMethodAttribute() { }

    public MixinMethodAttribute(string target) {
      this.target = target;
      order = 0;
    }

    public MixinMethodAttribute(string target, int order) {
      this.target = target;
      this.order = order;
    }

    public MixinMethodAttribute(string target, int order, string expression) {
      this.target = target;
      this.order = order;
      this.expression = expression;
    }

    public MixinMethodAttribute(string target, string expression) {
      this.target = target;
      this.expression = expression;
      order = 0;
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

  public abstract class SourceSelectorAttribute : Attribute {
    public readonly MixinInject source;
    public readonly string sourceName;
    public readonly int sourceIndex;

    protected SourceSelectorAttribute(MixinInject source) {
      this.source = source;
      sourceIndex = -1;
    }

    protected SourceSelectorAttribute(MixinInject source, string sourceName) {
      this.source = source;
      this.sourceName = sourceName;
      sourceIndex = -1;
    }

    protected SourceSelectorAttribute(MixinInject source, int sourceIndex) {
      this.source = source;
      this.sourceIndex = sourceIndex;
    }
  }

  [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
  public class MixinMethodGenericSource : SourceSelectorAttribute {
    public readonly int index;

    public MixinMethodGenericSource(int index, MixinInject source) : base(source) {
      this.index = index;
    }

    public MixinMethodGenericSource(int index, MixinInject source, string sourceName) : base(source, sourceName) {
      this.index = index;
    }

    public MixinMethodGenericSource(int index, MixinInject source, int sourceIndex) : base(source, sourceIndex) {
      this.index = index;
    }
  }

  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class AttributeMixinMethodProxyAttribute : Attribute {
    public Type target;
    public string method;
    public string[] variants;

    public AttributeMixinMethodProxyAttribute(Type target, string method) {
      this.target = target;
      this.method = method;
    }

    public AttributeMixinMethodProxyAttribute(Type target, params string[] variants) {
      this.target = target;
      this.variants = variants;
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


  [AttributeUsage(AttributeTargets.Parameter)]
  public class MixinInjectAttribute : Attribute {
    public readonly MixinInject type;
    public readonly string name;
    public bool CheckAssignment { get; set; } = false;

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

  public enum MixinInject {
    This = 0,
    Target = 1,
    Attribute = 2,
    Delegate = 3,
    ReturnValue = 4,
    Member = 5,
    Parameter = 6
  }

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

    public const string ComponentLoad = "^LoadComponent";
    public const string ComponentUnload = "^UnloadComponent";

    public const string RegistrationConfiguratorDelegate = "^*~HELIX.Context.RegistrationConfigurator";

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