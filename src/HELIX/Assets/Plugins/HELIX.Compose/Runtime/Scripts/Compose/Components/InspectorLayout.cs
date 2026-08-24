using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct InspectorLayoutSlots {
    public readonly Composable nameStart, nameEnd, valueStart, valueEnd;

    public InspectorLayoutSlots(
      Composable nameStart = null, Composable nameEnd = null,
      Composable valueStart = null, Composable valueEnd = null
    ) {
      this.nameStart = nameStart;
      this.nameEnd = nameEnd;
      this.valueStart = valueStart;
      this.valueEnd = valueEnd;
    }
  }

  /// <summary>Pure two-column inspector layout with optional content around both columns.</summary>
  public readonly struct InspectorLayout : ISpec {
    public readonly Composable name, value;
    public readonly InspectorLayoutSlots slots;
    public readonly Length nameWidth;
    public readonly bool stacked, hideName;

    public InspectorLayout(
      Composable name, Composable value, in InspectorLayoutSlots slots = default,
      Length? nameWidth = null, bool stacked = false, bool hideName = false
    ) {
      this.name = name;
      this.value = value;
      this.slots = slots;
      this.nameWidth = nameWidth ?? new Length(35f, LengthUnit.Percent);
      this.stacked = stacked;
      this.hideName = hideName;
    }

    public static void Default(ref Composition cx, in InspectorLayout spec) {
      if (spec.stacked) {
        using (cx.Column(cross: Align.Stretch)) {
          if (spec is { hideName: false, name: not null }) {
            spec.name(ref cx);
            cx.Spacing(1);
          }
          ComposeValue(ref cx, spec, includeNameSlots: true);
        }
        return;
      }
      using (cx.Row(cross: Align.Center)) {
        using (var name = cx.Row(cross: Align.Center, flex: Flex.Shrink(0))) {
          name.With(BoxConstraints.Preferred(spec.nameWidth, StyleKeyword.Auto));
          ComposeSlot(ref cx, spec.slots.nameStart);
          spec.name?.Invoke(ref cx);
          if (spec.slots.nameEnd != null) {
            cx.Gap();
            spec.slots.nameEnd(ref cx);
          }
        }
        ComposeValue(ref cx, spec, includeNameSlots: false);
      }
    }

    private static void ComposeValue(ref Composition cx, in InspectorLayout spec, bool includeNameSlots) {
      using (cx.Row(cross: Align.Center, flex: Flex.FillFlexible())) {
        if (includeNameSlots) {
          ComposeSlot(ref cx, spec.slots.nameStart);
          ComposeSlot(ref cx, spec.slots.nameEnd);
        }
        ComposeSlot(ref cx, spec.slots.valueStart);
        spec.value?.Invoke(ref cx);
        if (spec.slots.valueEnd != null) {
          cx.Gap();
          spec.slots.valueEnd(ref cx);
        }
      }
    }

    private static void ComposeSlot(ref Composition cx, Composable content) {
      content?.Invoke(ref cx);
    }
  }
}
