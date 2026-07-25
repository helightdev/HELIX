using System;
using System.Collections.Generic;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.NW {
  public interface ISpec {
  }

  public interface IBakeableSpec : ISpec {
    Composable Bake();
  }

  public static class HXBaker {
    public static Composable Flex(
      IReadOnlyList<Composable> children,
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      bool reverse = false,
      bool clear = false
    ) => (ref Composition cx) => {
      using (cx.Flex(mainAxis: mainAxis, main: main, cross: cross, reverse: reverse, clear: clear)) {
        for (var i = 0; i < children.Count; i++) {
          children[i](ref cx);
        }
      }
    };
    public static Composable Flex(
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      bool reverse = false,
      bool clear = false,
      params Composable[] children
    ) => Flex(children, mainAxis, main, cross, reverse, clear);
  }

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
      if (specs is IBakeableSpec bakeable) return bakeable.Bake();
      return (ref Composition cx) => cx.Spec(in specs);
    }
  }
}