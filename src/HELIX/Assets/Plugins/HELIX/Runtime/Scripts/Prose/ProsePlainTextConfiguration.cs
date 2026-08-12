using System;

namespace HELIX.Prose {
  /// <summary>Controls how semantic Prose scopes are projected into plain-text trees.</summary>
  public sealed class ProsePlainTextConfiguration {
    public ProsePlainTextConfiguration(
      string childPrefix,
      string lastChildPrefix,
      string continuationPrefix,
      string lastContinuationPrefix,
      string propertyValueSeparator = ": ",
      string rootNamePrefix = "",
      string rootNameSuffix = "",
      string treeNamePrefix = "",
      string treeNameSuffix = "",
      string nameContinuationPrefix = "",
      string propertyPrefix = "",
      string propertyContinuationPrefix = "",
      string treeSeparator = "",
      bool alignWrappedPropertyValues = true,
      bool showTrees = true,
      bool showNames = true,
      bool showProperties = true,
      string lineBreak = "\n",
      bool lineBreakProperties = true,
      string wrappedLinePrefix = "",
      string explicitLineBreakPrefix = "",
      string beforeProperties = "",
      string afterProperties = "",
      string mandatoryAfterProperties = "",
      string propertySeparator = "",
      string beforeChildren = "",
      string footer = "",
      string mandatoryFooter = ""
    ) {
      ChildPrefix = childPrefix ?? throw new ArgumentNullException(nameof(childPrefix));
      LastChildPrefix = lastChildPrefix ?? throw new ArgumentNullException(nameof(lastChildPrefix));
      ContinuationPrefix = continuationPrefix ?? throw new ArgumentNullException(nameof(continuationPrefix));
      LastContinuationPrefix = lastContinuationPrefix ??
                               throw new ArgumentNullException(nameof(lastContinuationPrefix));
      if (ChildPrefix.Length != LastChildPrefix.Length ||
          ChildPrefix.Length != ContinuationPrefix.Length ||
          ChildPrefix.Length != LastContinuationPrefix.Length)
        throw new ArgumentException("All tree boundary prefixes must have the same character width.");

      PropertyValueSeparator = propertyValueSeparator ?? string.Empty;
      RootNamePrefix = rootNamePrefix ?? string.Empty;
      RootNameSuffix = rootNameSuffix ?? string.Empty;
      TreeNamePrefix = treeNamePrefix ?? string.Empty;
      TreeNameSuffix = treeNameSuffix ?? string.Empty;
      NameContinuationPrefix = nameContinuationPrefix ?? string.Empty;
      PropertyPrefix = propertyPrefix ?? string.Empty;
      PropertyContinuationPrefix = propertyContinuationPrefix ?? string.Empty;
      TreeSeparator = treeSeparator ?? string.Empty;
      LineBreak = lineBreak ?? throw new ArgumentNullException(nameof(lineBreak));
      if (LineBreak.Length == 0 && lineBreakProperties)
        throw new ArgumentException("A non-empty line break is required when properties break onto lines.");
      WrappedLinePrefix = wrappedLinePrefix ?? string.Empty;
      ExplicitLineBreakPrefix = explicitLineBreakPrefix ?? string.Empty;
      BeforeProperties = beforeProperties ?? string.Empty;
      AfterProperties = afterProperties ?? string.Empty;
      MandatoryAfterProperties = mandatoryAfterProperties ?? string.Empty;
      PropertySeparator = propertySeparator ?? string.Empty;
      BeforeChildren = beforeChildren ?? string.Empty;
      Footer = footer ?? string.Empty;
      MandatoryFooter = mandatoryFooter ?? string.Empty;
      LineBreakProperties = lineBreakProperties;
      AlignWrappedPropertyValues = alignWrappedPropertyValues;
      ShowTrees = showTrees;
      ShowNames = showNames;
      ShowProperties = showProperties;
    }

    public int BranchWidth => ChildPrefix.Length;
    public string ChildPrefix { get; }
    public string LastChildPrefix { get; }
    public string ContinuationPrefix { get; }
    public string LastContinuationPrefix { get; }
    public string PropertyValueSeparator { get; }
    public string RootNamePrefix { get; }
    public string RootNameSuffix { get; }
    public string TreeNamePrefix { get; }
    public string TreeNameSuffix { get; }
    public string NameContinuationPrefix { get; }
    public string PropertyPrefix { get; }
    public string PropertyContinuationPrefix { get; }
    public string TreeSeparator { get; }
    /// <summary>The physical separator emitted for semantic, explicit, and wrapping line breaks.</summary>
    public string LineBreak { get; }
    /// <summary>Whether each property is emitted as a separate entry line.</summary>
    public bool LineBreakProperties { get; }
    /// <summary>Additional prefix emitted only on automatically wrapped continuation lines.</summary>
    public string WrappedLinePrefix { get; }
    /// <summary>Additional prefix emitted only after an explicit line break in written prose.</summary>
    public string ExplicitLineBreakPrefix { get; }
    /// <summary>Injected once when an object has at least one property.</summary>
    public string BeforeProperties { get; }
    /// <summary>Injected after an object's properties only when at least one property was written.</summary>
    public string AfterProperties { get; }
    /// <summary>Injected after the property section even when it is empty.</summary>
    public string MandatoryAfterProperties { get; }
    /// <summary>Injected between adjacent properties when <see cref="LineBreakProperties"/> is false.</summary>
    public string PropertySeparator { get; }
    /// <summary>Injected once when an object has at least one accepted child tree.</summary>
    public string BeforeChildren { get; }
    /// <summary>Injected after child trees only when at least one child was written.</summary>
    public string Footer { get; }
    /// <summary>Injected when an object closes, whether or not it has children.</summary>
    public string MandatoryFooter { get; }
    public bool AlignWrappedPropertyValues { get; }
    public bool ShowTrees { get; }
    public bool ShowNames { get; }
    public bool ShowProperties { get; }
  }

  /// <summary>Common projections for plain-text consumers.</summary>
  public static class ProsePlainTextConfigurations {
    /// <summary>Full Unicode tree boundaries with retained ancestor lines.</summary>
    public static readonly ProsePlainTextConfiguration Unicode = new(
      childPrefix: "├─ ",
      lastChildPrefix: "└─ ",
      continuationPrefix: "│  ",
      lastContinuationPrefix: "   ",
      wrappedLinePrefix: " "
    );

    /// <summary>ASCII-only tree boundaries with retained ancestor lines.</summary>
    public static readonly ProsePlainTextConfiguration Ascii = new(
      childPrefix: "+- ",
      lastChildPrefix: "`- ",
      continuationPrefix: "|  ",
      lastContinuationPrefix: "   ",
      wrappedLinePrefix: " ",
      alignWrappedPropertyValues: false
    );

    /// <summary>
    /// Represents hierarchy using indentation alone. All tree content is retained, but no visible
    /// branch or continuation glyphs are emitted.
    /// </summary>
    public static readonly ProsePlainTextConfiguration Whitespace = new(
      childPrefix: "  ",
      lastChildPrefix: "  ",
      continuationPrefix: "  ",
      lastContinuationPrefix: "  ",
      wrappedLinePrefix: " "
    );

    /// <summary>
    /// Renders the complete hierarchy as a single flat sequence of lines. Tree frames remain visible,
    /// so this differs from <see cref="CurrentObjectFlat"/> when children exist.
    /// </summary>
    public static readonly ProsePlainTextConfiguration Flat = new(
      childPrefix: "",
      lastChildPrefix: "",
      continuationPrefix: "",
      lastContinuationPrefix: ""
    );

    /// <summary>
    /// Renders only names and properties of the current/root object as flat lines. Child tree frames
    /// are rejected at <c>BeginFrame</c>, allowing immediate-mode producers to skip their contents.
    /// </summary>
    public static readonly ProsePlainTextConfiguration CurrentObjectFlat = new(
      childPrefix: "",
      lastChildPrefix: "",
      continuationPrefix: "",
      lastContinuationPrefix: "",
      showTrees: false
    );
  }
}
