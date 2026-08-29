using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace HELIX.Context {
  public interface IManaged {
    // ReSharper disable once InconsistentNaming
    RuntimeManagedData managed { get; }
    void LoadManaged(ManagedLoadContext context) {}
    void LoadManagedLate(ManagedLoadContext context) { }
    void UnloadManaged() { }
  }

  [UsedImplicitly]
  public delegate void ManagedLoadMethod(ManagedLoadContext context);

  [UsedImplicitly]
  public delegate void ManagedUnloadMethod();

  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public sealed class RuntimeManagedData {
    public ManagedId id;
    public ManagedRegistration registration;
    public ManagedScope scope;
    public ManagedContainer container;
    public bool isLoaded;
    public bool isDisposed;
    private int _nextBindingId = 1;

    internal ManagedId ReserveBindingId() {
      if (_nextBindingId > ushort.MaxValue)
        throw new ScopeLifecycleException($"Managed object {id.Owner} exceeded the binding ID limit.");
      return new ManagedId(id.owner, (ushort)_nextBindingId++);
    }

    internal void ContinueBindingIdsAt(int nextBindingId) {
      if (nextBindingId > _nextBindingId) _nextBindingId = nextBindingId;
    }

    public void SetLoaded(bool loaded) {
      isLoaded = loaded;
    }

    public void SetDisposed(bool disposed) {
      isDisposed = disposed;
      if (disposed) {
        isLoaded = false;
      }
    }

    public bool IsActive =>
      isLoaded && !isDisposed && scope is { IsActive: true }; // TODO: Notify from scope to update state

    public T ResolveKey<T>(TypeKey key) where T : class {
      if (scope == null) throw new ComponentStateException("Component is not yet attached to a scope");
      return scope.Resolve(key) as T;
    }

    public T Resolve<T>(string qualifier = null) where T : class => ResolveKey<T>(new TypeKey(typeof(T), qualifier));

    public T Resolve<T>(Type type, string qualifier = null) where T : class =>
      ResolveKey<T>(new TypeKey(type, qualifier));

    public T ResolveOptional<T>(string qualifier = null) where T : class {
      if (scope == null) throw new ComponentStateException("Component is not yet attached to a scope");
      return scope.TryResolve(new TypeKey(typeof(T), qualifier), out var value) ? value as T : null;
    }

    public List<T> ResolveAll<T>(string qualifier = null) where T : class {
      if (scope == null) throw new ComponentStateException("Component is not yet attached to a scope");
      return scope.ResolveAll(new TypeKey(typeof(T), qualifier)).Cast<T>().ToList();
    }

    public ManagedScopeBuilder CreateScope() => container.CreateScope(scope);
  }

  public static class ManagedTargets {
    public const string RegistrationConfiguratorDelegate = "^*~HELIX.Context.RegistrationConfigurator";
    public const string ComponentLoadDelegate = "^LoadManaged:HELIX.Context.ManagedLoadMethod";
    public const string ComponentLoadLateDelegate = "^LoadManagedLate:HELIX.Context.ManagedLoadMethod";
    public const string ComponentUnloadDelegate = "^UnloadManaged:HELIX.Context.ManagedUnloadMethod";
  }

  [AttributeUsage(AttributeTargets.Class)]
  [MixinImport(typeof(ContextMixinLibrary))]
  public class ManagedAttribute : Attribute {
    public ManagedAttribute(
      Type scope = null,
      bool optional = false,
      int order = 0,
      int phase = LoadPhase.Normal
    ) { }
  }
}
