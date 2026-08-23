using System;
using Cysharp.Threading.Tasks;
using HELIX;
using HELIX.Boot;
using HELIX.Context;
using HELIX.Prose;
using HELIX.UI;
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
  import: new[] { typeof(HelixCoreModule), typeof(BootModule), typeof(HelixUIModule) }
)]
public partial class HelixExampleApplication { }