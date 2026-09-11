using Hix.Compiler;
using System.Linq;
using Xunit;

namespace Hix.Standalone.Tests;

public sealed class HixFormatterTests {
  [Fact]
  public void FormatsBlocksPatternsCallsBlankLinesAndFileEnding() {
    var formatted = HixFormatter.Format("""
      type Person=@{
      string name,
      number age
      }



      mixin Example{
      expression{
      local person=Person(name=<Ada>,age=2)
      }
      }


      """);

    Assert.Equal("""
      type Person = @{
        string name,
        number age
      }

      mixin Example {
        expression {
          local person = Person(name = <Ada>, age = 2)
        }
      }
      
      """, formatted);
    Assert.Equal(formatted, AntlrSyntax.Format(formatted));
  }

  [Fact]
  public void SmartRootsRemainAttachedToTheirIdentifiers() {
    var formatted = HixFormatter.Format(
      "func pair(tuple value) -> string { return(join($first:tuple,$second,$0#name,)) }\n");

    Assert.DoesNotContain("$ ", formatted);
    Assert.Contains("$first:tuple, $second, $0#name", formatted);
    Assert.Empty(AntlrSyntax.Parse(formatted).Diagnostics);
  }

  [Fact]
  public void ContentBlocksRemainOpaqueAndFollowingIndentationIsStable() {
    const string source = "mixin Example {\nexpression {\nemit @> Hello {{local#name}}\n  @+ world\nlocal next=1\n}\n}\n";

    var formatted = HixFormatter.Format(source);

    Assert.Contains("@> Hello {{local#name}}\n  @+ world\n", formatted);
    Assert.Contains("\n    local next = 1\n", formatted);
    Assert.Equal(formatted, HixFormatter.Format(formatted));
    Assert.Empty(AntlrSyntax.Parse(formatted).Diagnostics);
  }

  [Fact]
  public void ChoppedPatternTablesAndConstructorArgumentsAreIndentedByStructure() {
    const string source = "type Person = table\ntype Pet = table\n%tagged type House = @{\nstring address,\n%optional string tag,\n%many Person inhabitants,\n%many Pet pets\n}\n" +
      "func build() -> House {\nreturn(House(\naddress = <Road>,\ninhabitants = @[],\npets = @[]\n))\n}\n";

    var formatted = HixFormatter.Format(source);

    Assert.Contains("%tagged type House = @{\n  string address,\n  %optional string tag,\n" +
      "  %many Person inhabitants,\n  %many Pet pets\n}\n", formatted);
    Assert.Contains("return(House(\n      address = <Road>,\n      inhabitants = @[],\n      pets = @[]\n    ))", formatted);
    Assert.Equal(formatted, HixFormatter.Format(formatted));
    Assert.Empty(AntlrSyntax.Parse(formatted).Diagnostics);
  }

  [Fact]
  public void ArgumentSuffixesRemainAttachedToCallsAndMetadata() {
    const string source = "%name <House>\ntype Pet = %union<Dog> <Cat>\n" +
      "func main() -> null { writeFile <house.schema.json> <pretty>; return(null) }\n";

    var formatted = HixFormatter.Format(source);

    Assert.Contains("%name<House>", formatted);
    Assert.Contains("%union<Dog><Cat>", formatted);
    Assert.Contains("writeFile<house.schema.json>", formatted);
    Assert.Contains("writeFile<house.schema.json><pretty>", formatted);
    Assert.DoesNotContain("writeFile <", formatted);
    Assert.Equal(formatted, HixFormatter.Format(formatted));
  }

  [Fact]
  public void FileMetadataStaysAttachedToItsSectionDelimiter() {
    const string source = "%uid<houses/myhouse>\n\n\n---\nmixin House {}\n";

    var formatted = HixFormatter.Format(source);

    Assert.StartsWith("%uid<houses/myhouse>\n---\n", formatted);
    Assert.Equal(formatted, HixFormatter.Format(formatted));
  }

  [Fact]
  public void QuotedStringsAreAtomicDecodedValues() {
    var unit = AntlrSyntax.Parse("pure func value => \"line\\n\\\"quoted\\\" \\x41 {{not interpolation}}\";");

    Assert.Empty(unit.Diagnostics);
    var function = Assert.Single(unit.Declarations.OfType<FunctionDeclarationIr>());
    var returned = Assert.IsType<ControlFlowStatementIr>(Assert.Single(function.Body.Statements));
    var literal = Assert.IsType<StringExpressionIr>(Assert.Single(returned.Values));
    Assert.Equal("line\n\"quoted\" A {{not interpolation}}", literal.Value);
  }
}
