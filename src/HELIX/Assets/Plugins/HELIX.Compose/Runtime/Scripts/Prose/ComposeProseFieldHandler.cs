using System;
using System.Collections.Generic;
using HELIX.Prose;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct ComposeProseFieldPart {
    public ComposeProseFieldPart(IProseScope scope, Composable content) {
      Scope = scope;
      Content = content;
    }

    public IProseScope Scope { get; }
    public Composable Content { get; }
  }

  /// <summary>
  ///   Everything a field's prose said about its presentation, already reduced to composables. A factory
  ///   reads what it cares about, overwrites what it wants to change, and either assembles the field
  ///   itself or hands the decoration to <see cref="ComposeProseFieldFactories.Default" />.
  /// </summary>
  public struct ProseFieldDecoration {
    public Composable Label, Description, Tooltip, Prefix, Suffix, Before, Between, After;
    public Composable NameStart, NameEnd, ValueStart, ValueEnd;
    public Length? LabelWidth;
    public bool FullWidth, HideName;

    /// <summary>The field frame's modifiers, for factories that define their own.</summary>
    public IReadOnlyList<IProseModifier> Modifiers;

    public InspectorLayoutSlots Slots() => new(NameStart, NameEnd, ValueStart, ValueEnd);

    /// <summary>The last modifier of the given kind, or null when the field declared none.</summary>
    public TModifier Modifier<TModifier>() where TModifier : class, IProseModifier {
      if (Modifiers == null) return null;
      for (var i = Modifiers.Count - 1; i >= 0; i--)
        if (Modifiers[i] is TModifier match)
          return match;
      return null;
    }
  }

  /// <summary>
  ///   Turns a field and its decoration into content. Returning false declines the field and lets the next
  ///   factory — and finally <see cref="ComposeProseFieldFactories.Default" /> — handle it, keeping any
  ///   change the declining factory made to the decoration.
  /// </summary>
  public delegate bool ComposeProseFieldFactory(
    IProseField field,
    ref ProseFieldDecoration decoration,
    out Composable result
  );

  /// <summary>Delegates only typed field scopes to a separately configurable field reducer.</summary>
  public sealed class ComposeProseFieldHandler : IProseScopeHandler<Composable> {
    private readonly ComposeProseFieldReducer _reducer;
    private readonly Stack<ReducingProseWriter<ComposeProseFieldPart>> _pool = new();

    public ComposeProseFieldHandler(ComposeProseFieldReducer reducer) {
      _reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));
    }

    public bool TryCreate<T>(T scope, out IProseWriter writer) where T : IProseScope {
      if (scope is not IProseField) {
        writer = null;
        return false;
      }
      writer = _pool.Count > 0
        ? _pool.Pop()
        : new ReducingProseWriter<ComposeProseFieldPart>(_reducer);
      return true;
    }

    public Composable Finish(IProseWriter writer) {
      if (writer is not ReducingProseWriter<ComposeProseFieldPart> fieldWriter)
        throw new ArgumentException("The field handler can only finish its own writer.", nameof(writer));
      var content = fieldWriter.Build().Content;
      fieldWriter.Reset();
      _pool.Push(fieldWriter);
      return content;
    }
  }

  /// <summary>Maps field decorations to Compose content and dispatches the field to a typed factory.</summary>
  public sealed class ComposeProseFieldReducer : ProseReducer<ComposeProseFieldPart> {
    private readonly ComposeProseReducer _compose;
    private readonly List<ComposeProseFieldFactory> _factories = new();

    public ComposeProseFieldReducer(ComposeProseReducer compose = null) {
      _compose = compose ?? new ComposeProseReducer();
    }

    public ComposeProseFieldReducer Add(ComposeProseFieldFactory factory) {
      if (factory == null) throw new ArgumentNullException(nameof(factory));
      _factories.Add(factory);
      return this;
    }

    public override bool TryMap<TProse>(
      TProse prose,
      IReadOnlyList<IProseModifier> modifiers,
      out ComposeProseFieldPart result
    ) {
      var mapped = _compose.TryMap(prose, modifiers, out var content);
      result = new ComposeProseFieldPart(null, content);
      return mapped;
    }

    public override bool TryMap(
      string text,
      IReadOnlyList<IProseModifier> modifiers,
      out ComposeProseFieldPart result
    ) {
      var mapped = _compose.TryMap(text, modifiers, out var content);
      result = new ComposeProseFieldPart(null, content);
      return mapped;
    }

    public override bool TryMap<T>(
      T value,
      IDatatype<T> datatype,
      IReadOnlyList<IProseModifier> modifiers,
      out ComposeProseFieldPart result
    ) {
      var mapped = _compose.TryMap(value, datatype, modifiers, out var content);
      result = new ComposeProseFieldPart(null, content);
      return mapped;
    }

    public override ComposeProseFieldPart Reduce(
      IProseScope scope,
      IReadOnlyList<ComposeProseFieldPart> children,
      IReadOnlyList<IProseModifier> modifiers
    ) {
      if (scope is not IProseField proseField) {
        var content = ReduceContent(scope, children, modifiers);
        return new ComposeProseFieldPart(scope, content);
      }

      var decoration = Decoration(children, modifiers);
      for (var i = 0; i < _factories.Count; i++)
        if (_factories[i](proseField, ref decoration, out var custom))
          return new ComposeProseFieldPart(scope, custom);

      return ComposeProseFieldFactories.Default(proseField, ref decoration, out var field)
        ? new ComposeProseFieldPart(scope, field)
        : default;
    }

    public override ComposeProseFieldPart Reduce(IReadOnlyList<ComposeProseFieldPart> children) {
      return new ComposeProseFieldPart(null, ReduceContent(null, children, Array.Empty<IProseModifier>()));
    }

    public override bool IsEmpty(ComposeProseFieldPart value) {
      return value.Content == null;
    }

    /// <summary>Collects the part scopes and modifiers a field's prose produced. The one place that reads them.</summary>
    private static ProseFieldDecoration Decoration(
      IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers
    ) {
      var decoration = new ProseFieldDecoration { Modifiers = modifiers };
      for (var i = 0; i < parts.Count; i++) {
        if (parts[i].Scope is not ProseFieldPart part) continue;
        var content = parts[i].Content;
        switch (part.Kind) {
          case ProseFieldPartKind.Label: decoration.Label = content; break;
          case ProseFieldPartKind.Description: decoration.Description = content; break;
          case ProseFieldPartKind.Tooltip: decoration.Tooltip = content; break;
          case ProseFieldPartKind.Prefix: decoration.Prefix = content; break;
          case ProseFieldPartKind.Suffix: decoration.Suffix = content; break;
          case ProseFieldPartKind.Before: decoration.Before = content; break;
          case ProseFieldPartKind.Between: decoration.Between = content; break;
          case ProseFieldPartKind.After: decoration.After = content; break;
        }
      }
      for (var i = 0; i < modifiers.Count; i++)
        switch (modifiers[i]) {
          case ProseFieldLabelWidthModifier width: decoration.LabelWidth = width.Width; break;
          case ProseFullWidthModifier: decoration.FullWidth = true; break;
          case ProseHideNameModifier: decoration.HideName = true; break;
        }
      return decoration;
    }

    private Composable ReduceContent(
      IProseScope scope,
      IReadOnlyList<ComposeProseFieldPart> children,
      IReadOnlyList<IProseModifier> modifiers
    ) {
      if (children.Count == 0) return null;
      var content = new List<Composable>(children.Count);
      for (var i = 0; i < children.Count; i++) {
        if (children[i].Content != null)
          content.Add(children[i].Content);
      }
      return scope == null ? _compose.Reduce(content) : _compose.Reduce(scope, content, modifiers);
    }
  }

  /// <summary>
  ///   The conventional field assembly: a two-column inspector row registered as a form field. The control
  ///   itself comes from the field's own datatype through <see cref="SpecConfig" />, so adding a value type
  ///   means registering a spec handler and nothing here.
  /// </summary>
  public static class ComposeProseFieldFactories {
    public static bool Default(
      IProseField field,
      ref ProseFieldDecoration decoration,
      out Composable result
    ) {
      if (field == null) throw new ArgumentNullException(nameof(field));
      if (string.IsNullOrEmpty(field.Path)) {
        result = null;
        return false;
      }
      var control = field.CreateControl();
      if (control == null) {
        result = null;
        return false;
      }

      var label = decoration.Label ?? Text(field.Name);
      var prefix = decoration.Prefix;
      var suffix = decoration.Suffix;
      Composable value = prefix == null && suffix == null
        ? control
        : (ref Composition cx) => {
          prefix?.Invoke(ref cx);
          control(ref cx);
          suffix?.Invoke(ref cx);
        };

      var layout = new InspectorLayout(
        label,
        value,
        decoration.Slots(),
        decoration.LabelWidth,
        decoration.FullWidth || decoration.HideName,
        decoration.HideName
      );

      var before = decoration.Before;
      var between = decoration.Between;
      var description = decoration.Description;
      var after = decoration.After;
      Composable content = (ref Composition cx) => {
        before?.Invoke(ref cx);
        cx.Spec(in layout);
        between?.Invoke(ref cx);
        description?.Invoke(ref cx);
        after?.Invoke(ref cx);
      };

      var path = field.Path;
      var initialValue = field.TryGetDefaultValue(out var declared)
        ? declared
        : FormController.NoInitialValue;
      result = (ref Composition cx) => cx.FormField(
        path,
        content,
        // The row above already carries label, description and affixes, so the decorator layout would
        // render them a second time. Field decoration happens here, not in HXDecorator.
        new HXFormFieldStyle(Passthrough),
        initialValue: initialValue
      );
      return true;
    }

    /// <summary>A plain text composable, for a field that supplied no label prose.</summary>
    public static Composable Text(string text) => (ref Composition cx) => cx.Text(text);

    private static void Passthrough(
      ref Composition cx,
      Composable field,
      in FormFieldDecorators decorators
    ) {
      field(ref cx);
    }
  }
}
