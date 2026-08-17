using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HELIX.Widgets.Prompts.Kenny;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HELIX.Context {

  public class LoggingDisposable : IDisposable {
    public void Dispose() {
      Debug.Log("LoggingDisposable disposed!");
    }
  }

  [Service(typeof(ApplicationScope))]
  public partial class MyRootDependency { }

  [Service(typeof(ApplicationScope))]
  public partial class ExampleSingleton {
    [Inject] public MyRootDependency myRootDependency;

    [EventHandler]
    public async UniTask OnAsyncInit(ComponentAsyncInitEvent evt) {
      Debug.Log($"ExampleSingleton async init starting! {myRootDependency}");
      await UniTask.Delay(1000); // Simulate async initialization
      Debug.Log($"ExampleSingleton async init complete! {myRootDependency}");
    }
  }

  // [EnableMixins]
  [Service(typeof(ApplicationScope))]
  public partial class ExampleUser : MonoBehaviour {
    [AutoDispose]
    public IDisposable myResource2;

    [Inject, NonSerialized]
    public ExampleSingleton mySingleton;

    [Inject, NonSerialized]
    public MyRootDependency myRootDependency;

    [Inject(Source.Addressables, "Assets/ExampleAddressable"), ShowInInspector]
    public GameObject addressablePrefab;

    [Inject(Source.Resources), ShowInInspector]
    public List<KennyPromptSvgCollection> svgCollections;

    [MixinCallback]
    private void OnInit() {
      Debug.Log($"Self on awake! Implicitly referenced! {mySingleton} and {myRootDependency};");
      myResource2 = new LoggingDisposable(); //32
    }

    [MixinCallback(MixinOn.ConfigureComponent)]
    private static void OnConfigureSelf(RegistrationEntry entry) {

    }

    [EventHandler]
    private void OnTestEvent(ref TestEvent evt) { }

    [EventHandler]
    private void OnTestAsync(TestAsyncEvt evt) { }

    [MixinCallback(MixinOn.ComponentLoad)]
    private void OnComponentLoad() {

    }
  }

  [Service(typeof(SceneScope))]
  public partial class SceneService {
    [MixinCallback]
    public void OnInit() {
      Debug.Log("SceneService has initialized!");
    }
  }

  public struct TestEvent : Evt<TestEvent> { }

  public class TestAsyncEvt : AsyncChainEvt<TestAsyncEvt> { }
}