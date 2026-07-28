using System;
using System.Collections.Generic;
using HELIX.Types;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public interface ISpec { }

  public class SpecConfig {
    public readonly Dictionary<Type, Delegate> transformers = new();
    public readonly SpecConfig parent;

    public SpecConfig(SpecConfig parent = null) {
      this.parent = parent;
    }

    public SpecConfig AddFactory<T>(ReadComposable<T> composable) where T : struct, ISpec {
      transformers[typeof(T)] = composable;
      return this;
    }

    public ReadComposable<T> GetFactory<T>() where T : struct, ISpec {
      if (transformers.TryGetValue(typeof(T), out var factory)) {
        return factory as ReadComposable<T>;
      }
      return parent?.GetFactory<T>();
    }

    public static readonly ContextKey<SpecConfig> Key = new("specs", Default);
    public static SpecConfig Empty => new();
    public static SpecConfig Default => new SpecConfig()
      .AddFactory<LabelSpec>(LabelSpec.Default)
      .AddFactory<IconRef>(IconRef.Default)
      .AddFactory<ChevronSpec>(ChevronSpec.Default)
    ;
  }

  public static class SpecExtensions {
    public static void Spec<T>(ref this Composition cx, in T specs) where T : struct, ISpec {
      var configuration = SpecConfig.Key[in cx];
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

  public static class HXBaker {
    public static Composable Flex(
      IReadOnlyList<Composable> children,
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      float gap = 0f,
      bool reverse = false,
      bool clear = false
    ) => (ref Composition cx) => {
      using (cx.Flex(mainAxis: mainAxis, main: main, cross: cross, reverse: reverse, clear: clear)) {
        for (var i = 0; i < children.Count; i++) {
          if (i > 0 && gap >= math.EPSILON) cx.Gap(gap);
          children[i](ref cx);
        }
      }
    };

    public static Composable Flex(
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      float gap = 0f,
      bool reverse = false,
      bool clear = false,
      params Composable[] children
    ) => Flex(children, mainAxis, main, cross, gap, reverse, clear);
  }

  public struct BakeableComposition<TInput> {
    private TInput _lastInput;
    private bool _hasInput;
    private Composable _composable;
    private readonly Func<TInput, Composable> _baker;

    public BakeableComposition(Func<TInput, Composable> baker) : this() {
      _baker = baker;
    }

    public void Compose(TInput input, ref Composition cx) {
      if (!_hasInput || !_lastInput.Equals(input)) Regenerate(input);
      _composable(ref cx);
    }

    private void Regenerate(TInput input) {
      _hasInput = true;
      _lastInput = input;
      _composable = _baker(input);
    }
  }
}