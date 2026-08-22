using System;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct HXDecoratorStyle {
    public static readonly HXDecoratorStyle Default = new(
      padding: EdgeInsets.Zero,
      decoratorPadding: EdgeInsets.Symmetric(0f, 1f),
      stackedGap: 4f,
      inlineGap: 8f,
      textGap: 2f
    );

    public readonly StyleLength4 padding, decoratorPadding;
    public readonly Length stackedGap, inlineGap, textGap;

    public HXDecoratorStyle(
      StyleLength4 padding,
      StyleLength4 decoratorPadding,
      Length stackedGap,
      Length inlineGap,
      Length textGap
    ) {
      this.padding = padding;
      this.decoratorPadding = decoratorPadding;
      this.stackedGap = stackedGap;
      this.inlineGap = inlineGap;
      this.textGap = textGap;
    }
  }

  /// <summary>Describes the content available to a decorator layout.</summary>
  public readonly struct FormFieldDecorators {
    public readonly Composable label, description, prefix, suffix, before, between, after;

    public FormFieldDecorators(
      Composable label = null,
      Composable description = null,
      Composable prefix = null,
      Composable suffix = null,
      Composable before = null,
      Composable between = null,
      Composable after = null
    ) {
      this.label = label;
      this.description = description;
      this.prefix = prefix;
      this.suffix = suffix;
      this.before = before;
      this.between = between;
      this.after = after;
    }
  }

  /// <summary>
  /// Places field and decorator content into an <see cref="HXDecorator"/>. Custom layouts can omit slots,
  /// reorder their content, or compose an entirely different structure.
  /// </summary>
  public delegate void DecoratorLayout(
    ref Composition cx,
    Composable field,
    in FormFieldDecorators decorators
  );

  /// <summary>Common physical arrangements for the built-in decorator layout.</summary>
  public enum DecoratorArrangement : byte {
    /// <summary>Stacked layout whose element consumes the remaining row width.</summary>
    Stacked,
    /// <summary>Stacked layout whose element keeps its intrinsic width.</summary>
    StackedCompact,
    /// <summary>Inline layout with a compact element beside a growing text column.</summary>
    Inline,
    /// <summary>Inline layout whose element also consumes available row width.</summary>
    InlineGrow
  }

  public enum DecoratorSlotType {
    None,
    Before,
    After,
    Prefix,
    Element,
    Suffix,
    Between,
    Label,
    Descriptor
  }

  public static class DecoratorExtensions {
    public static void ComposeDefaultDecorator(
      ref Composition cx,
      Composable field,
      in FormFieldDecorators decorators
    ) => ComposeDefaultDecorator(ref cx, field, in decorators, DecoratorArrangement.Stacked, null);

    public static void ComposeDefaultDecorator(
      ref Composition cx,
      Composable field,
      in FormFieldDecorators decorators,
      DecoratorArrangement arrangement,
      HXDecoratorStyle? style
    ) {
      using (cx.Decorator(out var slots, arrangement, style)) {
        if (decorators.label != null)
          using (slots.Label(ref cx))
            decorators.label.Invoke(ref cx);
        if (decorators.before != null)
          using (slots.Before(ref cx))
            decorators.before.Invoke(ref cx);
        if (decorators.prefix != null)
          using (slots.Prefix(ref cx))
            decorators.prefix.Invoke(ref cx);
        if (field != null)
          using (slots.Element(ref cx))
            field.Invoke(ref cx);
        if (decorators.suffix != null)
          using (slots.Suffix(ref cx))
            decorators.suffix.Invoke(ref cx);
        if (decorators.between != null)
          using (slots.Between(ref cx))
            decorators.between.Invoke(ref cx);
        if (decorators.description != null)
          using (slots.Description(ref cx))
            decorators.description.Invoke(ref cx);
        if (decorators.after != null)
          using (slots.After(ref cx))
            decorators.after.Invoke(ref cx);
      }
    }

    public static TextStyle? GetTextStyle(this DecoratorSlotType type, ThemeData data) {
      return type switch {
        DecoratorSlotType.Label => ThemeProperties.LabelStyle[data],
        DecoratorSlotType.Descriptor => ThemeProperties.DescriptionStyle[data],
        DecoratorSlotType.Prefix => ThemeProperties.PrefixStyle[data],
        DecoratorSlotType.Suffix => ThemeProperties.SuffixStyle[data],
        DecoratorSlotType.Before or DecoratorSlotType.Between or DecoratorSlotType.After =>
          ThemeProperties.DecoratorStyle[data],
        _ => null
      };
    }

    // Decorator
    private static readonly ushort _decoratorId = CompositionId.GetTypeId("HXDecorator");

    public static ScopeHandle Decorator(
      this ref Composition cx,
      out DecoratorSlots slots,
      DecoratorArrangement arrangement = DecoratorArrangement.Stacked,
      HXDecoratorStyle? style = null
    ) {
      if (!cx.AUTHORING.RequireComposable<HXDecorator>(_decoratorId, out var decorator, out var retained)) {
        decorator = new HXDecorator();
      }
      if (!retained) decorator.Initialize(cx);
      decorator.Configure(
        arrangement,
        style ?? HXDecorator.Style.ReadOrThemeProperty(in cx, ThemeProperties.Decorator)
      );

      slots = new DecoratorSlots(decorator, cx);
      return cx.AUTHORING.YieldScope(ref cx, decorator);
    }
  }

  public readonly ref struct DecoratorSlots {
    private readonly HXDecorator _element;
    private readonly Composition _composition;

    public DecoratorSlots(HXDecorator element, Composition composition) {
      _element = element;
      _composition = composition;
    }

    public ScopeHandle Element(ref Composition cx) => _element.element.Scope(ref cx);
    public ScopeHandle Label(ref Composition cx) => _element.label.Scope(ref cx);
    public ScopeHandle Description(ref Composition cx) => _element.description.Scope(ref cx);
    public ScopeHandle Prefix(ref Composition cx) => _element.prefix.Scope(ref cx);
    public ScopeHandle Suffix(ref Composition cx) => _element.suffix.Scope(ref cx);
    public ScopeHandle Before(ref Composition cx) => _element.before.Scope(ref cx);
    public ScopeHandle Between(ref Composition cx) => _element.between.Scope(ref cx);
    public ScopeHandle After(ref Composition cx) => _element.after.Scope(ref cx);
  }

  public class HXDecorator : ComposableElement, ISlotHost {
    /// <summary>Inherited default used by form fields when their style does not provide a layout.</summary>
    public static readonly ContextKey<DecoratorLayout> Layout =
      new("DecoratorLayout", DecoratorExtensions.ComposeDefaultDecorator);
    public static readonly ContextKey<HXDecoratorStyle> Style =
      new("DecoratorStyle", HXDecoratorStyle.Default);

    public static readonly UniqueStyleString ClassElement = new("hx-decorator-element");
    public static readonly UniqueStyleString ClassLabel = new("hx-decorator-label");
    public static readonly UniqueStyleString ClassDescription = new("hx-decorator-description");
    public static readonly UniqueStyleString ClassPrefix = new("hx-decorator-prefix");
    public static readonly UniqueStyleString ClassSuffix = new("hx-decorator-suffix");
    public static readonly UniqueStyleString ClassBefore = new("hx-decorator-before");
    public static readonly UniqueStyleString ClassAfter = new("hx-decorator-after");
    public static readonly UniqueStyleString ClassBetween = new("hx-decorator-between");

    public static DecoratorSlotType GetType(ComposableSlot slot) {
      var id = slot.SlotType;
      if (id == ClassElement.id) return DecoratorSlotType.Element;
      if (id == ClassLabel.id) return DecoratorSlotType.Label;
      if (id == ClassDescription.id) return DecoratorSlotType.Descriptor;
      if (id == ClassPrefix.id) return DecoratorSlotType.Prefix;
      if (id == ClassSuffix.id) return DecoratorSlotType.Suffix;
      if (id == ClassBefore.id) return DecoratorSlotType.Before;
      if (id == ClassAfter.id) return DecoratorSlotType.After;
      if (id == ClassBetween.id) return DecoratorSlotType.Between;
      return DecoratorSlotType.None;
    }

    public static void Label(ref Composition cx, LabelSpec spec) {
      if (cx.Slot == null) return;
      var type = GetType(cx.Slot);
      var data = cx.ReadContext(ThemeData.Key);

      using (cx.Group(Axis.Horizontal)) {
        cx.CURSOR.AlignSelf(Align.FlexStart);
        type.GetTextStyle(data)?.Apply(cx.CURSOR.composable);

        LabelSpec.Default(ref cx, spec);
      }
    }

    public readonly ComposableSlot before, after, prefix, element, suffix, between; // Decorator slots
    public readonly ComposableSlot label, description; // Semantic primary slots

    private readonly VisualElement _column, _row, _textColumn;
    private DecoratorArrangement? _arrangement;

    public DecoratorArrangement Arrangement => _arrangement ?? DecoratorArrangement.Stacked;

    public IBoundary Boundary { get; private set; }
    public override VisualElement contentContainer => element;

    public HXDecorator() {
      _column = new VisualElement().FlexContainer(crossAxisAlign: Align.Stretch).AddTo(hierarchy);
      _row = new VisualElement().Flexible(0, 1, Align.Stretch);
      _textColumn = new VisualElement().FlexContainer(crossAxisAlign: Align.Stretch);

      label = new ComposableSlot(this, ClassLabel).WithClasses(ClassLabel);
      before = new ComposableSlot(this, ClassBefore).WithClasses(ClassBefore);
      between = new ComposableSlot(this, ClassBetween).WithClasses(ClassBetween);
      description = new ComposableSlot(this, ClassDescription).WithClasses(ClassDescription);
      after = new ComposableSlot(this, ClassAfter).WithClasses(ClassAfter);
      prefix = new ComposableSlot(this, ClassPrefix).WithClasses(ClassPrefix);
      element = new ComposableSlot(this, ClassElement).WithClasses(ClassElement);
      suffix = new ComposableSlot(this, ClassSuffix).WithClasses(ClassSuffix);
      Configure(DecoratorArrangement.Stacked, HXDecoratorStyle.Default);
    }

    public void Configure(
      DecoratorArrangement arrangement,
      in HXDecoratorStyle style
    ) {
      var inline = arrangement is DecoratorArrangement.Inline or DecoratorArrangement.InlineGrow;
      var growElement = arrangement is DecoratorArrangement.Stacked or DecoratorArrangement.InlineGrow;
      _column.Padding(style.padding);
      ConfigureSlot(label, style.decoratorPadding, inline ? EdgeInsets.Only(bottom: style.textGap) :
        EdgeInsets.Only(bottom: style.stackedGap));
      ConfigureSlot(description, style.decoratorPadding,
        inline ? EdgeInsets.Zero : EdgeInsets.Only(top: style.stackedGap));
      ConfigureSlot(before, style.decoratorPadding, EdgeInsets.Only(bottom: style.stackedGap));
      ConfigureSlot(after, style.decoratorPadding, EdgeInsets.Only(top: style.stackedGap));
      ConfigureSlot(prefix, style.decoratorPadding, EdgeInsets.Only(right: style.inlineGap));
      ConfigureSlot(suffix, style.decoratorPadding, EdgeInsets.Only(left: style.inlineGap));
      ConfigureSlot(between, style.decoratorPadding, inline ? EdgeInsets.Only(bottom: style.textGap) : EdgeInsets.Zero);
      element.Padding(EdgeInsets.Zero).Margin(inline ? EdgeInsets.Only(right: style.inlineGap) : EdgeInsets.Zero);

      if (_arrangement == arrangement) return;
      _arrangement = arrangement;
      _column.Clear();
      _row.Clear();
      _textColumn.Clear();

      _row.FlexContainer(
        Axis.Horizontal,
        crossAxisAlign: inline ? Align.Center : Align.Stretch
      );
      element.Flexible(growElement ? 1f : 0f, growElement ? 1f : 0f, inline ? Align.Center : Align.Stretch);
      _textColumn.Flexible(inline ? 1f : 0f, 1f, Align.Stretch);
      _column.Add(before);
      if (!inline) {
        _column.Insert(0, label);
        _row.Add(prefix);
        _row.Add(element);
        _row.Add(suffix);
        _column.Add(_row);
        _column.Add(between);
        _column.Add(description);
      } else {
        _row.Add(prefix);
        _row.Add(element);
        _textColumn.Add(label);
        _textColumn.Add(between);
        _textColumn.Add(description);
        _row.Add(_textColumn);
        _row.Add(suffix);
        _column.Add(_row);
      }
      _column.Add(after);
    }

    private static void ConfigureSlot(VisualElement slot, StyleLength4 padding, StyleLength4 margin) {
      slot.Padding(padding).Margin(margin);
    }

    public void Initialize(in Composition cx) {
      Boundary = cx.boundary;
    }

    public override void Reset() {
      base.Reset();
      before?.Reset();
      after?.Reset();
      prefix?.Reset();
      element?.Reset();
      suffix?.Reset();
      between?.Reset();
      label?.Reset();
      description?.Reset();
      Boundary = null;
    }
  }
}
