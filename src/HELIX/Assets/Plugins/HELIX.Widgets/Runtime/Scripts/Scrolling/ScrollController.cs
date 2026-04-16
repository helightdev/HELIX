using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Signals;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Scrolling
{
    public class ScrollController : ValueSignal<float>, ISignalObserver {
        private float? _lastOffset;
        private float _initialScrollOffset;

        public float InitialScrollOffset {
            get => _initialScrollOffset;
            set {
                if (!_lastOffset.HasValue) _lastOffset = value;
                _initialScrollOffset = value;
            }
        }

        public bool KeepScrollOffset { get; set; } = true;

        public float Offset {
            get => ScrollPosition?.Extent ?? _lastOffset.GetValueOrDefault(0f);
            set {
                if (ScrollPosition == null) {
                    _lastOffset = value;
                } else {
                    ScrollPosition.Extent = value;
                }
            }
        }

        public float NormalizedScrollOffset {
            get => ScrollPosition.NormalizedOffset;
            set => ScrollPosition.NormalizedOffset = value;
        }

        public float MinOffset => ScrollPosition?.Min ?? 0f;
        public float MaxOffset => ScrollPosition?.Max ?? float.MaxValue;

        public ScrollPosition ScrollPosition { get; private set; }

        public void JumpTo(float offset) {
            Offset = offset;
        }

        public void AnimateTo(float offset, TimeValue duration, EasingMode easing = EasingMode.Linear) {
            if (ScrollPosition == null) {
                Offset = offset; 
            } else {
                ScrollPosition.AnimateTo(offset, duration, easing);
            }
        }

        public void ScrollTo(VisualElement element) {
            
        }
        
        public void Attach(ScrollPosition position) {
            if (ScrollPosition != null) {
                HelixDiagnostics.Build(
                    "ScrollController is already attached to a ScrollPosition.",
                    description: "This error occurs when trying to attach a ScrollController to a ScrollPosition " +
                                 "while it is already attached to another ScrollPosition. " +
                                 "A ScrollController can only be attached to one ScrollPosition at a time. " +
                                 "To fix this issue, detach the ScrollController from the current ScrollPosition " +
                                 "before attaching it to a new one.",
                    details: new DiagnosticsNode[] {
                        new ErrorProperty("The current position is", ScrollPosition),
                        new ErrorProperty("The new position is", position)
                    }
                ).Report(DiagnosticLevel.Error);
                ScrollPosition.RemoveObserver(this);
                ScrollPosition = null;
            }

            ScrollPosition = position;
            position.AddObserver(this);
            
            if (KeepScrollOffset) {
                position.Restore(_lastOffset.GetValueOrDefault(position.Extent));
            } else {
                position.Restore(_initialScrollOffset);
            }
        }

        public void Detach(ScrollPosition position) {
            if (ScrollPosition != position) {
                HelixDiagnostics.Build(
                    "Attempting to detach ScrollController from a ScrollPosition it is not attached to.",
                    description: "This error occurs when trying to detach a ScrollController from a ScrollPosition " +
                                 "that it is not currently attached to. ",
                    details: new DiagnosticsNode[] {
                        new ErrorProperty("The current position is", ScrollPosition),
                        new ErrorProperty("The position being detached is", position)
                    }
                ).Report(DiagnosticLevel.Error);
                return;
            }
            
            ScrollPosition?.RemoveObserver(this);
        }

        public void OnSignalChanged(Signal signal) {
            if (signal != ScrollPosition) {
                HelixDiagnostics.Build(
                    "Received signal change from unexpected signal",
                    details: new DiagnosticsNode[] {
                        new ErrorProperty("The unexpected signal is", signal),
                        new ErrorProperty("The expected signal is", ScrollPosition)
                    }
                ).Report(DiagnosticLevel.Warning);
                return;
            }
            
            SetValue(ScrollPosition.Extent);
            _lastOffset = value;
        }

        public void OnSignalRemoved(Signal signal) {
            ScrollPosition = null;
        }
    }
}