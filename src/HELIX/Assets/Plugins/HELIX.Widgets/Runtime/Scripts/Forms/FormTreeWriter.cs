using System.Collections.Generic;

namespace HELIX.Widgets.Forms {
  internal static class FormTreeWriter {
    public static void InsertValue(
      Dictionary<string, object> root,
      string path,
      IReadOnlyList<FormPathSegment> segments,
      object value,
      List<string> errors
    ) {
      object current = root;
      for (var i = 0; i < segments.Count; i++) {
        var segment = segments[i];
        var isLeaf = i == segments.Count - 1;
        var nextIsIndex = !isLeaf && segments[i + 1].IsIndex;

        if (segment.IsIndex) {
          errors.Add($"Path '{path}' cannot start with or assign through a list index without a list parent.");
          return;
        }

        var dict = current as Dictionary<string, object>;
        if (dict == null) {
          errors.Add($"Path '{path}' conflicts with an existing list value at '{segment.Name}'.");
          return;
        }

        if (isLeaf) {
          if (dict.TryGetValue(segment.Name, out var existing) && IsContainer(existing)) {
            errors.Add($"Path '{path}' conflicts with an existing container at '{segment.Name}'.");
            return;
          }

          dict[segment.Name] = value;
          return;
        }

        if (!dict.TryGetValue(segment.Name, out var child)) {
          child = nextIsIndex ? new List<object>() : new Dictionary<string, object>();
          dict[segment.Name] = child;
        } else if (!IsExpectedContainer(child, nextIsIndex)) {
          errors.Add($"Path '{path}' conflicts with an existing scalar at '{segment.Name}'.");
          return;
        }

        current = WalkContainer(child, segments, ref i, path, value, errors);
        if (current == null) return;
      }
    }

    private static object WalkContainer(
      object current,
      IReadOnlyList<FormPathSegment> segments,
      ref int index,
      string path,
      object value,
      List<string> errors
    ) {
      while (index + 1 < segments.Count && segments[index + 1].IsIndex) {
        index++;
        var segment = segments[index];
        var list = current as List<object>;
        if (list == null) {
          errors.Add($"Path '{path}' expected a list before index {segment.Index}.");
          return null;
        }

        while (list.Count <= segment.Index) list.Add(null);
        var isLeaf = index == segments.Count - 1;
        if (isLeaf) {
          if (IsContainer(list[segment.Index])) {
            errors.Add($"Path '{path}' conflicts with an existing container at index {segment.Index}.");
            return null;
          }

          list[segment.Index] = value;
          return null;
        }

        var nextIsIndex = segments[index + 1].IsIndex;
        var child = list[segment.Index];
        if (child == null) {
          child = nextIsIndex ? new List<object>() : new Dictionary<string, object>();
          list[segment.Index] = child;
        } else if (!IsExpectedContainer(child, nextIsIndex)) {
          errors.Add($"Path '{path}' conflicts with an existing scalar at index {segment.Index}.");
          return null;
        }

        current = child;
      }

      return current;
    }

    private static bool IsExpectedContainer(object value, bool list) {
      return list ? value is List<object> : value is Dictionary<string, object>;
    }

    private static bool IsContainer(object value) {
      return value is Dictionary<string, object> || value is List<object>;
    }
  }
}
