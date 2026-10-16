// Metadata names of the HELIX and Unity types the mixins match against. Strings, because the
// authoring project deliberately has no reference to the runtime or to Unity.
using Helix.Weave;

namespace HELIX.Weave;

public static class Types {
  public const string String = "System.String";
  public const string Enumerable = "System.Collections.IEnumerable";
  public const string UnityObject = "UnityEngine.Object";
  public const string MonoBehaviour = "UnityEngine.MonoBehaviour";
  public const string VisualElement = "UnityEngine.UIElements.VisualElement";
  public const string UniTask = "Cysharp.Threading.Tasks.UniTask";

  public const string Evt = "HELIX.Context.Evt";
  public const string AsyncChainEvt = "HELIX.Context.IAsyncChainEvt";

  public const string RegistrationConfigurator = "HELIX.Context.RegistrationConfigurator";
  public const string ManagedLoadMethod = "HELIX.Context.ManagedLoadMethod";
  public const string ManagedUnloadMethod = "HELIX.Context.ManagedUnloadMethod";
  public const string Composable = "HELIX.Compose.Composable";
  public const string StateChangedHandler = "HELIX.Compose.StateChangedHandler";
}

/// <summary>Injection targets that are the same on every type.</summary>
public static class Targets {
  public static readonly InjectionTarget ConfigureManaged =
    InjectionTarget.FromDelegate(Types.RegistrationConfigurator, isStatic: true, isPublic: true);
  public static readonly InjectionTarget LoadManaged =
    new("LoadManaged", Types.ManagedLoadMethod, isPublic: true);
  public static readonly InjectionTarget LoadManagedLate =
    new("LoadManagedLate", Types.ManagedLoadMethod, isPublic: true);
  public static readonly InjectionTarget UnloadManaged =
    new("UnloadManaged", Types.ManagedUnloadMethod, isPublic: true);

  /// <summary>Hix aliased <c>$Init</c> to Awake and <c>$Dispose</c> to OnDestroy by default.</summary>
  public static readonly InjectionTarget Init = LoadManaged;
  public static readonly InjectionTarget Dispose = UnloadManaged;

  /// <summary>Boot's <c>$Init</c>, which Hix aliased to Awake like the Context one.</summary>
  public static readonly InjectionTarget BootInit = LoadManaged;

  public static readonly InjectionTarget MonoAwake = new("Awake");
  public static readonly InjectionTarget MonoDestroy = new("OnDestroy");

  // Per-type aliases, defined by whichever boundary mixin applies. See DefineTarget.
  public const string Compose = "$Compose";
  public const string Recompose = "$Recompose";
  public const string Reset = "$Reset";
  public const string BoundaryInit = "$Init";
  public const string BoundaryDispose = "$Dispose";
  public const string PostConstruct = "$PostConstruct";
  public const string AttachBoundary = "AttachBoundary";
  public const string DetachBoundary = "DetachBoundary";
}
