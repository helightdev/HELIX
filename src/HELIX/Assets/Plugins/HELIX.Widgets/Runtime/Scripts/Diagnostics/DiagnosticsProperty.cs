using System;
using System.Collections.Generic;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics {
    public delegate T ComputePropertyValueCallback<T>();

    public class DiagnosticsProperty<T> : DiagnosticsNode {

        private readonly ComputePropertyValueCallback<T> _computeValue;
        private readonly DiagnosticLevel _defaultLevel;
        private readonly string _description;
        private Exception _exception;
        private T _value;
        private bool _valueComputed;

        public DiagnosticsProperty(
            string name,
            T value,
            string description = null,
            string ifNull = null,
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            object defaultValue = null,
            string tooltip = null,
            bool missingIfNull = false,
            string linePrefix = null,
            bool expandableValue = false,
            bool allowWrap = true,
            bool allowNameWrap = true,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
            DiagnosticLevel level = DiagnosticLevel.Info
        )
            : base(name, style, showName, showSeparator, linePrefix) {
            _description = description;
            _valueComputed = true;
            _value = value;
            _computeValue = null;
            IfNull = ifNull ?? (missingIfNull ? "MISSING" : null);
            IfEmpty = ifEmpty;
            DefaultValue = defaultValue;
            Tooltip = tooltip;
            MissingIfNull = missingIfNull;
            ExpandableValue = expandableValue;
            AllowWrapImpl = allowWrap;
            AllowNameWrapImpl = allowNameWrap;
            _defaultLevel = level;
        }

        public DiagnosticsProperty(
            string name,
            ComputePropertyValueCallback<T> computeValue,
            string description,
            string ifNull,
            string ifEmpty,
            bool showName,
            bool showSeparator,
            object defaultValue,
            string tooltip,
            bool missingIfNull,
            bool expandableValue,
            bool allowWrap,
            bool allowNameWrap,
            DiagnosticsTreeStyle style,
            DiagnosticLevel level
        )
            : base(name, style, showName, showSeparator) {
            _description = description;
            _valueComputed = false;
            _computeValue = computeValue;
            _defaultLevel = level;
            IfNull = ifNull ?? (missingIfNull ? "MISSING" : null);
            IfEmpty = ifEmpty;
            DefaultValue = defaultValue;
            Tooltip = tooltip;
            MissingIfNull = missingIfNull;
            ExpandableValue = expandableValue;
            AllowWrapImpl = allowWrap;
            AllowNameWrapImpl = allowNameWrap;
        }

        public override bool AllowWrap => AllowWrapImpl;
        public override bool AllowNameWrap => AllowNameWrapImpl;
        public bool AllowWrapImpl { get; }
        public bool AllowNameWrapImpl { get; }
        public bool ExpandableValue { get; }
        public string IfNull { get; }
        public string IfEmpty { get; }
        public string Tooltip { get; }
        public bool MissingIfNull { get; }
        public object DefaultValue { get; }

        public Exception Exception {
            get {
                MaybeCacheValue();
                return _exception;
            }
        }

        public override object Value => ValueTyped;

        public virtual T ValueTyped {
            get {
                MaybeCacheValue();
                return _value;
            }
        }

        protected virtual bool IsInteresting {
            get {
                if (DefaultValue == null) return true;

                object value = ValueTyped;
                return !Equals(value, DefaultValue);
            }
        }

        public override DiagnosticLevel Level {
            get {
                if (_defaultLevel == DiagnosticLevel.Hidden) return _defaultLevel;
                if (Exception != null) return DiagnosticLevel.Error;
                if (ValueTyped == null && MissingIfNull) return DiagnosticLevel.Warning;
                if (!IsInteresting) return DiagnosticLevel.Fine;
                return _defaultLevel;
            }
        }

        public static DiagnosticsProperty<T> Lazy(
            string name,
            ComputePropertyValueCallback<T> computeValue,
            string description = null,
            string ifNull = null,
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            object defaultValue = null,
            string tooltip = null,
            bool missingIfNull = false,
            bool expandableValue = false,
            bool allowWrap = true,
            bool allowNameWrap = true,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
            DiagnosticLevel level = DiagnosticLevel.Info
        ) {
            return new DiagnosticsProperty<T>(
                name,
                computeValue,
                description,
                ifNull,
                ifEmpty,
                showName,
                showSeparator,
                defaultValue,
                tooltip,
                missingIfNull,
                expandableValue,
                allowWrap,
                allowNameWrap,
                style,
                level
            );
        }

        protected void MaybeCacheValue() {
            if (_valueComputed) return;

            _valueComputed = true;
            try {
                _value = _computeValue();
            } catch (Exception ex) {
                _exception = ex;
                _value = default;
            }
        }

        public virtual string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            object v = ValueTyped;
            return v?.ToString() ?? "null";
        }

        public override string ToDescription(TextTreeConfiguration parentConfiguration = null) {
            if (_description != null) return AddTooltip(_description);
            if (Exception != null) return "EXCEPTION (" + Exception.GetType().Name + ")";
            if (IfNull != null && ValueTyped == null) return AddTooltip(IfNull);

            var result = ValueToString(parentConfiguration);
            if (result.Length == 0 && IfEmpty != null) result = IfEmpty;
            return AddTooltip(result);
        }

        protected string AddTooltip(string text) {
            return Tooltip == null ? text : text + " (" + Tooltip + ")";
        }

        public override List<DiagnosticsNode> GetProperties() {
            return new List<DiagnosticsNode>();
        }

        public override List<DiagnosticsNode> GetChildren() {
            return new List<DiagnosticsNode>();
        }

    }
}