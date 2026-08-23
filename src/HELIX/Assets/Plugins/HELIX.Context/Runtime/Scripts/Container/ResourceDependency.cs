using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HELIX.Context {
  public sealed class ResourceDependency<T> : ManagedDependency where T : UnityEngine.Object {
    public string Path { get; }
    public bool IsRequired { get; }

    public ResourceDependency(string path, string wireKey, bool isRequired = false) : base(wireKey) {
      Path = path;
      IsRequired = isRequired;
    }

    public override ManagedLoadResult Load(ManagedLoadContext context) {
      var value = Resources.Load<T>(PathOrRoot(Path));
      if (value == null && IsRequired)
        throw new ComponentInitializationException($"Resource '{Path}' was not found as {typeof(T).FullName}.");
      if (value == null) return new ManagedLoadResult(false);
      context.PublishKey(new TypeKey(typeof(T), WireKey), value);

      return new ManagedLoadResult(true);
    }

    internal static string PathOrRoot(string path) => string.IsNullOrWhiteSpace(path) ? "" : path;
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