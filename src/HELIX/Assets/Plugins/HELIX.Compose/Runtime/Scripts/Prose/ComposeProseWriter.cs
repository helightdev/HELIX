using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Prose {
  public class ComposeProseWriter : ReducingProseWriter<Composable> {
    public ComposeProseWriter(
      ProseReducerChain<Composable> delegates = null
    ) : this(new ComposeProseReducer(), delegates) { }

    public ComposeProseWriter(
      ProseReducer<Composable> reducer, ProseReducerChain<Composable> delegates = null
    ) : base(reducer, delegates) { }
  }

  public class ComposeProseReducer : ProseReducer<Composable> {
    public override bool TryMap(IProse prose, IReadOnlyList<IProseModifier> modifiers, out Composable result) {
      var writer = CreateTextWriter(modifiers);
      if (prose == null) writer.Write((IProse)null);
      else prose.ToProse(writer);
      result = ConvertText(FinishText(writer, modifiers.Count > 0), modifiers);
      return result != null;
    }

    public override bool TryMap(string text, IReadOnlyList<IProseModifier> modifiers, out Composable result) {
      if (text == null) {
        result = null;
        return false;
      }
      var writer = CreateTextWriter(modifiers);
      writer.Write(text);
      result = ConvertText(FinishText(writer, modifiers.Count > 0), modifiers);
      return result != null;
    }

    public override bool TryMap<T>(
      T value, IDatatype<T> datatype, IReadOnlyList<IProseModifier> modifiers,
      out Composable result
    ) {
      if (datatype is IComposableIDatatype<T> composeFormatter) {
        result = composeFormatter.ToComposable(value);
        return true;
      }
      var writer = CreateTextWriter(modifiers);
      datatype.ToProse(writer, value);
      result = ConvertText(FinishText(writer, modifiers.Count > 0), modifiers);
      return result != null;
    }

    public override Composable Reduce(
      IProseScope scope, IReadOnlyList<Composable> children, IReadOnlyList<IProseModifier> modifiers
    ) {
      var axis = scope is ProseSection or ProseList or ProseTable or ProseTree
        ? Axis.Vertical
        : Axis.Horizontal;
      return ReduceFlex(children, modifiers, axis);
    }

    protected Composable ReduceFlex(
      IReadOnlyList<Composable> children, IReadOnlyList<IProseModifier> modifiers, Axis axis,
      float defaultGap = 0f, Align? defaultCross = null
    ) {
      if (children.Count == 0) return null;
      var main = Justify.FlexStart;
      var cross = defaultCross ?? (axis == Axis.Vertical ? Align.Stretch : Align.Center);
      var gap = defaultGap;
      var reverse = false;
      var clear = false;
      var hasLayout = false;
      for (var i = 0; i < modifiers.Count; i++) {
        if (modifiers[i] is not ComposeFlexModifier modifier) continue;
        hasLayout = true;
        if (modifier.Axis.HasValue) axis = modifier.Axis.Value;
        if (modifier.Main.HasValue) main = modifier.Main.Value;
        if (modifier.Cross.HasValue) cross = modifier.Cross.Value;
        if (modifier.Gap.HasValue) gap = modifier.Gap.Value;
        if (modifier.Reverse.HasValue) reverse = modifier.Reverse.Value;
        if (modifier.Clear.HasValue) clear = modifier.Clear.Value;
      }
      if (children.Count == 1 && !hasLayout) return children[0];
      return HXBaker.Flex(children, axis, main, cross, gap, reverse, clear);
    }

    public override Composable Reduce(IReadOnlyList<Composable> children) {
      if (children.Count == 0) return null;
      return children.Count == 1
        ? children[0]
        : HXBaker.Flex(children, Axis.Vertical, cross: Align.Stretch);
    }

    private static ProseUnityRichTextWriter CreateTextWriter(IReadOnlyList<IProseModifier> modifiers) {
      var writer = new ProseUnityRichTextWriter();
      if (modifiers.Count == 0) return writer;
      writer.Begin(ProseScopes.Span);
      for (var i = 0; i < modifiers.Count; i++) writer.Push(modifiers[i]);
      return writer;
    }

    private static string FinishText(ProseUnityRichTextWriter writer, bool decorated) {
      if (decorated) writer.End();
      return writer.Build();
    }

    private static Composable ConvertText(
      string richText, IReadOnlyList<IProseModifier> modifiers
    ) {
      if (string.IsNullOrEmpty(richText)) return null;
      var wrap = WhiteSpace.PreWrap;
      for (var i = 0; i < modifiers.Count; i++)
        if (modifiers[i] is ProseNoWrapModifier) {
          wrap = WhiteSpace.Pre;
          break;
        }
      var style = new TextStyle(wrap: wrap);
      return (ref Composition cx) => {
        ref var text = ref cx.Text(richText);
        style.Apply(text.composable);
      };
    }
  }
}
