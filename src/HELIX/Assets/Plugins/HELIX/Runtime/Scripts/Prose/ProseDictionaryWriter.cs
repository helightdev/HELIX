using System;
using System.Collections.Generic;

namespace HELIX.Prose {
  /// <summary>
  /// Captures the semantic data represented by Prose. Presentation frames, formatters, and modifiers are not
  /// serialized: properties become dictionary entries, values remain their original values, and tree scopes
  /// become dictionaries in a children list.
  /// </summary>
  public sealed class ProseDictionaryWriter : ProseWriter {
    public const string NameKey = "$name";
    public const string ChildrenKey = "$children";
    public const string ContentKey = "$content";

    private readonly Dictionary<string, object> _root = new();
    private Frame[] _frames;
    private int _frameCount;

    private struct Frame {
      public IProseScope Scope;
      public Dictionary<string, object> Tree;
      public object Payload;
      public string PropertyKey;
      public object PropertyValue;
      public bool HasPayload;
      public bool HasPropertyValue;
    }

    public ProseDictionaryWriter(int initialFrameCapacity = 16) {
      if (initialFrameCapacity < 1) throw new ArgumentOutOfRangeException(nameof(initialFrameCapacity));
      _frames = new Frame[initialFrameCapacity];
    }

    public Dictionary<string, object> Root => _root;
    public override bool BeginFrame(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      EnsureFrameCapacity();
      _frames[_frameCount++] = new Frame {
        Scope = scope,
        Tree = scope is ProseTree ? new Dictionary<string, object>() : null
      };
      return true;
    }

    public override void PopFrame() {
      if (_frameCount == 0) throw new InvalidOperationException("There is no Prose frame to pop.");

      var index = _frameCount - 1;
      var frame = _frames[index];
      _frames[index] = default;
      _frameCount--;

      if (frame.Scope is ProsePropertyKey) {
        if (_frameCount > 0 && _frames[_frameCount - 1].Scope is ProseProperty)
          _frames[_frameCount - 1].PropertyKey = PayloadText(frame);
        return;
      }

      if (frame.Scope is ProsePropertyValue) {
        if (_frameCount > 0 && _frames[_frameCount - 1].Scope is ProseProperty) {
          _frames[_frameCount - 1].PropertyValue = frame.Payload;
          _frames[_frameCount - 1].HasPropertyValue = frame.HasPayload;
        }
        return;
      }

      if (frame.Scope is ProseProperty) {
        if (!string.IsNullOrEmpty(frame.PropertyKey))
          AddValue(CurrentDictionary, frame.PropertyKey, frame.HasPropertyValue ? frame.PropertyValue : null);
        return;
      }

      if (frame.Scope is ProseName) {
        var name = PayloadText(frame);
        if (!string.IsNullOrEmpty(name)) CurrentDictionary[NameKey] = name;
        return;
      }

      if (frame.Scope is ProseTree) {
        AddChild(CurrentDictionary, frame.Tree);
        return;
      }

      if (frame.HasPayload) AddValue(CurrentDictionary, ContentKey, frame.Payload);
    }

    public override void PushModifier(IProseModifier modifier) {
      if (modifier == null) throw new ArgumentNullException(nameof(modifier));
      if (_frameCount == 0) throw new InvalidOperationException("A modifier requires an active Prose frame.");
      // Modifiers describe presentation. This data-only sink intentionally ignores them.
    }

    public override void Write(string text) {
      if (text == null) return;
      StoreValue(text);
    }

    public override void Write(IProse prose) {
      if (prose == null) StoreValue<object>(null);
      else if (prose is ProseLineBreak || prose is ProseSpace) return;
      else prose.ToProse(this);
    }

    public override void Write<T>(T value, IProseFormatter<T> formatter) {
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));

      if (formatter is IProsePropertyFormatter<T> propertyFormatter)
        AddValue(CurrentDictionary, propertyFormatter.Key, value);
      else
        StoreValue(value);
    }

    public void Reset() {
      _root.Clear();
      Array.Clear(_frames, 0, _frameCount);
      _frameCount = 0;
    }

    private Dictionary<string, object> CurrentDictionary {
      get {
        for (var i = _frameCount - 1; i >= 0; i--)
          if (_frames[i].Tree != null) return _frames[i].Tree;
        return _root;
      }
    }

    private void StoreValue<T>(T value) {
      if (_frameCount == 0) {
        AddValue(_root, ContentKey, value);
        return;
      }

      var index = _frameCount - 1;
      var frame = _frames[index];
      if (!frame.HasPayload) {
        frame.Payload = value;
        frame.HasPayload = true;
      } else if (frame.Payload is string previous && value is string next) {
        frame.Payload = previous + next;
      } else if (frame.Payload is List<object> values) {
        values.Add(value);
      } else {
        frame.Payload = new List<object> { frame.Payload, value };
      }

      _frames[index] = frame;
    }

    private static string PayloadText(Frame frame) {
      return frame.HasPayload ? frame.Payload as string ?? frame.Payload?.ToString() : null;
    }

    private static void AddChild(Dictionary<string, object> dictionary, Dictionary<string, object> child) {
      if (!dictionary.TryGetValue(ChildrenKey, out var existing)) {
        dictionary[ChildrenKey] = new List<object> { child };
        return;
      }

      ((List<object>)existing).Add(child);
    }

    private static void AddValue(Dictionary<string, object> dictionary, string key, object value) {
      if (!dictionary.TryGetValue(key, out var existing)) {
        dictionary[key] = value;
        return;
      }

      if (existing is List<object> values) values.Add(value);
      else dictionary[key] = new List<object> { existing, value };
    }

    private void EnsureFrameCapacity() {
      if (_frameCount < _frames.Length) return;
      Array.Resize(ref _frames, _frames.Length * 2);
    }

  }
}
