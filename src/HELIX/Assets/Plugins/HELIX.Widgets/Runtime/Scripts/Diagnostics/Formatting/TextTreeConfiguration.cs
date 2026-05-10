namespace HELIX.Widgets.Diagnostics.Formatting {
  public sealed class TextTreeConfiguration {
    public TextTreeConfiguration(
      string prefixLineOne,
      string prefixOtherLines,
      string prefixLastChildLineOne,
      string prefixOtherLinesRootNode,
      string linkCharacter,
      string propertyPrefixIfChildren,
      string propertyPrefixNoChildren,
      string lineBreak = "\n",
      bool lineBreakProperties = true,
      string afterName = ":",
      string afterDescriptionIfBody = "",
      string afterDescription = "",
      string beforeProperties = "",
      string afterProperties = "",
      string mandatoryAfterProperties = "",
      string propertySeparator = "",
      string bodyIndent = "",
      string footer = "",
      bool showChildren = true,
      bool addBlankLineIfNoChildren = true,
      bool isNameOnOwnLine = false,
      bool isBlankLineBetweenPropertiesAndChildren = true,
      string beforeName = "",
      string suffixLineOne = "",
      string mandatoryFooter = ""
    ) {
      PrefixLineOne = prefixLineOne;
      PrefixOtherLines = prefixOtherLines;
      PrefixLastChildLineOne = prefixLastChildLineOne;
      PrefixOtherLinesRootNode = prefixOtherLinesRootNode;
      LinkCharacter = linkCharacter;
      PropertyPrefixIfChildren = propertyPrefixIfChildren;
      PropertyPrefixNoChildren = propertyPrefixNoChildren;
      LineBreak = lineBreak;
      LineBreakProperties = lineBreakProperties;
      AfterName = afterName;
      AfterDescriptionIfBody = afterDescriptionIfBody;
      AfterDescription = afterDescription;
      BeforeProperties = beforeProperties;
      AfterProperties = afterProperties;
      MandatoryAfterProperties = mandatoryAfterProperties;
      PropertySeparator = propertySeparator;
      BodyIndent = bodyIndent;
      Footer = footer;
      ShowChildren = showChildren;
      AddBlankLineIfNoChildren = addBlankLineIfNoChildren;
      IsNameOnOwnLine = isNameOnOwnLine;
      IsBlankLineBetweenPropertiesAndChildren = isBlankLineBetweenPropertiesAndChildren;
      BeforeName = beforeName;
      SuffixLineOne = suffixLineOne;
      MandatoryFooter = mandatoryFooter;
      ChildLinkSpace = new string(' ', linkCharacter.Length);
    }

    public string PrefixLineOne { get; }
    public string SuffixLineOne { get; }
    public string PrefixOtherLines { get; }
    public string PrefixLastChildLineOne { get; }
    public string PrefixOtherLinesRootNode { get; }
    public string PropertyPrefixIfChildren { get; }
    public string PropertyPrefixNoChildren { get; }
    public string LinkCharacter { get; }
    public string ChildLinkSpace { get; }
    public string LineBreak { get; }
    public bool LineBreakProperties { get; }
    public string BeforeName { get; }
    public string AfterName { get; }
    public string AfterDescriptionIfBody { get; }
    public string AfterDescription { get; }
    public string BeforeProperties { get; }
    public string AfterProperties { get; }
    public string MandatoryAfterProperties { get; }
    public string PropertySeparator { get; }
    public string BodyIndent { get; }
    public bool ShowChildren { get; }
    public bool AddBlankLineIfNoChildren { get; }
    public bool IsNameOnOwnLine { get; }
    public string Footer { get; }
    public string MandatoryFooter { get; }
    public bool IsBlankLineBetweenPropertiesAndChildren { get; }
  }

  // Example usage:
  //
  // public sealed class ExampleNode : DiagnosticableTreeBase
  // {
  //     public string Label;
  //     public int Count;
  //     public List<ExampleNode> Children = new List<ExampleNode>();
  //
  //     public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
  //     {
  //         properties.Add(new StringProperty("label", Label));
  //         properties.Add(new IntProperty("count", Count));
  //     }
  //
  //     public override List<DiagnosticsNode> DebugDescribeChildren()
  //     {
  //         return Children.Select(c => c.ToDiagnosticsNode()).ToList();
  //     }
  // }
}