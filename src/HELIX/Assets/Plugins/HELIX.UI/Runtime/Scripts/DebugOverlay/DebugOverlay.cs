using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using HELIX.Compose;
using HELIX.Prose;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.UI.DebugOverlay {
  public readonly struct ScreenOverlayBuilder {
    private readonly IScreenOverlay _overlay;
    internal ScreenOverlayBuilder(IScreenOverlay overlay) => _overlay = overlay;
    public ScreenOverlayBuilder Field<T>(string label, T value, string unit = null, Color? color = null) {
      _overlay?.Field(label, value, Datatypes.Object<T>(), unit, color); return this;
    }
    public ScreenOverlayBuilder Field<T>(
      string label, T value, IDatatype<T> datatype, string unit = null, Color? color = null
    ) {
      _overlay?.Field(label, value, datatype, unit, color); return this;
    }
    public ScreenOverlayBuilder Field(string label, float value, int decimals = 2, string unit = null, Color? color = null) {
      _overlay?.Field(label, value, OverlayDatatypes.Float(decimals), unit, color); return this;
    }
    public ScreenOverlayBuilder Field(string label, Vector3 value, int decimals = 1, string unit = null, Color? color = null) {
      _overlay?.Field(label, value, OverlayDatatypes.Vector3(decimals), unit, color); return this;
    }
  }

  public readonly struct ScreenOverlayInitiator {
    private readonly IScreenOverlay _overlay;
    public ScreenOverlayInitiator(IScreenOverlay overlay) => _overlay = overlay;
    public ScreenOverlayBuilder Section(string title, int order = 0, string subtitle = null) {
      _overlay?.BeginSection(title, order, subtitle); return new ScreenOverlayBuilder(_overlay);
    }
    public ScreenOverlayBuilder Global() { _overlay?.BeginSection("", int.MinValue); return new ScreenOverlayBuilder(_overlay); }
  }

  public readonly struct WorldOverlayBuilder {
    private readonly ulong _id;
    private readonly IWorldOverlay _overlay;
    internal WorldOverlayBuilder(ulong id, IWorldOverlay overlay) { _id = id; _overlay = overlay; }
    public WorldOverlayBuilder Field<T>(string label, T value, string unit = null, Color? color = null) {
      _overlay?.Field(_id, label, value, Datatypes.Object<T>(), unit, color); return this;
    }
    public WorldOverlayBuilder Field<T>(
      string label, T value, IDatatype<T> datatype, string unit = null, Color? color = null
    ) {
      _overlay?.Field(_id, label, value, datatype, unit, color); return this;
    }
    public WorldOverlayBuilder Field(string label, float value, int decimals = 2, string unit = null, Color? color = null) =>
      Field(label, value, OverlayDatatypes.Float(decimals), unit, color);
    public WorldOverlayBuilder Field(string label, Vector3 value, int decimals = 1, string unit = null, Color? color = null) {
      _overlay?.Field(_id, label, value, OverlayDatatypes.Vector3(decimals), unit, color); return this;
    }

  }

  public readonly struct WorldOverlayInitiator {
    private readonly IWorldOverlay _overlay;
    public WorldOverlayInitiator(IWorldOverlay overlay) => _overlay = overlay;
    public WorldOverlayBuilder Box(ulong id, string title, Vector3 position, string subtitle = null) {
      _overlay?.Begin(id, title, position, subtitle); return new WorldOverlayBuilder(id, _overlay);
    }
    public WorldOverlayBuilder Box(string key, string title, Vector3 position, string subtitle = null) =>
      Box(StableId(key), title, position, subtitle);
    public WorldOverlayBuilder Continue(ulong id) => _overlay != null && _overlay.Contains(id) ? new WorldOverlayBuilder(id, _overlay) : default;
    internal static ulong StableId(string value) {
      unchecked { var hash = 14695981039346656037UL; for (var i = 0; i < value.Length; i++) { hash ^= value[i]; hash *= 1099511628211UL; } return hash; }
    }
  }

  public interface IScreenOverlay {
    void BeginSection(string title, int order = 0, string subtitle = null);
    void Field<T>(string label, T value, IDatatype<T> datatype, string unit = null, Color? color = null);
  }

  public interface IWorldOverlay {
    void Begin(ulong id, string title, Vector3 position, string subtitle = null);
    bool Contains(ulong id);
    void Field<T>(ulong id, string label, T value, IDatatype<T> datatype, string unit = null, Color? color = null);
    void SetCamera(Camera camera);
  }

  internal static class OverlayDatatypes {
    private static readonly FloatDatatype[] Floats = {
      new("F0"), new("F1"), new("F2"), new("F3"), new("F4"),
      new("F5"), new("F6"), new("F7"), new("F8"), new("F9")
    };
    private static readonly OverlayVector3Datatype[] Vectors = {
      new("F0"), new("F1"), new("F2"), new("F3"), new("F4"),
      new("F5"), new("F6"), new("F7"), new("F8"), new("F9")
    };

    public static FloatDatatype Float(int decimals) => Floats[ValidateDecimals(decimals)];
    public static OverlayVector3Datatype Vector3(int decimals) => Vectors[ValidateDecimals(decimals)];

    private static int ValidateDecimals(int decimals) {
      if ((uint)decimals >= Floats.Length) throw new ArgumentOutOfRangeException(nameof(decimals));
      return decimals;
    }
  }

  internal sealed class OverlayVector3Datatype : IDatatype<Vector3> {
    public OverlayVector3Datatype(string format) => Format = format;
    public string Format { get; }
    public void ToProse(IProseWriter writer, Vector3 value) {
      writer.Write(value.x, Datatypes.Float);
      writer.Write(", ");
      writer.Write(value.y, Datatypes.Float);
      writer.Write(", ");
      writer.Write(value.z, Datatypes.Float);
    }
  }

  public struct CollectScreenOverlayEvent : Context.Evt<CollectScreenOverlayEvent> {
    public readonly ScreenOverlayInitiator overlay;
    public CollectScreenOverlayEvent(IScreenOverlay overlay) => this.overlay = new ScreenOverlayInitiator(overlay);
  }
  public struct CollectWorldOverlayEvent : Context.Evt<CollectWorldOverlayEvent> {
    public readonly WorldOverlayInitiator overlay;
    public CollectWorldOverlayEvent(IWorldOverlay overlay) => this.overlay = new WorldOverlayInitiator(overlay);
  }

  internal abstract class OverlayFieldModel {
    public string label, unit;
    public Color? color;
    public float seen;
    public abstract void SetText(TextElement element);
  }

  internal sealed class OverlayFieldModel<T> : OverlayFieldModel {
    private TextElementProseWriter _fallbackWriter;
    public T value;
    public IDatatype<T> datatype;

    public override void SetText(TextElement element) {
      if (typeof(T) == typeof(int)) {
        element.SetText(Unsafe.As<T, int>(ref value));
      } else if (typeof(T) == typeof(float) && datatype is FloatDatatype floatDatatype) {
        element.SetText(Unsafe.As<T, float>(ref value), floatDatatype.Format);
      } else if (typeof(T) == typeof(string)) {
        var text = Unsafe.As<T, string>(ref value);
        element.SetText(text == null ? "null".AsSpan() : text.AsSpan());
      } else if (typeof(T) == typeof(Vector3) && datatype is OverlayVector3Datatype vectorDatatype) {
        var vector = Unsafe.As<T, Vector3>(ref value);
        Span<char> buffer = stackalloc char[96];
        var cursor = 0;
        if (!vector.x.TryFormat(buffer[cursor..], out var written, vectorDatatype.Format, CultureInfo.InvariantCulture)) return;
        cursor += written; buffer[cursor++] = ','; buffer[cursor++] = ' ';
        if (!vector.y.TryFormat(buffer[cursor..], out written, vectorDatatype.Format, CultureInfo.InvariantCulture)) return;
        cursor += written; buffer[cursor++] = ','; buffer[cursor++] = ' ';
        if (!vector.z.TryFormat(buffer[cursor..], out written, vectorDatatype.Format, CultureInfo.InvariantCulture)) return;
        element.SetText(buffer[..(cursor + written)]);
      } else {
        (_fallbackWriter ??= new TextElementProseWriter()).Element = element;
        datatype.ToProse(_fallbackWriter, value);
      }
    }
  }
  internal sealed class OverlayEntryModel {
    public string title, subtitle;
    public Vector3 position;
    public float seen;
    public readonly Dictionary<string, OverlayFieldModel> fields = new();
  }

  public sealed class DebugOverlayController : IScreenOverlay, IWorldOverlay {
    internal readonly DynamicComposableController<DynamicFlexLayout> screen = new();
    internal readonly DynamicComposableController<DynamicStackLayout> world = new();
    private readonly DebugOverlayProseWriter _screenWriter = new();
    private readonly DebugOverlayProseWriter _worldWriter = new();
    internal Camera camera;
    private OverlayEntryModel _section;
    public void BeginFrame() => _section = null;
    public void BeginSection(string title, int order = 0, string subtitle = null) {
      var entry = screen.FindEntry(title);
      if (entry == null) {
        _section = new OverlayEntryModel { title = title };
        entry = screen.AddEntry(title, DebugOverlayElement.ComposeCard, default, _section, order);
      } else {
        _section = (OverlayEntryModel)entry.UserData;
        if (entry.order != order) { entry.order = order; screen.NotifyEntriesChanged(); }
      }
      _section.subtitle = subtitle; _section.seen = Time.unscaledTime;
      _screenWriter.Target = _section;
    }
    public void Field<T>(string label, T value, IDatatype<T> datatype, string unit = null, Color? color = null) {
      if (_section == null) BeginSection("");
      _screenWriter.Property(label, value, datatype, unit, color);
    }
    public void Begin(ulong id, string title, Vector3 position, string subtitle = null) {
      if (id == 0) return;
      var dynamicEntry = world.FindEntry(id);
      OverlayEntryModel entry;
      if (dynamicEntry == null) {
        entry = new OverlayEntryModel();
        world.AddEntry(id, DebugOverlayElement.ComposeCard, default, entry);
      } else entry = (OverlayEntryModel)dynamicEntry.UserData;
      entry.title = title; entry.subtitle = subtitle; entry.position = position; entry.seen = Time.unscaledTime;
      _worldWriter.Target = entry;
    }
    public bool Contains(ulong id) => world.FindEntry(id) != null;
    public void Field<T>(ulong id, string label, T value, IDatatype<T> datatype, string unit = null, Color? color = null) {
      var entry = world.FindEntry(id);
      if (entry == null) return;
      _worldWriter.Target = (OverlayEntryModel)entry.UserData;
      _worldWriter.Property(label, value, datatype, unit, color);
    }
    public void SetCamera(Camera value) => camera = value;
  }

  /// <summary>Allocation-stable semantic sink used to write overlay properties directly into an entry model.</summary>
  internal sealed class DebugOverlayProseWriter : ProseWriter {
    private readonly IProseScope[] _frames = new IProseScope[8];
    private int _frameCount;
    private string _key, _unit;
    private Color? _color;
    private OverlayFieldModel _field;

    internal OverlayEntryModel Target { get; set; }

    internal void Property<T>(string key, T value, IDatatype<T> datatype, string unit, Color? color) {
      _color = color;
      Begin(ProseScopes.Property);
      Begin(ProseScopes.PropertyKey);
      Write(key);
      End();
      Begin(ProseScopes.PropertyValue);
      Write(value, datatype);
      End();
      if (unit != null) {
        Begin(ProseScopes.PropertyDescription);
        Write(unit);
        End();
      }
      End();
    }

    public override bool TryBegin<T>(T scope) {
      if (scope is null) throw new ArgumentNullException(nameof(scope));
      if (_frameCount == _frames.Length)
        throw new InvalidOperationException("Debug overlay prose nesting exceeds its fixed capacity.");
      if (scope is not ProseProperty and not ProsePropertyKey and not ProsePropertyValue and
        not ProsePropertyDescription and not ProseSpan) return false;
      if (scope is ProseProperty) {
        _key = null;
        _unit = null;
        _field = null;
      }
      _frames[_frameCount++] = scope;
      return true;
    }

    public override void Begin<T>(T scope) {
      if (!TryBegin(scope)) throw new NotSupportedException(
        $"The debug overlay prose writer does not support {(scope is null ? "null" : scope.GetType().Name)}."
      );
    }

    public override void End() {
      if (_frameCount == 0) throw new InvalidOperationException("There is no debug overlay prose frame to end.");
      var scope = _frames[--_frameCount];
      _frames[_frameCount] = null;
      if (scope is not ProseProperty || _field == null) return;
      _field.unit = _unit;
      _field.color = _color;
      _field.seen = Time.unscaledTime;
    }

    public override void Push<T>(T modifier) {
      if (modifier is null) throw new ArgumentNullException(nameof(modifier));
    }

    public override void Write<T>(T prose) {
      if (prose is null) Write("null");
      else prose.ToProse(this);
    }

    public override void Write<T>(T value, IDatatype<T> datatype) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      if (_frameCount == 0 || _frames[_frameCount - 1] is not ProsePropertyValue || Target == null || _key == null)
        return;
      if (!Target.fields.TryGetValue(_key, out var current) || current is not OverlayFieldModel<T> field) {
        field = new OverlayFieldModel<T> { label = _key };
        Target.fields[_key] = field;
      }
      field.value = value;
      field.datatype = datatype;
      _field = field;
    }

    public override void Write(string text) {
      if (_frameCount == 0 || text == null) return;
      var scope = _frames[_frameCount - 1];
      if (scope is ProsePropertyKey) _key = text;
      else if (scope is ProsePropertyDescription) _unit = text;
    }
  }

  internal sealed class TextElementProseWriter : ProseWriter {
    public TextElement Element { get; set; }
    public override bool TryBegin<T>(T scope) => true;
    public override void Begin<T>(T scope) { }
    public override void End() { }
    public override void Push<T>(T modifier) { }
    public override void Write<T>(T prose) {
      if (prose is not null) prose.ToProse(this);
    }
    public override void Write<T>(T value, IDatatype<T> datatype) => datatype.ToProse(this, value);
    public override void Write(string text) => Element.SetText((text ?? "null").AsSpan());
  }

  internal sealed class OverlayPropertyElement : VisualElement {
    private readonly TextElement _key, _value, _unit;

    public OverlayPropertyElement() {
      pickingMode = PickingMode.Ignore;
      style.flexDirection = FlexDirection.Row;
      Add(_key = new TextElement());
      Add(_value = new TextElement());
      Add(_unit = new TextElement());
      _key.style.color = new Color(.61f, .65f, .71f);
      _key.style.marginRight = 8;
      _unit.style.marginLeft = 4;
    }

    public void Bind(OverlayFieldModel field, float now) {
      style.display = now - field.seen < .15f ? DisplayStyle.Flex : DisplayStyle.None;
      _key.SetText(field.label.AsSpan());
      field.SetText(_value);
      _value.style.color = field.color ?? Color.white;
      _unit.SetText(field.unit == null ? ReadOnlySpan<char>.Empty : field.unit.AsSpan());
      _unit.style.display = string.IsNullOrEmpty(field.unit) ? DisplayStyle.None : DisplayStyle.Flex;
      _unit.style.color = field.color ?? Color.white;
    }
  }

  internal sealed class OverlayCardElement : VisualElement {
    private readonly Label _title, _subtitle;
    private readonly VisualElement _fields;
    private readonly Dictionary<string, OverlayPropertyElement> _properties = new();
    public OverlayCardElement() {
      pickingMode = PickingMode.Ignore;
      style.backgroundColor = new Color(.035f, .045f, .06f, .88f); style.borderTopLeftRadius = 5;
      style.borderTopRightRadius = 5; style.borderBottomLeftRadius = 5; style.borderBottomRightRadius = 5;
      style.paddingLeft = 8; style.paddingRight = 8; style.paddingTop = 5; style.paddingBottom = 5;
      style.marginBottom = 4; style.minWidth = 170;
      Add(_title = new Label()); _title.style.unityFontStyleAndWeight = FontStyle.Bold; _title.style.color = new Color(.75f, .9f, 1f);
      Add(_subtitle = new Label()); _subtitle.style.fontSize = 10; _subtitle.style.color = new Color(.65f, .7f, .78f);
      Add(_fields = new VisualElement());
    }
    public void Bind(OverlayEntryModel entry, float now) {
      _title.text = entry.title; _title.style.display = string.IsNullOrEmpty(entry.title) ? DisplayStyle.None : DisplayStyle.Flex;
      _subtitle.text = entry.subtitle; _subtitle.style.display = string.IsNullOrEmpty(entry.subtitle) ? DisplayStyle.None : DisplayStyle.Flex;
      foreach (var pair in entry.fields) {
        var field = pair.Value;
        if (!_properties.TryGetValue(pair.Key, out var property)) {
          _properties[pair.Key] = property = new OverlayPropertyElement();
          _fields.Add(property);
        }
        property.Bind(field, now);
      }
    }
  }

  public sealed class DebugOverlayElement : VisualElement {
    private static readonly ushort CardId = CompositionId.GetTypeId(nameof(OverlayCardElement));
    internal static readonly Composable ComposeCard = static (ref Composition cx) => {
      if (!cx.AUTHORING.RequireTracked<OverlayCardElement>(CardId, out var card, out _))
        card = new OverlayCardElement();
      var entry = DynamicComposable.Lookup(cx.boundary.Element);
      card.Bind((OverlayEntryModel)entry.UserData, Time.unscaledTime);
      cx.AUTHORING.YieldElement(ref cx, card);
    };

    private readonly DebugOverlayController _controller;
    private readonly VisualElement _screen, _world;
    private readonly List<string> _deadFields = new();
    private readonly Comparison<VisualElement> _worldDepth;
    private Vector3 _cameraPosition;
    public DebugOverlayElement(DebugOverlayController controller) {
      _controller = controller; pickingMode = PickingMode.Ignore;
      _worldDepth = (left, right) => {
        var a = Model(left); var b = Model(right); if (a == null || b == null) return 0;
        return (b.position - _cameraPosition).sqrMagnitude.CompareTo((a.position - _cameraPosition).sqrMagnitude);
      };
      style.position = Position.Absolute; style.left = 0; style.right = 0; style.top = 0; style.bottom = 0;
      Add(_world = new VisualElement { pickingMode = PickingMode.Ignore });
      Add(_screen = new VisualElement { pickingMode = PickingMode.Ignore });
      _world.style.position = Position.Absolute; _world.style.left = 0; _world.style.right = 0;
      _world.style.top = 0; _world.style.bottom = 0;
      _screen.style.position = Position.Absolute; _screen.style.left = 10; _screen.style.top = 10;
      schedule.Execute(UpdateOverlay).Every(16);
    }
    private void UpdateOverlay() {
      var now = Time.unscaledTime;
      UpdateScreen(now); UpdateWorld(now);
    }
    private void UpdateScreen(float now) {
      for (var i = _controller.screen.Count - 1; i >= 0; i--) {
        var entry = _controller.screen.Entries[i];
        var model = (OverlayEntryModel)entry.UserData;
        if (now - model.seen > 2) { _controller.screen.RemoveEntry(entry); continue; }
        CleanupFields(model, now);
      }
      DynamicCollectionHelper.Synchronize(_screen, _controller.screen);
      UpdateElements(_screen, now, false, null);
    }
    private void UpdateWorld(float now) {
      var camera = _controller.camera ? _controller.camera : Camera.main; if (!camera) return;
      _cameraPosition = camera.transform.position;
      for (var i = _controller.world.Count - 1; i >= 0; i--) {
        var entry = _controller.world.Entries[i];
        var model = (OverlayEntryModel)entry.UserData;
        if (now - model.seen > 2) { _controller.world.RemoveEntry(entry); continue; }
        CleanupFields(model, now);
      }
      DynamicCollectionHelper.Synchronize(_world, _controller.world);
      UpdateElements(_world, now, true, camera);
      _world.hierarchy.Sort(_worldDepth);
    }

    private void UpdateElements(VisualElement parent, float now, bool world, Camera camera) {
      for (var i = 0; i < parent.childCount; i++) {
        if (parent.ElementAt(i) is not DynamicComposableElement element) continue;
        var model = (OverlayEntryModel)element.Entry.UserData;
        var visible = now - model.seen < .15f;
        if (world) {
          var point = camera.WorldToScreenPoint(model.position);
          visible &= point.z > 0;
          if (visible) {
            element.style.position = Position.Absolute; element.style.left = point.x;
            element.style.top = resolvedStyle.height - point.y;
            element.style.translate = new Translate(Length.Percent(-50), 0);
            var distance = Vector3.Distance(_cameraPosition, model.position);
            var scale = Mathf.Clamp(10f / Mathf.Max(.001f, distance), .5f, 1.5f);
            element.style.scale = new Scale(new Vector3(scale, scale, 1f));
          }
        }
        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        if (element.childCount > 0 && element.ElementAt(0) is OverlayCardElement card) card.Bind(model, now);
      }
    }

    private void CleanupFields(OverlayEntryModel entry, float now) {
      _deadFields.Clear();
      foreach (var pair in entry.fields) if (now - pair.Value.seen > 2f) _deadFields.Add(pair.Key);
      for (var i = 0; i < _deadFields.Count; i++) entry.fields.Remove(_deadFields[i]);
    }

    private static OverlayEntryModel Model(VisualElement element) =>
      (element as DynamicComposableElement)?.Entry?.UserData as OverlayEntryModel;
  }
}
