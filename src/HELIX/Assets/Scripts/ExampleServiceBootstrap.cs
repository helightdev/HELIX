using System;
using Cysharp.Threading.Tasks;
using HELIX.Context;
using HELIX.Prose;
using UnityEngine;


public class ExampleServiceBootstrap : MonoBehaviour {

  [NonSerialized]
  public ManagedContainer container;

  private async UniTaskVoid Start() {
    var discovered = HelixExampleApplication.Discover();

    var writer = new ProseUnityRichTextWriter();
    ManagedGraphProse.WriteDeclared(writer, discovered);
    Debug.Log(writer.Build());

    container = new ManagedContainerBuilder().Build();
    container.PrepareRegistrar(discovered);
    await container.StartApplication();

    writer.Reset();
    ManagedGraphProse.WriteLive(writer, container);
    Debug.Log(writer.Build());
  }


  private void OnDestroy() {
    container?.Dispose();
    container = null;
  }
}

[HelixApplication(
  name: "Example",
  import: new[] { typeof(HelixCoreModule) }
)]
public partial class HelixExampleApplication { }