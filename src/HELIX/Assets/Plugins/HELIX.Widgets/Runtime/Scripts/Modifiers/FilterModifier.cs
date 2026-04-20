using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
  public class FilterModifier : Modifier {
    public readonly List<FilterFunction> filters;

    public FilterModifier(List<FilterFunction> filters) {
      this.filters = filters;
    }

    public override void Apply(VisualElement element) {
      element.style.filter = filters;
    }

    public override void Reset(VisualElement element) {
      element.style.filter = null;
    }

    public override bool HasChanged(Modifier previous) {
      if (previous is not FilterModifier prev) return true;
      if (filters == null || prev.filters == null) return prev.filters != filters;
      return !filters.SequenceEqual(prev.filters);
    }

    public static FilterModifier Of(params FilterFunction[] filters) {
      return new FilterModifier(filters.ToList());
    }
  }
}