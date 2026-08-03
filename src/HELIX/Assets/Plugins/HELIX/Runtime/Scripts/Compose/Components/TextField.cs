using System;
using System.Globalization;
using HELIX.Extensions;
using HELIX.Signals;
using HELIX.Theming;
using HELIX.Widgets.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [BoundaryComposable(Base = typeof(InputBoundaryComposable<>), Extension = true)]
  public partial class HXTextField {
    private static readonly ushort _textFieldElementId = CompositionId.GetTypeId("TextFieldElement");

    public partial struct Props {
      [PropDefault(null)] public TextEditingController controller;
      [PropDefault(null)] public TextSelectionStyle? selectionStyle;
      [PropDefault(null)] public HXControlBoxStyle? style;

      // Implicit controller definition
      [PropDefault(null)] public TextEditingValue? value;
      [PropDefault(null)] public TextEditingValue? initialValue;

      [PropDefault(null)] public CompositionAction<TextEditingValue> onChanged;
      [PropDefault(null)] public CompositionAction onEditingStarted;
      [PropDefault(null)] public CompositionAction<TextEditingValue, TextEditEndReason> onEditingEnded;
      [PropDefault(null)] public TextEditProcessor processor;
      [PropDefault(true)] public bool enabled;
      [PropDefault(true)] public bool valueIgnoreSelection;

      [PropDefault("TextInputOptions.Default", PropInit.Deferred)]
      public TextInputOptions options;
    }

    public TextEditingController controller;
    public bool isAutomaticController = true;

    protected override void OnRecompose(ref Composition cx) {
      EnsureController(props.controller);
      cx.SubscribeTo(controller);

      var style = props.style ?? ThemeProperties.TextField[in cx];
      var selectionStyle = props.selectionStyle ?? ThemeProperties.TextSelectionStyle[in cx];

      var passedState = controller.State | InputState;
      using (cx.WriteContext(out var context)) {
        style.RenderContext(in context, passedState);
      }

      cx.CURSOR.Margin(style.margin[passedState]).Size(style.constraints[passedState]);

      if (!cx.AUTHORING.RequireComposable<TextFieldElement>(_textFieldElementId, out var input, out _)) {
        input = new TextFieldElement();
      }

      input.Owner = controller;
      input.Configure(Node, controller, selectionStyle);
      input.ApplyEditingValue(controller.value);
      input.Field.Padding(style.padding[passedState]);

      cx.AUTHORING.YieldElement(ref cx, input);

      using (input.BackgroundScope(cx)) {
        style.RenderBackground(ref cx, passedState);
      }
    }

    public void EnsureController(TextEditingController given) {
      if (ReferenceEquals(given, controller) && controller != null) return;
      if (given == null) {
        if (isAutomaticController && controller != null) { // Update retained state
          if (!props.value.HasValue) goto configureAutomatic;

          controller.value = props.valueIgnoreSelection
            ? controller.value.ReplaceText(props.value.Value.text)
            : props.value.Value;
        } else { // Configure initial state
          controller = new TextEditingController<string>(TextInputAdapters.String);
          controller.value = props.value ?? props.initialValue ?? TextEditingValue.Empty;
          controller.initialValue = controller.value;
          isAutomaticController = true;
        }

        configureAutomatic: ConfigureAutomaticController(); // Shared non-value updates
      } else {
        DisposeAutomaticController();
        controller = given;
      }
    }

    private void ConfigureAutomaticController() {
      controller.onChanged = props.onChanged;
      controller.onEditingStarted = props.onEditingStarted;
      controller.onEditingEnded = props.onEditingEnded;
      controller.processor = props.processor;
      controller.enabled = props.enabled;
      controller.options = props.options;
    }

    private void DisposeAutomaticController() {
      if (!isAutomaticController) return;
      controller?.Dispose();
      isAutomaticController = false;
    }

    protected override void OnDetach() {
      DisposeAutomaticController();
      base.OnDetach();
    }
  }

  public struct TextSelectionStyle {
    public Color cursor;
    public Color selection;
    public TextSelectionStyleType type;
  }

  public enum TextSelectionStyleType : byte { Light, Dark, Custom, LightNeutral, DarkNeutral }

  public readonly struct TextInputOptions : IEquatable<TextInputOptions> {
    public static readonly TextInputOptions Default = new(
      multiline: false,
      autocorrect: true,
      readOnly: false,
      password: false,
      errorOnInvalidValue: true,
      hideMobileInput: true,
      submitOnEnter: true,
      expands: true,
      keyboardType: TouchScreenKeyboardType.Default,
      maskCharacter: '*',
      maxLength: -1
    );

    public readonly bool multiline;
    public readonly bool autocorrect;
    public readonly bool readOnly;
    public readonly bool password;
    public readonly bool errorOnInvalidValue;
    public readonly bool hideMobileInput;
    public readonly bool submitOnEnter;
    public readonly bool expands;
    public readonly TouchScreenKeyboardType keyboardType;
    public readonly char maskCharacter;
    public readonly int maxLength;

    public TextInputOptions(
      bool multiline = false,
      bool autocorrect = true,
      bool readOnly = false,
      bool password = false,
      bool errorOnInvalidValue = true,
      bool hideMobileInput = true,
      bool submitOnEnter = true,
      bool expands = true,
      TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default,
      char maskCharacter = '*',
      int maxLength = -1
    ) {
      this.multiline = multiline;
      this.autocorrect = autocorrect;
      this.readOnly = readOnly;
      this.password = password;
      this.errorOnInvalidValue = errorOnInvalidValue;
      this.hideMobileInput = hideMobileInput;
      this.submitOnEnter = submitOnEnter;
      this.expands = expands;
      this.keyboardType = keyboardType;
      this.maskCharacter = maskCharacter;
      this.maxLength = maxLength;
    }

    public bool Equals(TextInputOptions other) {
      return multiline == other.multiline &&
             autocorrect == other.autocorrect &&
             readOnly == other.readOnly &&
             password == other.password &&
             errorOnInvalidValue == other.errorOnInvalidValue &&
             hideMobileInput == other.hideMobileInput &&
             submitOnEnter == other.submitOnEnter &&
             expands == other.expands &&
             keyboardType == other.keyboardType &&
             maskCharacter == other.maskCharacter &&
             maxLength == other.maxLength;
    }

    public override bool Equals(object obj) => obj is TextInputOptions other && Equals(other);

    public override int GetHashCode() {
      var first = HashCode.Combine(
        multiline, autocorrect, readOnly, password, errorOnInvalidValue, hideMobileInput, submitOnEnter, expands
      );
      return HashCode.Combine(first, keyboardType, maskCharacter, maxLength);
    }
  }


  public delegate bool TextInputParser<TValue>(string text, out TValue value);

  public abstract class TextEditingController : Signal {
    public TextEditingValue value = TextEditingValue.Empty;
    public TextEditingValue initialValue = TextEditingValue.Empty;

    public CompositionAction<TextEditingValue> onChanged;
    public CompositionAction onEditingStarted;
    public CompositionAction<TextEditingValue, TextEditEndReason> onEditingEnded;
    public TextEditProcessor processor;

    public bool enabled = true;
    public TextInputOptions options = TextInputOptions.Default;

    public bool IsUserInputValid { get; protected set; } = true;
    public bool Editing { get; protected set; }

    internal int lastKeyboardSubmitFrame = -1;
    internal int textInputSkipFrame = -1;
    internal bool isModifying;
    internal bool hasTabbedIn;
    internal TextEditEndReason endReason;
    internal InputKeyEventBuffering buffering;

    public State State {
      get {
        var basis = State.None;
        basis |= enabled ? State.None : State.Disabled;
        basis |= Editing ? State.Focused : State.None;
        basis |= IsUserInputValid ? State.None : State.Error;
        return basis;
      }
    }

    protected TextEditingController() : base("TextEditingController", typeof(TextEditingController)) { }

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
      IBoundary boundary, in TextEditingValue physical,
      TextEditTrigger trigger, out TextEditProcessorContext context
    ) {
      context = new TextEditProcessorContext(
        ctx: new CompositionContext(boundary),
        trigger: trigger,
        previous: value,
        physical: physical,
        initial: initialValue,
        next: physical,
        result: TextEditResult.Continue()
      );
    }

    public void EndHandle(IBoundary boundary, ref TextEditProcessorContext context) {
      if (!context.next.Equals(context.previous)) {
        onChanged?.Call(boundary, context.next);
      }

      if (context.next.Equals(context.physical)) {
        SetEditingValue(context.next); // Accepted
      } else {
        SetEditingValue(context.next); // Rejected / Reverted
        textInputSkipFrame = Time.frameCount;
      }

      if (context.result.isInterrupted && context.trigger.evt != null) {
        context.trigger.evt.StopPropagation();
        textInputSkipFrame = Time.frameCount;
      }

      if (context.result.endReason == TextEditEndReason.None) {
        // if not submitted, empty
      } else {
        endReason = context.result.endReason;
      }

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
    public TextInputValueAdapter<TValue> valueAdapter;
    public TValue typedValue;

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
      value = new TextEditingValue {
        text = valueAdapter.ToText(typed)
      };
    }
  }

  public abstract class InputValueAdapter<TInput, TOutput> {
    public abstract bool TryConvert(TInput input, out TOutput output);
    public abstract TInput InverseConvert(TOutput output);
  }

  public sealed class TextInputValueAdapter<TValue> : InputValueAdapter<string, TValue> {
    private readonly Func<TValue, string> _toText;
    private readonly TextInputParser<TValue> _fromText;

    public TextInputValueAdapter(Func<TValue, string> toText, TextInputParser<TValue> fromText) {
      _toText = toText ?? throw new ArgumentNullException(nameof(toText));
      _fromText = fromText ?? throw new ArgumentNullException(nameof(fromText));
    }

    public string ToText(TValue value) => _toText(value) ?? string.Empty;
    public bool TryFromText(string text, out TValue value) => _fromText(text ?? string.Empty, out value);

    public override bool TryConvert(string input, out TValue output) => TryFromText(input, out output);
    public override string InverseConvert(TValue output) => ToText(output);
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
        text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value
      )
    );

    public static readonly TextInputValueAdapter<float> Single = new(
      value => value.ToString(CultureInfo.InvariantCulture),
      (string text, out float value) => float.TryParse(
        text, NumberStyles.Float, CultureInfo.InvariantCulture, out value
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

  public sealed class TextFieldElement : VisualElement, ISlotHost {
    private const string _selectionLightClass = "helix-textfield-style-light";
    private const string _selectionDarkClass = "helix-textfield-style-dark";
    private const string _selectionLightNeutralClass = "helix-textfield-style-light-neutral";
    private const string _selectionDarkNeutralClass = "helix-textfield-style-dark-neutral";

    private static readonly UniqueStyleString _backgroundSlotType = new("hx-textfield-background");
    private readonly ComposableSlot _background;
    private readonly TextField _field;
    private readonly TextElement _textEdition;
    private bool _hasAppliedSelectionStyle;
    private TextSelectionStyleType _appliedSelectionStyleType;
    private Color _appliedSelectionColor;
    private Color _appliedCursorColor;

    public IBoundary Boundary { get; private set; }
    public UssFlag Flag { get; set; }
    public ulong PackedId { get; set; }
    public TextEditingController Owner { get; set; }
    public TextField Field => _field;
    public TextElement TextEdition => _textEdition;
    public VisualElement Element => this;

    internal ScopeHandle BackgroundScope(Composition cx) => _background.Scope(cx);


    public TextFieldElement() {
      _field = new TextField();
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

      _field.Fill();
      hierarchy.Add(_field);
      _field.Q<VisualElement>(className: "unity-base-field__input");
      _textEdition = _field.textEdition as TextElement ?? _field.Q<TextElement>("unity-text-input");
      if (_textEdition == null) {
        throw new InvalidOperationException("TextField text edition is not a TextElement.");
      }

      _field.ClearClassList();
      _textEdition.TextAlign(TextAnchor.MiddleLeft);

      _field.RegisterValueChangedCallback(OnValueChanged);
      _textEdition.RegisterCallback<FocusEvent>(OnFocus);
      _textEdition.RegisterCallback<BlurEvent>(OnBlur);
      RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
      _textEdition.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
      _textEdition.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);

      _textEdition.selection.OnCursorIndexChange += OnSelectionChanged;
      _textEdition.selection.OnSelectIndexChange += OnSelectionChanged;
      _textEdition.RegisterCallback<KeyDownEvent>(OnKeyDownEventElement);
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
      var physical = TextEditingValue.FromElement(_textEdition);
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

    public void Configure(
      IBoundary boundary, TextEditingController controller,
      TextSelectionStyle selectionStyle
    ) {
      this.Owner = controller;
      Boundary = boundary;
      var options = controller.options;
      _field.isDelayed = false;
      _field.multiline = options.multiline;
      _field.autoCorrection = options.autocorrect;
      _field.isReadOnly = options.readOnly;
      _field.isPasswordField = options.password;
      _field.hideMobileInput = options.hideMobileInput;
      _field.keyboardType = options.keyboardType;
      _field.maskChar = options.maskCharacter;
      _field.maxLength = options.maxLength;
      _field.Flexible(options.expands ? 1f : 0f, options.expands ? 1f : 0f);
      ApplySelectionStyle(selectionStyle);
    }

    public void Reset() {
      Boundary = null;
      _hasAppliedSelectionStyle = false;
      _field.SetEnabled(true);
      _background.Reset();
      _field.SetValueWithoutNotify(string.Empty);
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

      _field.RemoveFromClassList(_selectionLightClass);
      _field.RemoveFromClassList(_selectionDarkClass);
      _field.RemoveFromClassList(_selectionLightNeutralClass);
      _field.RemoveFromClassList(_selectionDarkNeutralClass);
      switch (inputStyle.type) {
        case TextSelectionStyleType.Light: _field.AddToClassList(_selectionLightClass); break;
        case TextSelectionStyleType.Dark: _field.AddToClassList(_selectionDarkClass); break;
        case TextSelectionStyleType.LightNeutral: _field.AddToClassList(_selectionLightNeutralClass); break;
        case TextSelectionStyleType.DarkNeutral: _field.AddToClassList(_selectionDarkNeutralClass); break;
        case TextSelectionStyleType.Custom: break;
        default: throw new ArgumentOutOfRangeException();
      }

      if (inputStyle.type != TextSelectionStyleType.Custom) {
#pragma warning disable CS0618
        _field.textSelection.selectionColor = inputStyle.selection;
        _field.textSelection.cursorColor = inputStyle.cursor;
#pragma warning restore CS0618
      }
    }


    public void ApplyEditingValue(TextEditingValue value) {
      if (value.Equals(TextEditingValue.FromElement(_textEdition))) return;
      Owner.isModifying = true;
      try {
        value.Apply(_textEdition);
      } finally {
        Owner.isModifying = false;
      }
    }

    public void ApplyLastEditingValue() {
      Owner.isModifying = true;
      try {
        Owner.value.Apply(_textEdition);
      } finally {
        Owner.isModifying = false;
      }
    }

    public void BeginHandle(in TextEditTrigger trigger, out TextEditProcessorContext context) {
      var physical = TextEditingValue.FromElement(_textEdition);
      Owner.BeginHandle(Boundary, in physical, trigger, out context);
    }

    public void Handle(ref TextEditProcessorContext context) {
      Owner.Handle(ref context);
    }

    public void EndHandle(ref TextEditProcessorContext context) {
      Owner.EndHandle(Boundary, ref context);
      if (context.result.endReason != TextEditEndReason.None) {
        _textEdition.Blur(); // Cancel
      }
    }

    private void OnSelectionChanged() {
      if (Owner == null) return;
      if (Owner.isModifying || !Owner.Editing) return;
      if (Owner.value.SelectionEquals(_textEdition)) return;
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
      if (evt.target != _textEdition) return;
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
            context.result = TextEditResult.Continue(interrupt: true);
          }
          if (submit) context.result = TextEditResult.EndEdit(TextEditEndReason.Submitted, breaking: false);
        }

        if (evt.keyCode is KeyCode.Escape) {
          context.result = TextEditResult.EndEdit(TextEditEndReason.Cancelled, breaking: false);
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
            .GetNextFocusable(_textEdition, VisualElementFocusChangeDirection.right)
            .Focus();
          return;
        }
      }

      EndHandle(ref context);
    }

    private void OnNavigationSubmit(NavigationSubmitEvent evt) {
      if (Owner == null) return;
      if (evt.target != _textEdition) return;
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
  }
}