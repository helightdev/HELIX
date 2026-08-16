using UnityEngine;

namespace HELIX.Context {
  public static class DefaultComponentActivators {

    public static ComponentActivator MonoBehaviour<T>() where T : MonoBehaviour => context => {
      var obj = new GameObject();
      Object.DontDestroyOnLoad(obj);
      var instance = obj.AddComponent<T>();
      return instance;
    };

    public static ComponentActivator PlainObject<T>() where T: new()  => context => new T();


    public static T ActivatePlainObject<T>() where T: new() {
      return new T();
    }
  }

  public delegate object ComponentActivator(ComponentLoadContext context);
}