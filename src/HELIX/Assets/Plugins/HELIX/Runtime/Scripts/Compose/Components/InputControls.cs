using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using HELIX.Widgets.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public enum TextSelectionStyle : byte { Light, Dark, Custom, LightNeutral, DarkNeutral }

  public sealed class InputFieldStyle {
    public static readonly InputFieldStyle Default = BuildDefault(HXThemes.DefaultDark);
    public static readonly ContextKey<InputFieldStyle> Key = new("InputFieldStyle", Default);

    public readonly StateProperty<StyleLength4> padding;
    public readonly StateProperty<StyleLength4> margin;
    public readonly StateProperty<BoxConstraints> constraints;
    public readonly StateProperty<TextStyle> textStyle;
    public readonly Composable<State> background;
    public readonly TextSelectionStyle selectionStyle;
    public readonly Color selectionColor;
    public readonly Color cursorColor;

    public InputFieldStyle(
      StateProperty<StyleLength4> padding = null,
      StateProperty<StyleLength4> margin = null,
      StateProperty<BoxConstraints> constraints = null,
      StateProperty<TextStyle> textStyle = null,
      Composable<State> background = null,
      TextSelectionStyle selectionStyle = TextSelectionStyle.Dark,
      Color? selectionColor = null,
      Color? cursorColor = null
    ) {
      this.padding = padding ?? StateProperties.Never<StyleLength4>();
      this.margin = margin ?? StateProperties.Never<StyleLength4>();
      this.constraints = constraints ?? StateProperties.Never<BoxConstraints>();
      this.textStyle = textStyle ?? StateProperties.Never<TextStyle>();
      this.background = background;
      this.selectionStyle = selectionStyle;
      this.selectionColor = selectionColor ?? Color.red;
      this.cursorColor = cursorColor ?? Color.white;
    }

    internal void ApplyLayout(State state, IComposable composable) {
      var element = composable.Element;
      padding.ResolveOrDefault(state, StyleLength4.Zero).ApplyPadding(element);
      margin.ResolveOrDefault(state, StyleLength4.Zero).ApplyMargin(element);
      constraints.ResolveOrDefault(state, BoxConstraints.Initial).Apply(element);
      composable.Flag |= UssFlag.Padding | UssFlag.Margin | UssFlag.Size;
      if (textStyle.TryResolve(state, out var resolvedTextStyle)) {
        resolvedTextStyle.Apply(composable);
      } else if ((composable.Flag & UssFlag.Text) != 0) {
        UssFlag.Text.ClearFlags(element);
        composable.Flag &= ~UssFlag.Text;
      }
    }

    internal void RenderBoundary(ref Composition cx, State state) {
      ApplyLayout(state, cx.boundary);
      background?.Invoke(ref cx, state);
    }

    internal void RenderBackground(ref Composition cx, State state) {
      background?.Invoke(ref cx, state);
    }

    internal bool TryResolveTextStyle(State state, out TextStyle style) {
      return textStyle.TryResolve(state, out style);
    }

    public static InputFieldStyle BuildDefault(ThemeData theme) {
      var text = new StatePropertyMap<TextStyle>();
      var normalText = theme[TextRole.BodyMedium].style;
      var disabledText = normalText;
      disabledText.color = theme[ColorRoles.DisabledHigh];
      text[State.Disabled] = disabledText;
      text[State.None] = normalText;

      return new InputFieldStyle(
        padding: StyleLength4.Symmetric(horizontal: 8f, vertical: 5f),
        constraints: BoxConstraints.Min(new StyleLength2(32f)),
        textStyle: text,
        background: ThemeProperties.InputBox[theme],
        selectionStyle: TextSelectionStyle.Custom,
        selectionColor: theme.GetColor(ColorRoles.Focus).WithOpacity(0.4f),
        cursorColor: theme.GetColor(ColorRoles.OnSurface)
      );
    }
  }

  public readonly struct TextInputOptions : IEquatable<TextInputOptions> {
    public static readonly TextInputOptions Default = new(
      multiline: false,
      autocorrect: true,
      readOnly: false,
      password: false,
      delayed: false,
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
    public readonly bool delayed;
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
      bool delayed = false,
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
      this.delayed = delayed;
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
             delayed == other.delayed &&
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
        multiline, autocorrect, readOnly, password, delayed, hideMobileInput, submitOnEnter, expands
      );
      return HashCode.Combine(first, keyboardType, maskCharacter, maxLength);
    }
  }

  public readonly struct NumericInputOptions : IEquatable<NumericInputOptions> {
    public static readonly NumericInputOptions Default = new(expands: true);

    public readonly bool delayed;
    public readonly string format;
    public readonly bool expands;

    public NumericInputOptions(bool delayed = false, string format = null, bool expands = true) {
      this.delayed = delayed;
      this.format = format;
      this.expands = expands;
    }

    public bool Equals(NumericInputOptions other) {
      return delayed == other.delayed &&
             expands == other.expands &&
             string.Equals(format, other.format, StringComparison.Ordinal);
    }

    public override bool Equals(object obj) => obj is NumericInputOptions other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(delayed, format, expands);
  }

  internal interface ITextFieldAdapter<TValue, TField, TOptions>
    where TField : TextInputBaseField<TValue>
    where TOptions : struct {
    TOptions DefaultOptions { get; }
    TValue DefaultValue { get; }

    void SubmitOnEnter(
      in TOptions options,
      IKeyboardEvent evt,
      out bool addEnter, out bool submit
    );

    void Apply(TField field, in TOptions options);
  }

  internal readonly struct StringTextFieldAdapter
    : ITextFieldAdapter<string, TextField, TextInputOptions> {
    public TextInputOptions DefaultOptions => TextInputOptions.Default;
    public string DefaultValue => string.Empty;

    public void SubmitOnEnter(
      in TextInputOptions options,
      IKeyboardEvent evt,
      out bool addEnter, out bool submit
    ) {
      addEnter = false;
      submit = options.submitOnEnter;

      if (!evt.altKey || !options.multiline || !options.submitOnEnter) return;
      addEnter = true;
      submit = false;
    }

    public void Apply(TextField field, in TextInputOptions options) {
      field.multiline = options.multiline;
      field.autoCorrection = options.autocorrect;
      field.isReadOnly = options.readOnly;
      field.isPasswordField = options.password;
      field.hideMobileInput = options.hideMobileInput;
      field.keyboardType = options.keyboardType;
      field.maskChar = options.maskCharacter;
      field.maxLength = options.maxLength;
      field.Flexible(options.expands ? 1f : 0f, options.expands ? 1f : 0f);
    }
  }

  internal readonly struct NumericTextFieldAdapter<TValue, TField>
    : ITextFieldAdapter<TValue, TField, NumericInputOptions>
    where TValue : struct
    where TField : TextValueField<TValue> {
    public NumericInputOptions DefaultOptions => NumericInputOptions.Default;
    public TValue DefaultValue => default;

    public void SubmitOnEnter(
      in NumericInputOptions options,
      IKeyboardEvent evt,
      out bool addEnter, out bool submit
    ) {
      addEnter = false;
      submit = true;
    }

    public void Apply(TField field, in NumericInputOptions options) {
      field.formatString = options.format;
      field.Flexible(options.expands ? 1f : 0f, options.expands ? 1f : 0f);
    }
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

    public readonly bool IsReady => frame == Time.frameCount && isBuffering && (character != 0 || keyCode != KeyCode.None);
    public readonly bool IsBuffering => frame == Time.frameCount && isBuffering;
  }

  internal sealed class TextFieldElement<TValue, TField, TOptions, TAdapter> : VisualElement, IComposable
    where TField : TextInputBaseField<TValue>, new()
    where TOptions : struct, IEquatable<TOptions>
    where TAdapter : struct, ITextFieldAdapter<TValue, TField, TOptions> {
    private const string _selectionLightClass = "helix-textfield-style-light";
    private const string _selectionDarkClass = "helix-textfield-style-dark";
    private const string _selectionLightNeutralClass = "helix-textfield-style-light-neutral";
    private const string _selectionDarkNeutralClass = "helix-textfield-style-dark-neutral";

    private static readonly CustomStyleProperty<Color> _selectionColorProperty = new("--unity-selection-color");
    private static readonly CustomStyleProperty<Color> _cursorColorProperty = new("--unity-cursor-color");
    private static readonly EqualityComparer<TValue> _equality = EqualityComparer<TValue>.Default;

    private TAdapter _adapter;
    private readonly CompositionBoundaryNode _background;
    private readonly TField _field;
    private readonly VisualElement _inputContainer;
    private readonly TextElement _textEdition;
    private CompositionAction<TValue> _onChanged;
    private CompositionAction<TValue> _onSubmitted;
    private CompositionAction _onEditingStarted;
    private CompositionAction<TextEditingValue, TextEditEndReason> _onEditingEnded;
    private IBoundary _callbackBoundary;
    private InputFieldStyle _inputStyle;
    private State _inputState;
    private bool _enabled = true;
    private bool _editing;
    private bool _hasAppliedSelectionStyle;
    private TextSelectionStyle _appliedSelectionStyle;
    private Color _appliedSelectionColor;
    private Color _appliedCursorColor;

    private int _lastKeyboardSubmitFrame = -1;
    private int _textInputSkipFrame = -1;

    private TextEditingValue _lastValue;
    private TextEditingValue _initialValue;
    private TextEditProcessor _processor;
    private bool _isModifying = false;
    private bool _hasTabbedIn = false;
    private TextEditEndReason _endReason;
    private InputKeyEventBuffering _buffering;

    private TOptions _options;

    public TextFieldElement() {
      _field = new TField();
      delegatesFocus = true;
      focusable = false;
      this.MakeRelative();
      AddToClassList("helix-generic-text-input");
      this.WithStylesheet(AuxiliaryStylesheets.Helix);

      _background = new CompositionBoundaryNode {
        name = "InputVisual", composable = ComposeBackground, pickingMode = PickingMode.Ignore
      }.Stretched();
      hierarchy.Add(_background);

      _field.Fill();
      hierarchy.Add(_field);
      _inputContainer = _field.Q<VisualElement>(className: "unity-base-field__input");
      _textEdition = _field.textEdition as TextElement ?? _field.Q<TextElement>("unity-text-input");
      if (_textEdition == null) {
        throw new InvalidOperationException($"{typeof(TField).Name} text edition is not a TextElement.");
      }

      _field.ClearClassList();
      _textEdition.TextAlign(TextAnchor.MiddleLeft);

      _field.RegisterValueChangedCallback(OnValueChanged);
      _field.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
      RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
      _textEdition.RegisterCallback<FocusEvent>(OnFocus);
      _textEdition.RegisterCallback<BlurEvent>(OnBlur);
      RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
      _textEdition.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
      _textEdition.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);

      _textEdition.selection.OnCursorIndexChange += OnSelectionChanged;
      _textEdition.selection.OnSelectIndexChange += OnSelectionChanged;
      _textEdition.RegisterCallback<KeyDownEvent>(OnKeyDownEventElement);
      _lastValue = TextEditingValue.FromElement(_textEdition);
    }

    private void OnKeyDownEventElement(KeyDownEvent evt) {
      _buffering.ClearIfFrameOutdated();
      if (evt.keyCode != KeyCode.None) {
        _buffering.keyCode = evt.keyCode;
        _buffering.modifiers |= evt.modifiers;
        _buffering.BeginBuffering();
      }
      if (evt.character != 0) {
        _buffering.character = evt.character;
        _buffering.modifiers |= evt.modifiers;
        _buffering.BeginBuffering();
      }
      if (!_buffering.IsReady) return;
      var physical = TextEditingValue.FromElement(_textEdition);
      if (_lastValue.Equals(physical)) return;

      var trigger = new TextEditTrigger(
        TextEditingValue.GetModificationType(in _lastValue, in physical),
        _buffering.modifiers,
        _buffering.keyCode,
        _buffering.character
      );

      BeginHandle(trigger, out var context);
      Handle(ref context);
      EndHandle(in context);
    }

    public void Configure(in TOptions options) {
      if (_options.Equals(options)) return;
      _options = options;
      _adapter.Apply(_field, in options);
      ApplyStyle();
    }

    public VisualElement Element => this;
    public UssFlag Flag { get; set; }
    public ulong TypeId { get; set; }

    public void Update(
      TValue value,
      bool enabled,
      bool error,
      InputFieldStyle style,
      IBoundary callbackBoundary,
      TextEditProcessor processor,
      CompositionAction<TValue> onChanged,
      CompositionAction<TValue> onSubmitted,
      CompositionAction onEditingStarted,
      CompositionAction<TextEditingValue, TextEditEndReason> onEditingEnded
    ) {
      _callbackBoundary = callbackBoundary;
      _onChanged = onChanged;
      _onSubmitted = onSubmitted;
      _onEditingStarted = onEditingStarted;
      _onEditingEnded = onEditingEnded;
      _processor = processor;

      if (!_equality.Equals(_field.value, value)) {
        _field.SetValueWithoutNotify(value);
      }

      var stateChanged = SetState(State.Disabled, !enabled);
      stateChanged |= SetState(State.Error, error);
      if (_enabled != enabled) {
        _enabled = enabled;
        _field.SetEnabled(enabled);
      }

      if (!ReferenceEquals(_inputStyle, style)) {
        if (Flag != UssFlag.None) {
          Flag.ClearFlags(this);
          Flag = UssFlag.None;
        }
        _inputStyle = style;
        stateChanged = true;
      }

      if (stateChanged) ApplyStyle();
    }

    public void Reset() {
      _onChanged = null;
      _onSubmitted = null;
      _onEditingStarted = null;
      _onEditingEnded = null;
      _callbackBoundary = null;
      _inputStyle = null;
      _inputState = State.None;
      _editing = false;
      _enabled = true;
      _hasAppliedSelectionStyle = false;
      _lastKeyboardSubmitFrame = -1;
      _field.SetEnabled(true);
      var defaults = _adapter.DefaultOptions;
      Configure(in defaults);
      _field.SetValueWithoutNotify(_adapter.DefaultValue);
    }

    private void BeginEditing() {
      if (_editing || !_enabled) return;
      _editing = true;
      _lastValue = TextEditingValue.FromElement(_textEdition);
      _initialValue = _lastValue;
      _onEditingStarted?.Call(_callbackBoundary);
    }

    private void CheckEndEditingLater() {
      schedule.Execute(EndEditing).ExecuteLater(1);
    }

    private void EndEditing() {
      if (!_editing) return;
      _editing = false;
      try {
        _onEditingEnded?.Call(_callbackBoundary, _lastValue, _endReason);
      } catch (Exception ex) {
        Debug.LogException(ex);
      } finally {
        _endReason = TextEditEndReason.FocusLost;
      }
      _initialValue = default;
    }

    private void ApplyStyle() {
      var style = _inputStyle ?? InputFieldStyle.Default;
      style.ApplyLayout(_inputState, this);
      if (style.TryResolveTextStyle(_inputState, out var textStyle)) {
        ApplyNativeTextStyle(_field, in textStyle);
        ApplyNativeTextStyle(_inputContainer, in textStyle);
        ApplyNativeTextStyle(_textEdition, in textStyle);
      } else {
        ClearNativeTextStyle(_field);
        ClearNativeTextStyle(_inputContainer);
        ClearNativeTextStyle(_textEdition);
      }
      ApplySelectionStyle(style);
      _background.MarkDirty();
    }

    private void ApplySelectionStyle(InputFieldStyle style) {
      var customColorsChanged =
        style.selectionStyle == TextSelectionStyle.Custom &&
        (!_appliedSelectionColor.Equals(style.selectionColor) || !_appliedCursorColor.Equals(style.cursorColor));
      if (_hasAppliedSelectionStyle && _appliedSelectionStyle == style.selectionStyle && !customColorsChanged) return;

      _field.RemoveFromClassList(_selectionLightClass);
      _field.RemoveFromClassList(_selectionDarkClass);
      _field.RemoveFromClassList(_selectionLightNeutralClass);
      _field.RemoveFromClassList(_selectionDarkNeutralClass);
      switch (style.selectionStyle) {
        case TextSelectionStyle.Light: _field.AddToClassList(_selectionLightClass); break;
        case TextSelectionStyle.Dark: _field.AddToClassList(_selectionDarkClass); break;
        case TextSelectionStyle.LightNeutral: _field.AddToClassList(_selectionLightNeutralClass); break;
        case TextSelectionStyle.DarkNeutral: _field.AddToClassList(_selectionDarkNeutralClass); break;
        case TextSelectionStyle.Custom: break;
        default: throw new ArgumentOutOfRangeException();
      }

      _appliedSelectionStyle = style.selectionStyle;
      _appliedSelectionColor = style.selectionColor;
      _appliedCursorColor = style.cursorColor;
      _hasAppliedSelectionStyle = true;
      ApplyResolvedSelectionColors(style);
    }

    private void OnCustomStyleResolved(CustomStyleResolvedEvent evt) {
      ApplyResolvedSelectionColors(_inputStyle ?? InputFieldStyle.Default);
    }

    private void ApplyResolvedSelectionColors(InputFieldStyle style) {
      var selection = style.selectionColor;
      var cursor = style.cursorColor;
      if (style.selectionStyle != TextSelectionStyle.Custom) {
        if (!_field.customStyle.TryGetValue(_selectionColorProperty, out selection)) {
          selection = DefaultSelectionColor(style.selectionStyle);
        }
        if (!_field.customStyle.TryGetValue(_cursorColorProperty, out cursor)) {
          cursor = style.selectionStyle is TextSelectionStyle.Light or TextSelectionStyle.LightNeutral
            ? Color.black
            : Color.white;
        }
      }
#pragma warning disable CS0618
      _field.textSelection.selectionColor = selection;
      _field.textSelection.cursorColor = cursor;
#pragma warning restore CS0618
    }

    private static Color DefaultSelectionColor(TextSelectionStyle style) {
      return style switch {
        TextSelectionStyle.Light => new Color(50f / 255f, 151f / 255f, 253f / 255f, 0.35f),
        TextSelectionStyle.Dark => new Color(50f / 255f, 151f / 255f, 253f / 255f, 0.5f),
        TextSelectionStyle.LightNeutral => new Color(0f, 0f, 0f, 0.25f),
        TextSelectionStyle.DarkNeutral => new Color(1f, 1f, 1f, 0.35f),
        _ => Color.clear
      };
    }

    private static void ApplyNativeTextStyle(VisualElement element, in TextStyle textStyle) {
      if (element == null) return;
      element.style.unityTextAlign = textStyle.align;
      element.style.color = textStyle.color;
      element.style.fontSize = textStyle.size;
      element.style.letterSpacing = textStyle.letterSpacing;
      element.style.unityFontStyleAndWeight = textStyle.style;
      element.style.whiteSpace = textStyle.wrap;
      element.style.textOverflow = textStyle.overflow;
      element.style.unityFontDefinition = textStyle.font;
    }

    private static void ClearNativeTextStyle(VisualElement element) {
      if (element == null) return;
      element.style.unityTextAlign = StyleKeyword.Null;
      element.style.color = StyleKeyword.Null;
      element.style.fontSize = StyleKeyword.Null;
      element.style.letterSpacing = StyleKeyword.Null;
      element.style.unityFontStyleAndWeight = StyleKeyword.Null;
      element.style.whiteSpace = StyleKeyword.Null;
      element.style.textOverflow = StyleKeyword.Null;
      element.style.unityFontDefinition = StyleKeyword.Null;
    }

    private void ComposeBackground(ref Composition cx) {
      (_inputStyle ?? InputFieldStyle.Default).RenderBackground(ref cx, _inputState);
    }

    private bool SetState(State flag, bool enabled) {
      var previous = _inputState;
      if (enabled) _inputState |= flag;
      else _inputState &= ~flag;
      return previous != _inputState;
    }

    public void CommitEditingValue() {
      _lastValue = TextEditingValue.FromElement(_textEdition);
    }

    public void ApplyEditingValue(TextEditingValue value) {
      _isModifying = true;
      try {
        _lastValue = value;
        value.Apply(_textEdition);
      } finally {
        _isModifying = false;
      }
    }

    public void ApplyLastEditingValue() {
      _isModifying = true;
      try {
        _lastValue.Apply(_textEdition);
      } finally {
        _isModifying = false;
      }
    }

    private void OnSelectionChanged() {
      if (_isModifying || !_editing) return;
      if (_lastValue.SelectionEquals(_textEdition)) return;
      if (_textInputSkipFrame == Time.frameCount) {
        ApplyLastEditingValue();
        return;
      }
      _hasTabbedIn = false;
      if (_buffering.IsBuffering) return;

      BeginHandle(new TextEditTrigger(TextEditTriggerType.SelectionModification), out var context);
      Handle(ref context);
      EndHandle(in context);
    }

    private void Handle(ref TextEditProcessorContext context) {
      if (_processor == null) return;
      try {
        _processor.Invoke(ref context);
      } catch (Exception ex) {
        Debug.LogException(ex);
      }
    }

    private void BeginHandle(TextEditTrigger trigger, out TextEditProcessorContext context) {
      var chain = TextEditingValue.FromElement(_textEdition);
      context = new TextEditProcessorContext(
        ctx: new CompositionContext(this),
        trigger: trigger,
        previous: _lastValue,
        physical: chain,
        initial: _initialValue,
        next: chain,
        result: TextEditResult.Continue()
      );
    }

    private void EndHandle(
      in TextEditProcessorContext context
    ) {
      if (context.next.Equals(context.physical)) {
        CommitEditingValue(); // Accepted
      } else {
        ApplyEditingValue(context.next); // Rejected / Reverted
        _textInputSkipFrame = Time.frameCount;
      }

      if (context.result.isInterrupted && context.trigger.evt != null) {
        context.trigger.evt.StopPropagation();
        _textInputSkipFrame = Time.frameCount;
      }

      if (context.HasTextChanged) {
        _onChanged?.Call(_callbackBoundary, _field.value); // TODO: Replace with adapter
      }

      if (context.result.endReason == TextEditEndReason.None) return;
      _endReason = context.result.endReason;
      if (context.result.endReason == TextEditEndReason.Submitted) _lastKeyboardSubmitFrame = Time.frameCount;
      _textEdition.Blur();
    }

    private void OnValueChanged(ChangeEvent<TValue> evt) {
      _hasTabbedIn = false;
      if (_isModifying) return;
      if (_textInputSkipFrame == Time.frameCount) {
        _textInputSkipFrame = -1;
        ApplyLastEditingValue();
        return;
      }
      if (!_enabled) return;
      if (_buffering.IsBuffering) return;

      BeginHandle(new TextEditTrigger(evt), out var context);
      Handle(ref context);
      EndHandle(in context);
    }

    private void OnPointerEnter(PointerEnterEvent evt) {
      if (SetState(State.Hovered, true)) ApplyStyle();
    }

    private void OnPointerLeave(PointerLeaveEvent evt) {
      if (SetState(State.Hovered, false)) ApplyStyle();
    }

    private void OnBlur(BlurEvent evt) {
      _hasTabbedIn = false;
      if (!_editing) return;
      if (SetState(State.Focused, false)) ApplyStyle();
      CheckEndEditingLater();
    }

    private void OnFocus(FocusEvent evt) {
      if (_editing) return;
      _endReason = TextEditEndReason.FocusLost;
      BeginEditing();
      _hasTabbedIn = evt.direction == VisualElementFocusChangeDirection.right;
      if (SetState(State.Focused, true)) ApplyStyle();
    }

    private void OnKeyDown(KeyDownEvent evt) {
      if (evt.target != _textEdition) return;
      _hasTabbedIn = false;
      _buffering.BeginBuffering();

      BeginHandle(new TextEditTrigger(evt), out var context);
      if (context.result.IsContinuedEditing && _enabled) {
        if (evt.keyCode is KeyCode.Space) _lastKeyboardSubmitFrame = Time.frameCount;

        if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter) {
          _adapter.SubmitOnEnter(in _options, evt, out var addEnter, out var submit);
          if (addEnter) {
            context.next = _lastValue.Insert("\n");
            context.result = TextEditResult.Continue(interrupt: true);
          }
          if (submit) context.result = TextEditResult.EndEdit(TextEditEndReason.Submitted, breaking: false);
        }

        if (evt.keyCode is KeyCode.Escape) {
          context.result = TextEditResult.EndEdit(TextEditEndReason.Cancelled, breaking: false);
          context.next = _initialValue;
        }
      }
      Handle(ref context);
      if (context.result.IsContinuedEditing && context.physical.Equals(context.next)) {
        if (evt.keyCode == KeyCode.Tab && !evt.shiftKey && (_lastValue.IsSelectedAll || _hasTabbedIn)) {
          _textInputSkipFrame = Time.frameCount;
          _hasTabbedIn = false;
          evt.StopPropagation();
          new VisualElementFocusRing(panel.visualTree)
            .GetNextFocusable(_textEdition, VisualElementFocusChangeDirection.right)
            .Focus();
          return;
        }
      }

      EndHandle(in context);
    }

    private void OnNavigationSubmit(NavigationSubmitEvent evt) {
      if (evt.target != _textEdition) return;
      if (_lastKeyboardSubmitFrame == Time.frameCount || _textInputSkipFrame == Time.frameCount) {
        evt.StopPropagation();
        return;
      }

      _endReason = TextEditEndReason.Submitted;
      BeginHandle(new TextEditTrigger(evt), out var context);
      context.result = TextEditResult.EndEdit(TextEditEndReason.Submitted, breaking: false);
      context.next = _lastValue;
      Handle(ref context);
      EndHandle(in context);
    }

    private void OnNavigationCancel(NavigationCancelEvent evt) {
      _endReason = TextEditEndReason.Cancelled;
      BeginHandle(new TextEditTrigger(evt), out var context);
      context.next = _initialValue;
      context.result = TextEditResult.EndEdit(breaking: false);
      Handle(ref context);
      EndHandle(in context);
    }
  }

  public static partial class HXBuiltins {
    private static readonly ushort _textInputId = CompositionId.GetTypeId("TextInput");
    private static readonly ushort _integerInputId = CompositionId.GetTypeId("IntegerInput");
    private static readonly ushort _floatInputId = CompositionId.GetTypeId("FloatInput");

    public static ref ElementRef TextInput(
      this ref Composition cx,
      string value,
      TextEditProcessor processor = null,
      CompositionAction<string> onChanged = null,
      CompositionAction<string> onSubmitted = null,
      CompositionAction onEditingStarted = null,
      CompositionAction<TextEditingValue, TextEditEndReason> onEditingEnded = null,
      TextInputOptions? options = null,
      bool enabled = true,
      bool error = false,
      InputFieldStyle style = null
    ) {
      if (!cx.AUTHORING.RequireComposable<
        TextFieldElement<string, TextField, TextInputOptions, StringTextFieldAdapter>
      >(_textInputId, out var input, out _)) {
        input = new TextFieldElement<string, TextField, TextInputOptions, StringTextFieldAdapter>();
      }

      var resolvedOptions = options ?? TextInputOptions.Default;
      input.Configure(in resolvedOptions);
      input.Update(
        value ?? string.Empty,
        enabled,
        error,
        style ?? cx.ReadContextOrDefault(InputFieldStyle.Key, InputFieldStyle.Default),
        cx.boundary,
        processor,
        onChanged,
        onSubmitted,
        onEditingStarted,
        onEditingEnded
      );
      return ref cx.AUTHORING.YieldElement(ref cx, input);
    }

    public static ref ElementRef IntInput(
      this ref Composition cx,
      int value,
      TextEditProcessor processor = null,
      CompositionAction<int> onChanged = null,
      CompositionAction<int> onSubmitted = null,
      CompositionAction onEditingStarted = null,
      CompositionAction<TextEditingValue, TextEditEndReason> onEditingEnded = null,
      NumericInputOptions? options = null,
      bool enabled = true,
      bool error = false,
      InputFieldStyle style = null
    ) {
      if (!cx.AUTHORING.RequireComposable
        <TextFieldElement<int, IntegerField, NumericInputOptions, NumericTextFieldAdapter<int, IntegerField>>>
        (_integerInputId, out var input, out _)) {
        input =
          new TextFieldElement<int, IntegerField, NumericInputOptions, NumericTextFieldAdapter<int, IntegerField>>();
      }

      var resolvedOptions = options ?? NumericInputOptions.Default;
      input.Configure(in resolvedOptions);
      input.Update(
        value,
        enabled,
        error,
        style ?? cx.ReadContextOrDefault(InputFieldStyle.Key, InputFieldStyle.Default),
        cx.boundary,
        processor,
        onChanged,
        onSubmitted,
        onEditingStarted,
        onEditingEnded
      );
      return ref cx.AUTHORING.YieldElement(ref cx, input);
    }

    public static ref ElementRef FloatInput(
      this ref Composition cx,
      float value,
      TextEditProcessor processor = null,
      CompositionAction<float> onChanged = null,
      CompositionAction<float> onSubmitted = null,
      CompositionAction onEditingStarted = null,
      CompositionAction<TextEditingValue, TextEditEndReason> onEditingEnded = null,
      NumericInputOptions? options = null,
      bool enabled = true,
      bool error = false,
      InputFieldStyle style = null
    ) {
      if (!cx.AUTHORING.RequireComposable<
        TextFieldElement<
          float,
          FloatField,
          NumericInputOptions,
          NumericTextFieldAdapter<float, FloatField>
        >
      >(_floatInputId, out var input, out _)) {
        input = new TextFieldElement<
          float,
          FloatField,
          NumericInputOptions,
          NumericTextFieldAdapter<float, FloatField>
        >();
      }

      var resolvedOptions = options ?? NumericInputOptions.Default;
      input.Configure(in resolvedOptions);
      input.Update(
        value,
        enabled,
        error,
        style ?? cx.ReadContextOrDefault(InputFieldStyle.Key, InputFieldStyle.Default),
        cx.boundary,
        processor,
        onChanged,
        onSubmitted,
        onEditingStarted,
        onEditingEnded
      );
      return ref cx.AUTHORING.YieldElement(ref cx, input);
    }
  }

  internal static class InputStyleExtensions {
    public static void ApplyPadding(this StyleLength4 padding, VisualElement element) {
      element.Padding(padding);
    }

    public static void ApplyMargin(this StyleLength4 margin, VisualElement element) {
      element.Margin(margin);
    }
  }
}