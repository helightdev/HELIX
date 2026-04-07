using System;
using System.Collections;
using System.Collections.Generic;

namespace HELIX.Widgets {
    public class WidgetList : IReadOnlyList<Widget>, IWidgetListCandidate {
        public readonly List<Widget> widgets;

        public WidgetList() {
            widgets = new List<Widget>();
        }

        public WidgetList(List<Widget> widgets) {
            this.widgets = widgets;
        }

        public void Add(IWidgetListCandidate widget) {
            TryAdd(widget);
        }

        private void TryAdd(IWidgetListCandidate candidate) {
            while (true) {
                switch (candidate) {
                    case ConditionalCandidate conditional:
                        if (conditional.condition) {
                            candidate = conditional.candidate;
                            continue;
                        }

                        break;
                    case WidgetList list: widgets.AddRange(list.widgets); break;
                    case SpreadCandidate spread:
                        foreach (var c in spread.candidates) { TryAdd(c); }

                        break;
                    case Widget widget: widgets.Add(widget); break;
                    default: throw new ArgumentException("Value must be a Widget or WidgetList", nameof(candidate));
                }

                break;
            }
        }

        public IEnumerator<Widget> GetEnumerator() {
            return widgets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count => widgets.Count;

        public Widget this[int index] => widgets[index];

        public static implicit operator WidgetList(List<Widget> widgets) => new(widgets);
    }

    public interface IWidgetListCandidate { }

    public readonly struct ConditionalCandidate : IWidgetListCandidate {
        public readonly bool condition;
        public readonly IWidgetListCandidate candidate;

        public ConditionalCandidate(bool condition, IWidgetListCandidate candidate) {
            this.condition = condition;
            this.candidate = candidate;
        }
    }

    public readonly struct SpreadCandidate : IWidgetListCandidate {
        public readonly IEnumerable<IWidgetListCandidate> candidates;

        public SpreadCandidate(IEnumerable<IWidgetListCandidate> candidates) {
            this.candidates = candidates;
        }
    }

    public static class WidgetListExtensions {
        public static ConditionalCandidate If(this IWidgetListCandidate candidate, bool condition) =>
            new(condition, candidate);

        public static SpreadCandidate Spread(this IEnumerable<IWidgetListCandidate> candidates) => new(candidates);
    }
}