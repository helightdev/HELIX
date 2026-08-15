using UnityEngine;

namespace HELIX.Context {
  public static class DefaultComponentActivators {
    public static T ActivateMonoBehaviour<T>() where T : MonoBehaviour {
      var obj = new GameObject();
      Object.DontDestroyOnLoad(obj);
      return obj.AddComponent<T>();
    }

    public static T ActivatePlainObject<T>() where T: new() {
      return new T();
    }
  }
}