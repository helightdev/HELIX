using System;
using System.Collections.Generic;

namespace HELIX.NW {
  public interface ISpec { }

  public delegate void SpecComposable<T>(ref Composition cx, in T spec) where T : struct, ISpec;

  public class SpecConfiguration {
    public readonly Dictionary<Type, Delegate> transformers = new();
    public readonly SpecConfiguration parent;

    public SpecConfiguration(SpecConfiguration parent = null) {
      this.parent = parent;
    }

    public SpecConfiguration AddFactory<T>(SpecComposable<T> composable) where T : struct, ISpec {
      transformers[typeof(T)] = composable;
      return this;
    }

    public SpecComposable<T> GetFactory<T>() where T : struct, ISpec {
      if (transformers.TryGetValue(typeof(T), out var factory)) {
        return factory as SpecComposable<T>;
      }
      return parent?.GetFactory<T>();
    }

    public static readonly ContextKey<SpecConfiguration> Key = new("specs");

    public static SpecConfiguration Empty => new();
  }

  public static class SpecExtensions {
    public static void Spec<T>(ref this Composition cx, in T specs) where T : struct, ISpec {
      var configuration = cx.ReadContext(SpecConfiguration.Key) ?? SpecConfiguration.Empty;
      var factory = configuration.GetFactory<T>();
      if (factory == null) throw new Exception($"No factory found for spec type {typeof(T)}");
      factory(ref cx, in specs);
    }

    public static void Compose<T>(this T specs, ref Composition cx) where T : struct, ISpec {
      cx.Spec(specs);
    }

    public static Composable Composable<T>(this T specs) where T : struct, ISpec {
      return (ref Composition cx) => cx.Spec(in specs);
    }
  }
}