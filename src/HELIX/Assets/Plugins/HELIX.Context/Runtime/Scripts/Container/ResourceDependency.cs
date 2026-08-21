using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HELIX.Context {
  public sealed class ResourceDependency<T> : ManagedDependency where T : UnityEngine.Object {
    public string Path { get; }

    public ResourceDependency(string path, string wireKey) : base(wireKey) { Path = path; }

    public override ManagedLoadResult Load(ManagedLoadContext context) {
      var value = Resources.Load<T>(PathOrRoot(Path));
      if (value == null)
        throw new ComponentInitializationException($"Resource '{Path}' was not found as {typeof(T).FullName}.");
      context.PublishKey(new TypeKey(typeof(T), WireKey), value);

      return new ManagedLoadResult(true);
    }

    internal static string PathOrRoot(string path) => string.IsNullOrWhiteSpace(path) ? "/" : path;
  }

  public sealed class ResourceListDependency<T> : ManagedDependency where T : UnityEngine.Object {

    public string Path { get; }

    public ResourceListDependency(string path, string wireKey) : base(wireKey) { Path = path; }

    public override ManagedLoadResult Load(ManagedLoadContext context) {
      var assets = Resources.LoadAll<T>(ResourceDependency<T>.PathOrRoot(Path));
      var list = assets.ToList();
      context.PublishKey(new TypeKey(typeof(List<T>), WireKey), list);
      return new ManagedLoadResult(true);
    }
  }
}