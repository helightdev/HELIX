using Hix.Hal;
using Xunit;

namespace Hix.Standalone.Tests;

public sealed class HalFormatterTests {
  [Fact]
  public void FormatsSectionsTablesSpacingBlankLinesAndFileEnding() {
    var formatted = HalFormatter.Format("""
          --- Manifest
        name=sample
        sources={
        main=["src/**",   "other/**"],
        nested={first=1,second = 2}
        }



        """);

    Assert.Equal("""
      --- Manifest
      name = sample
      sources = {
        main = ["src/**", "other/**"],
        nested = {first = 1, second = 2}
      }
      
      """, formatted);
  }

  [Fact]
  public void SectionHeadersAlwaysReturnToColumnZeroAndBlankLinesAreCapped() {
    var formatted = HalDocumentSyntax.Format("--- First\nvalue={ x=1 }\n\n\n   --- Second &id\nother=2\n\n");

    Assert.Equal("--- First\nvalue = {x = 1}\n\n--- Second &id\nother = 2\n", formatted);
  }

  [Fact]
  public void SectionMetadataStaysAttachedToItsHeader() {
    var formatted = HalFormatter.Format("%uid<houses/myhouse>\n\n\n--- House & test\naddress=\"somewhere\"\n");

    Assert.StartsWith("%uid<houses/myhouse>\n--- House &test\n", formatted);
    Assert.Equal(formatted, HalFormatter.Format(formatted));
  }

  [Fact]
  public void SectionAnchorAmpersandStaysAttachedToItsIdentifier() {
    Assert.Equal("--- House &test\n", HalFormatter.Format("--- House & test\n"));
  }
}
