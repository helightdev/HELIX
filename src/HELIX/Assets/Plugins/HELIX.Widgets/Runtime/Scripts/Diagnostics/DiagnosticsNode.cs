using System;
using System.Collections.Generic;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics {
  public delegate IEnumerable<DiagnosticsNode> DiagnosticsNodeBuilder();

  public abstract class DiagnosticsNode {
    public static readonly DiagnosticsNode Null = new DiagnosticsProperty<object>(
      "<null>",
      null,
      "<null>",
      style: DiagnosticsTreeStyle.SingleLine,
      showName: false,
      allowWrap: false,
      level: DiagnosticLevel.Error
    );

    protected DiagnosticsNode(
      string name,
      DiagnosticsTreeStyle? style = null,
      bool showName = true,
      bool showSeparator = true,
      string linePrefix = null
    ) {
      if (name != null && name.EndsWith(":", StringComparison.Ordinal))
        throw new ArgumentException("Diagnostic node names must not end with ':'.", nameof(name));

      Name = name;
      Style = style;
      ShowName = showName;
      ShowSeparator = showSeparator;
      LinePrefix = linePrefix;
    }

    public string Name { get; }
    public virtual object Value => null;
    public DiagnosticsTreeStyle? Style { get; }
    public bool ShowSeparator { get; }
    public bool ShowName { get; }
    public string LinePrefix { get; }
    public virtual bool AllowWrap => false;
    public virtual bool AllowNameWrap => false;
    public virtual bool AllowTruncate => false;
    public virtual string EmptyBodyDescription => null;
    public virtual DiagnosticLevel Level => DiagnosticLevel.Info;

    protected virtual TextTreeConfiguration TextTreeConfiguration {
      get {
        if (!Style.HasValue) return DiagnosticsTextConfigurations.Sparse;

        switch (Style.Value) {
          case DiagnosticsTreeStyle.Dense: return DiagnosticsTextConfigurations.Dense;
          case DiagnosticsTreeStyle.Sparse: return DiagnosticsTextConfigurations.Sparse;
          case DiagnosticsTreeStyle.Whitespace: return DiagnosticsTextConfigurations.Whitespace;
          case DiagnosticsTreeStyle.Transition: return DiagnosticsTextConfigurations.Transition;
          case DiagnosticsTreeStyle.SingleLine: return DiagnosticsTextConfigurations.SingleLine;
          case DiagnosticsTreeStyle.ErrorProperty: return DiagnosticsTextConfigurations.ErrorProperty;
          case DiagnosticsTreeStyle.Shallow: return DiagnosticsTextConfigurations.Shallow;
          case DiagnosticsTreeStyle.Error: return DiagnosticsTextConfigurations.Error;
          case DiagnosticsTreeStyle.Flat: return DiagnosticsTextConfigurations.Flat;
          case DiagnosticsTreeStyle.TruncateChildren: return DiagnosticsTextConfigurations.Whitespace;
          case DiagnosticsTreeStyle.None:
          default: return DiagnosticsTextConfigurations.Sparse;
        }
      }
    }

    public bool IsFiltered(DiagnosticLevel minLevel) {
      return (int)Level < (int)minLevel;
    }

    public abstract string ToDescription(TextTreeConfiguration parentConfiguration = null);
    public abstract List<DiagnosticsNode> GetProperties();
    public abstract List<DiagnosticsNode> GetChildren();

    public string ToStringDeep(
      string prefixLineOne = "",
      string prefixOtherLines = null,
      TextTreeConfiguration parentConfiguration = null,
      DiagnosticLevel minLevel = DiagnosticLevel.Debug,
      int wrapWidth = 65
    ) {
      return new TextTreeRenderer(minLevel, wrapWidth).Render(
        this,
        prefixLineOne,
        prefixOtherLines,
        parentConfiguration
      );
    }

    public override string ToString() {
      if (IsSingleLine(Style)) return ToStringDeep(parentConfiguration: null, minLevel: DiagnosticLevel.Info);

      var description = ToDescription();
      if (string.IsNullOrEmpty(Name) || !ShowName) return description;

      return description.Contains("\n")
        ? Name + (ShowSeparator ? ":" : "") + "\n" + description
        : Name + (ShowSeparator ? ":" : "") + " " + description;
    }

    public static DiagnosticsNode Message(
      string message,
      DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
      DiagnosticLevel level = DiagnosticLevel.Info,
      bool allowWrap = true
    ) {
      return new DiagnosticsProperty<object>(
        string.Empty,
        null,
        message,
        style: style,
        showName: false,
        allowWrap: allowWrap,
        level: level
      );
    }

    internal TextTreeConfiguration GetTextTreeConfiguration() {
      return TextTreeConfiguration;
    }

    internal static bool IsSingleLine(DiagnosticsTreeStyle? style) {
      return style == DiagnosticsTreeStyle.SingleLine;
    }
  }
}