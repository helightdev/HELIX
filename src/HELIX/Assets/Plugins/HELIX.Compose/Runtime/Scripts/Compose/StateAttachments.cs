using System;
using HELIX.Compose.Collections;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public struct StateAttachmentStore : IDisposable {
    private RefKeyedSet<AttachedState, int> _states;

    public IDisposable GetOrNull(int key) {
      if (_states.IsEmpty) return null;
      var index = _states.FindIndex(key);
      return index == -1 ? null : _states[index].value;
    }

    public void Attach(int key, IDisposable data) {
      if (_states.IsNull) _states.EnsureCapacity(5);
      var state = new AttachedState { key = key, value = data };
      var result = _states.TryAdd(state);
      if (result) return;
      data.Dispose();
      throw new InvalidOperationException($"State with key {key} already exists.");
    }

    public void Dispose() {
      if (_states.IsNull) return;
      for (var i = 0; i < _states.Count; i++) {
        ref var state = ref _states[i];
        state.value.Dispose();
        state.value = null;
      }
      _states.Clear();
    }
  }

  public struct AttachedState : IRefKeyed<int> {
    public int key;
    public IDisposable value;

    public int Key => key;
  }

  public static class StateAttachmentExtensions {
    public static void EventAction<T>(
      this ref ElementRef element, CompositionAction<T> action,
      Func<VisualElement, VisualElement> targetSelector = null,
      CallbackOptions options = CallbackOptions.Default
    ) where T : EventBase<T>, new() {
      if (element.composable is not IStateAttachmentHolder holder) {
        throw new InvalidOperationException(
          $"Composable {element.composable} does not implement IStateAttachmentHolder, cannot attach event action."
        );
      }
      UITKEventListener<T>.Apply(element.composable, ref holder.StateAttachmentStore, action, targetSelector, options);
    }

    public static void EventAction<T>(
      this ref ElementRef element, PlainEventAccessor<T> accessor, CompositionAction<T> action,
      Func<VisualElement, VisualElement> targetSelector = null
    ) {
      if (element.composable is not IStateAttachmentHolder holder) {
        throw new InvalidOperationException(
          $"Composable {element.composable} does not implement IStateAttachmentHolder, cannot attach native event action."
        );
      }
      PlainEventListener<T>.Apply(
        element.composable, ref holder.StateAttachmentStore, accessor, action, targetSelector
      );
    }
  }

  public sealed class UITKEventListener<T> : IDisposable where T : EventBase<T>, new() {
    // ReSharper disable once StaticMemberInGenericType
    public static readonly int GeneratedId = CompositionId.GetGeneralId();

    private readonly VisualElement _element;
    private readonly IComposable _composable;
    private readonly CallbackOptions _options;
    public CompositionAction<T> callback;

    public UITKEventListener(VisualElement element, IComposable composable, CallbackOptions options) {
      _element = element;
      _composable = composable;
      _options = options;
      element.RegisterCallback<T>(Handler, options);
    }

    public void Handler(T evt) {
      if (callback == null) return;
      var context = new CompositionContext(_composable, _element);
      using (HXComposer.BeginBatch()) callback.Invoke(context, evt);
    }

    public void Dispose() {
      _element.UnregisterCallback<T>(Handler, _options);
    }

    public static void Apply(
      IComposable composable, ref StateAttachmentStore attachmentStore,
      CompositionAction<T> action,
      Func<VisualElement, VisualElement> targetSelector = null,
      CallbackOptions option = CallbackOptions.Default
    ) {
      if (attachmentStore.GetOrNull(GeneratedId) is not UITKEventListener<T> current) {
        var target = composable.Element;
        if (targetSelector != null) target = targetSelector(target);
        current = new UITKEventListener<T>(target, composable, option);
        attachmentStore.Attach(GeneratedId, current);
      }
      current.callback = action;
    }

    public sealed class Binding {
      public readonly Func<VisualElement, VisualElement> targetSelector;
      public readonly CallbackOptions options;

      public Binding(
        Func<VisualElement, VisualElement> targetSelector = null,
        CallbackOptions options = CallbackOptions.Default
      ) {
        this.targetSelector = targetSelector;
        this.options = options;
      }

      public void Bind(VisualElement element, CompositionAction<T> action) {
        var promoted = CompositionInternals.PromoteHolder(element);
        Apply(promoted, ref promoted.StateAttachmentStore, action, targetSelector, options);
      }
    }
  }

  public sealed class PlainEventAccessor<T> {
    public readonly Action<VisualElement, Action<T>> subscribe;
    public readonly Action<VisualElement, Action<T>> unsubscribe;
    public readonly int generatedId = CompositionId.GetGeneralId();

    public PlainEventAccessor(
      Action<VisualElement, Action<T>> subscribe,
      Action<VisualElement, Action<T>> unsubscribe
    ) {
      this.subscribe = subscribe;
      this.unsubscribe = unsubscribe;
    }

    public static PlainEventAccessor<T> Casting<TElement>(
      Action<TElement, Action<T>> subscribe,
      Action<TElement, Action<T>> unsubscribe
    ) where TElement : VisualElement => new(
      (element, action) => {
        if (element is not TElement casted)
          throw new InvalidOperationException($"Element {element} is not of type {typeof(TElement)}");
        subscribe(casted, action);
      },
      (element, action) => {
        if (element == null) return;
        if (element is not TElement casted)
          throw new InvalidOperationException($"Element {element} is not of type {typeof(TElement)}");
        unsubscribe(casted, action);
      }
    );
  }

  public sealed class PlainEventListener<T> : IDisposable {
    private readonly VisualElement _element;
    private readonly IComposable _composable;
    private readonly PlainEventAccessor<T> _accessor;
    public CompositionAction<T> callback;

    public PlainEventListener(VisualElement element, IComposable composable, PlainEventAccessor<T> accessor) {
      _element = element;
      _composable = composable;
      _accessor = accessor;
      accessor.subscribe(element, Handler);
    }

    public void Handler(T evt) {
      if (callback == null) return;
      var context = new CompositionContext(_composable, _element);
      using (HXComposer.BeginBatch()) callback.Invoke(context, evt);
    }

    public void Dispose() {
      _accessor.unsubscribe(_element, Handler);
    }

    public static void Apply(
      IComposable composable, ref StateAttachmentStore attachmentStore,
      PlainEventAccessor<T> accessor,
      CompositionAction<T> action,
      Func<VisualElement, VisualElement> targetSelector = null
    ) {
      if (attachmentStore.GetOrNull(accessor.generatedId) is not PlainEventListener<T> current) {
        var target = composable.Element;
        if (targetSelector != null) target = targetSelector(target);
        current = new PlainEventListener<T>(target, composable, accessor);
        attachmentStore.Attach(accessor.generatedId, current);
      }
      current.callback = action;
    }

    public sealed class Binding {
      public readonly PlainEventAccessor<T> accessor;
      public readonly Func<VisualElement, VisualElement> targetSelector;

      public Binding(
        PlainEventAccessor<T> accessor,
        Func<VisualElement, VisualElement> targetSelector = null
      ) {
        this.accessor = accessor;
        this.targetSelector = targetSelector;
      }

      public void Bind(VisualElement element, CompositionAction<T> action) {
        var promoted = CompositionInternals.PromoteHolder(element);
        Apply(promoted, ref promoted.StateAttachmentStore, accessor, action, targetSelector);
      }
    }
  }
}
