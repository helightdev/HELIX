using System;
using System.Collections.Generic;
using HELIX.Widgets.Forms;
using HELIX.Widgets.Theming;

namespace HELIX.Widgets.Universal {
  public abstract class WidgetSpec {
    public static implicit operator Widget(WidgetSpec spec) => new HSpecWidget(spec);
  }

  public class WidgetSpecConfiguration {
    public readonly Dictionary<Type, WidgetSpecTransformer> transformers = new();
    public readonly WidgetSpecConfiguration parent;

    public WidgetSpecConfiguration(WidgetSpecConfiguration parent = null) {
      this.parent = parent;
    }

    public WidgetSpecConfiguration AddFactory(WidgetSpecTransformer widgetSpecTransformer) {
      transformers[widgetSpecTransformer.SpecType] = widgetSpecTransformer;
      return this;
    }

    public WidgetSpecConfiguration AddFactory<TSpec>(WidgetSpecTransformer widgetSpecTransformer) {
      transformers[typeof(TSpec)] = widgetSpecTransformer;
      return this;
    }

    public WidgetSpecTransformer GetFactory(WidgetSpec spec) {
      var type = spec.GetType();
      return transformers.TryGetValue(type, out var factory) ? factory : parent?.GetFactory(spec);
    }

    public static readonly ThemeProperty<WidgetSpecConfiguration> Property = new("widget-specs", Empty);

    public static WidgetSpecConfiguration Empty => new();
  }

  public class HSpecWidget : StatelessWidget<HSpecWidget> {
    public readonly WidgetSpec spec;

    public HSpecWidget(
      WidgetSpec spec,
      Key key = default,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, modifiers) {
      this.spec = spec;
    }

    public override Widget Build(BuildContext context) {
      var config = context.GetThemed(WidgetSpecConfiguration.Property);
      var factory = config?.GetFactory(spec);
      if (factory != null) return factory.Build(context, spec);
      return new HError($"No factory found for spec type {spec.GetType()}");
    }
  }

  public abstract class FieldSpecTransformer<TValue, TSpec> : WidgetSpecTransformer where TSpec : FieldSpec<TValue> {
    private readonly BuildFunction<TSpec> _builder;

    public FieldSpecTransformer(BuildFunction<TSpec> builder) {
      _builder = builder;
    }

    public override Widget Build(BuildContext context, WidgetSpec spec) {
      if (spec is TSpec typedSpec) {
        return _builder(context, typedSpec);
      }

      throw new ArgumentException($"Invalid spec type: expected {typeof(TSpec)}, got {spec.GetType()}");
    }

    public override Type SpecType => typeof(TSpec);
    public Type ValueType => typeof(TValue);
  }

  public abstract class WidgetSpecTransformer {
    public abstract Widget Build(BuildContext context, WidgetSpec spec);

    public abstract Type SpecType { get; }
  }


  public class WidgetSpecTransformer<TSpec> : WidgetSpecTransformer where TSpec : WidgetSpec {
    public readonly BuildFunction<TSpec> builder;

    public WidgetSpecTransformer(BuildFunction<TSpec> builder) {
      this.builder = builder;
    }

    public override Widget Build(BuildContext context, WidgetSpec spec) {
      return builder(context, (TSpec)spec);
    }

    public override Type SpecType => typeof(TSpec);
  }
}