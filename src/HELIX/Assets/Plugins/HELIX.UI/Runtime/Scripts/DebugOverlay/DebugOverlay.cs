using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.UI.DebugOverlay {
  public readonly struct ScreenOverlayBuilder {
    private readonly IScreenOverlay _overlay;
    internal ScreenOverlayBuilder(IScreenOverlay overlay) => _overlay = overlay;
    public ScreenOverlayBuilder Field(string label, object value, string unit = null, Color? color = null) {
      _overlay?.Field(label, value, unit, color); return this;
    }
    public ScreenOverlayBuilder Field(string label, float value, int decimals = 2, string unit = null, Color? color = null) {
      _overlay?.Field(label, value.ToString($"F{decimals}", CultureInfo.InvariantCulture), unit, color); return this;
    }
    public ScreenOverlayBuilder Field(string label, Vector3 value, int decimals = 1, string unit = null, Color? color = null) {
      var format = $"F{decimals}";
      return Field(label, $"{value.x.ToString(format, CultureInfo.InvariantCulture)}, {value.y.ToString(format, CultureInfo.InvariantCulture)}, {value.z.ToString(format, CultureInfo.InvariantCulture)}", unit, color);
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
    public WorldOverlayBuilder Field(string label, object value, string unit = null, Color? color = null) {
      _overlay?.Field(_id, label, value, unit, color); return this;
    }
    public WorldOverlayBuilder Field(string label, float value, int decimals = 2, string unit = null, Color? color = null) =>
      Field(label, value.ToString($"F{decimals}", CultureInfo.InvariantCulture), unit, color);
    public WorldOverlayBuilder Field(string label, Vector3 value, int decimals = 1, string unit = null, Color? color = null) {
      var format = $"F{decimals}";
      return Field(label, $"{value.x.ToString(format, CultureInfo.InvariantCulture)}, {value.y.ToString(format, CultureInfo.InvariantCulture)}, {value.z.ToString(format, CultureInfo.InvariantCulture)}", unit, color);
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
    void Field(string label, object value, string unit = null, Color? color = null);
  }

  public interface IWorldOverlay {
    void Begin(ulong id, string title, Vector3 position, string subtitle = null);
    bool Contains(ulong id);
    void Field(ulong id, string label, object value, string unit = null, Color? color = null);
    void SetCamera(Camera camera);
  }

  public struct CollectScreenOverlayEvent : Context.Evt<CollectScreenOverlayEvent> {
    public readonly ScreenOverlayInitiator overlay;
    public CollectScreenOverlayEvent(IScreenOverlay overlay) => this.overlay = new ScreenOverlayInitiator(overlay);
  }
  public struct CollectWorldOverlayEvent : Context.Evt<CollectWorldOverlayEvent> {
    public readonly WorldOverlayInitiator overlay;
    public CollectWorldOverlayEvent(IWorldOverlay overlay) => this.overlay = new WorldOverlayInitiator(overlay);
  }

  internal sealed class OverlayFieldModel {
    public string label, value, unit;
    public Color? color;
    public float seen;
  }
  internal sealed class OverlayEntryModel {
    public ulong id;
    public string title, subtitle;
    public int order;
    public Vector3 position;
    public float seen;
    public readonly Dictionary<string, OverlayFieldModel> fields = new();
    public OverlayCardElement element;
  }

  public sealed class DebugOverlayController : IScreenOverlay, IWorldOverlay {
    internal readonly Dictionary<string, OverlayEntryModel> screen = new();
    internal readonly Dictionary<ulong, OverlayEntryModel> world = new();
    internal Camera camera;
    private OverlayEntryModel _section;
    public void BeginFrame() => _section = null;
    public void BeginSection(string title, int order = 0, string subtitle = null) {
      if (!screen.TryGetValue(title, out _section)) screen[title] = _section = new OverlayEntryModel { title = title };
      _section.subtitle = subtitle; _section.order = order; _section.seen = Time.unscaledTime;
    }
    public void Field(string label, object value, string unit = null, Color? color = null) {
      if (_section == null) BeginSection("");
      UpdateField(_section, label, value, unit, color);
    }
    public void Begin(ulong id, string title, Vector3 position, string subtitle = null) {
      if (id == 0) return;
      if (!world.TryGetValue(id, out var entry)) world[id] = entry = new OverlayEntryModel { id = id };
      entry.title = title; entry.subtitle = subtitle; entry.position = position; entry.seen = Time.unscaledTime;
    }
    public bool Contains(ulong id) => world.ContainsKey(id);
    public void Field(ulong id, string label, object value, string unit = null, Color? color = null) {
      if (world.TryGetValue(id, out var entry)) UpdateField(entry, label, value, unit, color);
    }
    public void SetCamera(Camera value) => camera = value;
    private static void UpdateField(OverlayEntryModel entry, string label, object value, string unit, Color? color) {
      if (!entry.fields.TryGetValue(label, out var field)) entry.fields[label] = field = new OverlayFieldModel { label = label };
      field.value = value?.ToString() ?? "null"; field.unit = unit; field.color = color; field.seen = Time.unscaledTime;
    }
  }

  internal sealed class OverlayCardElement : VisualElement {
    private readonly Label _title, _subtitle;
    private readonly VisualElement _fields;
    private readonly Dictionary<string, Label> _labels = new();
    public OverlayEntryModel Entry { get; private set; }
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
      Entry = entry;
      _title.text = entry.title; _title.style.display = string.IsNullOrEmpty(entry.title) ? DisplayStyle.None : DisplayStyle.Flex;
      _subtitle.text = entry.subtitle; _subtitle.style.display = string.IsNullOrEmpty(entry.subtitle) ? DisplayStyle.None : DisplayStyle.Flex;
      foreach (var pair in entry.fields) {
        var field = pair.Value;
        if (!_labels.TryGetValue(pair.Key, out var label)) { _labels[pair.Key] = label = new Label(); _fields.Add(label); label.enableRichText = true; }
        label.style.display = now - field.seen < .15f ? DisplayStyle.Flex : DisplayStyle.None;
        label.style.color = field.color ?? Color.white;
        label.text = $"<color=#9BA7B5>{field.label}</color>  {field.value}{(string.IsNullOrEmpty(field.unit) ? "" : " " + field.unit)}";
      }
    }
  }

  public sealed class DebugOverlayElement : VisualElement {
    private readonly DebugOverlayController _controller;
    private readonly VisualElement _screen, _world;
    private readonly List<string> _deadScreen = new();
    private readonly List<ulong> _deadWorld = new();
    private readonly List<string> _deadFields = new();
    private readonly Comparison<VisualElement> _screenOrder;
    private readonly Comparison<VisualElement> _worldDepth;
    private Vector3 _cameraPosition;
    public DebugOverlayElement(DebugOverlayController controller) {
      _controller = controller; pickingMode = PickingMode.Ignore;
      _screenOrder = (left, right) => Entry(left)?.order.CompareTo(Entry(right)?.order ?? 0) ?? 0;
      _worldDepth = (left, right) => {
        var a = Entry(left); var b = Entry(right); if (a == null || b == null) return 0;
        return (b.position - _cameraPosition).sqrMagnitude.CompareTo((a.position - _cameraPosition).sqrMagnitude);
      };
      style.position = Position.Absolute; style.left = 0; style.right = 0; style.top = 0; style.bottom = 0;
      Add(_world = new VisualElement { pickingMode = PickingMode.Ignore });
      Add(_screen = new VisualElement { pickingMode = PickingMode.Ignore });
      _screen.style.position = Position.Absolute; _screen.style.left = 10; _screen.style.top = 10;
      schedule.Execute(UpdateOverlay).Every(16);
    }
    private void UpdateOverlay() {
      var now = Time.unscaledTime;
      UpdateScreen(now); UpdateWorld(now);
    }
    private void UpdateScreen(float now) {
      _deadScreen.Clear();
      foreach (var pair in _controller.screen) {
        var entry = pair.Value;
        if (now - entry.seen > 2) { entry.element?.RemoveFromHierarchy(); _deadScreen.Add(pair.Key); continue; }
        entry.element ??= new OverlayCardElement(); if (entry.element.parent == null) _screen.Add(entry.element);
        entry.element.style.display = now - entry.seen < .15f ? DisplayStyle.Flex : DisplayStyle.None; entry.element.Bind(entry, now);
        CleanupFields(entry, now);
      }
      for (var i = 0; i < _deadScreen.Count; i++) _controller.screen.Remove(_deadScreen[i]);
      _screen.hierarchy.Sort(_screenOrder);
    }
    private void UpdateWorld(float now) {
      var camera = _controller.camera ? _controller.camera : Camera.main; if (!camera) return;
      _cameraPosition = camera.transform.position;
      _deadWorld.Clear();
      foreach (var pair in _controller.world) {
        var entry = pair.Value;
        if (now - entry.seen > 2) { entry.element?.RemoveFromHierarchy(); _deadWorld.Add(pair.Key); continue; }
        entry.element ??= new OverlayCardElement(); if (entry.element.parent == null) _world.Add(entry.element);
        var point = camera.WorldToScreenPoint(entry.position); var visible = point.z > 0 && now - entry.seen < .15f;
        entry.element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        if (visible) { entry.element.style.position = Position.Absolute; entry.element.style.left = point.x;
          entry.element.style.top = resolvedStyle.height - point.y; entry.element.style.translate = new Translate(Length.Percent(-50), 0);
          var distance = Vector3.Distance(_cameraPosition, entry.position);
          var scale = Mathf.Clamp(10f / Mathf.Max(.001f, distance), .5f, 1.5f);
          entry.element.style.scale = new Scale(new Vector3(scale, scale, 1f));
          entry.element.Bind(entry, now); }
        CleanupFields(entry, now);
      }
      for (var i = 0; i < _deadWorld.Count; i++) _controller.world.Remove(_deadWorld[i]);
      _world.hierarchy.Sort(_worldDepth);
    }

    private void CleanupFields(OverlayEntryModel entry, float now) {
      _deadFields.Clear();
      foreach (var pair in entry.fields) if (now - pair.Value.seen > 2f) _deadFields.Add(pair.Key);
      for (var i = 0; i < _deadFields.Count; i++) entry.fields.Remove(_deadFields[i]);
    }

    private static OverlayEntryModel Entry(VisualElement element) => (element as OverlayCardElement)?.Entry;
  }
}
