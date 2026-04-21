using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Theming;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
  /// <summary>
  /// Represents the base class for all widgets.
  /// Provides a foundation for defining user interface components, with support
  /// for diagnostics, modifiers, state reconciliation, and defining widget-specific behavior.
  /// </summary>
  public abstract class Widget : DiagnosticableTreeBase, IWidgetListCandidate {
    /// <summary>
    /// An array of constant objects associated with the widget.
    /// <para>
    /// The <c>constants</c> property is used to prevent reconciliation (rebuilding)
    /// when updating the widget tree. If the array of constants in the current widget
    /// matches the array in the previous widget during a reconciliation check,
    /// the system assumes the widgets are equivalent, and no further updates are applied.
    /// </para>
    /// <para>
    /// The exact same widget with the <b>same reference</b> is <b>always considered equivalent</b> and
    /// therefore treated as constant, using a <c>constants</c> array here is not beneficial.
    /// </para>
    /// <para>
    /// If the constants provided in two widgets do not match, the reconciliation system
    /// performs a normal rebuild of the corresponding widget subtree.
    /// </para>
    /// <br/>
    /// <seealso cref="ModifierExtensions.Const{T}(T, object[])"/>
    /// <seealso cref="Reconciler.MaybeReconcile(IWidgetElement, Widget)"/>
    /// </summary>
    public object[] constants;

    /// <summary>
    /// A unique identifier for the widget instance.
    /// <para>
    /// The <c>key</c> property is used to determine whether a widget can be reused
    /// or must be replaced during the reconciliation process. Keys also play a critical
    /// role in preserving widget states when the widget tree is updated.
    /// </para>
    /// <para>
    /// If two widgets with the same parent have keys that are equal, those widgets
    /// are considered positionally equivalent and their states can be reused if their types match
    /// and reconciliation checks are successful.
    /// </para>
    /// <para>
    /// If no key is specified, parent widgets will attempt to match children based primarily on their actual position.
    /// Insertions and removal in the middle of the widget list may lead to unintended rebuilds
    /// and state recreation as possibly valid widgets are being discarded.
    /// </para>
    /// <para>
    /// Keys may also be used to for clarity or persistent tracking of a widget's identity
    /// using a <see cref="GlobalKey"/> which is guaranteed to be unique across multiple parents and holds a reference
    /// to the underlying <see cref="IWidgetElement"/>
    /// </para>
    /// <br/>
    /// <seealso cref="HELIX.Widgets.Key"/>
    /// </summary>
    public Key key;


    /// <summary>
    /// An effectively immutable collection of attributes applied to a widget to modify its behavior or appearance.
    /// <para>
    /// The <c>modifiers</c> property allows for associating additional configuration
    /// or behavior with a widget, enabling customization of layout, styling, event handling,
    /// or other properties defined in the widget's structure.
    /// </para>
    /// </summary>
    protected ModifierSet modifiers = ModifierSet.Empty;


    /// <param name="modifiers">
    /// An optional collection of modifiers to apply to the widget.
    /// See also: <see cref="modifiers"/>
    /// </param>
    /// <param name="constants">
    /// An array of constant objects associated with the widget, used to prevent rebuilding duration reconciliation.
    /// See also: <see cref="constants"/>
    /// </param>
    /// <param name="key">
    /// An optional unique identifier for the widget instance.
    /// It is used to determine whether a widget can be reused during reconciliation. If not specified, the
    /// position of the widget in the parent's list is used to determine widget identity.
    /// See also: <see cref="key"/>
    /// </param>
    protected Widget(
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) {
      this.constants = constants;
      this.key = key;
      if (modifiers is ModifierSet set) this.modifiers = set;
      if (modifiers != null) AddModifiers(modifiers);
    }

    protected Widget() { }

    public abstract IWidgetElement CreateElement();

    public ModifierSet GetModifiers() {
      return modifiers;
    }

    public void AddModifier(Modifier modifier) {
      if (modifiers.ReadOnly) modifiers = new ModifierSet(modifiers);
      modifiers.AddThrowing(modifier);
    }

    public void AddModifiers(IEnumerable<Modifier> additions) {
      if (modifiers.ReadOnly) modifiers = new ModifierSet(modifiers);
      modifiers.AddThrowing(additions);
    }

    protected void DefaultModifiers(ModifierSet defaults, IReadOnlyCollection<Modifier> user) {
      if (user == null) {
        modifiers = defaults;
        return;
      }

      var applied = new ModifierSet(modifiers.Count + defaults.Count + user.Count);
      applied.AddCollection(modifiers);
      applied.AddCollection(defaults);
      applied.AddCollection(user);
      modifiers = applied;
    }

    public override string ToStringShort() {
      var name = GetWidgetName();
      if (key.IsNone) return name;
      return $"{name}-{key}";
    }

    public virtual string GetWidgetName() {
      return GetType().Name;
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      var visibleModifiers = new List<Modifier>(modifiers.Count);
      var fallbackModifiers = new List<Modifier>(modifiers.Count);
      foreach (var modifier in modifiers) {
        if (modifier.isFallback) fallbackModifiers.Add(modifier);
        else visibleModifiers.Add(modifier);
      }

      properties.Add(
        new IterableProperty<object>(
          "retention",
          constants,
          ifNull: null,
          identityOnly: true,
          ifEmpty: "Constant",
          level: constants == null ? DiagnosticLevel.Hidden : DiagnosticLevel.Info
        )
      );

      properties.Add(
        new IterableProperty<Modifier>(
          "modifiers",
          visibleModifiers,
          ifEmpty: null,
          level: DiagnosticLevel.Info
        )
      );

      properties.Add(
        new IterableProperty<Modifier>(
          "modifiers[fallback]",
          fallbackModifiers,
          ifEmpty: null,
          level: DiagnosticLevel.Fine
        )
      );

      base.DebugFillProperties(properties);
    }

    protected IWidgetElement ReconcileInto(IWidgetElement element) {
      element.Element.RegisterCallbackOnce<AttachToPanelEvent>(_ =>
        Reconciler.Reconcile(element, this)
      );
      return element;
    }

    public static implicit operator BuildFunction(Widget widget) {
      return _ => widget;
    }

    public static implicit operator BuildFunction<WidgetState>(Widget widget) {
      return (_, _) => widget;
    }
  }

  public abstract class SingleChildWidget : Widget, IEnumerable<Widget> {
    public Widget child;

    protected SingleChildWidget() { }

    protected SingleChildWidget(
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.child = child;
    }

    public IEnumerator<Widget> GetEnumerator() {
      if (child != null) yield return child;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public override List<DiagnosticsNode> DebugDescribeChildren() {
      return child == null
        ? new List<DiagnosticsNode>()
        : new List<DiagnosticsNode> { child.ToDiagnosticsNodeSafe() };
    }

    public void Add(Widget candidate) {
      if (child != null) throw new InvalidOperationException("SingleChildWidget already has a child");
      child = candidate;
    }
  }

  public abstract class MultiChildWidget : Widget, IReadOnlyList<Widget> {
    public IReadOnlyList<Widget> children;

    protected MultiChildWidget() { }

    protected MultiChildWidget(
      IReadOnlyList<Widget> children = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.children = children;
    }

    public IEnumerator<Widget> GetEnumerator() {
      return children?.GetEnumerator() ?? Enumerable.Empty<Widget>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public int Count => children?.Count ?? 0;

    public Widget this[int index] => children[index];

    public override List<DiagnosticsNode> DebugDescribeChildren() {
      if (children == null) return new List<DiagnosticsNode>();

      var result = new List<DiagnosticsNode>(children.Count);
      foreach (var child in children) result.Add(child.ToDiagnosticsNodeSafe());
      return result;
    }

    public void Add(IWidgetListCandidate candidate) {
      if (children is WidgetList list)
        list.Add(candidate);
      else {
        var newList = new WidgetList();
        if (children != null) newList.Add(children.Spread());
        newList.Add(candidate);
        children = newList;
      }
    }
  }

  public interface IWidgetElement : BuildContext {
    int HierarchyDepth { get; }
    bool CanReconcile(Widget updated);
    bool Reconcile(Widget updated);
  }

  public interface ITreeAncestorTraversalHint {
    IWidgetElement Owner { get; }
  }

  public class ElementTreeAncestorTraversalHint : ITreeAncestorTraversalHint {
    public ElementTreeAncestorTraversalHint(IWidgetElement owner) {
      Owner = owner;
    }

    public IWidgetElement Owner { get; }
  }

  public static class WidgetExtensions {
    public static IBuildable ToBuildable(this Widget widget) {
      return new FunctionBuildable(_ => widget);
    }

    public static ElementFactory<VisualElement> ToFactory(this Widget widget, Action<VisualElement> apply = null) {
      return new InlineElementFactory<VisualElement>(_ => {
          var element = new WidgetHostElement { Buildable = widget.ToBuildable() };
          apply?.Invoke(element);
          return element;
        }
      );
    }

    public static Widget ToWidget(this ElementFactory factory) {
      return new FactoryWidget<VisualElement> { creator = () => factory.Create(null) };
    }

    public static InformationCollector WithSpace(this InformationCollector collector) {
      collector.Add(new ErrorSpacer());
      return collector;
    }

    public static InformationCollector OffendingWidget(this InformationCollector collector, Widget widget) {
      collector.AddRange(new ErrorSpacer(), new ErrorProperty("The offending widget was", widget));
      return collector;
    }

    public static InformationCollector OffendingElement(
      this InformationCollector collector,
      IWidgetElement widget
    ) {
      if (widget == null) return collector;
      collector.AddRange(new ErrorSpacer(), new ErrorProperty("The offending element was", widget));
      return collector;
    }

    public static InformationCollector OwnerChain(this InformationCollector collector, BuildContext context) {
      collector.AddRange(new ErrorSpacer(), OwnershipChainErrorProperty.FromBuildContext(context));
      return collector;
    }

    public static T Get<T>(this ThemeProperty<T> property, IThemeProvider context, bool listen = true) {
      if (context != null) return context.GetThemed(property, listen);
      return ThemeProviderElement.Resolve(null, property);
    }

    public static bool TryGet<T>(
      this ThemeProperty<T> property,
      IThemeProvider context,
      out T value,
      bool listen = true
    ) {
      if (context != null) return context.TryGetThemed(property, out value, listen);
      return ThemeProviderElement.TryResolve(null, property, out value);
    }
  }
}