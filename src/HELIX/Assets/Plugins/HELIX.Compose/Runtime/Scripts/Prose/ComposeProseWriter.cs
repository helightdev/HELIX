using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Prose {
  /// <summary>
  /// Materializes prose text as Unity rich text and reduces nested prose frames to composables.
  /// The conversion and baking steps are virtual so semantic scopes can be specialized later.
  /// </summary>
  public class ComposeProseWriter : ProseWriter {
    private sealed class Frame {
      public IProseScope scope;
      public bool active;
      public readonly List<IProseModifier> modifiers = new();
      public readonly List<Composable> children = new();
    }

    private readonly List<Frame> _frames = new();
    private readonly List<Composable> _root = new();

    public int FrameCount => _frames.Count;

    public override bool TryBeginFrame(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      if (IsInactive || !AcceptFrame(scope)) return false;
      _frames.Add(new Frame { scope = scope, active = true });
      return true;
    }

    public override void BeginFrame(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      if (TryBeginFrame(scope)) return;
      _frames.Add(new Frame { scope = scope, active = false });
    }

    public override void End() {
      if (_frames.Count == 0) throw new InvalidOperationException("There is no Prose frame to pop.");
      var index = _frames.Count - 1;
      var frame = _frames[index];
      _frames.RemoveAt(index);
      if (!frame.active) return;

      var composable = BakeFrame(frame.scope, frame.children, frame.modifiers);
      if (composable != null) Add(composable);
    }

    public override void PushModifier(IProseModifier modifier) {
      if (modifier == null) throw new ArgumentNullException(nameof(modifier));
      if (_frames.Count == 0) throw new InvalidOperationException("A modifier requires an active Prose frame.");
      if (!IsInactive) _frames[^1].modifiers.Add(modifier);
    }

    public override void Write(IProse prose) {
      if (IsInactive) return;
      var writer = CreateTextWriter();
      var decorated = ApplyModifiers(writer);
      if (prose == null) writer.Write((IProse)null);
      else prose.ToProse(writer);
      AddText(FinishText(writer, decorated));
    }

    public override void Write<T>(T value, IProseFormatter<T> formatter) {
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));
      if (IsInactive) return;
      if (formatter is IComposeProseFormatter<T> composeFormatter) {
        Add(composeFormatter.ToComposable(value));
        return;
      }
      var writer = CreateTextWriter();
      var decorated = ApplyModifiers(writer);
      formatter.ToProse(writer, value);
      AddText(FinishText(writer, decorated));
    }

    public override void Write(string text) {
      if (text == null || IsInactive) return;
      var writer = CreateTextWriter();
      var decorated = ApplyModifiers(writer);
      writer.Write(text);
      AddText(FinishText(writer, decorated));
    }

    /// <summary>Reduces all completed root content to one callable composable.</summary>
    public virtual Composable Build() {
      if (_frames.Count != 0)
        throw new InvalidOperationException("All Prose frames must be ended before building the composition.");
      var children = new Composable[_root.Count];
      _root.CopyTo(children);
      return BakeRoot(children);
    }

    public virtual void Reset() {
      _frames.Clear();
      _root.Clear();
    }

    protected virtual bool AcceptFrame(IProseScope scope) => true;
    protected virtual ProseUnityRichTextWriter CreateTextWriter() => new();
    protected virtual Composable ConvertText(string richText) => (ref Composition cx) =>
      cx.Text(richText).WhiteSpace(WhiteSpace.Pre);

    protected virtual Axis GetFrameAxis(IProseScope scope) => scope switch {
      ProseSection or ProseList or ProseTable or ProseTree => Axis.Vertical,
      _ => Axis.Horizontal
    };

    protected virtual Align GetFrameCrossAlignment(IProseScope scope) =>
      GetFrameAxis(scope) == Axis.Vertical ? Align.Stretch : Align.Center;

    protected virtual Composable BakeFrame(
      IProseScope scope,
      IReadOnlyList<Composable> children,
      IReadOnlyList<IProseModifier> modifiers
    ) {
      var axis = GetFrameAxis(scope);
      var main = Justify.FlexStart;
      var cross = GetFrameCrossAlignment(scope);
      var gap = 0f;
      var reverse = false;
      var clear = false;

      for (var i = 0; i < modifiers.Count; i++) {
        if (modifiers[i] is not ComposeFlexModifier modifier) continue;
        if (modifier.Axis.HasValue) axis = modifier.Axis.Value;
        if (modifier.Main.HasValue) main = modifier.Main.Value;
        if (modifier.Cross.HasValue) cross = modifier.Cross.Value;
        if (modifier.Gap.HasValue) gap = modifier.Gap.Value;
        if (modifier.Reverse.HasValue) reverse = modifier.Reverse.Value;
        if (modifier.Clear.HasValue) clear = modifier.Clear.Value;
      }

      return HXBaker.Flex(children, axis, main, cross, gap, reverse, clear);
    }

    protected virtual Composable BakeRoot(IReadOnlyList<Composable> children) =>
      HXBaker.Flex(children, Axis.Vertical, cross: Align.Stretch);

    private bool IsInactive => _frames.Count > 0 && !_frames[^1].active;

    private void Add(Composable composable) {
      if (_frames.Count == 0) _root.Add(composable);
      else _frames[^1].children.Add(composable);
    }

    private void AddText(string text) {
      if (!string.IsNullOrEmpty(text)) Add(ConvertText(text));
    }

    private bool ApplyModifiers(ProseUnityRichTextWriter writer) {
      var decorated = false;
      for (var i = 0; i < _frames.Count && !decorated; i++)
        decorated = _frames[i].active && _frames[i].modifiers.Count > 0;
      if (!decorated) return false;

      writer.BeginFrame(ProseScopes.Span);
      for (var i = 0; i < _frames.Count; i++) {
        var modifiers = _frames[i].modifiers;
        for (var j = 0; j < modifiers.Count; j++) writer.PushModifier(modifiers[j]);
      }
      return true;
    }

    private static string FinishText(ProseUnityRichTextWriter writer, bool decorated) {
      if (decorated) writer.End();
      return writer.Build();
    }
  }
}
