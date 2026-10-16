using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
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

  /// <summary>
  ///   Colours and metrics for an overlay box, resolved from the theme once rather than read per tick.
  ///   Overlays update every frame and deliberately sit outside recomposition, so they take their
  ///   styling as plain data instead of reaching for a <see cref="Composition" />.
  /// </summary>
  public readonly struct CameraOverlayBoxStyle : IEquatable<CameraOverlayBoxStyle> {
    public readonly Color background, title, subtitle, label, value, description;
    public readonly float radius, paddingX, paddingY, marginBottom, minWidth, gap;

    public CameraOverlayBoxStyle(
      Color background, Color title, Color subtitle, Color label, Color value, Color description,
      float radius, float paddingX, float paddingY, float marginBottom, float minWidth, float gap
    ) {
      this.background = background;
      this.title = title;
      this.subtitle = subtitle;
      this.label = label;
      this.value = value;
      this.description = description;
      this.radius = radius;
      this.paddingX = paddingX;
      this.paddingY = paddingY;
      this.marginBottom = marginBottom;
      this.minWidth = minWidth;
      this.gap = gap;
    }

    /// <summary>Used until a theme is supplied; the values the overlays were hardcoded to.</summary>
    public static readonly CameraOverlayBoxStyle Fallback = new(
      new Color(.035f, .045f, .06f, .88f),
      new Color(.75f, .9f, 1f),
      new Color(.65f, .7f, .78f),
      new Color(.61f, .65f, .71f),
      Color.white,
      new Color(.61f, .65f, .71f),
      5f, 8f, 5f, 4f, 170f, 8f
    );

    public bool Equals(CameraOverlayBoxStyle other) =>
      background == other.background && title == other.title && subtitle == other.subtitle &&
      label == other.label && value == other.value && description == other.description &&
      radius == other.radius && paddingX == other.paddingX && paddingY == other.paddingY &&
      marginBottom == other.marginBottom && minWidth == other.minWidth && gap == other.gap;

    public override bool Equals(object obj) => obj is CameraOverlayBoxStyle other && Equals(other);

    public override int GetHashCode() {
      unchecked {
        var hash = background.GetHashCode();
        hash = (hash * 397) ^ value.GetHashCode();
        hash = (hash * 397) ^ radius.GetHashCode();
        hash = (hash * 397) ^ minWidth.GetHashCode();
        return hash;
      }
    }

    public static CameraOverlayBoxStyle From(ThemeData theme) {
      if (theme == null) return Fallback;
      var surface = theme.GetColor(ColorRoles.SurfaceContainerHighest);
      return new CameraOverlayBoxStyle(
        new Color(surface.r, surface.g, surface.b, Fallback.background.a),
        theme.GetColor(ColorRoles.Primary),
        theme.GetColor(ColorRoles.OnSurfaceVariant),
        theme.GetColor(ColorRoles.OnSurfaceVariant),
        theme.GetColor(ColorRoles.OnSurface),
        theme.GetColor(ColorRoles.OnSurfaceVariant),
        theme[RadiusRole.Radius1],
        theme[SpacingRole.Spacing2],
        theme[SpacingRole.Spacing1],
        theme[SpacingRole.Spacing1],
        Fallback.minWidth,
        theme[SpacingRole.Spacing2]
      );
    }
  }

  internal abstract class OverlayFieldModel {
    public string label, description;
    public Color? color;

    /// <summary>The update that last wrote this field. A field left behind is dropped from the box.</summary>
    public int generation;

    public abstract string Prefix { get; }
    public abstract string Suffix { get; }
    public abstract string Unit { get; }

    /// <summary>Writes the value into <paramref name="values" /> and returns how many parts it used.</summary>
    public abstract int SetText(OverlayValueElements values);
  }

  internal sealed class OverlayFieldModel<T> : OverlayFieldModel {
    private TextElementProseWriter _fallbackWriter;
    public T value;
    public IDatatype<T> datatype;
    public override string Prefix => (datatype as IDatatypeAffix)?.Prefix;
    public override string Suffix => (datatype as IDatatypeAffix)?.Suffix;
    public override string Unit => (datatype as IDatatypeUnit)?.Unit;

    public override int SetText(OverlayValueElements values) {
      // The typed branches exist to avoid the formatting allocation on a per-frame path. Everything
      // they need beyond the value comes off the datatype, so any datatype gets them, not one class.
      if (typeof(T) == typeof(int)) {
        values.At(0).SetText(Unsafe.As<T, int>(ref value));
        return 1;
      }

      if (typeof(T) == typeof(float) && datatype is IDatatypeNumberFormat format && !format.Compact) {
        var number = Unsafe.As<T, float>(ref value);
        if (format.Clamp && datatype is IDatatypeRange<float> range)
          number = Math.Max(range.Min ?? float.MinValue, Math.Min(range.Max ?? float.MaxValue, number));
        values.At(0).SetText(number * format.Scale, format.Format);
        return 1;
      }

      if (typeof(T) == typeof(string)) {
        var text = Unsafe.As<T, string>(ref value);
        values.At(0).SetText(text == null ? "null".AsSpan() : text.AsSpan());
        return 1;
      }

      // A structured or multi-part value writes one element per component rather than being flattened
      // into a single element, where each write would overwrite the one before it. The typed overloads
      // are preferred because ICompositeDatatype.WriteComponent takes an object and would box the
      // value once per component per frame.
      if (datatype is CompositeDatatype<T> typedComposite && typedComposite.ComponentCount > 0) {
        var count = typedComposite.ComponentCount;
        _fallbackWriter ??= new TextElementProseWriter();
        for (var i = 0; i < count; i++) {
          _fallbackWriter.Element = values.At(i);
          typedComposite.WriteComponent(_fallbackWriter, value, i);
          _fallbackWriter.Flush();
        }
        return count;
      }

      if (datatype is StructureDatatype<T> structure && structure.Properties.Count > 0) {
        var count = structure.Properties.Count;
        _fallbackWriter ??= new TextElementProseWriter();
        for (var i = 0; i < count; i++) {
          _fallbackWriter.Element = values.At(i);
          structure.WriteComponent(_fallbackWriter, ref value, i);
          _fallbackWriter.Flush();
        }
        return count;
      }

      if (datatype is ICompositeDatatype composite && composite.ComponentCount > 0) {
        var count = composite.ComponentCount;
        _fallbackWriter ??= new TextElementProseWriter();
        for (var i = 0; i < count; i++) {
          _fallbackWriter.Element = values.At(i);
          composite.WriteComponent(_fallbackWriter, value, i);
          _fallbackWriter.Flush();
        }
        return count;
      }

      (_fallbackWriter ??= new TextElementProseWriter()).Element = values.At(0);
      datatype.ToProse(_fallbackWriter, value);
      _fallbackWriter.Flush();
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
    public int generation;
    public CameraOverlayEntry owner;

    /// <summary>
    ///   Fields in the order they were written, which is the order they are read in. The lookup is only
    ///   there to find an existing field's storage again between updates.
    /// </summary>
    public readonly List<OverlayFieldModel> fields = new();
    public readonly Dictionary<string, int> fieldIndices = new();

    public OverlayFieldModel<T> Field<T>(string key) {
      if (fieldIndices.TryGetValue(key, out var index) && fields[index] is OverlayFieldModel<T> existing) {
        existing.generation = generation;
        return existing;
      }
      var field = new OverlayFieldModel<T> { label = key, generation = generation };
      if (fieldIndices.TryGetValue(key, out index)) fields[index] = field;
      else {
        fieldIndices[key] = fields.Count;
        fields.Add(field);
      }
      return field;
    }

    /// <summary>Drops every field the update just finished did not write.</summary>
    public void DropStaleFields() {
      var removed = false;
      for (var i = fields.Count - 1; i >= 0; i--) {
        if (fields[i].generation == generation) continue;
        fields.RemoveAt(i);
        removed = true;
      }
      if (!removed) return;
      fieldIndices.Clear();
      for (var i = 0; i < fields.Count; i++) fieldIndices[fields[i].label] = i;
    }
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

  /// <summary>
  ///   Shared registration and removal for the overlay controllers. The two differ only in how an
  ///   entry's position is applied, so everything else lives here once.
  /// </summary>
  public abstract class CameraOverlayController<TLayout> {
    internal readonly DynamicComposableController<TLayout> entries = new();

    public int UpdateIntervalMilliseconds { get; set; } = 16;

    internal OverlayEntryModel Register(ulong id, Vector3 position, int order) {
      if (id == 0) throw new ArgumentOutOfRangeException(nameof(id));
      var model = new OverlayEntryModel { position = position, order = order };
      model.dynamicEntry = entries.AddEntry(id, CameraOverlayElement.ComposeBox, default, model, order);
      return model;
    }

    internal void Remove(OverlayEntryModel model) => model.dynamicEntry.Remove();
    internal void ContentChanged(OverlayEntryModel model) => entries.NotifyEntriesChanged();
  }

  public sealed class ScreenCameraOverlayController : CameraOverlayController<DynamicFlexLayout> {
    public ScreenCameraOverlayEntry Subscribe(
      string key, ScreenCameraOverlayPosition position, CameraOverlayProducer producer,
      CameraOverlayCondition condition = null
    ) => Subscribe(CameraOverlayUtility.StableId(key), position, producer, condition);

    public ScreenCameraOverlayEntry Subscribe(
      ulong id, ScreenCameraOverlayPosition position, CameraOverlayProducer producer,
      CameraOverlayCondition condition = null
    ) => new(this, Register(id, position.Value, position.Order), producer, condition);

    internal void Place(OverlayEntryModel model, in ScreenCameraOverlayPosition position) {
      model.position = position.Value;
      if (model.dynamicEntry.order == position.Order) return;
      model.dynamicEntry.order = position.Order;
      entries.NotifyEntriesChanged();
    }
  }

  public sealed class WorldCameraOverlayController : CameraOverlayController<DynamicStackLayout> {
    internal Camera camera;

    public void SetCamera(Camera value) => camera = value;

    public WorldCameraOverlayEntry Subscribe(
      string key, Vector3 position, CameraOverlayProducer producer, CameraOverlayCondition condition = null
    ) => Subscribe(CameraOverlayUtility.StableId(key), position, producer, condition);

    public WorldCameraOverlayEntry Subscribe(
      ulong id, Vector3 position, CameraOverlayProducer producer, CameraOverlayCondition condition = null
    ) => new(this, Register(id, position, 0), producer, condition);
  }

  /// <summary>Allocation-stable semantic sink used to write overlay properties directly into an entry model.</summary>
  internal sealed class CameraOverlayProseWriter : ProseWriter {
    private enum Frame : byte { Property, Key, Value, Description, Header, Paragraph, Span, Tree, Ignored }
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
      unchecked { _target.generation++; }
    }
    internal void FinishUpdate() {
      if (_frameCount != 0) throw new InvalidOperationException("All prose scopes must be ended before finishing an overlay update.");
      if (!_updating) return;
      _updating = false;
      _target.DropStaleFields();
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
        // A structured datatype writes its properties inside a tree; without it the whole value is
        // refused and a generated structure cannot be written to an overlay at all.
        case ProseTree: frame = Frame.Tree; break;
        default: return false;
      }
      _frames[_frameCount++] = frame;
      return true;
    }

    /// <summary>
    ///   Retains an unsupported scope as an ignored frame rather than throwing, so a producer written
    ///   against a richer sink stays balanced here and simply contributes nothing.
    /// </summary>
    public override void Begin<T>(T scope) {
      if (TryBegin(scope)) return;
      if (_frameCount == _frames.Length)
        throw new InvalidOperationException("Camera overlay prose nesting exceeds its fixed capacity.");
      _frames[_frameCount++] = Frame.Ignored;
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
      var field = _target.Field<T>(_key);
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

  /// <summary>
  ///   Writes prose into a single <see cref="TextElement" />. Text accumulates rather than replacing
  ///   what came before, because a value written through a datatype usually arrives in several pieces.
  ///   When the prose names a property value, only that value is kept: an overlay cell shows the value,
  ///   and the key and description are already placed by the row around it.
  /// </summary>
  /// <remarks>
  ///   This runs every frame, so nothing here is allowed to allocate in the steady state. Numbers are
  ///   formatted into a reusable buffer through <see cref="IProseNumberWriter" /> rather than through
  ///   <c>ToString</c>, and the accumulated text reaches the element as the builder itself.
  /// </remarks>
  internal sealed class TextElementProseWriter : ProseWriter, IProseNumberWriter {
    private readonly StringBuilder _builder = new();
    private char[] _numberBuffer = new char[32];
    private int _depth;
    private int _valueDepth = -1;
    private bool _sawValue;

    public TextElement Element { get; set; }

    /// <summary>Writes what has accumulated into the element and starts over.</summary>
    public void Flush() {
      if (Element != null) {
        // SetText(StringBuilder) copies straight out of the builder. ToString would allocate a
        // string per value per frame, which is what this path exists to avoid.
        if (_builder.Length == 0) Element.SetText("null".AsSpan());
        else Element.SetText(_builder);
      }
      _builder.Clear();
      _depth = 0;
      _valueDepth = -1;
      _sawValue = false;
    }

    public override bool TryBegin<T>(T scope) {
      _depth++;
      if (scope is not ProsePropertyValue || _sawValue) return true;
      // The value supersedes anything written before it, such as the property's key.
      _sawValue = true;
      _valueDepth = _depth;
      _builder.Clear();
      return true;
    }

    public override void Begin<T>(T scope) => TryBegin(scope);

    public override void End() {
      if (_depth == _valueDepth) _valueDepth = -1;
      if (_depth > 0) _depth--;
    }

    public override void Push<T>(T modifier) { }

    public override void Write<T>(T prose) {
      if (prose is not null) prose.ToProse(this);
    }

    public override void Write<T>(T value, IDatatype<T> datatype) => datatype.ToProse(this, value);

    public override void Write(string text) {
      if (Suppressed) return;
      _builder.Append(text ?? "null");
    }

    // Written out per type rather than over ISpanFormattable, which this runtime's API profile
    // does not expose. Each one formats into the shared buffer and appends what it wrote.
    public void WriteNumber(int value, string format) {
      if (Suppressed) return;
      int written;
      while (!value.TryFormat(_numberBuffer, out written, format.AsSpan(), CultureInfo.InvariantCulture))
        Grow();
      _builder.Append(_numberBuffer, 0, written);
    }

    public void WriteNumber(long value, string format) {
      if (Suppressed) return;
      int written;
      while (!value.TryFormat(_numberBuffer, out written, format.AsSpan(), CultureInfo.InvariantCulture))
        Grow();
      _builder.Append(_numberBuffer, 0, written);
    }

    public void WriteNumber(float value, string format) {
      if (Suppressed) return;
      int written;
      while (!value.TryFormat(_numberBuffer, out written, format.AsSpan(), CultureInfo.InvariantCulture))
        Grow();
      _builder.Append(_numberBuffer, 0, written);
    }

    public void WriteNumber(double value, string format) {
      if (Suppressed) return;
      int written;
      while (!value.TryFormat(_numberBuffer, out written, format.AsSpan(), CultureInfo.InvariantCulture))
        Grow();
      _builder.Append(_numberBuffer, 0, written);
    }

    /// <summary>Once a value scope has been seen, only its contents belong in the cell.</summary>
    private bool Suppressed => _sawValue && _valueDepth < 0;

    /// <summary>Grows at most once or twice per writer; a formatted number is small and bounded.</summary>
    private void Grow() => _numberBuffer = new char[_numberBuffer.Length * 2];
  }

  /// <summary>
  ///   The text elements a field's value is written into, grown to however many parts the value has.
  ///   Growth happens once per field, so an update in the steady state allocates nothing.
  /// </summary>
  internal sealed class OverlayValueElements {
    private readonly List<TextElement> _elements = new();
    private readonly List<TextElement> _separators = new();
    private readonly VisualElement _parent;
    private readonly VisualElement _before;

    internal OverlayValueElements(VisualElement parent, VisualElement before) {
      _parent = parent;
      _before = before;
    }

    internal TextElement At(int index) {
      while (_elements.Count <= index) {
        if (_elements.Count > 0) {
          var separator = new TextElement();
          separator.SetText(", ".AsSpan());
          _separators.Add(separator);
          Insert(separator);
        }
        var element = new TextElement();
        _elements.Add(element);
        Insert(element);
      }
      return _elements[index];
    }

    /// <summary>Hides the parts a shorter value left behind, and tints the visible ones.</summary>
    internal void Show(int count, Color color) {
      for (var i = 0; i < _elements.Count; i++) {
        var visible = i < count;
        _elements[i].style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        if (visible) _elements[i].style.color = color;
      }
      for (var i = 0; i < _separators.Count; i++)
        _separators[i].style.display = i + 1 < count ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void Insert(VisualElement element) {
      var index = _parent.IndexOf(_before);
      if (index < 0) _parent.Add(element);
      else _parent.Insert(index, element);
    }
  }

  internal sealed class OverlayPropertyElement : VisualElement {
    private readonly VisualElement _row;
    private readonly TextElement _key, _prefix, _suffix, _unit, _description;
    private readonly OverlayValueElements _values;

    public OverlayPropertyElement() {
      pickingMode = PickingMode.Ignore;
      style.flexDirection = FlexDirection.Column;
      Add(_row = new VisualElement());
      _row.style.flexDirection = FlexDirection.Row;
      _row.Add(_key = new TextElement());
      _row.Add(_prefix = new TextElement());
      _row.Add(_suffix = new TextElement());
      _row.Add(_unit = new TextElement());
      Add(_description = new TextElement());
      // Value parts are created on demand, between the prefix and the suffix.
      _values = new OverlayValueElements(_row, _suffix);
    }

    public void Bind(OverlayFieldModel field, in CameraOverlayBoxStyle style) {
      _key.SetText(field.label.AsSpan());
      _key.style.color = style.label;
      _key.style.marginRight = style.gap;
      SetOptional(_prefix, field.Prefix);
      _values.Show(field.SetText(_values), field.color ?? style.value);
      SetOptional(_suffix, field.Suffix);
      SetOptional(_unit, field.Unit);
      _unit.style.marginLeft = style.gap * 0.5f;
      _unit.style.color = field.color ?? style.value;
      SetOptional(_description, field.description);
      _description.style.marginLeft = style.gap * 1.5f;
      _description.style.color = style.description;
    }

    private static void SetOptional(TextElement element, string text) {
      element.SetText(text == null ? ReadOnlySpan<char>.Empty : text.AsSpan());
      element.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
    }
  }

  internal sealed class CameraOverlayBoxElement : VisualElement {
    private readonly Label _title, _subtitle;
    private readonly VisualElement _fields;
    private readonly List<OverlayPropertyElement> _properties = new();
    private CameraOverlayBoxStyle _style = CameraOverlayBoxStyle.Fallback;
    private bool _styled;

    public CameraOverlayBoxElement() {
      pickingMode = PickingMode.Ignore;
      Add(_title = new Label());
      _title.style.unityFontStyleAndWeight = FontStyle.Bold;
      Add(_subtitle = new Label());
      Add(_fields = new VisualElement());
    }

    public void Bind(OverlayEntryModel entry, in CameraOverlayBoxStyle style) {
      ApplyStyle(in style);
      _title.text = entry.title;
      _title.style.display = string.IsNullOrEmpty(entry.title) ? DisplayStyle.None : DisplayStyle.Flex;
      _subtitle.text = entry.subtitle;
      _subtitle.style.display = string.IsNullOrEmpty(entry.subtitle) ? DisplayStyle.None : DisplayStyle.Flex;

      // Fields render in the order they were written, and a field that stopped being written is gone
      // from the model, so the row that showed it is hidden rather than left stale.
      for (var i = 0; i < entry.fields.Count; i++) {
        if (i == _properties.Count) {
          var created = new OverlayPropertyElement();
          _properties.Add(created);
          _fields.Add(created);
        }
        _properties[i].style.display = DisplayStyle.Flex;
        _properties[i].Bind(entry.fields[i], in _style);
      }
      for (var i = entry.fields.Count; i < _properties.Count; i++)
        _properties[i].style.display = DisplayStyle.None;
    }

    private void ApplyStyle(in CameraOverlayBoxStyle style) {
      if (_styled && style.Equals(_style)) return;
      _style = style;
      _styled = true;
      this.style.backgroundColor = style.background;
      this.style.borderTopLeftRadius = style.radius;
      this.style.borderTopRightRadius = style.radius;
      this.style.borderBottomLeftRadius = style.radius;
      this.style.borderBottomRightRadius = style.radius;
      this.style.paddingLeft = style.paddingX;
      this.style.paddingRight = style.paddingX;
      this.style.paddingTop = style.paddingY;
      this.style.paddingBottom = style.paddingY;
      this.style.marginBottom = style.marginBottom;
      this.style.minWidth = style.minWidth;
      _title.style.color = style.title;
      _subtitle.style.color = style.subtitle;
      _subtitle.style.fontSize = 10;
    }
  }

  public sealed class CameraOverlayElement : VisualElement {
    private static readonly ushort BoxId = CompositionId.GetTypeId(nameof(CameraOverlayBoxElement));

    internal static readonly Composable ComposeBox = static (ref Composition cx) => {
      if (!cx.AUTHORING.RequireTracked<CameraOverlayBoxElement>(BoxId, out var box, out _))
        box = new CameraOverlayBoxElement();
      var entry = DynamicComposable.Lookup(cx.boundary.Element);
      var model = (OverlayEntryModel)entry.UserData;
      var host = cx.boundary.Element?.GetFirstAncestorOfType<CameraOverlayElement>();
      box.Bind(model, host?.BoxStyle ?? CameraOverlayBoxStyle.Fallback);
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
      _screen.style.position = Position.Absolute;
      _screen.style.left = ScreenInset; _screen.style.top = ScreenInset;
      schedule.Execute(UpdateScreen).Every(screenController.UpdateIntervalMilliseconds);
      schedule.Execute(UpdateWorld).Every(worldController.UpdateIntervalMilliseconds);
    }

    private const float ScreenInset = 10f;

    /// <summary>
    ///   The box styling these overlays use. Resolved from the theme when it is set rather than read
    ///   per tick, because overlays update every frame and are not part of recomposition.
    /// </summary>
    internal CameraOverlayBoxStyle BoxStyle { get; private set; } = CameraOverlayBoxStyle.Fallback;

    public void SetTheme(ThemeData theme) {
      var resolved = CameraOverlayBoxStyle.From(theme);
      if (resolved.Equals(BoxStyle)) return;
      BoxStyle = resolved;
      Restyle(_screen);
      Restyle(_world);
    }

    private void Restyle(VisualElement parent) {
      for (var i = 0; i < parent.childCount; i++) {
        if (parent.ElementAt(i) is not DynamicComposableElement element) continue;
        if (element.childCount > 0 && element.ElementAt(0) is CameraOverlayBoxElement box)
          box.Bind((OverlayEntryModel)element.Entry.UserData, BoxStyle);
      }
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

        // Everything that can rule the entry out is asked before any work is done for it: an entry
        // that is off or failing its condition costs nothing but this check per tick.
        var visible = model.owner.Enabled && model.owner.CheckCondition();
        if (visible && world) visible = PositionInWorld(element, model, camera);
        else if (visible) PositionOnScreen(element, model);

        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        if (!visible) continue;

        model.owner.Produce();
        if (model.renderedRevision == model.revision) continue;
        if (element.childCount > 0 && element.ElementAt(0) is CameraOverlayBoxElement box) {
          box.Bind(model, BoxStyle);
          model.renderedRevision = model.revision;
        }
      }
    }

    /// <summary>Projects the entry onto the screen; false when it is behind the camera.</summary>
    private bool PositionInWorld(VisualElement element, OverlayEntryModel model, Camera camera) {
      model.owner.ResolveTrackedPosition();
      var point = camera.WorldToScreenPoint(model.position);
      if (point.z <= 0) return false;
      element.style.position = Position.Absolute;
      element.style.left = point.x;
      element.style.top = resolvedStyle.height - point.y;
      element.style.translate = new Translate(Length.Percent(-50), 0);
      var distance = Vector3.Distance(_cameraPosition, model.position);
      var scale = Mathf.Clamp(10f / Mathf.Max(.001f, distance), .5f, 1.5f);
      element.style.scale = new Scale(new Vector3(scale, scale, 1f));
      return true;
    }

    private static void PositionOnScreen(VisualElement element, OverlayEntryModel model) {
      element.style.position = Position.Absolute;
      element.style.left = model.position.x;
      element.style.top = model.position.y;
      element.style.translate = new Translate(0, 0);
      element.style.scale = new Scale(Vector3.one);
    }

    private static OverlayEntryModel Model(VisualElement element) =>
      (element as DynamicComposableElement)?.Entry?.UserData as OverlayEntryModel;
  }
}
