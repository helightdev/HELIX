using System;
using HELIX;

// MixinCallback method implementation
[assembly: HELIX.MixinPrepareGlobal(
  @"
@FUNC<MixinHookImpl>
  @LOCAL<Name> @attr#target:unwrap
  @SCOPE
    @MATCH @local#Name:eq<null>
    @ASSERT @target:name:matches<^On.*>
    @LOCAL<IsImplicit> true
    @Local<Name> $@target:name:replaceFirst<^On><>
  @END

  @SCOPE
    @MATCH @arg#0:!?exists
    @MIXIN<(@local#Name)><(@attr#order)> @target:name();
    @RETURN
  @END

  @RESOLVE_MIXIN<Delegate> @local#Name
  @ASSERT @local#Delegate:!?eq<null>
  @MIXIN<(@local#Name)><(@attr#order)> @target:name(@local#Delegate:wire<(@target)>);
@END

@FUNC<SetStructurePropertyDatatype>
  @CODE datatype.GetProperty(""@target:name"").ValueDatatype = @param;
@END

@FUNC<AddStructurePropertyModifier>
  @CODE datatype.GetProperty(""@target:name"").Modifiers.Add(@param);
@END
"
)]

namespace HELIX {
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
  public class MixinPrepareGlobalAttribute : Attribute {
    public readonly string content;

    public MixinPrepareGlobalAttribute(string content) {
      this.content = content;
    }
  }

  [AttributeUsage(AttributeTargets.Method)]
  [MixinExpression("@CALL<MixinHookImpl>")]
  public class HookAttribute : Attribute {
    public readonly string target;
    public readonly int order;

    public HookAttribute(string target = null, int order = 0) {
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


  [AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method,
    AllowMultiple = true
  )]
  [MixinExpression("@USING @attr#statement:unwrap;")]
  public class MixinUsingAttribute : Attribute, IMixin {
    public MixinUsingAttribute(string statement) { }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
  [MixinExpression("@CALL<SetStructurePropertyDatatype> @attr#datatype:unwrap")]
  public class PropertyDatatypeAttribute : Attribute {

    public PropertyDatatypeAttribute(string datatype) {

    }

  }

  /// <summary>
  /// Default mixin targets.
  /// </summary>
  /// <remarks>
  /// These default mixin targets may be implicitly referenced using On[MemberName]
  /// </remarks>
  public static class MixinOn {
    public const string Init = "$Init"; // Automatic lifecycle hook
    public const string Dispose = "$Dispose"; // Automatic lifecycle hook
    public const string ConfigureManaged = "$ConfigureManaged";

    public const string LoadManaged = "$LoadManaged";
    public const string LoadManagedLate = "$LoadManagedLate";
    public const string UnloadManaged = "$UnloadManaged";

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