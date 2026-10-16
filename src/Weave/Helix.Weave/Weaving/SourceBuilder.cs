using System;
using System.Collections.Generic;
using System.Text;

namespace Helix.Weave;

/// <summary>Indent-aware C# text builder. Same job as Hix's SharpStringBuilder.</summary>
public sealed class SourceBuilder {
  private readonly StringBuilder _builder = new();
  private readonly string _indentText;
  private bool _atLineStart = true;

  public SourceBuilder(string indentText = "  ") => _indentText = indentText;
  public int Indent { get; private set; }

  public SourceBuilder Append(string value) {
    if (string.IsNullOrEmpty(value)) return this;
    foreach (var character in value) {
      if (character == '\r') continue;
      if (character == '\n') { _builder.Append('\n'); _atLineStart = true; continue; }
      if (_atLineStart) { WriteIndent(); _atLineStart = false; }
      _builder.Append(character);
    }
    return this;
  }

  public SourceBuilder Line(string value = null) {
    if (!string.IsNullOrEmpty(value)) Append(value);
    _builder.Append('\n');
    _atLineStart = true;
    return this;
  }

  /// <summary>Appends a multi-line block, re-indenting every line to the current level.</summary>
  public SourceBuilder Block(string value) {
    if (string.IsNullOrWhiteSpace(value)) return this;
    foreach (var line in Code.Dedent(value).Split('\n')) Line(line.Length == 0 ? null : line);
    return this;
  }

  public IDisposable Braces(string header) {
    Line(header + " {");
    Indent++;
    return new Scope(this);
  }

  private void WriteIndent() {
    for (var i = 0; i < Indent; i++) _builder.Append(_indentText);
  }

  public override string ToString() => _builder.ToString();

  private sealed class Scope : IDisposable {
    private readonly SourceBuilder _owner;
    public Scope(SourceBuilder owner) => _owner = owner;
    public void Dispose() {
      _owner.Indent--;
      _owner.Line("}");
    }
  }
}
