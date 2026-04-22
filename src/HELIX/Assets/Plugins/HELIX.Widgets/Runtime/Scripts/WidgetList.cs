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

    /// <summary>
    /// Conditionally adds the given candidate to the widget list.
    /// </summary>
    /// <param name="candidate">The candidate to add.</param>
    /// <param name="condition">The condition under which to add the candidate.</param>
    public static ConditionalCandidate If(this IWidgetListCandidate candidate, bool condition) {
      return new ConditionalCandidate(condition, candidate);
    }

    /// <summary>
    /// Spreads the given enumerable of candidates into the widget list.
    /// </summary>
    public static SpreadCandidate Spread(this IEnumerable<IWidgetListCandidate> candidates) {
      return new SpreadCandidate(candidates);
    }

    /// <summary>
    /// Spreads the given enumerable of candidates into the widget list, with the given gap between each.
    /// </summary>
    /// <param name="gap">
    /// A dummy build function that will be used to create gaps between each candidate.
    /// The context parameter of the build function will be null.
    /// </param>
    /// <param name="candidates">The candidates to spread.</param>
    public static SpreadCandidate Spread(this IEnumerable<IWidgetListCandidate> candidates, BuildFunction gap) {
      return new SpreadCandidate(Interleave(candidates, gap));
    }

    private static IEnumerable<IWidgetListCandidate> Interleave(IEnumerable<IWidgetListCandidate> candidates, BuildFunction gap) {
      using var enumerator = candidates.GetEnumerator();
      if (!enumerator.MoveNext()) yield break;
      while (true) {
        yield return enumerator.Current;
        if (!enumerator.MoveNext()) break;
        yield return gap(null);
      }
    }
  }
}