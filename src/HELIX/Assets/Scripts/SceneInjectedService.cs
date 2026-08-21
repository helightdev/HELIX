using System;
using Cysharp.Threading.Tasks;
using HELIX.Prose;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HELIX.Context {
  [Managed]
  public partial class SceneInjectedService : MonoBehaviour {
    [Inject] public SceneService scope;
    [Inject] public ExampleUser user;

    [NonSerialized]
    public GameObject testGameObject;
    
    [Hook]
    public void OnInit() {
      Debug.Log("SceneInjectedService.OnInit");
    }

    [Hook]
    public void OnDispose() {
      Debug.Log("SceneInjectedService.OnDispose");
    }
    
    [Button]
    public void ActivateGameObjectScope() {
      testGameObject = new GameObject();
      testGameObject.AddComponent<SceneInjectedService>();

      managed.CreateScope()
        .From(new GameObjectScope { gameObject = testGameObject })
        .StartSync();
    }

    [Button]
    public void ActivateGameObjectScopeBuilder() {
      testGameObject = new GameObject();

      managed.CreateScope()
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
      ManagedGraphProse.WriteLive(writer, this.managed.container);
      Debug.Log(writer.Build());
    }
  }
}