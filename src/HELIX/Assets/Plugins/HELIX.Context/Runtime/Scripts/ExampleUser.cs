using System;
using HELIX.Context.Events;
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

  [EnableMixins]
  public partial class ExampleUser : MonoBehaviour, IExampleMixin, IComponentMixin {
    [AutoDispose]
    public IDisposable myResource;

    [MixinMethod]
    private void OnInit() {
      Debug.Log("Self on awake! Implicitly referenced!");
      myResource = new LoggingDisposable();
    }

    [EventHandler]
    private void OnTestEvent(ref TestEvent evt) {

    }

    [EventHandler]
    private void OnTestAsync(TestAsyncEvt evt) {

    }

    [MixinMethod(MixinOn.ConfigureRegistration)]
    private static void OnConfigureRegistration(RegistrationEntry registration) {

    }
  }

  public struct TestEvent : Evt<TestEvent> {}
  public class TestAsyncEvt : AsyncChainEvt<TestAsyncEvt> {}
}