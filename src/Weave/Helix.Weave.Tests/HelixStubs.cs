namespace Helix.Weave.Tests;

/// <summary>
/// A stand-in for the HELIX runtime: the attributes user code annotates with, and enough of the
/// surface the mixins emit against for the generated code to be checked. Deliberately minimal —
/// verifying real behaviour is the downstream project's job.
/// </summary>
public static class HelixStubs {
  public const string Runtime = """
    using System;

    namespace HELIX {
      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
      public sealed class StructureAttribute : Attribute {
        public StructureAttribute(bool datatype = false) { }
      }

      public enum PropInit { Literal = 0, Constant = 1, Deferred = 2, None = 3 }

      [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
      public sealed class PropAttribute : Attribute {
        public object defaultValue;
        public PropInit defaultInit;
        public bool Equatable { get; set; } = true;
        public string EqualitySyntax { get; set; } = "{0} == {1}";
        public string HashCodeSyntax { get; set; } = "{0}";
        public string Datatype { get; set; }

        public PropAttribute(object defaultValue, PropInit defaultInit = PropInit.Literal) {
          this.defaultValue = defaultValue;
          this.defaultInit = defaultInit;
        }

        public PropAttribute() {
          defaultValue = null;
          defaultInit = PropInit.None;
        }
      }

      [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
      public sealed class PropertyDatatypeAttribute : Attribute {
        public PropertyDatatypeAttribute(Type datatype = null) { }
        public Type Datatype { get; set; }
      }

      [AttributeUsage(AttributeTargets.Method)]
      public sealed class HookAttribute : Attribute {
        public HookAttribute(string target = null, int order = 0) { }
      }

      [AttributeUsage(AttributeTargets.Class)]
      public sealed class MixinUsingAttribute : Attribute {
        public MixinUsingAttribute(string statement = null) { }
      }

      [AttributeUsage(AttributeTargets.Class)]
      public sealed class OdinSerializableAttribute : Attribute { }
      [AttributeUsage(AttributeTargets.Class)]
      public sealed class OdinSerializableScriptAttribute : Attribute { }

      public static class Datatypes {
        public static readonly object String = null, Int = null, Long = null, Float = null,
                                      Double = null, Bool = null, Color = null,
                                      Vector2 = null, Vector3 = null, Vector4 = null;
        public static object Enum<T>() => null;
        public static object Object<T>() => null;
      }

      public class StructureDatatype<T> {
        public StructureDatatype(string name, StructurePropertyDatatype<T>[] properties) { }
        public StructurePropertyDatatype<T> GetProperty(string name) => null;
      }

      public class StructurePropertyDatatype<T> { public ModifierList Modifiers = new(); }
      public sealed class ModifierList { public void Add(object modifier) { } }

      public sealed class StructurePropertyDatatype<T, TValue> : StructurePropertyDatatype<T> {
        public delegate TValue Getter(ref T value);
        public delegate void Setter(ref T value, TValue propertyValue);
        public StructurePropertyDatatype(string name, object datatype, Getter get, Setter set,
                                         bool required = true, object defaultValue = null) { }
      }

      public sealed class ConfigurableStructureDatatype<T> {
        public ConfigurableStructureDatatype(StructureDatatype<T> datatype,
                                             Action<StructureDatatype<T>> configure) { Datatype = datatype; }
        public StructureDatatype<T> Datatype { get; }
      }
    }

    namespace HELIX.Context {
      public class Evt { }
      public interface IAsyncChainEvt { }
      public interface IEventListener { EventHandlerList HandlerList { get; } }

      public sealed class EventHandlerList {
        public static EventHandlerList Create() => new();
        public void Register<T>(object handler, int priority) { }
        public void RegisterAsync<T>(object handler, int priority) { }
        public void UnregisterAll() { }
      }

      public sealed class TypeKey { public TypeKey(Type type, string qualifier) { } }
      public sealed class ComponentDependency {
        public ComponentDependency(object key, bool required, bool collection = false) { }
      }

      public sealed class ManagedRegistrations {
        public void Register(Type type, RegistrationConfigurator configurator) { }
      }

      public sealed class Registration {
        public string name; public bool optional; public int phase; public int order;
        public Type scope; public object activator;
        public void Dependency(ComponentDependency dependency) { }
        public void Publication(Type type, string qualifier, bool required) { }
        public void Condition(Func<object, bool> condition) { }
        public void RegisterHandlerBinding<T>(int priority) { }
      }

      public delegate void RegistrationConfigurator(Registration registration);
      public delegate void ManagedLoadMethod(LoadContext context);
      public delegate void ManagedUnloadMethod();

      public sealed class LoadContext {
        public void PublishBind(Type type, string qualifier, object value) { }
        public void PublishProxyBind(Type type, string qualifier, object value) { }
      }

      public interface IManaged { RuntimeManagedData managed { get; } }
      public sealed class RuntimeManagedData {
        public bool IsActive => true;
        public T Resolve<T>(string qualifier) => default;
        public T ResolveOptional<T>(string qualifier) => default;
        public object ResolveAll<T>(string qualifier) => null;
      }

      public static class DefaultComponentActivators {
        public static object MonoBehaviour<T>() => null;
        public static object PlainObject<T>() => null;
      }

      public static class AutoDisposeMethods {
        public static void DisposeDisposable<T>(ref T value) { }
      }

      public sealed class FrameCountTicker {
        public FrameCountTicker(int frames) { }
        public bool running;
        public bool Tick() => true;
      }

      public sealed class FrameTimeTicker {
        public FrameTimeTicker(float seconds) { }
        public bool running;
        public bool Tick() => true;
      }

      public interface IHelixModule { void Discover(ManagedRegistrations registrations); }

      [AttributeUsage(AttributeTargets.Method)]
      public sealed class EventHandlerAttribute : Attribute {
        public EventHandlerAttribute(int priority = 0) { }
      }

      [AttributeUsage(AttributeTargets.Field)]
      public sealed class InjectAttribute : Attribute {
        public InjectAttribute(string qualifier = null, bool required = true) { }
      }

      [AttributeUsage(AttributeTargets.Field)]
      public sealed class BindAttribute : Attribute {
        public BindAttribute(Type type = null, string qualifier = null, bool required = true,
                             bool proxied = false) { }
      }

      [AttributeUsage(AttributeTargets.Field)]
      public sealed class AutoDisposeAttribute : Attribute { }

      [AttributeUsage(AttributeTargets.Class)]
      public sealed class ManagedAttribute : Attribute {
        public ManagedAttribute(Type scope = null, bool optional = false, int order = 0, int phase = 0) { }
      }

      [AttributeUsage(AttributeTargets.Method)]
      public sealed class TickerAttribute : Attribute {
        public TickerAttribute(string time = "tick", string target = "Update",
                               string condition = null, int order = 10) { }
      }

      [AttributeUsage(AttributeTargets.Class)]
      public sealed class HelixModuleAttribute : Attribute {
        public HelixModuleAttribute(string name = null, string filter = null, Type[] import = null) { }
        public string Filter { get; set; }
      }

      [AttributeUsage(AttributeTargets.Class)]
      public sealed class HelixApplicationAttribute : Attribute {
        public HelixApplicationAttribute(string name = null, string filter = null, Type[] import = null) { }
        public string Filter { get; set; }
      }
    }

    namespace HELIX.Compose {
      public struct Composition {
        public Composition(object owner) { }
        public Authoring AUTHORING;
        public IDisposable WriteContext(out object context) { context = null; return null; }
      }

      public struct Authoring {
        public bool RequireComposable<T>(ushort id, out T instance, out bool retained)
          where T : new() { instance = new T(); retained = false; return true; }
        public bool RequireTracked<T>(ushort id, out T instance, out bool retained)
          where T : new() { instance = new T(); retained = false; return true; }
        public ref ElementRef YieldBoundary<T>(ref Composition cx, T instance) => ref ElementRef.Shared;
        public ref ElementRef YieldElement<T>(ref Composition cx, T instance) => ref ElementRef.Shared;
        public ScopeHandle YieldScope<T>(ref Composition cx, T instance, ScopeCallback callback) => default;
        public void SetId(CompositionId id) { }
      }

      public delegate void ScopeCallback(BoundaryCell cell, in ScopeHandle handle);
      public struct ScopeHandle { }
      public struct ElementRef { public static ElementRef Shared; }
      public delegate void Composable(ref Composition cx);
      public delegate void StateChangedHandler();

      public struct CompositionId {
        public ushort composition, type; public byte local;
        public static ushort GetTypeId(string name) => 0;
        public static ushort GetCompositionId(string name) => 0;
      }

      public static class LocalId { public const byte Initial = 0; }

      public sealed class BoundaryCell {
        public static readonly BoundaryCell Shared = new();
        public void TrimChildren() { }
      }

      public interface IBoundary {
        object Element { get; }
        void MarkDirty();
      }

      public interface IRecomposeMixinTargets { }
      public interface IContextComposable { }
      public interface IStateHolder { ref State InputState { get; } }
      public struct State { }
      public sealed class ContextData { }
      public sealed class SparseContextMap { public bool TryGet(int key, out ContextData data) { data = null; return false; } }
      public struct LookupCache {
        public void Clear() { } public void Claim() { } public void Release() { }
        public bool TryLookup(IContextComposable parent, int key, out ContextData data) { data = null; return false; }
      }
      public struct UssFlag { }

      public static class BoundaryHelper {
        public static void ContextBefore(object boundary) { }
        public static void ContextAfter(object boundary) { }
        public static int GetDepth(object boundary) => 0;
      }

      public static class HXComposer {
        public static bool IsProcessing => false;
        public static void MarkDirty(object boundary, bool immediate = false) { }
        public static void RemoveDirty(object boundary) { }
        public static void NotifyDetach(object boundary) { }
        public static void RegisterActiveBoundary(object boundary) { }
      }

      [AttributeUsage(AttributeTargets.Class)]
      public sealed class CustomBoundaryElementAttribute : Attribute {
        public CustomBoundaryElementAttribute(bool constructor = true, bool composable = true,
                                              bool extension = false, bool cacheLookups = false,
                                              bool trimChildren = true, string name = null) { }
      }

      [AttributeUsage(AttributeTargets.Class)]
      public sealed class BoundaryElementMixinAttribute : Attribute {
        public BoundaryElementMixinAttribute(bool composable = true, bool extension = false,
                                             bool cacheLookups = false, bool trimChildren = true,
                                             string name = null) { }
      }

      [AttributeUsage(AttributeTargets.Method)]
      public sealed class WriteContextHandlerAttribute : Attribute { }

      [AttributeUsage(AttributeTargets.Method)]
      public sealed class ComposableMethodAttribute : Attribute {
        public ComposableMethodAttribute(bool requiresTracking = false, bool scope = false,
                                         bool extension = false, string name = null,
                                         string scopeCallback = "cell.TrimChildren()") { }
      }
    }

    namespace HELIX.Boot {
      [AttributeUsage(AttributeTargets.Method)]
      public sealed class CommandAttribute : Attribute {
        public CommandAttribute(string name = null, string description = null, string parent = null) { }
      }

      [AttributeUsage(AttributeTargets.Parameter)]
      public sealed class NamedArgAttribute : Attribute { }

      [AttributeUsage(AttributeTargets.Field)]
      public sealed class RegisterCommandAttribute : Attribute { }

      [AttributeUsage(AttributeTargets.Field)]
      public sealed class RegisterOptionAttribute : Attribute {
        public RegisterOptionAttribute(string path = null) { }
      }

      public class Option { }

      public static class CommandBridge<T> {
        public static object Create(HELIX.StructureDatatype<T> datatype, Action<T> invoke,
                                    string name, string description, string parent) => null;
      }
    }

    namespace HELIX.UI.Console { public class Command { } }
    """;
}
