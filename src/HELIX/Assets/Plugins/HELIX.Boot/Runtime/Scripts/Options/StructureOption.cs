using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Context;
using HELIX.Prose;
using UnityEngine;

namespace HELIX.Boot {
  /// <summary>
  ///   An option whose value is a generated structure. Every property becomes a form field below the
  ///   option's own path, so a <c>[Structure(datatype: true)]</c> type is a settings group without the
  ///   properties being declared a second time as individual options.
  /// </summary>
  /// <remarks>
  ///   Labels and descriptions come from the datatype, which takes them from the declaration's
  ///   documentation comment, so nothing here needs annotating for the page to read well.
  /// </remarks>
  public class StructureOption<T> : Option {
    public readonly StructureDatatype<T> datatype;
    public T defaultValue;
    public T value;
    public bool defaultResettable = true;

    public StructureOption(StructureDatatype<T> datatype, T defaultValue = default) {
      this.datatype = datatype ?? throw new ArgumentNullException(nameof(datatype));
      this.defaultValue = defaultValue;
      value = defaultValue;
    }

    public void SetInferredName(string inferred) {
      path ??= inferred;
      name = Casing.Convert(inferred, Casing.CaseStyle.TitleCase);
      path = Casing.Convert(inferred, Casing.CaseStyle.DomainCase);
      SetGroupPath();
    }

    public void SetInferredPath(string inferred) {
      path ??= inferred;
      if (string.IsNullOrEmpty(name)) name = Casing.Convert(inferred, Casing.CaseStyle.TitleCase);
      SetGroupPath();
    }

    private void SetGroupPath() {
      var separator = path.LastIndexOf('.');
      groupPath = separator < 0 ? "" : path.Substring(0, separator);
    }

    public override void ToProse(IProseWriter writer) => writer.WriteStructure(datatype, path);

    public override void LoadInto(FormController controller) =>
      Each(new Load(controller, path, FormChangeReason.Programmatic), ref value);

    public override void LoadDefaultInto(FormController controller) =>
      Each(new Load(controller, path, FormChangeReason.User), ref defaultValue);

    public override void LoadDefaultInto(FormController controller, string fieldPath) {
      var property = Property(fieldPath);
      if (property == null) {
        LoadDefaultInto(controller);
        return;
      }
      var defaults = defaultValue;
      var visitor = new Load(controller, path, FormChangeReason.User);
      property.Visit(ref defaults, ref visitor);
    }

    public override void AcceptFrom(FormController controller) =>
      Each(new Accept(controller, path), ref value);

    public override bool IsNonDefaultAt(FormController controller, string fieldPath) {
      var property = Property(fieldPath);
      if (property == null) return false;
      var defaults = defaultValue;
      var visitor = new CompareToDefault(controller, path);
      property.Visit(ref defaults, ref visitor);
      return visitor.Differs;
    }

    public override void LoadData() => Each(new LoadPrefs(path), ref value);
    public override void SaveData() => Each(new SavePrefs(path), ref value);

    public override void Apply() => new StructureOptionApplyEvent<T> { option = this }.Raise();
    public override object CaptureValue() => value;
    public override void RestoreValue(object captured) => value = (T)captured;

    public override bool IsDefaultValue(object candidate) =>
      candidate is T typed && EqualityComparer<T>.Default.Equals(typed, defaultValue);

    private void Each<TVisitor>(TVisitor visitor, ref T structure)
    where TVisitor : struct, IStructurePropertyVisitor<T> {
      for (var i = 0; i < datatype.Properties.Count; i++)
        datatype.Properties[i].Visit(ref structure, ref visitor);
    }

    /// <summary>The property a field path names, or null when the path is not one of this option's.</summary>
    private StructurePropertyDatatype<T> Property(string fieldPath) {
      if (fieldPath == null || path == null) return null;
      if (fieldPath.Length <= path.Length + 1 || !fieldPath.StartsWith(path, StringComparison.Ordinal))
        return null;
      return datatype.GetProperty(fieldPath.Substring(path.Length + 1));
    }

    private static string Key(string prefix, string field) => prefix + "." + field;

    private readonly struct Load : IStructurePropertyVisitor<T> {
      private readonly FormController _controller;
      private readonly string _prefix;
      private readonly FormChangeReason _reason;

      internal Load(FormController controller, string prefix, FormChangeReason reason) {
        _controller = controller;
        _prefix = prefix;
        _reason = reason;
      }

      public void Visit<TValue>(ref T structure, StructurePropertyDatatype<T, TValue> property) =>
        _controller.SetValue(
          _controller.Path(Key(_prefix, property.FieldName)),
          property.GetValue(ref structure),
          _reason
        );
    }

    private readonly struct Accept : IStructurePropertyVisitor<T> {
      private readonly FormController _controller;
      private readonly string _prefix;

      internal Accept(FormController controller, string prefix) {
        _controller = controller;
        _prefix = prefix;
      }

      public void Visit<TValue>(ref T structure, StructurePropertyDatatype<T, TValue> property) {
        var formPath = _controller.Path(Key(_prefix, property.FieldName));
        if (!_controller.FieldHasValue(formPath)) return;
        property.SetValue(ref structure, _controller.GetValue(formPath, property.GetValue(ref structure)));
      }
    }

    private struct CompareToDefault : IStructurePropertyVisitor<T> {
      private readonly FormController _controller;
      private readonly string _prefix;
      internal bool Differs;

      internal CompareToDefault(FormController controller, string prefix) {
        _controller = controller;
        _prefix = prefix;
        Differs = false;
      }

      public void Visit<TValue>(ref T structure, StructurePropertyDatatype<T, TValue> property) {
        var formPath = _controller.Path(Key(_prefix, property.FieldName));
        var current = _controller.GetValue(formPath, property.GetValue(ref structure));
        Differs = !EqualityComparer<TValue>.Default.Equals(current, property.GetValue(ref structure));
      }
    }

    private readonly struct LoadPrefs : IStructurePropertyVisitor<T> {
      private readonly string _prefix;
      internal LoadPrefs(string prefix) => _prefix = prefix;

      public void Visit<TValue>(ref T structure, StructurePropertyDatatype<T, TValue> property) {
        if (property.Datatype is not IStringConvertible<TValue> convertible) {
          Debug.LogError($"No string converter for {property.Datatype} on '{property.FieldName}'.");
          return;
        }
        var key = Key(_prefix, property.FieldName);
        if (PlayerPrefs.HasKey(key))
          property.SetValue(ref structure, convertible.FromString(PlayerPrefs.GetString(key)));
      }
    }

    private readonly struct SavePrefs : IStructurePropertyVisitor<T> {
      private readonly string _prefix;
      internal SavePrefs(string prefix) => _prefix = prefix;

      public void Visit<TValue>(ref T structure, StructurePropertyDatatype<T, TValue> property) {
        if (property.Datatype is not IStringConvertible<TValue> convertible) {
          Debug.LogError($"No string converter for {property.Datatype} on '{property.FieldName}'.");
          return;
        }
        PlayerPrefs.SetString(
          Key(_prefix, property.FieldName),
          convertible.ToString(property.GetValue(ref structure))
        );
      }
    }
  }
}
