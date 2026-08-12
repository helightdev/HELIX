using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HELIX.Compose.Forms {
  /// <summary>A compact, controller-local address. It is only meaningful to the controller that created it.</summary>
  public readonly struct FormPath : IEquatable<FormPath> {
    internal readonly int id;
    internal FormPath(int id) => this.id = id;
    public bool IsRoot => id == 0;
    public bool IsValid => id >= 0;
    public bool Equals(FormPath other) => id == other.id;
    public override bool Equals(object obj) => obj is FormPath other && Equals(other);
    public override int GetHashCode() => id;
    public static bool operator ==(FormPath left, FormPath right) => left.Equals(right);
    public static bool operator !=(FormPath left, FormPath right) => !left.Equals(right);
    public override string ToString() => $"FormPath({id})";
  }

  internal enum FormPathPartKind : byte { Name, Index }

  internal struct FormPathPart : IEquatable<FormPathPart> {
    public int previous;
    public int current;
    public FormPathPartKind kind;

    public bool Equals(FormPathPart other) =>
      previous == other.previous && current == other.current && kind == other.kind;

    public override int GetHashCode() => unchecked((previous * 397) ^ current ^ (int)kind);
  }

  /// <summary>Interns path segments and chains. No string is retained by field/value dictionaries.</summary>
  public sealed class FormPathPool {
    private readonly List<string> _strings = new() { null };
    private readonly Dictionary<string, int> _stringIds = new(StringComparer.Ordinal);
    private readonly List<FormPathPart> _parts = new() { default };
    private readonly Dictionary<FormPathPart, int> _partIds = new();

    public FormPath Root => new(0);
    public int StringCount => _strings.Count - 1;
    public int ChainCount => _parts.Count - 1;

    public FormPath Append(FormPath previous, string current) {
      if (!previous.IsValid) throw new ArgumentException("Path belongs to no pool.", nameof(previous));
      if (string.IsNullOrWhiteSpace(current))
        throw new ArgumentException("Path segment cannot be empty.", nameof(current));
      return Intern(
        new FormPathPart { previous = previous.id, current = InternString(current), kind = FormPathPartKind.Name }
      );
    }

    public FormPath Append(FormPath previous, int current) {
      if (!previous.IsValid) throw new ArgumentException("Path belongs to no pool.", nameof(previous));
      if (current < 0) throw new ArgumentOutOfRangeException(nameof(current));
      return Intern(new FormPathPart { previous = previous.id, current = current, kind = FormPathPartKind.Index });
    }

    public FormPath Parse(string path, bool require = true) {
      return ParseRelative(Root, path, require);
    }

    /// <summary>Parses a dotted path below an existing chain. Intended for composition-time setup only.</summary>
    public FormPath ParseRelative(FormPath previous, string path, bool require = true) {
      if (!previous.IsValid) throw new ArgumentException("Path belongs to no pool.", nameof(previous));
      if (string.IsNullOrWhiteSpace(path)) {
        if (require) throw new ArgumentException("Form path cannot be empty.", nameof(path));
        return previous;
      }
      var result = previous;
      var start = 0;
      for (var i = 0; i <= path.Length; i++) {
        if (i != path.Length && path[i] != '.') continue;
        if (i == start) throw new ArgumentException($"Form path '{path}' contains an empty segment.", nameof(path));
        var token = path.Substring(start, i - start);
        result = int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index >= 0
          ? Append(result, index)
          : Append(result, token);
        start = i + 1;
      }
      return result;
    }

    public bool IsInSubtree(FormPath path, FormPath prefix) {
      for (var current = path.id; current != 0; current = _parts[current].previous)
        if (current == prefix.id)
          return true;
      return prefix.id == 0;
    }

    public FormPath Parent(FormPath path) => path.id == 0 ? Root : new FormPath(_parts[path.id].previous);

    /// <summary>Rebuilds only the suffix of a path; it never formats or reparses text.</summary>
    public FormPath ReplaceDirectIndex(FormPath list, FormPath path, int index) {
      if (!TryGetDirectIndex(list, path, out _))
        throw new ArgumentException("Path is not below the supplied list.", nameof(path));
      var suffix = new Stack<FormPathPart>();
      var current = path.id;
      while (_parts[current].previous != list.id) {
        suffix.Push(_parts[current]);
        current = _parts[current].previous;
      }
      var result = Append(list, index);
      while (suffix.Count > 0) {
        var part = suffix.Pop();
        result = part.kind == FormPathPartKind.Index
          ? Append(result, part.current)
          : Append(result, _strings[part.current]);
      }
      return result;
    }

    public bool TryGetDirectIndex(FormPath list, FormPath path, out int index) {
      index = -1;
      for (var current = path.id; current != 0; current = _parts[current].previous) {
        var part = _parts[current];
        if (part.previous != list.id) continue;
        if (part.kind != FormPathPartKind.Index) return false;
        index = part.current;
        return true;
      }
      return false;
    }

    public string Format(FormPath path) {
      if (path.id == 0) return string.Empty;
      var stack = new Stack<FormPathPart>();
      for (var current = path.id; current != 0; current = _parts[current].previous) stack.Push(_parts[current]);
      var text = new StringBuilder();
      while (stack.Count > 0) {
        if (text.Length > 0) text.Append('.');
        var part = stack.Pop();
        if (part.kind == FormPathPartKind.Index) text.Append(part.current);
        else text.Append(_strings[part.current]);
      }
      return text.ToString();
    }

    private int InternString(string value) {
      if (_stringIds.TryGetValue(value, out var id)) return id;
      id = _strings.Count;
      _strings.Add(value);
      _stringIds.Add(value, id);
      return id;
    }

    private FormPath Intern(FormPathPart part) {
      if (_partIds.TryGetValue(part, out var id)) return new FormPath(id);
      id = _parts.Count;
      _parts.Add(part);
      _partIds.Add(part, id);
      return new FormPath(id);
    }
  }
}
