using System;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeValue {
        public abstract void ReloadStyle(ICustomStyle style);
    }

    public class ThemeValue<T> : ThemeValue {
        private ThemeProperty<T> _property;
        private readonly VisualElement _owner;
        private T _value;
        private ThemeOverride<T> _override = new();
        private ThemeValueState _state = ThemeValueState.None;

        public event OnValueChangedDelegate OnValueChanged;

        public ThemeValue(VisualElement owner, ThemeProperty<T> property) {
            _property = property;
            _owner = owner;
        }

        public ThemeValue(VisualElement owner, ThemeProperty<T> property, OnValueChangedDelegate onValueChanged) : this(
            owner, property) {
            OnValueChanged += onValueChanged;
        }
        
        public ThemeOverride<T> Override {
            get => _override;
            set {
                _override = value;
                ApplyOverrides(value);
            }
        }

        public T Value {
            get => _state == ThemeValueState.None ? _property.defaultValue : _value;
            set {
                _value = value;
                _state = ThemeValueState.Override;
                OnValueChanged?.Invoke(_value);
            }
        }

        public void ApplyOverrides(ThemeOverride<T> value) {
            if (_state >= ThemeValueState.Override) return;
            switch (value.type) {
                case ThemeOverrideType.None:
                    if (_state == ThemeValueState.VisualOverride) {
                        _state = ThemeValueState.None;
                        ReloadStyle(_owner.customStyle);
                    }
                    break;
                case ThemeOverrideType.Value:
                    SetVisualOverride(value);
                    break;
                case ThemeOverrideType.PropertyReference:
                    _state = ThemeValueState.None;
                    SwapPropertyByReference(value.propertyReference);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetVisualOverride(ThemeOverride<T> value) {
            _value = value.constantValue;
            _state = ThemeValueState.VisualOverride;
            OnValueChanged?.Invoke(_value);
        }


        public void Notify() {
            OnValueChanged?.Invoke(Value);
        }

        public void SwapProperty(ThemeProperty<T> newProperty) {
            if (_property == null) return;
            _property = newProperty;
            ReloadStyle(_owner.customStyle);
        }

        public void SwapPropertyByReference(string reference) {
            if (string.IsNullOrEmpty(reference)) return;
            if (reference == "None") return;
            var property = RuntimeReflectionThemeLookup.GetProperty<T>(reference);
            if (property == null) return;
            SwapProperty(property);
        }

        public override void ReloadStyle(ICustomStyle style) {
            NotifyStyleChanged(_property.Resolve(style));
        }

        private void NotifyStyleChanged(T newValue) {
            if (_state > ThemeValueState.CustomStyle) return;
            _value = newValue;
            _state = ThemeValueState.CustomStyle;
            OnValueChanged?.Invoke(_value);
        }

        public delegate void OnValueChangedDelegate(T newValue);

        private enum ThemeValueState {
            None = 0,
            CustomStyle = 1,
            VisualOverride = 2,
            Override = 3
        }
    }
}