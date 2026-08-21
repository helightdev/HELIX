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

  [Managed(typeof(ApplicationScope))]
  public partial class MyRootDependency { }

  [Managed(typeof(ApplicationScope))]
  public partial class ExampleSingleton {
    [Inject] public MyRootDependency myRootDependency;

    [EventHandler]
    public async UniTask OnAsyncInit(AsyncManagedLoadEvent evt) {
      Debug.Log($"ExampleSingleton async init starting! {myRootDependency}");
      await UniTask.Delay(1000); // Simulate async initialization
      Debug.Log($"ExampleSingleton async init complete! {myRootDependency}");
    }
  }

  [Managed(typeof(ApplicationScope))]
  public partial class ExampleUser : MonoBehaviour {
    [AutoDispose]
    public IDisposable myResource2;

    [Inject, NonSerialized]
    public ExampleSingleton mySingleton;

    [Inject, NonSerialized]
    public MyRootDependency myRootDependency;

    [Inject("pipeline"), NonSerialized, ShowInInspector]
    public List<string> pipelineValue;

    [Resource(Source.Addressables, "Assets/ExampleAddressable"), ShowInInspector]
    public GameObject addressablePrefab;

    [Resource(Source.Resources), ShowInInspector]
    public List<KennyPromptSvgCollection> svgCollections;
    
    [Hook]
    private void OnInit() {
      Debug.Log($"Self on awake! Implicitly referenced! {mySingleton} and {myRootDependency};");
      myResource2 = new LoggingDisposable(); //46
    }

    [Hook(MixinOn.ConfigureManaged)]
    private static void OnConfigureSelf(ManagedRegistration entry) { }

    [Ticker("10s")]
    private void MyTickerFunc() {
      Debug.Log("This runs every 10 seconds!"); //
    }

    [Ticker]
    private async UniTask MyTickerFunc2() {
      Debug.Log("This runs every tick but takes a second!");
      await UniTask.Delay(1000);
    }
    
    [EventHandler]
    private void OnTestAsync(TestAsyncEvt evt) {
      
    }
    
    [EventHandler]
    private void OnComponentLoadEvt(ManagedLoadEvent evt) {
      evt.Context.PublishKey(new TypeKey(typeof(int), ""), 42);
    }

    [Hook(MixinOn.LoadManaged)]
    private void OnComponentLoad(ManagedLoadContext context) {


    }
  }

  [Managed(typeof(ApplicationScope), order: 1)]
  public partial class StageOne {
    [Inject("pipeline")] public string provided;
    [Bind("pipeline")] public string GetNext => $"{provided}1;";
  }


  [Managed(typeof(ApplicationScope))]
  public partial class StageTwo {
    [Inject("pipeline", required: false)] public string provided;
    [Bind(typeof(string), "pipeline")] public string GetNext => $"{provided}2;";
  }


  [Managed(typeof(ApplicationScope))]
  public partial class StageThree {
    [Inject("pipeline")] public List<string> provided;
    [Bind("pipeline")] public string GetNext => $"{string.Join(",", provided)}+3;";
  }

  [Managed(typeof(SceneScope))]
  public partial class SceneService {
    [Hook]
    public void OnInit() {
      Debug.Log("SceneService has initialized!");
    }
  }

  public struct TestEvent : Evt<TestEvent> { }

  public class TestAsyncEvt : AsyncChainEvt<TestAsyncEvt> { }
}