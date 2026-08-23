using System;
using System.Collections.Generic;
using HELIX.Prose;

namespace HELIX.Compose {
  public interface ISpec { }

  public interface ISpecHandler {
    ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec;
  }

  public class SpecConfig {
    public readonly Dictionary<Type, Delegate> transformers = new();
    public readonly List<ISpecHandler> handlers = new();
    public readonly SpecConfig parent;

    public SpecConfig(SpecConfig parent = null) {
      this.parent = parent;
    }

    public SpecConfig AddFactory<T>(ReadComposable<T> composable) where T : struct, ISpec {
      transformers[typeof(T)] = composable;
      return this;
    }

    public SpecConfig AddHandler(ISpecHandler handler) {
      handlers.Add(handler);
      return this;
    }

    public SpecConfig AddHandler<T>() where T : ISpecHandler, new() => AddHandler(new T());

    public ReadComposable<T> GetFactory<T>(in T spec) where T : struct, ISpec {
      if (transformers.TryGetValue(typeof(T), out var factory)) {
        return factory as ReadComposable<T>;
      }
      for (var i = 0; i < handlers.Count; i++) {
        var handled = handlers[i].GetFactory(in spec);
        if (handled != null) return handled;
      }
      return parent?.GetFactory(in spec);
    }

    public static readonly ContextKey<SpecConfig> Key = new("specs", Default);
    public static SpecConfig Empty => new();
    public static SpecConfig Default => new SpecConfig()
      .AddFactory<LabelSpec>(LabelSpec.Default)
      .AddFactory<IconRef>(IconRef.Default)
      .AddFactory<ChevronSpec>(ChevronSpec.Default)
      .AddHandler<ChoiceControlSpecHandler>()
      .AddHandler<TextControlSpecHandler>()
      .AddHandler<IntegerControlSpecHandler>()
      .AddHandler<FloatControlSpecHandler>()
      .AddHandler<CheckboxControlSpecHandler>()
    ;
  }

  public static class SpecExtensions {
    public static void Spec<T>(ref this Composition cx, in T specs) where T : struct, ISpec {
      var configuration = SpecConfig.Key[in cx];
      var factory = configuration.GetFactory(in specs);
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
