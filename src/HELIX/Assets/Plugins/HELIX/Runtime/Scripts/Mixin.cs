using System;

namespace HELIX {

  [AttributeUsage(AttributeTargets.Method)]
  public class HookAttribute : Attribute {
    public readonly string target;
    public readonly int order;

    public HookAttribute(string target = null, int order = 0) {
      this.target = target;
      this.order = order;
    }
  }


  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
  public class MixableAttribute : Attribute { }

  public interface IMixin { }


  [AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method,
    AllowMultiple = true
  )]
  public class MixinUsingAttribute : Attribute, IMixin {
    public MixinUsingAttribute(string statement) { }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
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
    public const string Compose = "$Compose";
    public const string Recompose = "$Recompose";
    public const string Reset = "$Reset";

    public const string BoundaryPostConstruct = "$PostConstruct";
    public const string InputStateChanged = "^OnStateChanged:HELIX.Compose.StateChangedHandler";


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
