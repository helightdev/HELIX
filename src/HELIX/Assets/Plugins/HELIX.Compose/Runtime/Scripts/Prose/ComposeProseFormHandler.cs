using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Prose {
  /// <summary>Handles complete form scopes without adding form knowledge to the default Compose reducer.</summary>
  public sealed class ComposeProseFormHandler : IProseScopeHandler<Composable> {
    public bool TryCreate(IProseScope scope, out IProseWriter writer) {
      if (scope is not ProseForm) {
        writer = null;
        return false;
      }
      writer = new ComposeProseWriter(new FormReducer());
      return true;
    }

    public Composable Finish(IProseWriter writer) {
      if (writer is not ComposeProseWriter compose)
        throw new ArgumentException("The form handler can only finish its own Compose writer.", nameof(writer));
      return compose.Build();
    }

    private sealed class FormReducer : ComposeProseReducer {
      private static readonly Length DefaultLabelWidth = new(35f, LengthUnit.Percent);

      public override Composable Reduce(
        IProseScope scope,
        IReadOnlyList<Composable> children,
        IReadOnlyList<IProseModifier> modifiers
      ) {
        if (children.Count == 0) return null;

        for (var i = 0; i < modifiers.Count; i++)
          if (modifiers[i] is ProseFullWidthModifier)
            return HXBaker.Flex(children, Axis.Vertical, cross: Align.Stretch);

        if (scope is ProseFieldLabel label) return Label(children, label.Width ?? DefaultLabelWidth);
        if (scope is ProseFieldValue) return Value(children);
        if (scope is ProseForm or ProseField) return ReduceFlex(children, modifiers, Axis.Vertical);
        if (scope is ProseFieldRow) return ReduceFlex(children, modifiers, Axis.Horizontal);
        return base.Reduce(scope, children, modifiers);
      }

      private static Composable Label(IReadOnlyList<Composable> children, Length width) => (ref Composition cx) => {
        using (var column = cx.Group(FlexGroup.Column(cross: Align.Stretch))) {
          column.With(Flex.Shrink(0));
          column.With(BoxConstraints.Preferred(width, StyleKeyword.Auto));
          for (var i = 0; i < children.Count; i++) children[i](ref cx);
        }
      };

      private static Composable Value(IReadOnlyList<Composable> children) => (ref Composition cx) => {
        using (cx.Group(FlexGroup.Column(cross: Align.Stretch), Flex.FillFlexible()))
          for (var i = 0; i < children.Count; i++)
            children[i](ref cx);
      };
    }
  }
}