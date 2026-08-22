using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Prose;
using HELIX.Signals;
using UnityEngine.UIElements;

namespace HELIX {
  public sealed class OptionPageFieldPresentation {
    public OptionPageFieldPresentation(Composable label, Composable description) {
      Label = label;
      Description = description;
    }
    public Composable Label { get; }
    public Composable Description { get; }
  }

  public sealed class OptionPageFieldController : Signal {
    public OptionPageFieldPresentation Current { get; private set; }

    public void Show(OptionPageFieldPresentation presentation) {
      if (ReferenceEquals(Current, presentation)) return;
      Current = presentation;
      NotifyDirty();
      NotifyObservers();
    }
  }

  public readonly struct OptionPageFieldContext {
    public static readonly ContextKey<OptionPageFieldContext> Key = new("OptionPageFieldContext");
    public readonly OptionPageFieldController controller;
    public readonly FormController form;
    public OptionPageFieldContext(OptionPageFieldController controller, FormController form) {
      this.controller = controller;
      this.form = form;
    }
  }

  [BoundaryComposable(Extension = false)]
  public partial class OptionPageFieldElement {
    public partial struct Props {
      public FormController form;
      public OptionPageFieldController controller;
      [Prop(null, Equatable = false)] public OptionPageFieldPresentation presentation;
      [Prop(null)] public Composable content;
    }

    private OptionPageFieldController _controller;

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.RegisterCallback<FocusInEvent>(OnFocusIn);
    }

    protected override void OnDetach() {
      Node.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.UnregisterCallback<FocusInEvent>(OnFocusIn);
      _controller = null;
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      _controller = props.controller;
      using (cx.WriteContext(out var context)) FormContext.Key[context] = new FormContext(props.form);
      Node.style.alignSelf = Align.Stretch;
      props.content?.Invoke(ref cx);
    }

    private void OnPointerEnter(PointerEnterEvent evt) => _controller?.Show(props.presentation);
    private void OnFocusIn(FocusInEvent evt) => _controller?.Show(props.presentation);
  }

  /// <summary>Consumes option-page help prose, delegates control creation, then wraps the completed field.</summary>
  public static class OptionPageFieldFactory {
    public static bool Create(
      IProseField field, object formatter, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, out Composable result
    ) {
      var delegatedParts = new List<ComposeProseFieldPart>(parts.Count);
      Composable label = null;
      Composable description = null;
      for (var i = 0; i < parts.Count; i++) {
        var wrapped = Wrap(parts[i].Content);
        if (parts[i].Scope is ProseFieldPart { Kind: ProseFieldPartKind.Label }) label = wrapped;
        if (parts[i].Scope is ProseFieldPart { Kind: ProseFieldPartKind.Description }) {
          description = wrapped;
          continue;
        }
        delegatedParts.Add(new ComposeProseFieldPart(parts[i].Scope, wrapped));
      }
      if (!ComposeProseFieldFactories.Standard(field, formatter, delegatedParts, modifiers, out var fieldContent)) {
        result = null;
        return false;
      }
      label ??= (ref Composition cx) => cx.Text(field.Name);
      var presentation = new OptionPageFieldPresentation(label, description);
      result = (ref Composition cx) => {
        var context = cx.ReadContext(OptionPageFieldContext.Key, false);
        OptionPageFieldElement.ComposeBoundary(
          ref cx, context.form, context.controller, presentation, fieldContent
        );
      };
      return true;
    }

    private static Composable Wrap(Composable content) {
      if (content == null) return null;
      return (ref Composition cx) => {
        var parent = cx.Cell.scope.Element;
        var first = cx.Cell.cursor;
        content(ref cx);
        var last = cx.Cell.cursor;
        for (var i = first; i < last && i < parent.childCount; i++) EnableWrapping(parent.ElementAt(i));
      };
    }

    private static void EnableWrapping(VisualElement element) {
      if (element is TextElement) element.style.whiteSpace = WhiteSpace.Normal;
      for (var i = 0; i < element.childCount; i++) EnableWrapping(element.ElementAt(i));
    }
  }
}
