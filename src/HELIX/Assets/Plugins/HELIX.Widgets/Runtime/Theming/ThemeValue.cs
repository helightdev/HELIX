using System;
using System.Collections.Generic;
using HELIX.Widgets.Layout;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeValue {
        public abstract void ReloadStyle(ICustomStyle style);
    }
    
    public class ThemeValue<T> : ThemeValue {
        private readonly ThemeProperty<T> _property;
        private T _value;
        private ThemeValueState _state = ThemeValueState.None;

        public event OnValueChangedDelegate OnValueChanged;

        public ThemeValue(ThemeProperty<T> property) {
            _property = property;
        }

        public ThemeValue(ThemeProperty<T> property, OnValueChangedDelegate onValueChanged) : this(property) {
            OnValueChanged += onValueChanged;
        }

        public T Value {
            get => _state == ThemeValueState.None ? _property.defaultValue : _value;
            set {
                _value = value;
                _state = ThemeValueState.Override;
                OnValueChanged?.Invoke(_value);
            }
        }

        public void NotifyStyleChanged(T newValue) {
            if (_state > ThemeValueState.CustomStyle) return;
            _value = newValue;
            _state = ThemeValueState.CustomStyle;
            OnValueChanged?.Invoke(_value);
        }

        public void Notify() {
            OnValueChanged?.Invoke(Value);
        }

        public delegate void OnValueChangedDelegate(T newValue);

        private enum ThemeValueState {
            None = 0,
            CustomStyle = 1,
            Override = 2
        }

        public override void ReloadStyle(ICustomStyle style) {
            NotifyStyleChanged(_property.Resolve(style));
        }
    }
}