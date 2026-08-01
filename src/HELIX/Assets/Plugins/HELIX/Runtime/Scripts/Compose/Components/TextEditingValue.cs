using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public struct TextEditingValue : IEquatable<TextEditingValue> {
    public string text;
    public int cursorIndex;
    public int selectionIndex;

    public readonly bool HasSelection => cursorIndex != selectionIndex;

    public readonly int SelectionStart => Math.Min(cursorIndex, selectionIndex);

    public readonly int SelectionEnd => Math.Max(cursorIndex, selectionIndex);

    // The cursor is the active end of a selection.
    public readonly bool IsSelectionAtStart => cursorIndex == SelectionStart;

    public readonly bool IsSelectionAtEnd => cursorIndex == SelectionEnd;

    public readonly bool IsSelectedAll => SelectionStart == 0 && SelectionEnd == text.Length;

    public TextEditingValue SelectAll() {
      var value = Normalize();
      value.selectionIndex = 0;
      value.cursorIndex = value.text.Length;
      return value;
    }

    public TextEditingValue ClearSelection() {
      var value = Normalize();
      value.selectionIndex = value.cursorIndex;
      return value;
    }

    public TextEditingValue CollapseSelectionToStart() {
      var value = Normalize();
      value.cursorIndex = value.SelectionStart;
      value.selectionIndex = value.cursorIndex;
      return value;
    }

    public TextEditingValue CollapseSelectionToEnd() {
      var value = Normalize();
      value.cursorIndex = value.SelectionEnd;
      value.selectionIndex = value.cursorIndex;
      return value;
    }

    public TextEditingValue SetSelection(int start, int end) {
      var value = Normalize();
      value.selectionIndex = Math.Clamp(start, 0, value.text.Length);
      value.cursorIndex = Math.Clamp(end, 0, value.text.Length);
      return value;
    }

    // Keeps the anchor at selectionIndex and moves the active cursor.
    public TextEditingValue SelectTo(int index) {
      var value = Normalize();
      value.cursorIndex = Math.Clamp(index, 0, value.text.Length);
      return value;
    }

    public TextEditingValue SelectToStart() => SelectTo(0);

    public TextEditingValue SelectToEnd() {
      var value = Normalize();
      return value.SelectTo(value.text.Length);
    }

    public TextEditingValue DeleteSelection() {
      var value = Normalize();
      if (!value.HasSelection) return value;
      var start = value.SelectionStart;
      value.text = value.text.Remove(start, value.SelectionEnd - start);
      value.cursorIndex = start;
      value.selectionIndex = start;
      return value;
    }

    public TextEditingValue Insert(string insertedText) {
      var value = DeleteSelection();
      insertedText ??= string.Empty;

      value.text = value.text.Insert(value.cursorIndex, insertedText);
      value.cursorIndex += insertedText.Length;
      value.selectionIndex = value.cursorIndex;
      return value;
    }

    public readonly void ApplySelection(ITextSelection selection) {
      selection.cursorIndex = cursorIndex;
      selection.selectIndex = selectionIndex;
    }

    public readonly void Apply(TextElement element) {
      element.text = text;
      ApplySelection(element.selection);
    }

    public static TextEditingValue FromElement(TextElement element) {
      return new TextEditingValue {
        text = element.text,
        cursorIndex = element.selection.cursorIndex,
        selectionIndex = element.selection.selectIndex
      };
    }

    public readonly bool SelectionEquals(ITextSelection selection) {
      return cursorIndex == selection.cursorIndex && selectionIndex == selection.selectIndex;
    }

    public readonly bool Equals(TextEditingValue other) {
      return text == other.text && cursorIndex == other.cursorIndex && selectionIndex == other.selectionIndex;
    }

    public static TextEditTriggerType GetModificationType(in TextEditingValue before, in TextEditingValue after) {
      if (before.text != after.text) return TextEditTriggerType.TextModification;
      if (before.cursorIndex != after.cursorIndex || before.selectionIndex != after.selectionIndex)
        return TextEditTriggerType.SelectionModification;
      return TextEditTriggerType.GenericEvent;
    }

    public readonly override bool Equals(object obj) => obj is TextEditingValue other && Equals(other);
    public readonly override int GetHashCode() => HashCode.Combine(text, cursorIndex, selectionIndex);
    public readonly override string ToString() => $"{text} [{cursorIndex}-{selectionIndex}]";

    public readonly string ToFormattedString() {
      if (text == null) return "null";
      var builder = text;
      if (HasSelection) {
        builder = builder.Insert(SelectionEnd, "]");
        builder = builder.Insert(SelectionStart, "[");
      } else {
        builder = builder.Insert(cursorIndex, "^");
      }
      builder = builder.Replace(" ", "_").Replace("\n", "\\n").Replace("\r", "\\r");
      return builder;
    }

    private readonly TextEditingValue Normalize() {
      var value = this;
      value.text ??= string.Empty;
      value.cursorIndex = Math.Clamp(value.cursorIndex, 0, value.text.Length);
      value.selectionIndex = Math.Clamp(value.selectionIndex, 0, value.text.Length);
      return value;
    }
  }

  public delegate void TextEditProcessor(
    ref TextEditProcessorContext context
  );

  public ref struct TextEditProcessorContext {
    public readonly CompositionContext ctx;
    public readonly TextEditTrigger trigger;
    public readonly TextEditingValue previous;
    public readonly TextEditingValue physical;
    public readonly TextEditingValue initial;
    public TextEditingValue next;
    public TextEditResult result;

    public bool IsSubmitting => trigger.type == TextEditTriggerType.NavigationSubmitEvent ||
                                result.endReason == TextEditEndReason.Submitted;

    public bool IsCancelling => trigger.type == TextEditTriggerType.NavigationCancelEvent ||
                                result.endReason == TextEditEndReason.Cancelled;

    public bool HasTextChanged => previous.text != next.text;
    public int LengthDelta => next.text.Length - previous.text.Length;
    public int AbsoluteLengthDelta => Mathf.Abs(LengthDelta);

    public TextEditProcessorContext(
      CompositionContext ctx, TextEditTrigger trigger, TextEditingValue previous, TextEditingValue physical,
      TextEditingValue initial, TextEditingValue next, TextEditResult result
    ) {
      this.ctx = ctx;
      this.trigger = trigger;
      this.previous = previous;
      this.physical = physical;
      this.initial = initial;
      this.next = next;
      this.result = result;
    }
  }

  public enum TextEditEndReason { None, Submitted, Cancelled, FocusLost }

  public enum TextEditTriggerType {
    SelectionModification,
    TextModification,
    GenericEvent,
    RawKeyDownEvent,
    ChangeEvent,
    NavigationSubmitEvent,
    NavigationCancelEvent
  }

  public readonly struct TextEditTrigger {
    public readonly EventBase evt;
    public readonly TextEditTriggerType type;
    public readonly EventModifiers modifiers;
    public readonly KeyCode keyCode;
    public readonly char character;

    public TextEditTrigger(TextEditTriggerType type) : this() {
      this.type = type;
      evt = null;
    }

    public TextEditTrigger(
      TextEditTriggerType type, EventModifiers modifiers, KeyCode keyCode, char character
    ) : this() {
      this.type = type;
      this.modifiers = modifiers;
      this.keyCode = keyCode;
      this.character = character;
    }

    public TextEditTrigger(EventBase evt) : this() {
      this.evt = evt;
      type = evt switch {
        IKeyboardEvent => TextEditTriggerType.RawKeyDownEvent,
        IChangeEvent => TextEditTriggerType.ChangeEvent,
        NavigationSubmitEvent => TextEditTriggerType.NavigationSubmitEvent,
        NavigationCancelEvent => TextEditTriggerType.NavigationCancelEvent,
        _ => TextEditTriggerType.GenericEvent
      };

      if (evt is not IKeyboardEvent key) return;
      modifiers = key.modifiers;
      keyCode = key.keyCode;
      character = key.character;
    }

    public override string ToString() {
      if (evt is IKeyboardEvent key) return $"Trigger: {type} (Key: {key.keyCode}, Modifiers: {key.modifiers}, {evt})";
      return $"Trigger: {type} ({evt})";
    }
  }

  public readonly struct TextEditResult {
    public readonly bool isInterrupted;
    public readonly bool isBreak;
    public readonly TextEditEndReason endReason;

    public bool IsContinuedEditing => endReason == TextEditEndReason.None && !isBreak;

    private TextEditResult(bool isInterrupted, bool isBreak, TextEditEndReason endReason) {
      this.isInterrupted = isInterrupted;
      this.isBreak = isBreak;
      this.endReason = endReason;
    }

    public static TextEditResult Continue(bool interrupt = false) => new(interrupt, false, TextEditEndReason.None);

    public static TextEditResult Break(bool interrupt = true) => new(interrupt, true, TextEditEndReason.None);

    public static TextEditResult EndEdit(
      TextEditEndReason reason = TextEditEndReason.Cancelled,
      bool interrupt = true,
      bool breaking = true
    ) {
      return new TextEditResult(interrupt, breaking, reason);
    }
  }
}
