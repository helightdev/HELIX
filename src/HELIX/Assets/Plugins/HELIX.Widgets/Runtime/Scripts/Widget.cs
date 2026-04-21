using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
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
    /// <para>An array of constant objects associated with the widget.</para>
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
    /// </summary>
    /// <seealso cref="ModifierExtensions.Const"/>
    /// <seealso cref="Reconciler.MaybeReconcile"/>
    public object[] constants;

    /// <summary>
    /// <para>A unique identifier for the widget instance.</para>
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
    /// <para>
    /// An effectively immutable collection of modifications applied to a
    /// widget's underlying <see cref="VisualElement"/>.
    /// They can manipulate the element's style, layout, properties, and behavior generically.
    /// </para>
    /// <para>
    /// Modifiers may be added to a widget using the <see cref="modifiers"/> constructor parameter, the
    /// <see cref="AddModifier"/> or <see cref="AddModifiers"/> methods, or by using the <see cref="ModifierExtensions"/>.
    /// </para>
    /// </summary>
    /// <seealso cref="Modifier"/>
    protected ModifierSet modifiers = ModifierSet.Empty;


    /// <summary>
    /// Base constructor for a widget. See <see cref="key"/> and <see cref="constants"/> and <see cref="modifiers"/>
    /// for more information on the constructor parameters.
    /// </summary>
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

    public abstract IWidgetElement CreateElement();

    public ModifierSet GetModifiers() {
      return modifiers;
    }

    /// <summary>
    /// Adds a modifier to the widget.
    /// </summary>
    public void AddModifier(Modifier modifier) {
      if (modifiers.ReadOnly) modifiers = new ModifierSet(modifiers);
      modifiers.AddThrowing(modifier);
    }

    /// <summary>
    /// Adds a sequence of modifiers to the widget.
    /// </summary>
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

  /// <summary>
  /// Base class for a <see cref="Widget"/> that contains at most one child widget.
  /// </summary>
  public abstract class SingleChildWidget : Widget, IEnumerable<Widget> {
    public Widget child;


    /// <summary>
    /// Base constructor for a widget with a single child.
    /// </summary>
    /// <remarks>
    /// You may use this widget with a collection initializer to specify the child.
    /// </remarks>
    /// <inheritdoc/>
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

  /// <summary>
  /// Base class for a <see cref="Widget"/> that can contain multiple child widgets.
  /// </summary>
  public abstract class MultiChildWidget : Widget, IReadOnlyList<Widget> {
    public IReadOnlyList<Widget> children;

    /// <remarks>
    /// You may use this widget with a collection initializer to specify the children.
    /// </remarks>
    /// <inheritdoc/>
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

  /// <summary>
  /// A <see cref="VisualElement"/>s that is associated with a <see cref="Widget"/>.
  /// Most <see cref="BuildContext"/> implementations implement this interface.
  /// </summary>
  public interface IWidgetElement : BuildContext {
    int HierarchyDepth { get; }
    bool CanReconcile(Widget updated);
    bool Reconcile(Widget updated);
  }

  /// <summary>
  /// Provides a hint to the <see cref="Reconciler"/> that the target is a part of <see cref="ITreeAncestorTraversalHint.Owner"/>.
  /// <para>
  /// Some widgets and modifiers try to resolve their parent using <see cref="BuildContext.GetDirectParent"/> to access
  /// metadata like <see cref="IPreferExplicitFlex"/> or <see cref="IPreferStacking"/>. In cases where
  /// <see cref="BuildContext.ParentContext"/> is not available, the <see cref="Reconciler"/> will attempt to traverse
  /// the tree upwards once and check if the element is a <see cref="IWidgetElement"/> or provides a traversal hint
  /// via direct implementation or by setting a <see cref="ElementTreeAncestorTraversalHint"/> in its <see cref="VisualElement.userData"/>.
  /// </para>
  /// </summary>
  public interface ITreeAncestorTraversalHint {
    IWidgetElement Owner { get; }
  }

  /// <summary>
  /// Can be set as a <see cref="VisualElement.userData"/> to hint to the <see cref="Reconciler"/> that the element
  /// is a part of <see cref="ElementTreeAncestorTraversalHint.Owner"/>.
  /// </summary>
  public class ElementTreeAncestorTraversalHint : ITreeAncestorTraversalHint {
    public ElementTreeAncestorTraversalHint(IWidgetElement owner) {
      Owner = owner;
    }

    public IWidgetElement Owner { get; }
  }

  public static class WidgetExtensions {
    /// <summary>
    /// Converts a <see cref="Widget"/> instance to an <see cref="IBuildable"/>.
    /// </summary>
    public static IBuildable ToBuildable(this Widget widget) {
      return new FunctionBuildable(_ => widget);
    }

    /// <summary>
    /// Converts a <see cref="Widget"/> instance to an <see cref="ElementFactory"/>.
    /// </summary>
    public static ElementFactory<VisualElement> ToFactory(this Widget widget, Action<VisualElement> apply = null) {
      return new InlineElementFactory<VisualElement>(_ => {
          var element = new WidgetHostElement { Buildable = widget.ToBuildable() };
          apply?.Invoke(element);
          return element;
        }
      );
    }

    /// <summary>
    /// Converts a generic <see cref="ElementFactory"/> to a <see cref="Widget"/>.
    /// </summary>
    public static Widget ToWidget(this ElementFactory factory) {
      return new FactoryWidget<VisualElement> { creator = () => factory.Create(null) };
    }

    public static InformationCollector WithSpace(this InformationCollector collector) {
      collector.Add(new ErrorSpacer());
      return collector;
    }

    /// <summary>
    /// Adds information about the offending widget to the collector, including a spacer before for better readability.
    /// </summary>
    public static InformationCollector OffendingWidget(this InformationCollector collector, Widget widget) {
      collector.AddRange(new ErrorSpacer(), new ErrorProperty("The offending widget was", widget));
      return collector;
    }

    /// <summary>
    /// Adds information about the offending element to the collector, including a spacer before for better readability.
    /// </summary>
    public static InformationCollector OffendingElement(
      this InformationCollector collector,
      IWidgetElement widget
    ) {
      if (widget == null) return collector;
      collector.AddRange(new ErrorSpacer(), new ErrorProperty("The offending element was", widget));
      return collector;
    }

    /// <summary>
    /// Adds information about this context's owner chain to the collector, including a spacer before for better readability.
    /// </summary>
    public static InformationCollector OwnerChain(this InformationCollector collector, BuildContext context) {
      collector.AddRange(new ErrorSpacer(), OwnershipChainErrorProperty.FromBuildContext(context));
      return collector;
    }

    /// <summary>
    /// Resolves the value of the given <see cref="ThemeProperty{T}"/> using the given <see cref="IThemeProvider"/>.
    /// If the context is null, global theme values will be used as a fallback before
    /// resorting to the default value of the property.
    /// </summary>
    /// <param name="property">The property that is to be resolved.</param>
    /// <param name="context">The theme provider to use for resolution.</param>
    /// <param name="listen">Whether to listen for theme changes if the <paramref name="context"/> supports it.</param>
    /// <returns>The resolved value of the property.</returns>
    /// <remarks>
    /// The returned value may be invalid for struct types. Use <see cref="TryGet"/> in cases where you need to
    /// be sure that the value is valid and intentionally assigned.
    /// </remarks>
    public static T Get<T>(this ThemeProperty<T> property, IThemeProvider context, bool listen = true) {
      if (context != null) return context.GetThemed(property, listen);
      return ThemeProviderElement.Resolve(null, property);
    }


    /// <summary>
    /// Attempts to resolve the value of the given <see cref="ThemeProperty{T}"/> using the given <see cref="IThemeProvider"/>.
    /// If the context is null, global theme values will be used as a fallback before
    /// resorting to the default value of the property.
    /// </summary>
    /// <param name="value">The resolved value of the property.</param>
    /// <param name="property">The property that is to be resolved.</param>
    /// <param name="context">The theme provider to use for resolution.</param>
    /// <param name="listen">Whether to listen for theme changes if the <paramref name="context"/> supports it.</param>
    /// <returns><c>true</c> if the value retrieved is valid, otherwise <c>false</c>.</returns>
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