using System;
using System.Collections.Generic;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeValue : DiagnosticableBase {
        public abstract void ReloadStyles();
        public abstract ThemeProperty ThemeProperty { get; }
        public abstract ThemeValueState ThemeValueState { get; }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<ThemeProperty>("property", ThemeProperty));
            properties.Add(new EnumProperty<ThemeValueState>("state", ThemeValueState));
        }
    }

    public class ThemeValue<T> : ThemeValue {
        public delegate void OnValueChangedDelegate(T newValue);

        private readonly BaseThemeProperty<T> _fallbackProperty;
        private readonly BaseElement _owner;
        private ThemeOverride<T> _override = new();
        private BaseThemeProperty<T> _property;
        private ThemeValueState _state = ThemeValueState.None;
        private T _value;

        public EqualityComparer<T> valueComparer = EqualityComparer<T>.Default;

        public event OnValueChangedDelegate OnValueChanged;

        public ThemeValue(BaseElement owner, BaseThemeProperty<T> property) {
            _fallbackProperty = property;
            _property = property;
            _owner = owner;
        }

        public ThemeValue(
            BaseElement owner,
            BaseThemeProperty<T> property,
            OnValueChangedDelegate onValueChanged
        ) : this(
            owner,
            property
        ) {
            OnValueChanged += onValueChanged;
        }

        public override ThemeProperty ThemeProperty => _property;
        public override ThemeValueState ThemeValueState => _state;

        public ThemeOverride<T> Override {
            get => _override;
            set {
                _override = value;
                ApplyOverrides(value);
            }
        }

        public T Value {
            get => _state == ThemeValueState.None ? _property.TypedDefaultValue : _value;
            set {
                _state = ThemeValueState.Override;
                SetAndNotifyIfChanged(value);
            }
        }

        public void ApplyOverrides(ThemeOverride<T> value) {
            if (_state >= ThemeValueState.Override) return;
            switch (value.type) {
                case ThemeOverrideType.None:
                    if (_state <= ThemeValueState.VisualOverride) {
                        _state = ThemeValueState.None;
                        SwapProperty(_fallbackProperty);
                    }

                    break;
                case ThemeOverrideType.Value: SetVisualOverride(value); break;
                case ThemeOverrideType.PropertyReference:
                    _state = ThemeValueState.None;
                    SwapPropertyByReference(value.propertyReference);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void SetVisualOverride(ThemeOverride<T> value) {
            _state = ThemeValueState.VisualOverride;
            SetAndNotifyIfChanged(value.constantValue);
        }

        public void Notify() {
            OnValueChanged?.Invoke(Value);
        }

        public void SwapProperty(BaseThemeProperty<T> newProperty) {
            if (_property == null) return;
            _property = newProperty;
            ReloadStyles();
        }

        public void SwapPropertyByReference(string reference) {
            if (string.IsNullOrEmpty(reference)) return;
            if (reference == "None") return;
            var property = RuntimeReflectionThemeLookup.GetProperty<T>(reference);
            if (property == null) return;
            SwapProperty(property);
        }

        public override void ReloadStyles() {
            if (_state > ThemeValueState.CustomStyle) return;
            var newValue = ThemeProviderElement.Resolve(_owner.ThemeProviderElement, _property);
            NotifyStyleChanged(newValue);
        }

        private void NotifyStyleChanged(T newValue) {
            if (_state > ThemeValueState.CustomStyle) return;
            _state = ThemeValueState.CustomStyle;
            SetAndNotifyIfChanged(newValue);
        }

        private void SetAndNotifyIfChanged(T newValue) {
            if (valueComparer?.Equals(_value, newValue) ?? false) return;
            _value = newValue;
            OnValueChanged?.Invoke(_value);
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<T>("value", Value));
            properties.Add(
                new ObjectFlagProperty<EqualityComparer<T>>(
                    "equalityComparer",
                    valueComparer,
                    ifNull: "No EqualityComparer"
                )
            );
        }
    }
    
    public enum ThemeValueState {
        None = 0,
        CustomStyle = 1,
        VisualOverride = 2,
        Override = 3
    }
}