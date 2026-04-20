namespace HELIX.Widgets.Diagnostics.Formatting {
  public static class DiagnosticsTextConfigurations {
    public static readonly TextTreeConfiguration Sparse = new(
      "+-",
      "  ",
      "`-",
      linkCharacter: "|",
      propertyPrefixIfChildren: "| ",
      propertyPrefixNoChildren: "  ",
      prefixOtherLinesRootNode: "  "
    );

    public static readonly TextTreeConfiguration Dense = new(
      propertySeparator: ", ",
      beforeProperties: "(",
      afterProperties: ")",
      lineBreakProperties: false,
      prefixLineOne: "+",
      prefixOtherLines: "",
      prefixLastChildLineOne: "`",
      linkCharacter: "|",
      propertyPrefixIfChildren: "|",
      propertyPrefixNoChildren: " ",
      prefixOtherLinesRootNode: "",
      addBlankLineIfNoChildren: false,
      isBlankLineBetweenPropertiesAndChildren: false
    );

    public static readonly TextTreeConfiguration Transition = new(
      "+=+== ",
      prefixLastChildLineOne: "`=+== ",
      prefixOtherLines: " | ",
      footer: " `===========",
      linkCharacter: "|",
      propertyPrefixIfChildren: "",
      propertyPrefixNoChildren: "",
      prefixOtherLinesRootNode: "",
      afterName: " ==",
      afterDescriptionIfBody: ":",
      bodyIndent: "  ",
      isNameOnOwnLine: true,
      addBlankLineIfNoChildren: false,
      isBlankLineBetweenPropertiesAndChildren: false
    );

    public static readonly TextTreeConfiguration Error = new(
      "",
      prefixLastChildLineOne: "",
      prefixOtherLines: "",
      prefixOtherLinesRootNode: "",
      suffixLineOne: "",
      footer: "",
      mandatoryFooter: "",
      linkCharacter: "",
      propertyPrefixIfChildren: "",
      propertyPrefixNoChildren: "",
      beforeName: "",
      afterName: "",
      propertySeparator: "",
      afterDescriptionIfBody: "",
      bodyIndent: "",
      isNameOnOwnLine: false,
      addBlankLineIfNoChildren: false,
      isBlankLineBetweenPropertiesAndChildren: true
    );

    public static readonly TextTreeConfiguration Whitespace = new(
      "",
      prefixLastChildLineOne: "",
      prefixOtherLines: " ",
      prefixOtherLinesRootNode: "  ",
      propertyPrefixIfChildren: "",
      propertyPrefixNoChildren: "",
      linkCharacter: " ",
      addBlankLineIfNoChildren: false,
      afterDescriptionIfBody: ":",
      isBlankLineBetweenPropertiesAndChildren: false
    );

    public static readonly TextTreeConfiguration Flat = new(
      "",
      prefixLastChildLineOne: "",
      prefixOtherLines: "",
      prefixOtherLinesRootNode: "",
      propertyPrefixIfChildren: "",
      propertyPrefixNoChildren: "",
      linkCharacter: "",
      addBlankLineIfNoChildren: false,
      afterDescriptionIfBody: ":",
      isBlankLineBetweenPropertiesAndChildren: false
    );

    public static readonly TextTreeConfiguration SingleLine = new(
      propertySeparator: ", ",
      beforeProperties: "(",
      afterProperties: ")",
      prefixLineOne: "",
      prefixOtherLines: "",
      prefixLastChildLineOne: "",
      lineBreak: "",
      lineBreakProperties: false,
      addBlankLineIfNoChildren: false,
      showChildren: false,
      propertyPrefixIfChildren: "  ",
      propertyPrefixNoChildren: "  ",
      linkCharacter: "",
      prefixOtherLinesRootNode: ""
    );

    public static readonly TextTreeConfiguration ErrorProperty = new(
      propertySeparator: ", ",
      beforeProperties: "(",
      afterProperties: ")",
      prefixLineOne: "",
      prefixOtherLines: "",
      prefixLastChildLineOne: "",
      lineBreakProperties: false,
      addBlankLineIfNoChildren: false,
      showChildren: false,
      propertyPrefixIfChildren: "  ",
      propertyPrefixNoChildren: "  ",
      linkCharacter: "",
      prefixOtherLinesRootNode: "",
      isNameOnOwnLine: true
    );

    public static readonly TextTreeConfiguration Shallow = new(
      "",
      prefixLastChildLineOne: "",
      prefixOtherLines: " ",
      prefixOtherLinesRootNode: "  ",
      propertyPrefixIfChildren: "",
      propertyPrefixNoChildren: "",
      linkCharacter: " ",
      addBlankLineIfNoChildren: false,
      afterDescriptionIfBody: ":",
      isBlankLineBetweenPropertiesAndChildren: false,
      showChildren: false
    );
  }
}