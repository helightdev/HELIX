using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HELIX.Compose;
using HELIX.Prose;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.UI.CameraOverlays {
  public readonly struct ScreenCameraOverlayPosition {
    public readonly Vector2 Value;
    public readonly int Order;
    public ScreenCameraOverlayPosition(float left, float top, int order = 0) {
      Value = new Vector2(left, top); Order = order;
    }
  }

  public readonly struct CameraOverlayColor : IProseModifier {
    public readonly Color Value;
    public CameraOverlayColor(Color value) => Value = value;
  }

  /// <summary>Embeds arbitrary Compose content in a prose-authored overlay box.</summary>
  public readonly struct CameraOverlayContent : IProse {
    public readonly Composable Value;
    public CameraOverlayContent(Composable value) => Value = value;
    public void ToProse(IProseWriter writer) { }
  }

  public static class CameraOverlayProse {
    public static void Content(this IProseWriter writer, Composable content) =>
      writer.Write(new CameraOverlayContent(content));
  }

  internal static class CameraOverlayUtility {
    public static ulong StableId(string value) {
      if (value == null) throw new ArgumentNullException(nameof(value));
      unchecked {
        var hash = 14695981039346656037UL;
        for (var i = 0; i < value.Length; i++) { hash ^= value[i]; hash *= 1099511628211UL; }
        return hash;
      }
    }
  }

  public delegate void CameraOverlayProducer(IProseWriter writer);
  public delegate bool CameraOverlayCondition();
  public enum CameraOverlayUpdateMode : byte { EveryVisibleFrame, WhenInvalidated }

  /// <summary>Global registration and handle-management event.</summary>
  public struct CollectScreenCameraOverlayEvent : Context.Evt<CollectScreenCameraOverlayEvent> {
    public readonly ScreenCameraOverlayController overlays;
    public CollectScreenCameraOverlayEvent(ScreenCameraOverlayController overlays) => this.overlays = overlays;
  }
  public struct CollectWorldCameraOverlayEvent : Context.Evt<CollectWorldCameraOverlayEvent> {
    public readonly WorldCameraOverlayController overlays;
    public CollectWorldCameraOverlayEvent(WorldCameraOverlayController overlays) => this.overlays = overlays;
  }

  internal abstract class OverlayFieldModel {
    public string label, description;
    public Color? color;
    public abstract string Prefix { get; }
    public abstract string Suffix { get; }
    public abstract string Unit { get; }
    public abstract int SetText(TextElement first, TextElement second, TextElement third);
  }

  internal sealed class OverlayFieldModel<T> : OverlayFieldModel {
    private TextElementProseWriter _fallbackWriter;
    public T value;
    public IDatatype<T> datatype;
    public override string Prefix => (datatype as IDatatypeAffix)?.Prefix;
    public override string Suffix => (datatype as IDatatypeAffix)?.Suffix;
    public override string Unit => (datatype as IDatatypeUnit)?.Unit;

    public override int SetText(TextElement element, TextElement second, TextElement third) {
      if (typeof(T) == typeof(int)) {
        element.SetText(Unsafe.As<T, int>(ref value));
      } else if (typeof(T) == typeof(float) && datatype is FloatDatatype floatDatatype) {
        var number = Unsafe.As<T, float>(ref value);
        if (floatDatatype.Clamp)
          number = Math.Max(floatDatatype.Min ?? float.MinValue, Math.Min(floatDatatype.Max ?? float.MaxValue, number));
        number *= floatDatatype.Scale;
        if (floatDatatype.Compact) {
          (_fallbackWriter ??= new TextElementProseWriter()).Element = element;
          datatype.ToProse(_fallbackWriter, value);
        } else element.SetText(number, floatDatatype.Format);
      } else if (typeof(T) == typeof(string)) {
        var text = Unsafe.As<T, string>(ref value);
        element.SetText(text == null ? "null".AsSpan() : text.AsSpan());
      } else if (typeof(T) == typeof(Vector3) && ReferenceEquals(datatype, Datatypes.Vector3)) {
        var vector = Unsafe.As<T, Vector3>(ref value);
        element.SetText(vector.x, Datatypes.Float.Format);
        second.SetText(vector.y, Datatypes.Float.Format);
        third.SetText(vector.z, Datatypes.Float.Format);
        return 3;
      } else {
        (_fallbackWriter ??= new TextElementProseWriter()).Element = element;
        datatype.ToProse(_fallbackWriter, value);
      }
      return 1;
    }
  }
  internal sealed class OverlayEntryModel {
    public string title, subtitle;
    public Vector3 position;
    public int order;
    public Composable content;
    public DynamicComposable dynamicEntry;
    public int revision;
    public int renderedRevision = -1;
    public CameraOverlayEntry owner;
    public readonly Dictionary<string, OverlayFieldModel> fields = new();
  }

  /// <summary>A caller-owned overlay registration. Dispose it to remove the entry.</summary>
  public abstract class CameraOverlayEntry : IDisposable {
    private readonly Action<OverlayEntryModel> _remove;
    internal readonly OverlayEntryModel model;
    private readonly CameraOverlayProseWriter _writer;
    private CameraOverlayProducer _producer;
    private CameraOverlayCondition _condition;
    private Transform _trackedTransform;
    private Vector3 _trackingOffset;
    private bool _dirty = true;
    private bool _disposed;

    internal CameraOverlayEntry(
      Action<OverlayEntryModel> remove, Action<OverlayEntryModel> contentChanged,
      OverlayEntryModel model, CameraOverlayProducer producer, CameraOverlayCondition condition
    ) {
      _remove = remove; this.model = model;
      _producer = producer ?? throw new ArgumentNullException(nameof(producer));
      _condition = condition;
      _writer = new CameraOverlayProseWriter(contentChanged, model);
      model.owner = this;
    }

    public bool Enabled { get; set; } = true;
    public CameraOverlayUpdateMode UpdateMode { get; set; } = CameraOverlayUpdateMode.EveryVisibleFrame;
    public CameraOverlayCondition Condition { get => _condition; set => _condition = value; }
    public CameraOverlayProducer Producer {
      get => _producer;
      set { _producer = value ?? throw new ArgumentNullException(nameof(value)); Invalidate(); }
    }
    public Vector3 ResolvedPosition => model.position;
    public void Invalidate() => _dirty = true;

    public void Dispose() {
      if (_disposed) return;
      _disposed = true;
      _remove(model);
    }

    protected void TrackTransform(Transform transform, Vector3 offset) {
      ThrowIfDisposed(); _trackedTransform = transform; _trackingOffset = offset;
    }
    protected void ClearTrackedTransform() => _trackedTransform = null;
    internal void ResolveTrackedPosition() {
      if (_trackedTransform) model.position = _trackedTransform.position + _trackingOffset;
    }

    internal void Produce() {
      if (!Enabled || (UpdateMode == CameraOverlayUpdateMode.WhenInvalidated && !_dirty)) return;
      _writer.StartUpdate();
      try { _producer(_writer); }
      finally { _writer.FinishUpdate(); }
      _dirty = false;
    }
    internal bool CheckCondition() => _condition == null || _condition();

    private void ThrowIfDisposed() {
      if (_disposed) throw new ObjectDisposedException(nameof(CameraOverlayEntry));
    }
  }

  public sealed class ScreenCameraOverlayEntry : CameraOverlayEntry {
    private readonly ScreenCameraOverlayController _controller;
    internal ScreenCameraOverlayEntry(
      ScreenCameraOverlayController controller, OverlayEntryModel model, CameraOverlayProducer producer,
      CameraOverlayCondition condition
    ) : base(controller.Remove, controller.ContentChanged, model, producer, condition) => _controller = controller;
    public ScreenCameraOverlayPosition Position {
      set { _controller.Place(model, in value); }
    }
  }

  public sealed class WorldCameraOverlayEntry : CameraOverlayEntry {
    internal WorldCameraOverlayEntry(
      WorldCameraOverlayController controller, OverlayEntryModel model, CameraOverlayProducer producer,
      CameraOverlayCondition condition
    ) : base(controller.Remove, controller.ContentChanged, model, producer, condition) { }
    public Vector3 Position { set => model.position = value; }
    public void Track(Transform transform, Vector3 offset = default) => TrackTransform(transform, offset);
    public void StopTracking() => ClearTrackedTransform();
  }

  public sealed class ScreenCameraOverlayController {
    internal readonly DynamicComposableController<DynamicFlexLayout> entries = new();
    public int UpdateIntervalMilliseconds { get; set; } = 16;
    public ScreenCameraOverlayEntry Subscribe(
      string key, ScreenCameraOverlayPosition position, CameraOverlayProducer producer,
      CameraOverlayCondition condition = null
    ) => Subscribe(CameraOverlayUtility.StableId(key), position, producer, condition);
    public ScreenCameraOverlayEntry Subscribe(
      ulong id, ScreenCameraOverlayPosition position, CameraOverlayProducer producer,
      CameraOverlayCondition condition = null
    ) {
      if (id == 0) throw new ArgumentOutOfRangeException(nameof(id));
      var model = new OverlayEntryModel { position = position.Value, order = position.Order };
      model.dynamicEntry = entries.AddEntry(id, CameraOverlayElement.ComposeBox, default, model, position.Order);
      return new ScreenCameraOverlayEntry(this, model, producer, condition);
    }
    internal void Place(OverlayEntryModel model, in ScreenCameraOverlayPosition position) {
      model.position = position.Value;
      if (model.dynamicEntry.order == position.Order) return;
      model.dynamicEntry.order = position.Order; entries.NotifyEntriesChanged();
    }
    internal void Remove(OverlayEntryModel model) => model.dynamicEntry.Remove();
    internal void ContentChanged(OverlayEntryModel model) => entries.NotifyEntriesChanged();
  }

  public sealed class WorldCameraOverlayController {
    internal readonly DynamicComposableController<DynamicStackLayout> entries = new();
    internal Camera camera;
    public int UpdateIntervalMilliseconds { get; set; } = 16;
    public void SetCamera(Camera value) => camera = value;
    public WorldCameraOverlayEntry Subscribe(
      string key, Vector3 position, CameraOverlayProducer producer, CameraOverlayCondition condition = null
    ) => Subscribe(CameraOverlayUtility.StableId(key), position, producer, condition);
    public WorldCameraOverlayEntry Subscribe(
      ulong id, Vector3 position, CameraOverlayProducer producer, CameraOverlayCondition condition = null
    ) {
      if (id == 0) throw new ArgumentOutOfRangeException(nameof(id));
      var model = new OverlayEntryModel { position = position };
      model.dynamicEntry = entries.AddEntry(id, CameraOverlayElement.ComposeBox, default, model);
      return new WorldCameraOverlayEntry(this, model, producer, condition);
    }
    internal void Remove(OverlayEntryModel model) => model.dynamicEntry.Remove();
    internal void ContentChanged(OverlayEntryModel model) => entries.NotifyEntriesChanged();
  }

  /// <summary>Allocation-stable semantic sink used to write overlay properties directly into an entry model.</summary>
  internal sealed class CameraOverlayProseWriter : ProseWriter {
    private enum Frame : byte { Property, Key, Value, Description, Header, Paragraph, Span }
    private readonly Frame[] _frames = new Frame[8];
    private readonly Action<OverlayEntryModel> _contentChanged;
    private readonly OverlayEntryModel _target;
    private int _frameCount;
    private bool _updating;
    private string _key, _unit;
    private Color? _color;
    private OverlayFieldModel _field;

    internal CameraOverlayProseWriter(Action<OverlayEntryModel> contentChanged, OverlayEntryModel target) {
      _contentChanged = contentChanged; _target = target;
    }
    internal void StartUpdate() {
      if (_updating) throw new InvalidOperationException("The camera overlay entry is already being updated.");
      _updating = true;
    }
    internal void FinishUpdate() {
      if (_frameCount != 0) throw new InvalidOperationException("All prose scopes must be ended before finishing an overlay update.");
      if (!_updating) return;
      _updating = false;
      unchecked { _target.revision++; }
    }

    public override bool TryBegin<T>(T scope) {
      if (!_updating) throw new InvalidOperationException("Begin an update through the owning camera overlay entry first.");
      if (IsNull(scope)) throw new ArgumentNullException(nameof(scope));
      if (_frameCount == _frames.Length) throw new InvalidOperationException("Camera overlay prose nesting exceeds its fixed capacity.");
      Frame frame;
      switch (scope) {
        case ProseProperty:
          _key = null;
          _unit = null;
          _color = null;
          _field = null;
          frame = Frame.Property;
          break;
        case ProsePropertyKey: frame = Frame.Key; break;
        case ProsePropertyValue: frame = Frame.Value; break;
        case ProsePropertyDescription: frame = Frame.Description; break;
        case ProseSectionHeader: frame = Frame.Header; break;
        case ProseParagraph: frame = Frame.Paragraph; break;
        case ProseSpan: frame = Frame.Span; break;
        default: return false;
      }
      _frames[_frameCount++] = frame;
      return true;
    }

    public override void Begin<T>(T scope) {
      if (!TryBegin(scope)) throw new NotSupportedException(
        $"The camera overlay prose writer does not support {(IsNull(scope) ? "null" : scope.GetType().Name)}."
      );
    }

    public override void End() {
      if (_frameCount == 0) throw new InvalidOperationException("There is no camera overlay prose frame to end.");
      var scope = _frames[--_frameCount];
      if (scope == Frame.Property && _field != null) {
        _field.description = _unit; _field.color = _color;
      }
    }

    public override void Push<T>(T modifier) {
      if (IsNull(modifier)) throw new ArgumentNullException(nameof(modifier));
      if (typeof(T) == typeof(CameraOverlayColor)) {
        _color = Unsafe.As<T, CameraOverlayColor>(ref modifier).Value;
      }
    }

    public override void Write<T>(T prose) {
      if (IsNull(prose)) Write("null");
      else if (typeof(T) == typeof(CameraOverlayContent))
        SetContent(Unsafe.As<T, CameraOverlayContent>(ref prose).Value);
      else prose.ToProse(this);
    }

    public override void Write<T>(T value, IDatatype<T> datatype) {
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      if (_frameCount == 0 || _frames[_frameCount - 1] != Frame.Value || _key == null)
        return;
      if (!_target.fields.TryGetValue(_key, out var current) || current is not OverlayFieldModel<T> field) {
        field = new OverlayFieldModel<T> { label = _key };
        _target.fields[_key] = field;
      }
      field.value = value;
      field.datatype = datatype;
      _field = field;
    }

    public override void Write(string text) {
      if (_frameCount == 0 || text == null) return;
      var scope = _frames[_frameCount - 1];
      if (scope == Frame.Key) _key = text;
      else if (scope == Frame.Description) _unit = text;
      else if (scope == Frame.Header) _target.title = text;
      else if (scope == Frame.Paragraph) _target.subtitle = text;
    }

    private void SetContent(Composable content) {
      if (ReferenceEquals(_target.content, content)) return;
      _target.content = content;
      _contentChanged(_target);
    }

    private static bool IsNull<T>(T value) {
      if (typeof(T).IsValueType) return false;
      return ReferenceEquals(value, null);
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
    private readonly VisualElement _row;
    private readonly TextElement _key, _prefix, _value, _separator1, _value2, _separator2, _value3,
      _suffix, _unit, _description;

    public OverlayPropertyElement() {
      pickingMode = PickingMode.Ignore;
      style.flexDirection = FlexDirection.Column;
      Add(_row = new VisualElement()); _row.style.flexDirection = FlexDirection.Row;
      _row.Add(_key = new TextElement());
      _row.Add(_prefix = new TextElement());
      _row.Add(_value = new TextElement());
      _row.Add(_separator1 = new TextElement()); _separator1.SetText(", ".AsSpan());
      _row.Add(_value2 = new TextElement());
      _row.Add(_separator2 = new TextElement()); _separator2.SetText(", ".AsSpan());
      _row.Add(_value3 = new TextElement());
      _row.Add(_suffix = new TextElement());
      _row.Add(_unit = new TextElement());
      Add(_description = new TextElement());
      _key.style.color = new Color(.61f, .65f, .71f);
      _key.style.marginRight = 8;
      _unit.style.marginLeft = 4;
      _description.style.marginLeft = 12;
      _description.style.color = new Color(.61f, .65f, .71f);
    }

    public void Bind(OverlayFieldModel field) {
      _key.SetText(field.label.AsSpan());
      SetOptional(_prefix, field.Prefix);
      var parts = field.SetText(_value, _value2, _value3);
      _separator1.style.display = _value2.style.display = parts > 1 ? DisplayStyle.Flex : DisplayStyle.None;
      _separator2.style.display = _value3.style.display = parts > 2 ? DisplayStyle.Flex : DisplayStyle.None;
      _value.style.color = field.color ?? Color.white;
      _value2.style.color = _value3.style.color = field.color ?? Color.white;
      SetOptional(_suffix, field.Suffix);
      SetOptional(_unit, field.Unit);
      SetOptional(_description, field.description);
      _unit.style.color = field.color ?? Color.white;
    }

    private static void SetOptional(TextElement element, string text) {
      element.SetText(text == null ? ReadOnlySpan<char>.Empty : text.AsSpan());
      element.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
    }
  }

  internal sealed class CameraOverlayBoxElement : VisualElement {
    private readonly Label _title, _subtitle;
    private readonly VisualElement _fields;
    private readonly Dictionary<string, OverlayPropertyElement> _properties = new();
    public CameraOverlayBoxElement() {
      pickingMode = PickingMode.Ignore;
      style.backgroundColor = new Color(.035f, .045f, .06f, .88f); style.borderTopLeftRadius = 5;
      style.borderTopRightRadius = 5; style.borderBottomLeftRadius = 5; style.borderBottomRightRadius = 5;
      style.paddingLeft = 8; style.paddingRight = 8; style.paddingTop = 5; style.paddingBottom = 5;
      style.marginBottom = 4; style.minWidth = 170;
      Add(_title = new Label()); _title.style.unityFontStyleAndWeight = FontStyle.Bold; _title.style.color = new Color(.75f, .9f, 1f);
      Add(_subtitle = new Label()); _subtitle.style.fontSize = 10; _subtitle.style.color = new Color(.65f, .7f, .78f);
      Add(_fields = new VisualElement());
    }
    public void Bind(OverlayEntryModel entry) {
      _title.text = entry.title; _title.style.display = string.IsNullOrEmpty(entry.title) ? DisplayStyle.None : DisplayStyle.Flex;
      _subtitle.text = entry.subtitle; _subtitle.style.display = string.IsNullOrEmpty(entry.subtitle) ? DisplayStyle.None : DisplayStyle.Flex;
      foreach (var pair in entry.fields) {
        var field = pair.Value;
        if (!_properties.TryGetValue(pair.Key, out var property)) {
          _properties[pair.Key] = property = new OverlayPropertyElement();
          _fields.Add(property);
        }
        property.Bind(field);
      }
    }
  }

  public sealed class CameraOverlayElement : VisualElement {
    private static readonly ushort BoxId = CompositionId.GetTypeId(nameof(CameraOverlayBoxElement));
    internal static readonly Composable ComposeBox = static (ref Composition cx) => {
      if (!cx.AUTHORING.RequireTracked<CameraOverlayBoxElement>(BoxId, out var box, out _))
        box = new CameraOverlayBoxElement();
      var entry = DynamicComposable.Lookup(cx.boundary.Element);
      var model = (OverlayEntryModel)entry.UserData;
      box.Bind(model);
      model.renderedRevision = model.revision;
      cx.AUTHORING.YieldElement(ref cx, box);
      model.content?.Invoke(ref cx);
    };

    private readonly ScreenCameraOverlayController _screenController;
    private readonly WorldCameraOverlayController _worldController;
    private readonly VisualElement _screen, _world;
    private readonly Comparison<VisualElement> _worldDepth;
    private Vector3 _cameraPosition;
    public CameraOverlayElement(
      ScreenCameraOverlayController screenController, WorldCameraOverlayController worldController
    ) {
      _screenController = screenController; _worldController = worldController; pickingMode = PickingMode.Ignore;
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
      schedule.Execute(UpdateScreen).Every(screenController.UpdateIntervalMilliseconds);
      schedule.Execute(UpdateWorld).Every(worldController.UpdateIntervalMilliseconds);
    }
    private void UpdateScreen() {
      DynamicCollectionHelper.Synchronize(_screen, _screenController.entries);
      UpdateElements(_screen, false, null);
    }
    private void UpdateWorld() {
      var camera = _worldController.camera ? _worldController.camera : Camera.main; if (!camera) return;
      _cameraPosition = camera.transform.position;
      DynamicCollectionHelper.Synchronize(_world, _worldController.entries);
      UpdateElements(_world, true, camera);
      _world.hierarchy.Sort(_worldDepth);
    }

    private void UpdateElements(VisualElement parent, bool world, Camera camera) {
      for (var i = 0; i < parent.childCount; i++) {
        if (parent.ElementAt(i) is not DynamicComposableElement element) continue;
        var model = (OverlayEntryModel)element.Entry.UserData;
        var visible = model.owner.Enabled;
        if (world && visible) {
          model.owner.ResolveTrackedPosition();
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
        } else {
          element.style.position = Position.Absolute;
          element.style.left = model.position.x;
          element.style.top = model.position.y;
          element.style.translate = new Translate(0, 0);
          element.style.scale = new Scale(Vector3.one);
        }
        if (visible) visible = model.owner.CheckCondition();
        if (visible) model.owner.Produce();
        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        if (model.renderedRevision != model.revision && element.childCount > 0 &&
            element.ElementAt(0) is CameraOverlayBoxElement box) {
          box.Bind(model);
          model.renderedRevision = model.revision;
        }
      }
    }

    private static OverlayEntryModel Model(VisualElement element) =>
      (element as DynamicComposableElement)?.Entry?.UserData as OverlayEntryModel;
  }
}
