using System;
using System.Collections;
using System.Collections.Generic;

namespace HELIX.Widgets {
    public class WidgetList : IReadOnlyList<Widget>, IWidgetListCandidate {
        public readonly List<Widget> widgets;

        public WidgetList(int capacity = 1) : this(new List<Widget>(capacity)) { }

        public WidgetList(List<Widget> widgets) {
            this.widgets = widgets;
        }

        public IEnumerator<Widget> GetEnumerator() {
            return widgets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count => widgets.Count;

        public Widget this[int index] => widgets[index];

        public void Add(IWidgetListCandidate widget) {
            TryAdd(widget);
        }

        private void TryAdd(IWidgetListCandidate candidate) {
            while (true) {
                switch (candidate) {
                    case EmptyCandidate:
                    case null: break;
                    case ConditionalCandidate conditional:
                        if (conditional.condition) {
                            candidate = conditional.candidate;
                            continue;
                        }

                        break;
                    case WidgetList list: widgets.AddRange(list.widgets); break;
                    case SpreadCandidate spread:
                        foreach (var c in spread.candidates) TryAdd(c);
                        break;
                    case Widget widget: widgets.Add(widget); break;
                    default:
                        throw new ArgumentException(
                            $"Value must be a Widget or WidgetList, was {candidate.GetType()}",
                            nameof(candidate)
                        );
                }

                break;
            }
        }

        public static implicit operator WidgetList(List<Widget> widgets) {
            return new WidgetList(widgets);
        }
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

    public readonly struct EmptyCandidate : IWidgetListCandidate { }

    public readonly struct SpreadCandidate : IWidgetListCandidate {
        public readonly IEnumerable<IWidgetListCandidate> candidates;

        public SpreadCandidate(IEnumerable<IWidgetListCandidate> candidates) {
            this.candidates = candidates;
        }
    }

    public static class WidgetListExtensions {
        public static ConditionalCandidate If(this IWidgetListCandidate candidate, bool condition) {
            return new ConditionalCandidate(condition, candidate);
        }

        public static SpreadCandidate Spread(this IEnumerable<IWidgetListCandidate> candidates) {
            return new SpreadCandidate(candidates);
        }
    }
}