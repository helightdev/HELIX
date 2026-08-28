using System;
using System.Collections.Generic;

namespace HELIX.Prose {
  /// <summary>
  /// Captures the semantic data represented by Prose. Presentation modifiers and formatters are not serialized:
  /// values retain their original types, while trees, sections, lists, and tables retain their structure.
  /// </summary>
  public sealed class ProseDictionaryWriter : ProseWriter {
    public const string NameKey = "$name";
    public const string ChildrenKey = "$children";
    public const string ContentKey = "$content";
    public const string SectionsKey = "$sections";
    public const string HeaderKey = "$header";
    public const string ListsKey = "$lists";
    public const string ListKindKey = "$kind";
    public const string StartKey = "$start";
    public const string ItemsKey = "$items";
    public const string TablesKey = "$tables";
    public const string RowsKey = "$rows";
    public const string CellsKey = "$cells";
    public const string IsHeaderKey = "$isHeader";
    public const string CodeBlocksKey = "$codeBlocks";
    public const string LanguageKey = "$language";

    private readonly Dictionary<string, object> _root = new();
    private Frame[] _frames;
    private int _frameCount;

    private struct Frame {
      public IProseScope Scope;
      public Dictionary<string, object> Container;
      public List<object> Items;
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
    public override bool TryBegin<T>(T scope) {
      if (ProseValues.IsNull(scope)) throw new ArgumentNullException(nameof(scope));
      EnsureFrameCapacity();
      var frame = new Frame { Scope = scope };
      if (scope is ProseTree or ProseSection or ProseListItem or ProseTable) {
        frame.Container = new Dictionary<string, object>();
      }
      if (scope is ProseList list) {
        frame.Container = new Dictionary<string, object> {
          [ListKindKey] = list.Kind == ProseListKind.Ordered ? "ordered" : "unordered"
        };
        if (list.Kind == ProseListKind.Ordered && list.Start != 1)
          frame.Container[StartKey] = list.Start;
        frame.Items = new List<object>();
      } else if (scope is ProseTable or ProseTableRow) {
        frame.Items = new List<object>();
      }
      _frames[_frameCount++] = frame;
      return true;
    }

    public override void Begin<T>(T scope) => TryBegin(scope);

    public override void End() {
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

      if (frame.Scope is ProsePropertyValue or ProsePropertyDescription) {
        if (_frameCount > 0 && _frames[_frameCount - 1].Scope is ProseProperty) {
          ref var property = ref _frames[_frameCount - 1];
          if (!property.HasPropertyValue) {
            property.PropertyValue = frame.Payload;
            property.HasPropertyValue = frame.HasPayload;
          }
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
        AddChild(CurrentDictionary, frame.Container);
        return;
      }

      if (frame.Scope is ProseSectionHeader) {
        var header = PayloadText(frame);
        if (!string.IsNullOrEmpty(header)) CurrentDictionary[HeaderKey] = header;
        return;
      }

      if (frame.Scope is ProseParagraph) {
        if (frame.HasPayload) AddValue(CurrentDictionary, ContentKey, frame.Payload);
        return;
      }

      if (frame.Scope is ProseCodeBlock codeBlock) {
        var data = new Dictionary<string, object> {
          [ContentKey] = frame.HasPayload ? frame.Payload : string.Empty
        };
        if (!string.IsNullOrEmpty(codeBlock.Language)) data[LanguageKey] = codeBlock.Language;
        AddCollectionItem(CurrentDictionary, CodeBlocksKey, data);
        return;
      }

      if (frame.Scope is ProseSection) {
        if (frame.HasPayload) AddValue(frame.Container, ContentKey, frame.Payload);
        AddCollectionItem(CurrentDictionary, SectionsKey, frame.Container);
        return;
      }

      if (frame.Scope is ProseListItem) {
        if (frame.HasPayload) AddValue(frame.Container, ContentKey, frame.Payload);
        var listIndex = FindFrame<ProseList>();
        if (listIndex >= 0) _frames[listIndex].Items.Add(frame.Container);
        else AddValue(CurrentDictionary, ContentKey, frame.Container);
        return;
      }

      if (frame.Scope is ProseList) {
        frame.Container[ItemsKey] = frame.Items;
        AddCollectionItem(CurrentDictionary, ListsKey, frame.Container);
        return;
      }

      if (frame.Scope is ProseTableCell) {
        var rowIndex = FindFrame<ProseTableRow>();
        if (rowIndex >= 0)
          _frames[rowIndex].Items.Add(frame.HasPayload ? frame.Payload : null);
        return;
      }

      if (frame.Scope is ProseTableRow row) {
        var rowData = new Dictionary<string, object> { [CellsKey] = frame.Items };
        if (row.IsHeader) rowData[IsHeaderKey] = true;
        var tableIndex = FindFrame<ProseTable>();
        if (tableIndex >= 0) _frames[tableIndex].Items.Add(rowData);
        return;
      }

      if (frame.Scope is ProseTable) {
        frame.Container[RowsKey] = frame.Items;
        AddCollectionItem(CurrentDictionary, TablesKey, frame.Container);
        return;
      }

      if (frame.HasPayload) StoreCompletedPayload(frame.Payload);
    }

    public override void Push<T>(T modifier) {
      if (ProseValues.IsNull(modifier)) throw new ArgumentNullException(nameof(modifier));
      if (_frameCount == 0) throw new InvalidOperationException("A modifier requires an active Prose frame.");
      if (modifier is ProsePropertyValueModifier value) {
        var propertyIndex = FindFrame<ProseProperty>();
        if (propertyIndex >= 0) {
          _frames[propertyIndex].PropertyValue = value.Value;
          _frames[propertyIndex].HasPropertyValue = true;
        }
      }
      // Other modifiers describe presentation. This data-only sink intentionally ignores them.
    }

    public override void Write(string text) {
      if (text == null) return;
      StoreValue(text);
    }

    public override void Write<T>(T prose) {
      if (ProseValues.IsNull(prose)) StoreValue<object>(null);
      else if (prose is ProseLineBreak or ProseSoftLineBreak or ProseSpace) return;
      else prose.ToProse(this);
    }

    public override void Write<T>(T value, IDatatype<T> datatype) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));

      if (datatype is IPropertyDatatype<T> propertyFormatter)
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
          if (_frames[i].Container != null) return _frames[i].Container;
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

    private static void AddCollectionItem(
      Dictionary<string, object> dictionary, string key, object value
    ) {
      if (!dictionary.TryGetValue(key, out var existing)) {
        dictionary[key] = new List<object> { value };
        return;
      }
      ((List<object>)existing).Add(value);
    }

    private void StoreCompletedPayload(object payload) {
      if (_frameCount == 0) {
        AddValue(_root, ContentKey, payload);
        return;
      }

      var index = _frameCount - 1;
      var frame = _frames[index];
      if (!frame.HasPayload) {
        frame.Payload = payload;
        frame.HasPayload = true;
      } else if (frame.Payload is string previous && payload is string next) {
        frame.Payload = previous + next;
      } else if (frame.Payload is List<object> values) {
        values.Add(payload);
      } else {
        frame.Payload = new List<object> { frame.Payload, payload };
      }
      _frames[index] = frame;
    }

    private int FindFrame<TScope>() where TScope : IProseScope {
      for (var i = _frameCount - 1; i >= 0; i--)
        if (_frames[i].Scope is TScope) return i;
      return -1;
    }

    private void EnsureFrameCapacity() {
      if (_frameCount < _frames.Length) return;
      Array.Resize(ref _frames, _frames.Length * 2);
    }

  }
}
