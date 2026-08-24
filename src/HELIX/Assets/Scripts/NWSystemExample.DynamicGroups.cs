using HELIX.Compose;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Examples {
  public partial class HomeComposable {
    private enum DynamicEntryKind { Fixed, Grow1, Grow2, Grow3 }

    private static readonly EnumDatatype<DynamicEntryKind> DynamicEntryKindDatatype =
      Datatypes.Enum<DynamicEntryKind>();

    private sealed class DynamicExampleData {
      public readonly int number;
      public readonly DynamicEntryKind kind;
      public readonly string constraintsDescription;

      public DynamicExampleData(int number, DynamicEntryKind kind, string constraintsDescription) {
        this.number = number;
        this.kind = kind;
        this.constraintsDescription = constraintsDescription;
      }
    }

    private DynamicComposableController _dynamicGroupEntries;
    private int _dynamicEntrySequence;
    private bool _dynamicGroupsReversed;
    private DynamicEntryKind _dynamicEntryKind = DynamicEntryKind.Fixed;

    private void InitializeDynamicGroupsExample() {
      _dynamicGroupEntries = new DynamicComposableController();
      AddDynamicExampleEntry(10, DynamicEntryKind.Fixed);
      AddDynamicExampleEntry(20, DynamicEntryKind.Grow1);
      AddDynamicExampleEntry(30, DynamicEntryKind.Grow2);
      AddDynamicExampleEntry(40, DynamicEntryKind.Grow3);
    }

    private void DisposeDynamicGroupsExample() {
      _dynamicGroupEntries?.Dispose();
      _dynamicGroupEntries = null;
    }

    private void ComposeDynamicGroupsTab(ref Composition cx) {
      using (cx.ScrollView()) {
        cx.Text("Controller-backed dynamic composables", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text(
          "Both views below share one keyed controller. Insertions, removals and order changes reconcile " +
          "their independent entry boundaries without rebuilding either host composition.",
          TextRole.BodySmall
        );
        cx.Spacing(2);
        ComposeDynamicGroupActions(ref cx);
        cx.Spacing(2);

        cx.Text("DynamicFlexGroup", TextRole.LabelLarge);
        cx.Spacing(1);
        cx.DynamicFlexGroup(
          _dynamicGroupEntries,
          Axis.Horizontal,
          cross: Align.Stretch,
          gap: 8f,
          reverse: _dynamicGroupsReversed
        );

        cx.Spacing(3);
        cx.Text("DynamicScrollGroup", TextRole.LabelLarge);
        cx.Text("The same entries in a vertically scrolling group.", TextRole.BodySmall);
        cx.Spacing(1);
        using (cx.Container()) {
          if (cx.CursorDirty) cx.CURSOR.Height(300f).AlignSelf(Align.Stretch);
          cx.DynamicScrollGroup(
            _dynamicGroupEntries,
            Axis.Vertical,
            cross: Align.Stretch,
            gap: 8f,
            reverse: _dynamicGroupsReversed
          );
        }
      }
    }

    private void ComposeDynamicGroupActions(ref Composition cx) {
      cx.Text("Inserted item kind", TextRole.LabelMedium);
      cx.Spacing(1);
      cx.SegmentedChoice(
        _dynamicEntryKind,
        DynamicEntryKindDatatype,
        onChanged: static (context, value) => {
          using (context.Modify<HomeComposable>(out var owner)) owner._dynamicEntryKind = value;
        }
      );
      cx.Spacing(1);
      using (cx.Row(cross: Align.Stretch)) {
        cx.Button(
          static (ref Composition child) => child.Text("Insert first"),
          action: InsertFirstDynamicEntry
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Add last"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: AddLastDynamicEntry
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Remove last"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: RemoveLastDynamicEntry
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Reverse order"),
          style: ThemeProperties.ButtonGhost[in cx],
          action: ReverseDynamicEntryOrder
        );
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Toggle layout direction"),
          style: ThemeProperties.ButtonGhost[in cx],
          action: ToggleDynamicGroupDirection
        );
      }
    }

    private void AddDynamicExampleEntry(int order, DynamicEntryKind? kind = null) {
      var number = ++_dynamicEntrySequence;
      var resolvedKind = kind ?? _dynamicEntryKind;
      StyleFloat flex = resolvedKind switch {
        DynamicEntryKind.Grow1 => 1f,
        DynamicEntryKind.Grow2 => 2f,
        DynamicEntryKind.Grow3 => 3f,
        _ => StyleKeyword.Null
      };
      var constraint = resolvedKind switch {
        DynamicEntryKind.Fixed => AxisConstraint.Tight(220f),
        DynamicEntryKind.Grow1 => AxisConstraint.Min(160f),
        DynamicEntryKind.Grow2 => AxisConstraint.Only(preferred: 220f, min: 160f),
        _ => AxisConstraint.Only(preferred: 260f, min: 160f)
      };
      var constraintsDescription = resolvedKind switch {
        DynamicEntryKind.Fixed => "Grow: unset • Main axis: tight 220",
        DynamicEntryKind.Grow1 => "Grow: 1 • Main axis: minimum 160",
        DynamicEntryKind.Grow2 => "Grow: 2 • Main axis: preferred 220, minimum 160",
        _ => "Grow: 3 • Main axis: preferred 260, minimum 160"
      };
      _dynamicGroupEntries.AddEntry(
        $"dynamic-example-{number}",
        ComposeDynamicExampleEntry,
        new DynamicExampleData(number, resolvedKind, constraintsDescription),
        flex,
        order,
        constraint
      );
    }

    private static void ComposeDynamicExampleEntry(ref Composition cx) {
      var boundary = cx.Lookup<DynamicComposableElement>();
      var entry = boundary?.Entry;
      var data = entry?.UserData as DynamicExampleData;
      if (entry == null || data == null) return;

      using (cx.Column(cross: Align.Stretch, flex: Flex.Fill())) {
        if (cx.CursorDirty) {
          var theme = cx.ReadContext(ThemeData.Key);
          cx.CURSOR.Padding(10f).BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainer));
        }
        cx.Text($"Entry {data.number}", TextRole.LabelLarge);
        cx.Text($"Key: {entry.key}", TextRole.BodySmall);
        cx.Text($"Order: {entry.order}  •  Kind: {data.kind}", TextRole.BodySmall);
        cx.Text(data.constraintsDescription, TextRole.BodySmall);
        cx.Spacing(1);
        using (cx.Row(cross: Align.Stretch)) {
          cx.Button(
            static (ref Composition child) => child.Text("Earlier"),
            style: ThemeProperties.ButtonGhost[in cx],
            action: MoveDynamicEntryEarlier
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Later"),
            style: ThemeProperties.ButtonGhost[in cx],
            action: MoveDynamicEntryLater
          );
          cx.Spacing(1);
          cx.Button(
            static (ref Composition child) => child.Text("Delete"),
            style: ThemeProperties.ButtonOutlined[in cx],
            action: DeleteDynamicEntry
          );
        }
      }
    }

    private static void InsertFirstDynamicEntry(CompositionContext context) {
      var owner = context.Lookup<HomeComposable>();
      if (owner?._dynamicGroupEntries == null) return;
      var firstOrder = owner._dynamicGroupEntries.Count > 0
        ? owner._dynamicGroupEntries.Entries[0].order - 10
        : 0;
      owner.AddDynamicExampleEntry(firstOrder);
    }

    private static void AddLastDynamicEntry(CompositionContext context) {
      var owner = context.Lookup<HomeComposable>();
      if (owner?._dynamicGroupEntries == null) return;
      var entries = owner._dynamicGroupEntries.Entries;
      var lastOrder = entries.Count > 0 ? entries[^1].order + 10 : 0;
      owner.AddDynamicExampleEntry(lastOrder);
    }

    private static void RemoveLastDynamicEntry(CompositionContext context) {
      var owner = context.Lookup<HomeComposable>();
      var entries = owner?._dynamicGroupEntries?.Entries;
      if (entries == null || entries.Count == 0) return;
      owner._dynamicGroupEntries.RemoveEntry(entries[^1]);
    }

    private static void ReverseDynamicEntryOrder(CompositionContext context) {
      var controller = context.Lookup<HomeComposable>()?._dynamicGroupEntries;
      if (controller == null) return;
      var entries = controller.Entries;
      for (var i = 0; i < entries.Count; i++) entries[i].order = entries.Count - i;
      controller.NotifyEntriesChanged();
    }

    private static void ToggleDynamicGroupDirection(CompositionContext context) {
      using (context.Modify<HomeComposable>(out var owner))
        owner._dynamicGroupsReversed = !owner._dynamicGroupsReversed;
    }

    private static void MoveDynamicEntryEarlier(CompositionContext context) {
      ChangeDynamicEntryOrder(context, -15);
    }

    private static void MoveDynamicEntryLater(CompositionContext context) {
      ChangeDynamicEntryOrder(context, 15);
    }

    private static void ChangeDynamicEntryOrder(CompositionContext context, int offset) {
      var entry = context.Lookup<DynamicComposableElement>()?.Entry;
      if (entry?.Controller == null) return;
      entry.order += offset;
      entry.Controller.NotifyEntriesChanged();
    }

    private static void DeleteDynamicEntry(CompositionContext context) {
      var entry = context.Lookup<DynamicComposableElement>()?.Entry;
      entry?.Controller?.RemoveEntry(entry);
    }
  }
}
