using System;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct FormFieldDecorators {
    public readonly Composable prefix, suffix, before, between, after;

    public FormFieldDecorators(
      Composable prefix = null,
      Composable suffix = null,
      Composable before = null,
      Composable between = null,
      Composable after = null
    ) {
      this.prefix = prefix;
      this.suffix = suffix;
      this.before = before;
      this.between = between;
      this.after = after;
    }
  }

  public enum DecoratorSlotType { None, Before, After, Prefix, Element, Suffix, Between, Label, Descriptor }

  public static class DecoratorExtensions {
    public static StyleLength4 GetMargin(this DecoratorSlotType type, ThemeData data) {
      return type switch {
        DecoratorSlotType.Before => EdgeInsets.Only(bottom: ThemeProperties.DecoratorColumnGap[data]),
        DecoratorSlotType.After => EdgeInsets.Only(top: ThemeProperties.DecoratorColumnGap[data]),
        DecoratorSlotType.Prefix => EdgeInsets.Only(right: ThemeProperties.TextGap[data]),
        DecoratorSlotType.Suffix => EdgeInsets.Only(left: ThemeProperties.TextGap[data]),
        DecoratorSlotType.None or DecoratorSlotType.Element or DecoratorSlotType.Between
          or DecoratorSlotType.Label or DecoratorSlotType.Descriptor => EdgeInsets.Zero,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };
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

    public static ScopeHandle Decorator(this ref Composition cx, out DecoratorSlots slots) {
      if (!cx.AUTHORING.RequireComposable<HXDecorator>(_decoratorId, out var decorator, out var retained)) {
        decorator = new HXDecorator();
      }
      if (!retained) decorator.Initialize(cx);

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

    public ScopeHandle Element() => _element.element.Scope(_composition);
    public ScopeHandle Label() => _element.label.Scope(_composition);
    public ScopeHandle Description() => _element.description.Scope(_composition);
    public ScopeHandle Prefix() => _element.prefix.Scope(_composition);
    public ScopeHandle Suffix() => _element.suffix.Scope(_composition);
    public ScopeHandle Before() => _element.before.Scope(_composition);
    public ScopeHandle Between() => _element.between.Scope(_composition);
    public ScopeHandle After() => _element.after.Scope(_composition);
  }

  public class HXDecorator : ComposableElement, ISlotHost {
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
        cx.CURSOR.Margin(type.GetMargin(data));
        type.GetTextStyle(data)?.Apply(cx.CURSOR.composable);

        LabelSpec.Default(ref cx, spec);
      }
    }

    public readonly ComposableSlot before, after, prefix, element, suffix, between; // Decorator slots
    public readonly ComposableSlot label, description; // Semantic primary slots

    public IBoundary Boundary { get; private set; }
    public override VisualElement contentContainer => element;

    public HXDecorator() {
      var column = new VisualElement().AddTo(hierarchy);

      label = new ComposableSlot(this, ClassLabel).WithClasses(ClassLabel).AddTo(column);
      before = new ComposableSlot(this, ClassBefore).WithClasses(ClassBefore).AddTo(column);
      var row = new VisualElement()
        .Flexible(0, 1, Align.Stretch)
        .FlexContainer(Axis.Horizontal)
        .AddTo(column);
      between = new ComposableSlot(this, ClassBetween).WithClasses(ClassBetween).AddTo(column);
      description = new ComposableSlot(this, ClassDescription).WithClasses(ClassDescription).AddTo(column);
      after = new ComposableSlot(this, ClassAfter).WithClasses(ClassAfter).AddTo(column);
      prefix = new ComposableSlot(this, ClassPrefix).WithClasses(ClassPrefix).AddTo(row);
      element = new ComposableSlot(this, ClassElement).WithClasses(ClassElement).AddTo(row);
      suffix = new ComposableSlot(this, ClassSuffix).WithClasses(ClassSuffix).AddTo(row);
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