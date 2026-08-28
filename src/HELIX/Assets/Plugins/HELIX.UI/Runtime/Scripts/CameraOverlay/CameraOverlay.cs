using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HELIX.Compose;
using HELIX.Prose;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.UI.CameraOverlays {
  public enum CameraOverlaySpace : byte { Screen, World }

  /// <summary>Allocation-free placement metadata, consumed immediately when pushed inside a box.</summary>
  public readonly struct CameraOverlayPosition : IProseModifier {
    public readonly CameraOverlaySpace Space;
    public readonly Vector3 Value;
    public readonly int Order;
    private CameraOverlayPosition(CameraOverlaySpace space, Vector3 value, int order) {
      Space = space; Value = value; Order = order;
    }
    public static CameraOverlayPosition Screen(float left, float top, int order = 0) =>
      new(CameraOverlaySpace.Screen, new Vector3(left, top), order);
    public static CameraOverlayPosition World(Vector3 position, int order = 0) =>
      new(CameraOverlaySpace.World, position, order);
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
  public enum CameraOverlayUpdateMode : byte { EveryVisibleFrame, WhenInvalidated }

  /// <summary>Global registration and handle-management event.</summary>
  public struct CollectCameraOverlayEvent : Context.Evt<CollectCameraOverlayEvent> {
    public readonly CameraOverlayController overlays;
    public CollectCameraOverlayEvent(CameraOverlayController overlays) => this.overlays = overlays;
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
    public CameraOverlaySpace space;
    public int order;
    public Composable content;
    public DynamicComposable dynamicEntry;
    public int revision;
    public int renderedRevision = -1;
    public CameraOverlayEntry owner;
    public readonly Dictionary<string, OverlayFieldModel> fields = new();
  }

  /// <summary>A caller-owned overlay registration. Dispose it to remove the entry.</summary>
  public sealed class CameraOverlayEntry : IDisposable {
    private readonly CameraOverlayController _controller;
    internal readonly OverlayEntryModel model;
    private readonly CameraOverlayProseWriter _writer;
    private CameraOverlayProducer _producer;
    private Transform _trackedTransform;
    private Vector3 _trackingOffset;
    private bool _dirty = true;
    private bool _disposed;

    internal CameraOverlayEntry(
      CameraOverlayController controller, OverlayEntryModel model, CameraOverlayProducer producer
    ) {
      _controller = controller; this.model = model;
      _producer = producer ?? throw new ArgumentNullException(nameof(producer));
      _writer = new CameraOverlayProseWriter(controller, model);
      model.owner = this;
    }

    public bool Enabled { get; set; } = true;
    public CameraOverlayUpdateMode UpdateMode { get; set; } = CameraOverlayUpdateMode.EveryVisibleFrame;
    public CameraOverlayProducer Producer {
      get => _producer;
      set { _producer = value ?? throw new ArgumentNullException(nameof(value)); Invalidate(); }
    }
    public CameraOverlayPosition Position {
      set {
        ThrowIfDisposed();
        _controller.Place(model, in value);
      }
    }
    public Vector3 ResolvedPosition => model.position;

    public void Track(Transform transform, Vector3 offset = default) {
      ThrowIfDisposed();
      _trackedTransform = transform;
      _trackingOffset = offset;
    }

    public void StopTracking() => _trackedTransform = null;
    public void Invalidate() => _dirty = true;

    public void Dispose() {
      if (_disposed) return;
      _disposed = true;
      _controller.Remove(model);
    }

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

    private void ThrowIfDisposed() {
      if (_disposed) throw new ObjectDisposedException(nameof(CameraOverlayEntry));
    }
  }

  public sealed class CameraOverlayController {
    internal readonly DynamicComposableController<DynamicFlexLayout> screen = new();
    internal readonly DynamicComposableController<DynamicStackLayout> world = new();
    internal Camera camera;
    public void SetCamera(Camera value) => camera = value;

    public CameraOverlayEntry Subscribe(
      string key, CameraOverlayPosition position, CameraOverlayProducer producer
    ) => Subscribe(CameraOverlayUtility.StableId(key), position, producer);

    public CameraOverlayEntry Subscribe(
      ulong id, CameraOverlayPosition position, CameraOverlayProducer producer
    ) {
      if (id == 0) throw new ArgumentOutOfRangeException(nameof(id));
      var model = new OverlayEntryModel {
        space = position.Space, position = position.Value, order = position.Order
      };
      model.dynamicEntry = position.Space == CameraOverlaySpace.World
        ? world.AddEntry(id, CameraOverlayElement.ComposeBox, default, model, position.Order)
        : screen.AddEntry(id, CameraOverlayElement.ComposeBox, default, model, position.Order);
      return new CameraOverlayEntry(this, model, producer);
    }

    internal void Place(OverlayEntryModel model, in CameraOverlayPosition placement) {
      model.position = placement.Value;
      model.order = placement.Order;
      if (model.space != placement.Space) {
        model.dynamicEntry.Remove();
        model.space = placement.Space;
        model.dynamicEntry = placement.Space == CameraOverlaySpace.World
          ? world.AddEntry(model.dynamicEntry.key, CameraOverlayElement.ComposeBox, default, model, placement.Order)
          : screen.AddEntry(model.dynamicEntry.key, CameraOverlayElement.ComposeBox, default, model, placement.Order);
        return;
      }
      if (model.dynamicEntry.order == placement.Order) return;
      model.dynamicEntry.order = placement.Order;
      if (placement.Space == CameraOverlaySpace.World) world.NotifyEntriesChanged();
      else screen.NotifyEntriesChanged();
    }

    internal void Remove(OverlayEntryModel model) {
      model.dynamicEntry.Remove();
    }

    internal void ContentChanged(OverlayEntryModel model) {
      if (model.space == CameraOverlaySpace.World) world.NotifyEntriesChanged();
      else screen.NotifyEntriesChanged();
    }
  }

  /// <summary>Allocation-stable semantic sink used to write overlay properties directly into an entry model.</summary>
  internal sealed class CameraOverlayProseWriter : ProseWriter {
    private enum Frame : byte { Property, Key, Value, Description, Header, Paragraph, Span }
    private readonly Frame[] _frames = new Frame[8];
    private readonly CameraOverlayController _controller;
    private readonly OverlayEntryModel _target;
    private int _frameCount;
    private bool _updating;
    private string _key, _unit;
    private Color? _color;
    private OverlayFieldModel _field;

    internal CameraOverlayProseWriter(CameraOverlayController controller, OverlayEntryModel target) {
      _controller = controller; _target = target;
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
      if (typeof(T) == typeof(CameraOverlayPosition)) {
        ref var position = ref Unsafe.As<T, CameraOverlayPosition>(ref modifier);
        _controller.Place(_target, in position);
      } else if (typeof(T) == typeof(CameraOverlayColor)) {
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
      _controller.ContentChanged(_target);
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

    private readonly CameraOverlayController _controller;
    private readonly VisualElement _screen, _world;
    private readonly Comparison<VisualElement> _worldDepth;
    private Vector3 _cameraPosition;
    public CameraOverlayElement(CameraOverlayController controller) {
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
      DynamicCollectionHelper.Synchronize(_screen, _controller.screen);
      UpdateElements(_screen, now, false, null);
    }
    private void UpdateWorld(float now) {
      var camera = _controller.camera ? _controller.camera : Camera.main; if (!camera) return;
      _cameraPosition = camera.transform.position;
      DynamicCollectionHelper.Synchronize(_world, _controller.world);
      UpdateElements(_world, now, true, camera);
      _world.hierarchy.Sort(_worldDepth);
    }

    private void UpdateElements(VisualElement parent, float now, bool world, Camera camera) {
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
