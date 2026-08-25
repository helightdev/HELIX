using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Prose {
  public readonly struct ComposeProseFieldPart {
    public ComposeProseFieldPart(IProseScope scope, Composable content) {
      Scope = scope;
      Content = content;
    }
    public IProseScope Scope { get; }
    public Composable Content { get; }
  }

  public delegate bool ComposeProseFieldFactory(
    IProseField field,
    object formatter,
    IReadOnlyList<ComposeProseFieldPart> parts,
    IReadOnlyList<IProseModifier> modifiers,
    out Composable result
  );

  /// <summary>Delegates only typed field scopes to a separately configurable field reducer.</summary>
  public sealed class ComposeProseFieldHandler : IProseScopeHandler<Composable> {
    private readonly ComposeProseFieldReducer _reducer;

    public ComposeProseFieldHandler(ComposeProseFieldReducer reducer) =>
      _reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));

    public bool TryCreate(IProseScope scope, out IProseWriter writer) {
      if (scope is not IProseField) {
        writer = null;
        return false;
      }
      writer = new ReducingProseWriter<ComposeProseFieldPart>(_reducer);
      return true;
    }

    public Composable Finish(IProseWriter writer) {
      if (writer is not ReducingProseWriter<ComposeProseFieldPart> fieldWriter)
        throw new ArgumentException("The field handler can only finish its own writer.", nameof(writer));
      return fieldWriter.Build().Content;
    }
  }

  /// <summary>Maps field decorations to Compose content and dispatches the field to a typed factory.</summary>
  public sealed class ComposeProseFieldReducer : ProseReducer<ComposeProseFieldPart> {
    private readonly List<ComposeProseFieldFactory> _factories = new();
    private readonly ComposeProseReducer _compose;

    public ComposeProseFieldReducer(ComposeProseReducer compose = null) =>
      _compose = compose ?? new ComposeProseReducer();

    public ComposeProseFieldReducer Add(ComposeProseFieldFactory factory) {
      if (factory == null) throw new ArgumentNullException(nameof(factory));
      _factories.Add(factory);
      return this;
    }

    public override bool TryMap(
      IProse prose, IReadOnlyList<IProseModifier> modifiers, out ComposeProseFieldPart result
    ) {
      var mapped = _compose.TryMap(prose, modifiers, out var content);
      result = new ComposeProseFieldPart(null, content);
      return mapped;
    }

    public override bool TryMap(
      string text, IReadOnlyList<IProseModifier> modifiers, out ComposeProseFieldPart result
    ) {
      var mapped = _compose.TryMap(text, modifiers, out var content);
      result = new ComposeProseFieldPart(null, content);
      return mapped;
    }

    public override bool TryMap<T>(
      T value, IDatatype<T> datatype, IReadOnlyList<IProseModifier> modifiers,
      out ComposeProseFieldPart result
    ) {
      var mapped = _compose.TryMap(value, datatype, modifiers, out var content);
      result = new ComposeProseFieldPart(null, content);
      return mapped;
    }

    public override ComposeProseFieldPart Reduce(
      IProseScope scope, IReadOnlyList<ComposeProseFieldPart> children,
      IReadOnlyList<IProseModifier> modifiers
    ) {
      if (scope is IProseField proseField) {
        for (var i = 0; i < _factories.Count; i++)
          if (_factories[i](proseField, proseField.Formatter, children, modifiers, out var field))
            return new ComposeProseFieldPart(scope, field);
        return default;
      }

      var content = ReduceContent(scope, children, modifiers);
      return new ComposeProseFieldPart(scope, content);
    }

    public override ComposeProseFieldPart Reduce(IReadOnlyList<ComposeProseFieldPart> children) =>
      new(null, ReduceContent(null, children, Array.Empty<IProseModifier>()));

    public override bool IsEmpty(ComposeProseFieldPart value) => value.Content == null;

    private Composable ReduceContent(
      IProseScope scope, IReadOnlyList<ComposeProseFieldPart> children,
      IReadOnlyList<IProseModifier> modifiers
    ) {
      if (children.Count == 0) return null;
      var content = new List<Composable>(children.Count);
      for (var i = 0; i < children.Count; i++)
        if (children[i].Content != null)
          content.Add(children[i].Content);
      return scope == null ? _compose.Reduce(content) : _compose.Reduce(scope, content, modifiers);
    }
  }

  /// <summary>Conventional field factories. Applications can replace or extend these per value type.</summary>
  public static class ComposeProseFieldFactories {
    public static bool Standard(
      IProseField field, object formatter, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, out Composable result,
      InspectorLayoutSlots layoutSlots = default
    ) {
      if (formatter is ICollectionDatatype collection)
        return Collection(field, collection, parts, modifiers, layoutSlots, out result);
      if (formatter is ICompositeDatatype composite)
        return Composite(field, composite, parts, modifiers, layoutSlots, out result);
      if (formatter is IDatatypeChoice choices)
        return Choice(field, choices, parts, modifiers, layoutSlots, out result);
      if (formatter is IDatatype<string> && field is ProseField<string> text)
        return Text(text, parts, modifiers, layoutSlots, out result);
      if (formatter is IDatatype<int> && field is ProseField<int> integer)
        return Integer(integer, parts, modifiers, layoutSlots, out result);
      if (formatter is IDatatype<float> && field is ProseField<float> number)
        return Float(number, parts, modifiers, layoutSlots, out result);
      if (formatter is IDatatype<bool> && field is ProseField<bool> toggle)
        return Checkbox(toggle, parts, modifiers, layoutSlots, out result);
      result = null;
      return false;
    }

    private static bool Composite(
      IProseField field, ICompositeDatatype formatter, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, InspectorLayoutSlots layoutSlots, out Composable result
    ) {
      if (string.IsNullOrEmpty(field.Path)) { result = null; return false; }
      var controlDatatype = new UntypedCompositeDatatype(formatter);
      Composable control = (ref Composition cx) => {
        var formField = cx.Lookup<HXFormField>();
        cx.Spec(new ControlSpec<object>(
          formField.Value, controlDatatype, SetComposite, FinishEditing,
          IsEnabled(formField), HasError(formField)
        ));
        cx.CURSOR.Flexible();
      };
      result = Field(field.Path, field.Name, FormController.NoInitialValue, parts, modifiers, control, layoutSlots);
      return true;
    }

    private static bool Collection(
      IProseField field, ICollectionDatatype formatter, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, InspectorLayoutSlots layoutSlots, out Composable result
    ) {
      if (string.IsNullOrEmpty(field.Path)) { result = null; return false; }
      var controlDatatype = new UntypedCollectionDatatype(formatter);
      Composable control = (ref Composition cx) => {
        var formField = cx.Lookup<HXFormField>();
        cx.Spec(new ControlSpec<object>(
          formField.Value, controlDatatype, SetComposite, FinishEditing,
          IsEnabled(formField), HasError(formField)
        ));
        cx.CURSOR.Flexible();
      };
      result = Field(field.Path, field.Name, FormController.NoInitialValue, parts, modifiers, control, layoutSlots);
      return true;
    }

    private static bool Choice(
      IProseField field, IDatatypeChoice formatter, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, InspectorLayoutSlots layoutSlots, out Composable result
    ) {
      if (string.IsNullOrEmpty(field.Path) || formatter.ChoiceCount == 0) {
        result = null;
        return false;
      }
      var controlFormatter = new UntypedIDatatypeChoiceDatatype(formatter);
      Composable control = (ref Composition cx) => {
        var formField = cx.Lookup<HXFormField>();
        cx.Spec(new ControlSpec<object>(
          formField.Value,
          controlFormatter,
          SetChoice,
          enabled: IsEnabled(formField),
          error: HasError(formField)
        ));
        cx.CURSOR.Flexible();
      };
      result = Field(field.Path, field.Name, FormController.NoInitialValue, parts, modifiers, control, layoutSlots);
      return true;
    }

    private static bool Text(
      ProseField<string> field, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, InspectorLayoutSlots layoutSlots, out Composable result
    ) {
      if (string.IsNullOrEmpty(field.Path)) { result = null; return false; }
      Composable control = (ref Composition cx) => {
        var formField = cx.Lookup<HXFormField>();
        cx.Spec(new ControlSpec<string>(
          formField.GetValue(string.Empty), field.Datatype, SetText, FinishEditing,
          IsEnabled(formField), HasError(formField)
        ));
        cx.CURSOR.Flexible();
      };
      result = Field(field, parts, modifiers, control, layoutSlots);
      return true;
    }

    private static bool Integer(
      ProseField<int> field, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, InspectorLayoutSlots layoutSlots, out Composable result
    ) {
      if (string.IsNullOrEmpty(field.Path)) { result = null; return false; }
      Composable control = (ref Composition cx) => {
        var formField = cx.Lookup<HXFormField>();
        cx.Spec(new ControlSpec<int>(
          formField.GetValue(0), field.Datatype, SetInteger, FinishEditing,
          IsEnabled(formField), HasError(formField)
        ));
        cx.CURSOR.Flexible();
      };
      result = Field(field, parts, modifiers, control, layoutSlots);
      return true;
    }

    private static bool Float(
      ProseField<float> field, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, InspectorLayoutSlots layoutSlots, out Composable result
    ) {
      if (string.IsNullOrEmpty(field.Path)) { result = null; return false; }
      Composable control = (ref Composition cx) => {
        var formField = cx.Lookup<HXFormField>();
        cx.Spec(new ControlSpec<float>(
          formField.GetValue(0f), field.Datatype, SetFloat, FinishEditing,
          IsEnabled(formField), HasError(formField)
        ));
        cx.CURSOR.Flexible();
      };
      result = Field(field, parts, modifiers, control, layoutSlots);
      return true;
    }

    private static bool Checkbox(
      ProseField<bool> field, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, InspectorLayoutSlots layoutSlots, out Composable result
    ) {
      if (string.IsNullOrEmpty(field.Path)) { result = null; return false; }
      Composable control = (ref Composition cx) => {
        var formField = cx.Lookup<HXFormField>();
        cx.Spec(new ControlSpec<bool>(
          formField.GetValue(false), field.Datatype, SetBool,
          enabled: IsEnabled(formField), error: HasError(formField)
        ));
      };
      result = Field(field, parts, modifiers, control, layoutSlots);
      return true;
    }

    private static Composable Field<T>(
      ProseField<T> field, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, Composable control, InspectorLayoutSlots layoutSlots
    ) {
      var defaultValue = Default(field);
      return Field(
        field.Path,
        field.Name,
        defaultValue.hasValue ? defaultValue.value : FormController.NoInitialValue,
        parts,
        modifiers,
        control,
        layoutSlots
      );
    }

    private static Composable Field(
      string path, string name, object initialValue, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, Composable control, InspectorLayoutSlots layoutSlots
    ) {
      var decorators = Decorators(parts);
      if (decorators.label == null) decorators = new FormFieldDecorators(
        (ref Composition cx) => cx.Text(name), decorators.description, decorators.prefix, decorators.suffix,
        decorators.before, decorators.between, decorators.after
      );
      var labelWidth = new Length(35f, LengthUnit.Percent);
      var fullWidth = false;
      var hideName = false;
      for (var i = 0; i < modifiers.Count; i++) {
        if (modifiers[i] is ProseFieldLabelWidthModifier width) labelWidth = width.Width;
        else if (modifiers[i] is ProseFullWidthModifier) fullWidth = true;
        else if (modifiers[i] is ProseHideNameModifier) hideName = true;
      }

      Composable value = (ref Composition cx) => {
        decorators.prefix?.Invoke(ref cx);
        control(ref cx);
        decorators.suffix?.Invoke(ref cx);
      };
      Composable content = (ref Composition cx) => {
        decorators.before?.Invoke(ref cx);
        cx.Spec(new InspectorLayout(
          decorators.label, value, layoutSlots, labelWidth,
          stacked: fullWidth || hideName,
          hideName: hideName
        ));
        decorators.between?.Invoke(ref cx);
        decorators.description?.Invoke(ref cx);
        decorators.after?.Invoke(ref cx);
      };
      return (ref Composition cx) => cx.FormField(
        path,
        content,
        style: new HXFormFieldStyle(layout: Passthrough),
        initialValue: initialValue
      );
    }

    private static void Passthrough(
      ref Composition cx, Composable field, in FormFieldDecorators decorators
    ) => field(ref cx);

    private static void SetText(CompositionContext context, string value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void SetInteger(CompositionContext context, int value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void SetFloat(CompositionContext context, float value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void SetBool(CompositionContext context, bool value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void SetChoice(CompositionContext context, object value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void SetComposite(CompositionContext context, object value) =>
      context.Lookup<HXFormField>()?.SetUserValue(value);
    private static void FinishEditing(CompositionContext context) =>
      context.Lookup<HXFormField>()?.MarkFinishedEditing();

    private static bool IsEnabled(HXFormField field) =>
      field.FieldData?.HasFlag(FieldFlags.Disabled) != true;
    private static bool HasError(HXFormField field) =>
      field.FieldData?.HasFlag(FieldFlags.Error) == true;

    private static HXOptional<T> Default<T>(ProseField<T> field) =>
      field.Datatype is IDatatypeDefault<T> value && value.HasDefaultValue
        ? new HXOptional<T>(value.DefaultValue)
        : HXOptional<T>.None;

    private static FormFieldDecorators Decorators(IReadOnlyList<ComposeProseFieldPart> parts) {
      Composable label = null, description = null, prefix = null, suffix = null;
      Composable before = null, between = null, after = null;
      for (var i = 0; i < parts.Count; i++) {
        if (parts[i].Scope is not ProseFieldPart part) continue;
        switch (part.Kind) {
          case ProseFieldPartKind.Label: label = parts[i].Content; break;
          case ProseFieldPartKind.Description: description = parts[i].Content; break;
          case ProseFieldPartKind.Prefix: prefix = parts[i].Content; break;
          case ProseFieldPartKind.Suffix: suffix = parts[i].Content; break;
          case ProseFieldPartKind.Before: before = parts[i].Content; break;
          case ProseFieldPartKind.Between: between = parts[i].Content; break;
          case ProseFieldPartKind.After: after = parts[i].Content; break;
        }
      }
      return new FormFieldDecorators(label, description, prefix, suffix, before, between, after);
    }

    private sealed class UntypedIDatatypeChoiceDatatype :
      IDatatype<object>, IDatatypeChoice, IDatatypeAffix, IControlDatatypeWrapper {
      private readonly IDatatypeChoice _formatter;

      public UntypedIDatatypeChoiceDatatype(IDatatypeChoice formatter) => _formatter = formatter;
      object IControlDatatypeWrapper.Datatype => _formatter;
      public int ChoiceCount => _formatter.ChoiceCount;
      public object GetChoiceValue(int index) => _formatter.GetChoiceValue(index);
      public string GetChoiceLabel(int index) => _formatter.GetChoiceLabel(index);
      public bool IsChoiceEnabled(int index) => _formatter.IsChoiceEnabled(index);
      public string Prefix => (_formatter as IDatatypeAffix)?.Prefix;
      public string Suffix => (_formatter as IDatatypeAffix)?.Suffix;

      public void ToProse(IProseWriter writer, object value) {
        for (var i = 0; i < ChoiceCount; i++) {
          if (!Equals(GetChoiceValue(i), value)) continue;
          writer.Write(GetChoiceLabel(i));
          return;
        }
        writer.Write(value?.ToString() ?? "null");
      }
    }

    private sealed class UntypedCompositeDatatype : IDatatype<object>, ICompositeDatatype {
      private readonly ICompositeDatatype _datatype;
      public UntypedCompositeDatatype(ICompositeDatatype datatype) => _datatype = datatype;
      public int ComponentCount => _datatype.ComponentCount;
      public string GetComponentName(int index) => _datatype.GetComponentName(index);
      public Type GetComponentType(int index) => _datatype.GetComponentType(index);
      public object GetComponentDatatype(int index) => _datatype.GetComponentDatatype(index);
      public object GetComponentValue(object value, int index) => _datatype.GetComponentValue(value, index);
      public object SetComponentValue(object value, int index, object componentValue) =>
        _datatype.SetComponentValue(value, index, componentValue);
      public void WriteComponent(IProseWriter writer, object value, int index) =>
        _datatype.WriteComponent(writer, value, index);
      public void ToProse(IProseWriter writer, object value) {
        for (var i = 0; i < ComponentCount; i++) {
          if (i != 0) writer.Write(", ");
          WriteComponent(writer, value, i);
        }
      }
    }

    private sealed class UntypedCollectionDatatype : IDatatype<object>, ICollectionDatatype {
      private readonly ICollectionDatatype _datatype;
      public UntypedCollectionDatatype(ICollectionDatatype datatype) => _datatype = datatype;
      public object ItemDatatype => _datatype.ItemDatatype;
      public ICollectionProxy CollectionProxy => _datatype.CollectionProxy;
      public int ComponentCount => 0;
      public string GetComponentName(int index) => _datatype.GetComponentName(index);
      public Type GetComponentType(int index) => _datatype.GetComponentType(index);
      public object GetComponentDatatype(int index) => _datatype.GetComponentDatatype(index);
      public object GetComponentValue(object value, int index) => _datatype.GetComponentValue(value, index);
      public object SetComponentValue(object value, int index, object componentValue) =>
        _datatype.SetComponentValue(value, index, componentValue);
      public void WriteComponent(IProseWriter writer, object value, int index) =>
        _datatype.WriteComponent(writer, value, index);
      public void ToProse(IProseWriter writer, object value) {
        var count = value == null ? 0 : CollectionProxy.GetItemCount(value);
        for (var i = 0; i < count; i++) {
          if (i != 0) writer.Write(", ");
          WriteComponent(writer, value, i);
        }
      }
    }
  }
}
