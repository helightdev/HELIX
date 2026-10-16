using System;
using System.Collections.Generic;
using System.Text;

namespace Helix.Weave;

/// <summary>Small helpers for the string joining that code emission repeats endlessly.</summary>
public static class Code {
  public static string Join<T>(IEnumerable<T> items, Func<T, string> select, string separator = ", ") {
    var builder = new StringBuilder();
    var first = true;
    foreach (var item in items) {
      var text = select(item);
      if (string.IsNullOrEmpty(text)) continue;
      if (!first) builder.Append(separator);
      builder.Append(text);
      first = false;
    }
    return builder.ToString();
  }

  public static string Join(IEnumerable<string> items, string separator = ", ") =>
    Join(items, static item => item, separator);

  public static string If(bool condition, string text) => condition ? text : "";
  public static string If(bool condition, string text, string otherwise) => condition ? text : otherwise;

  /// <summary><paramref name="text"/> followed by a space when non-empty — e.g. a `ref ` prefix.</summary>
  public static string Prefix(string text) => string.IsNullOrEmpty(text) ? "" : text + " ";

  /// <summary>Strips one level of common leading whitespace, so emitted blocks keep their shape.</summary>
  public static string Dedent(string text) {
    if (string.IsNullOrEmpty(text)) return text ?? "";
    var lines = text.Replace("\r\n", "\n").Split('\n');
    var indent = int.MaxValue;
    foreach (var line in lines) {
      if (line.Trim().Length == 0) continue;
      var count = 0;
      while (count < line.Length && (line[count] == ' ' || line[count] == '\t')) count++;
      if (count < indent) indent = count;
    }
    if (indent is 0 or int.MaxValue) return string.Join("\n", lines).Trim('\n');
    for (var i = 0; i < lines.Length; i++)
      lines[i] = lines[i].Length >= indent ? lines[i].Substring(indent) : lines[i].TrimStart();
    return string.Join("\n", lines).Trim('\n');
  }
}
