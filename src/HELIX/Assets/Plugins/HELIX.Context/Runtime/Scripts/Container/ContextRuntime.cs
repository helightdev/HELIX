using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HELIX.Context {

  internal sealed class GameObjectScopeObserver : MonoBehaviour {
    private ManagedContainer _container;
    private IScope _scope;

    internal void Attach(ManagedContainer container, IScope scope) {
      _container = container;
      _scope = scope;
    }

    internal bool Observes(IScope scope) => ReferenceEquals(_scope, scope);

    internal void Detach() {
      _container = null;
      _scope = null;
    }

    private void OnDestroy() {
      var container = _container;
      var scope = _scope;
      Detach();
      container?.DisposeScopeFromUnity(scope);
    }
  }

  internal sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class {
    public static readonly ReferenceComparer<T> Instance = new();
    public bool Equals(T x, T y) => ReferenceEquals(x, y);
    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
  }

  public class ComponentContainerException : Exception {
    public ComponentContainerException(string message) : base(message) { }
    public ComponentContainerException(string message, Exception innerException) : base(message, innerException) { }
  }

  public sealed class ComponentGraphException : ComponentContainerException {
    public ComponentGraphException(string message) : base(message) { }
    public ComponentGraphException(string message, Exception innerException) : base(message, innerException) { }
  }

  public sealed class ComponentActivationException : ComponentContainerException {
    public ComponentActivationException(string message) : base(message) { }
  }

  public sealed class ComponentInitializationException : ComponentContainerException {
    public ComponentInitializationException(string message) : base(message) { }
    public ComponentInitializationException(string message, Exception innerException) : base(message, innerException) { }
  }

  public sealed class ComponentDeinitializationException : ComponentContainerException {
    public ComponentDeinitializationException(string message, Exception innerException) : base(message, innerException) { }
  }

  public sealed class ComponentResolutionException : ComponentContainerException {
    public ComponentResolutionException(string message) : base(message) { }
  }

  public sealed class ScopeLifecycleException : ComponentContainerException {
    public ScopeLifecycleException(string message) : base(message) { }
  }

  public sealed class AsyncScopeInitializationException : ComponentContainerException {
    public AsyncScopeInitializationException(string message) : base(message) { }
  }

  public static class HX {
    public static ManagedContainer container;

    public static object Resolve(TypeKey key) {
      if (container == null) throw new ScopeLifecycleException("No active HXContainer is installed.");
      return container.Application.Resolve(key);
    }
  }

}
