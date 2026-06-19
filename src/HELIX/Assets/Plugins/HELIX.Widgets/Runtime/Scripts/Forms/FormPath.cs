using System;
using System.Collections.Generic;
using System.Globalization;

namespace HELIX.Widgets.Forms {
  public static class FormPath {
    public static string Normalize(string path) {
      return path?.Trim('.') ?? string.Empty;
    }

    public static string Require(string path) {
      var normalized = Normalize(path);
      if (string.IsNullOrWhiteSpace(normalized)) throw new ArgumentException("Form path cannot be empty.", nameof(path));
      return normalized;
    }

    public static string Compose(string prefix, string path) {
      prefix = Normalize(prefix);
      path = Normalize(path);

      if (string.IsNullOrEmpty(prefix)) return path;
      if (string.IsNullOrEmpty(path)) return prefix;
      return prefix + "." + path;
    }

    public static string Item(string listPath, int index) {
      if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
      return Compose(listPath, index.ToString(CultureInfo.InvariantCulture));
    }

    public static bool IsInSubtree(string path, string prefix) {
      path = Normalize(path);
      prefix = Normalize(prefix);
      if (string.IsNullOrEmpty(prefix)) return true;
      return string.Equals(path, prefix, StringComparison.Ordinal)
             || path.StartsWith(prefix + ".", StringComparison.Ordinal);
    }

    public static bool TryParse(string path, out List<FormPathSegment> segments, out string error) {
      var parsed = new List<FormPathSegment>();
      segments = parsed;
      error = null;
      path = Normalize(path);

      if (string.IsNullOrWhiteSpace(path)) {
        error = "Form path cannot be empty.";
        return false;
      }

      foreach (var token in path.Split('.')) {
        if (string.IsNullOrWhiteSpace(token)) {
          error = $"Form path '{path}' contains an empty segment.";
          return false;
        }

        parsed.Add(ParseToken(token));
      }

      if (parsed.Count > 0) return true;
      error = "Form path cannot be empty.";
      return false;
    }

    public static bool TryGetListIndex(string listPath, string path, out int index) {
      index = -1;
      listPath = Normalize(listPath);
      path = Normalize(path);
      if (string.IsNullOrEmpty(listPath) || !path.StartsWith(listPath + ".", StringComparison.Ordinal)) return false;

      var start = listPath.Length + 1;
      var end = path.IndexOf('.', start);
      if (end < 0) end = path.Length;
      return int.TryParse(path.Substring(start, end - start), NumberStyles.None, CultureInfo.InvariantCulture, out index);
    }

    public static string ReplaceListIndex(string listPath, string path, int index) {
      if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
      listPath = Require(listPath);
      path = Require(path);

      var start = listPath.Length + 1;
      var end = path.IndexOf('.', start);
      if (end < 0) end = path.Length;
      return path.Substring(0, start)
             + index.ToString(CultureInfo.InvariantCulture)
             + path.Substring(end);
    }

    public static bool TryMoveListIndex(int index, int fromIndex, int toIndex, out int movedIndex) {
      movedIndex = index;
      if (index == fromIndex) {
        movedIndex = toIndex;
        return true;
      }

      if (fromIndex < toIndex && index > fromIndex && index <= toIndex) {
        movedIndex = index - 1;
        return true;
      }

      if (fromIndex > toIndex && index >= toIndex && index < fromIndex) {
        movedIndex = index + 1;
        return true;
      }

      return false;
    }

    private static FormPathSegment ParseToken(string token) {
      return int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index >= 0
        ? FormPathSegment.FromIndex(index)
        : FormPathSegment.FromName(token);
    }
  }

  public readonly struct FormPathSegment {
    public readonly string Name;
    public readonly int Index;
    public readonly bool IsIndex;

    private FormPathSegment(string name, int index, bool isIndex) {
      Name = name;
      Index = index;
      IsIndex = isIndex;
    }

    public static FormPathSegment FromName(string name) {
      return new FormPathSegment(name, -1, false);
    }

    public static FormPathSegment FromIndex(int index) {
      return new FormPathSegment(null, index, true);
    }
  }
}
