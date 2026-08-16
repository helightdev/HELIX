using System;
using HELIX.Prose;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HELIX.Context {
  [Service]
  public partial class SceneInjectedService : MonoBehaviour {
    [Inject] public SceneService scope;
    [Inject] public ExampleUser user;

    [NonSerialized]
    public GameObject testGameObject;

    [MixinMethod]
    public void OnInit() {
      Debug.Log("SceneInjectedService.OnInit");
    }

    [MixinMethod]
    public void OnDispose() {
      Debug.Log("SceneInjectedService.OnDispose");
    }

    [Button]
    public void ActivateGameObjectScope() {
      testGameObject = new GameObject();
      testGameObject.AddComponent<SceneInjectedService>();

      RuntimeComponentData.CreateScope()
        .From(new GameObjectScope { gameObject = testGameObject })
        .StartSync();

    }

    [Button]
    public void ActivateGameObjectScopeBuilder() {
      testGameObject = new GameObject();

      RuntimeComponentData.CreateScope()
        .From(new GameObjectScope { gameObject = testGameObject })
        .AddComponent<SceneInjectedService>()
        .StartSync();
    }

    [Button]
    public void DestroyGameObjectScope() {
      if (testGameObject == null) return;
      Destroy(testGameObject);
      testGameObject = null;
    }

    [Button]
    public void LogState() {
      var writer = new ProseUnityRichTextWriter();
      ComponentGraphProse.WriteLive(writer, RuntimeComponentData.container);
      Debug.Log(writer.Build());
    }
  }
}
