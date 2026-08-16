using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HELIX.Context {
  /// <summary>Topology, ownership, and teardown responsibilities of a managed scope.</summary>
  public sealed partial class ManagedScope {
    private readonly List<ManagedScope> _managedChildren = new();
    private readonly List<LoadedComponent> _loadedComponents = new();
    private readonly HashSet<object> _owned = new(ReferenceComparer<object>.Instance);

    internal void AddChild(ManagedScope child) {
      _managedChildren.Add(child);
      children.Add(child.scope);
    }

    internal void RemoveChild(ManagedScope child) {
      _managedChildren.Remove(child);
      var index = children.FindIndex(scope => ReferenceEquals(scope, child.scope));
      if (index >= 0) children.RemoveAt(index);
    }

    internal void RecordComponent(RegistrationEntry registration, object instance) =>
      _loadedComponents.Add(new LoadedComponent(registration, instance));

    internal void Own(object value) {
      EnsureCanPublish();
      switch (value) {
        case null: return;
        case UnityEngine.Object or IDisposable: _owned.Add(value); return;
        default:
          throw new ArgumentException(
            $"Scope-owned values must be a {nameof(UnityEngine.Object)} or {nameof(IDisposable)}.", nameof(value)
          );
      }
    }

    internal void Cancel(List<Exception> failures) {
      if (_cancellation.IsCancellationRequested) return;
      try { _cancellation.Cancel(); }
      catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException("Scope cancellation failed.", exception));
      }
    }

    internal void UnloadComponents(List<Exception> failures) {
      foreach (var loaded in _loadedComponents.AsEnumerable().Reverse()) Unload(loaded, failures);
      _bindings.Clear();
      _loadedComponents.Clear();
    }

    internal void DisposeOwnedResources(List<Exception> failures) {
      foreach (var resource in _owned.OfType<IDisposable>()) {
        try { resource.Dispose(); }
        catch (Exception exception) {
          failures.Add(new ComponentDeinitializationException("Failed to dispose a scope-owned resource.", exception));
        }
      }
      _owned.RemoveWhere(value => value is IDisposable);
    }

    internal void DestroyOwnedUnityObjects(List<Exception> failures) {
      foreach (var owned in _owned.OfType<UnityEngine.Object>()) {
        if (owned == null) continue;
        try {
          if (UnityEngine.Application.isPlaying) UnityEngine.Object.Destroy(owned);
          else UnityEngine.Object.DestroyImmediate(owned);
        } catch (Exception exception) {
          failures.Add(new ComponentDeinitializationException(
            $"Failed to destroy Unity object '{owned.name}' owned by {scope.GetType().Name}.", exception
          ));
        }
      }
      _owned.RemoveWhere(value => value is UnityEngine.Object);
      _cancellation.Dispose();
    }

    private static void Unload(LoadedComponent loaded, List<Exception> failures) {
      try {
        if (loaded.instance is IComponent component) component.UnloadComponent();
      } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException($"Failed to unload component '{loaded.registration.name}'.", exception));
      }
      try {
        if (loaded.instance is IEventListener listener) listener.HandlerList.UnregisterAll();
      } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException(
          $"Failed to unregister handlers for component '{loaded.registration.name}'.", exception
        ));
      }
      try {
        if (loaded.instance is IDisposable disposable) disposable.Dispose();
      } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException($"Failed to dispose component '{loaded.registration.name}'.", exception));
      }
    }
  }

}
