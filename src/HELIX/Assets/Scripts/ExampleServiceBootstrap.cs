using System;
using Cysharp.Threading.Tasks;
using HELIX.Context;
using HELIX.Prose;
using UnityEngine;


public class ExampleServiceBootstrap : MonoBehaviour {
  private async UniTaskVoid Start() {
    var discovered = ComponentDiscovery.Discover();

    var writer = new ProseUnityRichTextWriter();
    ComponentGraphProse.WriteDeclared(writer, discovered);
    Debug.Log(writer.Build());

    var container = new ManagedContainerBuilder().Build();
    container.PrepareRegistrar(discovered);
    await container.StartApplication();

    writer.Reset();
    ComponentGraphProse.WriteLive(writer, container);
    Debug.Log(writer.Build());
  }
}
