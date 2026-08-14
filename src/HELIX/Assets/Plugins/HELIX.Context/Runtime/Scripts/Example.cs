using System;
using UnityEngine;

namespace HELIX.Context {
  public interface IExampleMixin : IMixin {
    [MixinMethod(MixinOn.Init, -1)]
    static void MixinOnInit<T>(
      [MixinInject] T target
    ) { }

    [MixinMethod(MixinOn.Dispose)]
    static void MixinOnDestroy<T>(
      [MixinInject] T target
    ) { }
  }

  [EnableMixins]
  public partial class ExampleUser : MonoBehaviour, IExampleMixin {
    [AutoDispose]
    public IDisposable myResource;

    [MixinMethod]
    private void OnInit() {
      Debug.Log("Self on awake! Implicitly referenced!");
    }

    [MixinMethod(MixinOn.Init)]
    private void OnInit2() {
      Debug.Log("Self on awake2!");
    }
  }

  public partial class ExampleUser {
    private void Awake() {
      IExampleMixin.MixinOnInit(this);
      OnInit();
      OnInit2();
    }

    private void OnDestroy() {
      AutoDisposeMethods.DisposeDisposable(ref myResource);
      IExampleMixin.MixinOnDestroy(this);
    }
  }
}