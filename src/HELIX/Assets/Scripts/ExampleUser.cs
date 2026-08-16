using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HELIX.Context {
  [Mixin] public interface IExampleMixin : IMixin {
    [MixinMethod(MixinOn.Init, -1)]
    static void MixinOnInit<T>(
      [MixinInject] T target
    ) { }

    [MixinMethod(MixinOn.Dispose)]
    static void MixinOnDestroy<T>(
      [MixinInject] T target
    ) { }
  }

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

    [Inject] public ExampleSingleton mySingleton;
    [Inject] public MyRootDependency myRootDependency;

    [MixinMethod]
    private void OnInit() {
      Debug.Log($"Self on awake! Implicitly referenced! {mySingleton} and {myRootDependency};");
      myResource2 = new LoggingDisposable();
      //
    }

    [EventHandler]
    private void OnTestEvent(ref TestEvent evt) { }

    [EventHandler]
    private void OnTestAsync(TestAsyncEvt evt) { }

    [MixinMethod(MixinOn.ComponentLoad)]
    private void OnComponentLoad() { }
  }

  [Service(typeof(SceneScope))]
  public partial class SceneService {
    [MixinMethod]
    public void OnInit() {
      Debug.Log("SceneService has initialized!");
    }
  }

  public struct TestEvent : Evt<TestEvent> { }

  public class TestAsyncEvt : AsyncChainEvt<TestAsyncEvt> { }
}