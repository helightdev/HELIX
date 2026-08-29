using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public struct TextSelectionStyle {
    public Color cursor;
    public Color selection;
    public TextSelectionStyleType type;
  }

  public enum TextSelectionStyleType : byte { Light, Dark, Custom, LightNeutral, DarkNeutral }

  // [Flags]
  // public enum TextInputFlag {
  //   None = 0,
  //   Multiline = 1 << 0,
  //   Autocorrect = 1 << 1,
  //   ReadOnly = 1 << 2,
  //   Password = 1 << 3,
  //   ErrorOnInvalidValue = 1 << 4,
  //   HideMobileInput = 1 << 5,
  //   SubmitOnEnter = 1 << 6,
  //   Expands = 1 << 7
  // }

  [EnableMixins, Structure] public readonly partial struct TextInputOptions : IEquatable<TextInputOptions> {
    public static readonly TextInputOptions Default = new(maxLength: -1); // Force use of constructor

    [Prop(false)] public readonly bool multiline;
    [Prop(true)] public readonly bool autocorrect;
    [Prop(false)] public readonly bool readOnly;
    [Prop(false)] public readonly bool password;
    [Prop(true)] public readonly bool errorOnInvalidValue;
    [Prop(true)] public readonly bool hideMobileInput;
    [Prop(true)] public readonly bool submitOnEnter;
    [Prop(true)] public readonly bool expands;
    [Prop(TouchScreenKeyboardType.Default)] public readonly TouchScreenKeyboardType keyboardType;
    [Prop('*')] public readonly char maskCharacter;
    [Prop(-1)] public readonly int maxLength;
  }


  public delegate bool TextInputParser<TValue>(string text, out TValue value);

  public abstract class TextEditingController : Signal {
    internal InputKeyEventBuffering buffering;

    public bool enabled = true;
    internal TextEditEndReason endReason;
    internal bool hasTabbedIn;
    public TextEditingValue initialValue = TextEditingValue.Empty;
    internal bool isModifying;

    internal int lastKeyboardSubmitFrame = -1;

    public CompositionAction<TextEditingValue> onChanged;
    public CompositionAction<TextEditingValue, TextEditEndReason> onEditingEnded;
    public CompositionAction onEditingStarted;
    public TextInputOptions options = TextInputOptions.Default;
    public TextEditProcessor processor;
    internal int textInputSkipFrame = -1;
    public TextEditingValue value = TextEditingValue.Empty;

    protected TextEditingController() : base("TextEditingController", typeof(TextEditingController)) { }

    public bool IsUserInputValid { get; protected set; } = true;
    public bool Editing { get; protected set; }

    public State State {
      get {
        var basis = State.None;
        basis |= enabled ? State.None : State.Disabled;
        basis |= Editing ? State.Focused : State.None;
        basis |= IsUserInputValid ? State.None : State.Error;
        return basis;
      }
    }

    public abstract void SetEditingValue(in TextEditingValue updated);

    public void Handle(ref TextEditProcessorContext context) {
      if (processor == null) return;
      try {
        processor.Invoke(ref context);
      } catch (Exception ex) {
        Debug.LogException(ex);
      }
    }

    public void BeginHandle(
      IBoundary boundary,
      in TextEditingValue physical,
      TextEditTrigger trigger,
      out TextEditProcessorContext context
    ) {
      context = new TextEditProcessorContext(
        new CompositionContext(boundary),
        trigger,
        value,
        physical,
        initialValue,
        physical,
        TextEditResult.Continue()
      );
    }

    public void EndHandle(IBoundary boundary, ref TextEditProcessorContext context) {
      if (!context.next.Equals(context.previous)) onChanged?.Call(boundary, context.next);

      if (context.next.Equals(context.physical)) SetEditingValue(context.next); // Accepted
      else {
        SetEditingValue(context.next); // Rejected / Reverted
        textInputSkipFrame = Time.frameCount;
      }

      if (context.result.isInterrupted && context.trigger.evt != null) {
        context.trigger.evt.StopPropagation();
        textInputSkipFrame = Time.frameCount;
      }

      if (context.result.endReason == TextEditEndReason.None) {
        // if not submitted, empty
      } else endReason = context.result.endReason;

      NotifyListeners();
    }

    public void BeginEditing(IBoundary boundary) {
      if (Editing || !enabled) return;
      Editing = true;
      initialValue = value;
      onEditingStarted?.Call(boundary);
      NotifyListeners();
    }

    public void EndEditing(IBoundary boundary) {
      if (!Editing) return;
      Editing = false;
      try {
        onEditingEnded?.Call(boundary, value, endReason);
      } catch (Exception ex) {
        Debug.LogException(ex);
      } finally {
        endReason = TextEditEndReason.FocusLost;
      }

      initialValue = default;
      NotifyListeners();
    }

    protected void NotifyListeners() {
      NotifyDirty();
      NotifyObservers();
    }
  }

  public class TextEditingController<TValue> : TextEditingController {
    public TValue typedValue;
    public TextInputValueAdapter<TValue> valueAdapter;

    public TextEditingController(TextInputValueAdapter<TValue> valueAdapter) {
      this.valueAdapter = valueAdapter;
    }

    public override void SetEditingValue(in TextEditingValue updated) {
      value = updated;
      IsUserInputValid = valueAdapter.TryFromText(updated.text, out typedValue);
    }

    public void SetTypedValue(TValue typed) {
      typedValue = typed;
      IsUserInputValid = true;
      value = new TextEditingValue { text = valueAdapter.ToText(typed) };
    }
  }

  public abstract class InputValueAdapter<TInput, TOutput> {
    public abstract bool TryConvert(TInput input, out TOutput output);
    public abstract TInput InverseConvert(TOutput output);
  }

  public sealed class TextInputValueAdapter<TValue> : InputValueAdapter<string, TValue> {
    private readonly TextInputParser<TValue> _fromText;
    private readonly Func<TValue, string> _toText;

    public TextInputValueAdapter(Func<TValue, string> toText, TextInputParser<TValue> fromText) {
      _toText = toText ?? throw new ArgumentNullException(nameof(toText));
      _fromText = fromText ?? throw new ArgumentNullException(nameof(fromText));
    }

    public string ToText(TValue value) {
      return _toText(value) ?? string.Empty;
    }

    public bool TryFromText(string text, out TValue value) {
      return _fromText(text ?? string.Empty, out value);
    }

    public override bool TryConvert(string input, out TValue output) {
      return TryFromText(input, out output);
    }

    public override string InverseConvert(TValue output) {
      return ToText(output);
    }
  }

  public static class TextInputAdapters {
    public static readonly TextInputValueAdapter<string> String = new(
      value => value ?? string.Empty,
      (string text, out string value) => {
        value = text;
        return true;
      }
    );

    public static readonly TextInputValueAdapter<int> Int32 = new(
      value => value.ToString(CultureInfo.InvariantCulture),
      (string text, out int value) => int.TryParse(
        text,
        NumberStyles.Integer,
        CultureInfo.InvariantCulture,
        out value
      )
    );

    public static readonly TextInputValueAdapter<float> Single = new(
      value => value.ToString(CultureInfo.InvariantCulture),
      (string text, out float value) => float.TryParse(
        text,
        NumberStyles.Float,
        CultureInfo.InvariantCulture,
        out value
      )
    );
  }

  public struct InputKeyEventBuffering {
    public bool isBuffering;
    public EventModifiers modifiers;
    public char character;
    public KeyCode keyCode;
    public int frame;

    public void Clear() {
      isBuffering = false;
      modifiers = EventModifiers.None;
      character = '\0';
      keyCode = KeyCode.None;
      frame = -1;
    }

    public void ClearIfFrameOutdated() {
      if (frame == -1) return;
      if (Time.frameCount != frame) Clear();
    }

    public void BeginBuffering() {
      frame = Time.frameCount;
      isBuffering = true;
    }

    public readonly bool IsReady =>
      frame == Time.frameCount && isBuffering && (character != 0 || keyCode != KeyCode.None);
    public readonly bool IsBuffering => frame == Time.frameCount && isBuffering;
  }

  [EnableMixins]
  [CustomBoundaryElement(false, extension: true, trimChildren: false, name: "TextField")]
  [InputStateListener]
  public sealed partial class TextFieldElement : VisualElement, ISlotHost {
    private const string _selectionLightClass = "helix-textfield-style-light";
    private const string _selectionDarkClass = "helix-textfield-style-dark";
    private const string _selectionLightNeutralClass = "helix-textfield-style-light-neutral";
    private const string _selectionDarkNeutralClass = "helix-textfield-style-dark-neutral";

    private static readonly UniqueStyleString _backgroundSlotType = new("hx-textfield-background");
    private static readonly UniqueStyleString _prefixSlotType = new("hx-textfield-prefix");
    private static readonly UniqueStyleString _suffixSlotType = new("hx-textfield-suffix");
    private readonly ComposableSlot _background, _prefix, _suffix;
    private readonly VisualElement _row;
    private Color _appliedCursorColor;
    private Color _appliedSelectionColor;
    private TextSelectionStyleType _appliedSelectionStyleType;
    private bool _hasAppliedSelectionStyle;
    public TextEditingController controller;
    public bool isAutomaticController = true;


    public TextFieldElement() {
      Field = new TextField();
      delegatesFocus = true;
      focusable = false;
      this.MakeRelative();
      AddToClassList("helix-generic-text-input");
      this.WithStylesheet(AuxiliaryStylesheets.Helix);

      _background = new ComposableSlot(this, _backgroundSlotType)
        .WithClasses(_backgroundSlotType)
        .WithName("Background")
        .Stretched();
      _background.pickingMode = PickingMode.Ignore;
      hierarchy.Add(_background);

      _row = new VisualElement().Fill()
        .FlexContainer(Axis.Horizontal, crossAxisAlign: Align.Center);
      hierarchy.Add(_row);
      _prefix = new ComposableSlot(this, _prefixSlotType)
        .FlexContainer(Axis.Horizontal)
        .WithClasses(_prefixSlotType)
        .AddTo(_row);
      Field.Fill();
      _row.Add(Field);
      _suffix = new ComposableSlot(this, _suffixSlotType)
        .FlexContainer(Axis.Horizontal)
        .WithClasses(_suffixSlotType)
        .AddTo(_row);
      Field.Q<VisualElement>(className: "unity-base-field__input");
      TextEdition = Field.textEdition as TextElement ?? Field.Q<TextElement>("unity-text-input");
      if (TextEdition == null) throw new InvalidOperationException("TextField text edition is not a TextElement.");

      Field.ClearClassList();
      TextEdition.TextAlign(TextAnchor.MiddleLeft);

      Field.RegisterValueChangedCallback(OnValueChanged);
      TextEdition.RegisterCallback<FocusEvent>(OnFocus);
      TextEdition.RegisterCallback<BlurEvent>(OnBlur);
      RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
      TextEdition.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
      TextEdition.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);

      TextEdition.selection.OnCursorIndexChange += OnSelectionChanged;
      TextEdition.selection.OnSelectIndexChange += OnSelectionChanged;
      TextEdition.RegisterCallback<KeyDownEvent>(OnKeyDownEventElement);
      RegisterCallback<AttachToPanelEvent>(_ => AttachBoundary());
      RegisterCallback<DetachFromPanelEvent>(_ => DetachBoundary());
      PostConstruct();
    }

    public TextEditingController Owner { get; set; }
    public TextField Field { get; }
    public TextElement TextEdition { get; }

    public IBoundary Boundary => this;

    internal ScopeHandle BackgroundScope(Composition cx) {
      return _background.Scope(ref cx);
    }

    internal ScopeHandle PrefixScope(Composition cx) {
      return _prefix.Scope(ref cx);
    }

    internal ScopeHandle SuffixScope(Composition cx) {
      return _suffix.Scope(ref cx);
    }

    private void OnKeyDownEventElement(KeyDownEvent evt) {
      ref var buffering = ref Owner.buffering;

      buffering.ClearIfFrameOutdated();
      if (evt.keyCode != KeyCode.None) {
        buffering.keyCode = evt.keyCode;
        buffering.modifiers |= evt.modifiers;
        buffering.BeginBuffering();
      }
      if (evt.character != 0) {
        buffering.character = evt.character;
        buffering.modifiers |= evt.modifiers;
        buffering.BeginBuffering();
      }
      if (!buffering.IsReady) return;
      var physical = TextEditingValue.FromElement(TextEdition);
      if (Owner.value.Equals(physical)) return;

      var trigger = new TextEditTrigger(
        TextEditingValue.GetModificationType(in Owner.value, in physical),
        buffering.modifiers,
        buffering.keyCode,
        buffering.character
      );

      BeginHandle(trigger, out var context);
      Handle(ref context);
      EndHandle(ref context);
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      EnsureController(props.controller);
      cx.SubscribeTo(controller);
      var boxStyle = props.style ?? ThemeProperties.TextField[in cx];
      var selectionStyle = props.selectionStyle ?? ThemeProperties.TextSelectionStyle[in cx];
      var passedState = controller.State | InputState;
      using (cx.WriteContext(out var context)) boxStyle.RenderContext(in context, passedState);
      this.Margin(boxStyle.margin[passedState]);
      boxStyle.constraints[passedState].Apply(this);
      this.MarkFlag(UssFlag.Size);

      Owner = controller;
      var options = controller.options;
      Field.isDelayed = false;
      Field.multiline = options.multiline;
      Field.autoCorrection = options.autocorrect;
      Field.isReadOnly = options.readOnly;
      Field.isPasswordField = options.password;
      Field.hideMobileInput = options.hideMobileInput;
      Field.keyboardType = options.keyboardType;
      Field.maskChar = options.maskCharacter;
      Field.maxLength = options.maxLength;
      Field.Flexible(options.expands ? 1f : 0f, options.expands ? 1f : 0f);
      _row.Padding(boxStyle.padding[passedState]);
      Field.Padding(EdgeInsets.Zero);
      ApplySelectionStyle(selectionStyle);
      ApplyEditingValue(in controller.value);

      if (cx.Conditional(props.prefix != null))
        using (PrefixScope(cx))
          props.prefix(ref cx);
      if (cx.Conditional(props.suffix != null))
        using (SuffixScope(cx))
          props.suffix(ref cx);
      using (BackgroundScope(cx)) boxStyle.RenderBackground(ref cx, passedState);
    }

    [Hook]
    private void OnReset() {
      Owner = null;
      _hasAppliedSelectionStyle = false;
      Field.SetEnabled(true);
      _background.Reset();
      _prefix.Reset();
      _suffix.Reset();
      Field.SetValueWithoutNotify(string.Empty);
    }

    [Hook]
    private void OnDispose() {
      DisposeAutomaticController();
    }

    public void EnsureController(TextEditingController given) {
      if (ReferenceEquals(given, controller) && controller != null) return;
      if (given == null) {
        if (isAutomaticController && controller != null) {
          if (!props.value.HasValue || controller.Editing) goto configureAutomatic;
          controller.value = props.valueIgnoreSelection
            ? controller.value.ReplaceText(props.value.Value.text)
            : props.value.Value;
        } else {
          controller = new TextEditingController<string>(TextInputAdapters.String);
          controller.value = props.value ?? props.initialValue ?? TextEditingValue.Empty;
          controller.initialValue = controller.value;
          isAutomaticController = true;
        }

        configureAutomatic:
        controller.onChanged = props.onChanged;
        controller.onEditingStarted = props.onEditingStarted;
        controller.onEditingEnded = props.onEditingEnded;
        controller.processor = props.processor;
        controller.enabled = props.enabled;
        controller.options = props.options;
      } else {
        DisposeAutomaticController();
        controller = given;
      }
    }

    private void DisposeAutomaticController() {
      if (!isAutomaticController) return;
      controller?.Dispose();
      controller = null;
      isAutomaticController = false;
    }

    private void CheckEndEditingLater() {
      schedule.Execute(EndEditing).ExecuteLater(1);
    }

    private void BeginEditing() {
      Owner?.BeginEditing(Boundary);
    }

    private void EndEditing() {
      Owner?.EndEditing(Boundary);
    }

    private void ApplySelectionStyle(TextSelectionStyle inputStyle) {
      var customColorsChanged =
        inputStyle.type == TextSelectionStyleType.Custom &&
        (!_appliedSelectionColor.Equals(inputStyle.selection) ||
          !_appliedCursorColor.Equals(inputStyle.cursor));
      if (_hasAppliedSelectionStyle && _appliedSelectionStyleType == inputStyle.type &&
        !customColorsChanged) return;

      Field.RemoveFromClassList(_selectionLightClass);
      Field.RemoveFromClassList(_selectionDarkClass);
      Field.RemoveFromClassList(_selectionLightNeutralClass);
      Field.RemoveFromClassList(_selectionDarkNeutralClass);
      switch (inputStyle.type) {
        case TextSelectionStyleType.Light: Field.AddToClassList(_selectionLightClass); break;
        case TextSelectionStyleType.Dark: Field.AddToClassList(_selectionDarkClass); break;
        case TextSelectionStyleType.LightNeutral: Field.AddToClassList(_selectionLightNeutralClass); break;
        case TextSelectionStyleType.DarkNeutral: Field.AddToClassList(_selectionDarkNeutralClass); break;
        case TextSelectionStyleType.Custom: break;
        default: throw new ArgumentOutOfRangeException();
      }

      if (inputStyle.type != TextSelectionStyleType.Custom) {
#pragma warning disable CS0618
        Field.textSelection.selectionColor = inputStyle.selection;
        Field.textSelection.cursorColor = inputStyle.cursor;
#pragma warning restore CS0618
      }
    }


    public void ApplyEditingValue(in TextEditingValue value) {
      var physical = TextEditingValue.FromElement(TextEdition);
      if (value.Equals(physical)) return;
      Owner.isModifying = true;
      try {
        value.Apply(TextEdition);
      } finally {
        Owner.isModifying = false;
      }
    }

    public void ApplyLastEditingValue() {
      Owner.isModifying = true;
      try {
        Owner.value.Apply(TextEdition);
      } finally {
        Owner.isModifying = false;
      }
    }

    public void BeginHandle(in TextEditTrigger trigger, out TextEditProcessorContext context) {
      var physical = TextEditingValue.FromElement(TextEdition);
      Owner.BeginHandle(Boundary, in physical, trigger, out context);
    }

    public void Handle(ref TextEditProcessorContext context) {
      Owner.Handle(ref context);
    }

    public void EndHandle(ref TextEditProcessorContext context) {
      Owner.EndHandle(Boundary, ref context);
      if (context.result.endReason != TextEditEndReason.None) TextEdition.Blur(); // Cancel
    }

    private void OnSelectionChanged() {
      if (Owner == null) return;
      if (Owner.isModifying || !Owner.Editing) return;
      if (Owner.value.SelectionEquals(TextEdition)) return;
      if (Owner.textInputSkipFrame == Time.frameCount) {
        ApplyLastEditingValue();
        return;
      }
      Owner.hasTabbedIn = false;
      if (Owner.buffering.IsBuffering) return;

      BeginHandle(new TextEditTrigger(TextEditTriggerType.SelectionModification), out var context);
      Handle(ref context);
      EndHandle(ref context);
    }


    private void OnValueChanged(ChangeEvent<string> evt) {
      if (Owner == null) return;
      Owner.hasTabbedIn = false;
      if (Owner.isModifying) return;
      if (Owner.textInputSkipFrame == Time.frameCount) {
        Owner.textInputSkipFrame = -1;
        ApplyLastEditingValue();
        return;
      }
      if (!Owner.enabled) return;
      if (Owner.buffering.IsBuffering) return;

      BeginHandle(new TextEditTrigger(evt), out var context);
      Handle(ref context);
      EndHandle(ref context);
    }

    private void OnBlur(BlurEvent evt) {
      if (Owner == null) return;
      Owner.hasTabbedIn = false;
      if (!Owner.Editing) return;
      CheckEndEditingLater();
    }

    private void OnFocus(FocusEvent evt) {
      if (Owner == null) return;
      if (Owner.Editing) return;
      Owner.endReason = TextEditEndReason.FocusLost;
      Owner.hasTabbedIn = evt.direction == VisualElementFocusChangeDirection.right;
      BeginEditing();
    }

    private void OnKeyDown(KeyDownEvent evt) {
      if (Owner == null) return;
      if (evt.target != TextEdition) return;
      Owner.hasTabbedIn = false;
      Owner.buffering.BeginBuffering();

      BeginHandle(new TextEditTrigger(evt), out var context);
      if (context.result.IsContinuedEditing && Owner.enabled) {
        if (evt.keyCode is KeyCode.Space) Owner.lastKeyboardSubmitFrame = Time.frameCount;

        if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter) {
          var addEnter = evt.altKey && Owner.options is { multiline: true, submitOnEnter: true };
          var submit = Owner.options.submitOnEnter && !addEnter;
          if (addEnter) {
            context.next = context.next.Insert("\n");
            context.result = TextEditResult.Continue(true);
          }
          if (submit) context.result = TextEditResult.EndEdit(TextEditEndReason.Submitted, breaking: false);
        }

        if (evt.keyCode is KeyCode.Escape) {
          context.result = TextEditResult.EndEdit(breaking: false);
          context.next = Owner.initialValue;
        }
      }
      Handle(ref context);
      if (context.result.IsContinuedEditing && context.physical.Equals(context.next)) {
        if (evt.keyCode == KeyCode.Tab && !evt.shiftKey && (Owner.value.IsSelectedAll || Owner.hasTabbedIn)) {
          Owner.textInputSkipFrame = Time.frameCount;
          Owner.hasTabbedIn = false;
          evt.StopPropagation();
          new VisualElementFocusRing(panel.visualTree)
            .GetNextFocusable(TextEdition, VisualElementFocusChangeDirection.right)
            .Focus();
          return;
        }
      }

      EndHandle(ref context);
    }

    private void OnNavigationSubmit(NavigationSubmitEvent evt) {
      if (Owner == null) return;
      if (evt.target != TextEdition) return;
      if (Owner.lastKeyboardSubmitFrame == Time.frameCount || Owner.textInputSkipFrame == Time.frameCount) {
        evt.StopPropagation();
        return;
      }

      Owner.endReason = TextEditEndReason.Submitted;
      BeginHandle(new TextEditTrigger(evt), out var context);
      context.result = TextEditResult.EndEdit(TextEditEndReason.Submitted, breaking: false);
      context.next = Owner.value;
      Handle(ref context);
      EndHandle(ref context);
    }

    private void OnNavigationCancel(NavigationCancelEvent evt) {
      Owner.endReason = TextEditEndReason.Cancelled;
      BeginHandle(new TextEditTrigger(evt), out var context);
      context.next = Owner.initialValue;
      context.result = TextEditResult.EndEdit(breaking: false);
      Handle(ref context);
      EndHandle(ref context);
    }

    public partial struct Props {
      [Prop(null)] public TextEditingController controller;
      [Prop(null)] public TextSelectionStyle? selectionStyle;
      [Prop(null)] public HXControlBoxStyle? style;
      [Prop(null)] public TextEditingValue? value;
      [Prop(null)] public TextEditingValue? initialValue;
      [Prop(null)] public CompositionAction<TextEditingValue> onChanged;
      [Prop(null)] public CompositionAction onEditingStarted;
      [Prop(null)] public CompositionAction<TextEditingValue, TextEditEndReason> onEditingEnded;
      [Prop(null)] public TextEditProcessor processor;
      [Prop(true)] public bool enabled;
      [Prop(true)] public bool valueIgnoreSelection;
      [Prop("TextInputOptions.Default", PropInit.Deferred)] public TextInputOptions options;
      [Prop(null)] public Composable prefix;
      [Prop(null)] public Composable suffix;
    }
  }
}
