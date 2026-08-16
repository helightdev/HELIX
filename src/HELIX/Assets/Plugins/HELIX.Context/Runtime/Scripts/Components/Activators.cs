using UnityEngine;
using UnityEngine.SceneManagement;

namespace HELIX.Context {
  public static class DefaultComponentActivators {

    public static ComponentActivator MonoBehaviour<T>() where T : MonoBehaviour => context => {
      if (context.scope.scope is GameObjectScope gameObjectScope) {
        if (gameObjectScope.gameObject == null) {
          throw new ComponentActivationException($"Cannot activate {typeof(T).FullName} without a GameObject.");
        }
        var attached = gameObjectScope.gameObject.AddComponent<T>();
        context.Own(attached);
        return attached;
      }

      var obj = new GameObject(typeof(T).Name);
      context.Own(obj);

      if (context.scope.scope is SceneScope sceneScope) {
        if (!sceneScope.scene.IsValid() || !sceneScope.scene.isLoaded) {
          throw new ComponentActivationException(
            $"Cannot activate {typeof(T).FullName} in an invalid or unloaded scene."
          );
        }
        SceneManager.MoveGameObjectToScene(obj, sceneScope.scene);
      } else if (Application.isPlaying) {
        Object.DontDestroyOnLoad(obj);
      }
      return obj.AddComponent<T>();
    };

    public static ComponentActivator PlainObject<T>() where T : new() => context => new T();


    public static T ActivatePlainObject<T>() where T : new() {
      return new T();
    }
  }

  public delegate object ComponentActivator(ComponentLoadContext context);
}
