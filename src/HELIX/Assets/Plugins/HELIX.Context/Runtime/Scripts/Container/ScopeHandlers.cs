using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HELIX.Context {
  /// <summary>Extends a scope with discovery and lifecycle behavior without coupling it to the container.</summary>
  public interface IScopeHandler {
    bool Handles(IScope scope);
    IEnumerable<IComponent> DiscoverComponents(ManagedContainer container, ManagedScope scope);
    void Attach(ManagedContainer container) { }
    void Detach(ManagedContainer container) { }
    UniTask ApplicationStarted(ManagedContainer container, ManagedScope application) => UniTask.CompletedTask;
    void ApplicationStartedSync(ManagedContainer container, ManagedScope application) { }
    void ScopeActivated(ManagedContainer container, ManagedScope scope) { }
    void ScopeDisposing(ManagedContainer container, ManagedScope scope) { }
  }

  /// <summary>Type-safe base class for handlers associated with one scope type.</summary>
  public abstract class ScopeHandler<TScope> : IScopeHandler where TScope : IScope {
    public bool Handles(IScope scope) => scope is TScope;

    public virtual IEnumerable<IComponent> DiscoverComponents(
      ManagedContainer container,
      ManagedScope scope
    ) => Enumerable.Empty<IComponent>();

    public virtual void Attach(ManagedContainer container) { }
    public virtual void Detach(ManagedContainer container) { }
    public virtual UniTask ApplicationStarted(ManagedContainer container, ManagedScope application) =>
      UniTask.CompletedTask;
    public virtual void ApplicationStartedSync(ManagedContainer container, ManagedScope application) { }
    public virtual void ScopeActivated(ManagedContainer container, ManagedScope scope) { }
    public virtual void ScopeDisposing(ManagedContainer container, ManagedScope scope) { }

    protected static TScope Scope(ManagedScope managed) => (TScope)managed.scope;
  }

  public sealed class SceneScopeHandler : ScopeHandler<SceneScope> {
    private ManagedContainer _container;

    public override void Attach(ManagedContainer container) {
      if (_container != null) throw new ScopeLifecycleException("A scene scope handler is already attached.");
      _container = container;
      SceneManager.sceneLoaded += OnSceneLoaded;
      SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public override void Detach(ManagedContainer container) {
      if (!ReferenceEquals(_container, container)) return;
      SceneManager.sceneLoaded -= OnSceneLoaded;
      SceneManager.sceneUnloaded -= OnSceneUnloaded;
      _container = null;
    }

    public override IEnumerable<IComponent> DiscoverComponents(ManagedContainer container, ManagedScope scope) {
      var scene = Scope(scope).scene;
      if (!scene.IsValid() || !scene.isLoaded) yield break;
      foreach (var root in scene.GetRootGameObjects())
      foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true).OfType<IComponent>())
        yield return component;
    }

    public override async UniTask ApplicationStarted(ManagedContainer container, ManagedScope application) {
      foreach (var scene in LoadedScenesWithoutScope(container))
        await container.CreateScope(application).From(new SceneScope { scene = scene }).StartAsync();
    }

    public override void ApplicationStartedSync(ManagedContainer container, ManagedScope application) {
      foreach (var scene in LoadedScenesWithoutScope(container))
        container.CreateScope(application).From(new SceneScope { scene = scene }).StartSync();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
      if (_container == null || !_container.IsApplicationStarted || _container.HasScope<SceneScope>(candidate =>
        candidate.scene == scene
      )) return;
      CreateLoadedSceneScope(scene).Forget();
    }

    private async UniTaskVoid CreateLoadedSceneScope(Scene scene) {
      try {
        await _container.CreateScope(_container.Application).From(new SceneScope { scene = scene }).StartAsync();
      } catch (Exception exception) {
        Debug.LogException(exception);
      }
    }

    private void OnSceneUnloaded(Scene scene) {
      if (_container == null) return;
      foreach (var scope in _container.FindScopes<SceneScope>(candidate => candidate.scene == scene).ToArray())
        _container.DisposeScopeFromHandler(scope.scope);
    }

    private static IEnumerable<Scene> LoadedScenesWithoutScope(ManagedContainer container) {
      for (var i = 0; i < SceneManager.sceneCount; i++) {
        var scene = SceneManager.GetSceneAt(i);
        if (scene.IsValid() && scene.isLoaded && !container.HasScope<SceneScope>(candidate => candidate.scene == scene))
          yield return scene;
      }
    }
  }

  public sealed class GameObjectScopeHandler : ScopeHandler<GameObjectScope> {
    public override IEnumerable<IComponent> DiscoverComponents(ManagedContainer container, ManagedScope scope) {
      var gameObject = Scope(scope).gameObject;
      return gameObject == null
        ? Enumerable.Empty<IComponent>()
        : gameObject.GetComponents<MonoBehaviour>().OfType<IComponent>();
    }

    public override void ScopeActivated(ManagedContainer container, ManagedScope scope) {
      var gameObject = Scope(scope).gameObject;
      if (gameObject == null) return;
      var observer = gameObject.AddComponent<GameObjectScopeObserver>();
      observer.Attach(container, scope.scope);
    }

    public override void ScopeDisposing(ManagedContainer container, ManagedScope scope) {
      var gameObject = Scope(scope).gameObject;
      if (gameObject == null) return;
      foreach (var observer in gameObject.GetComponents<GameObjectScopeObserver>()) {
        if (!observer.Observes(scope.scope)) continue;
        observer.Detach();
        if (Application.isPlaying) UnityEngine.Object.Destroy(observer);
        else UnityEngine.Object.DestroyImmediate(observer);
      }
    }
  }
}
