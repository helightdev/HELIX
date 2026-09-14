using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Hix.Hal;
using Hix.Hal.Generated;
using Newtonsoft.Json;

namespace Hix.Standalone.Analysis;

/// Extracts containers without reserializing surrounding source or metadata.
internal static class HalGraphSections {
  public static string Extract(string text) {
    try {
      if (HalDocumentSyntax.Parse(text).Diagnostics.Count != 0) return text;
    } catch (Exception e) when (e is ArgumentException or NullReferenceException) { return text; }
    while (true) {
      var parser = new HalParser(new CommonTokenStream(new HalLexer(new AntlrInputStream(text))));
      parser.RemoveErrorListeners();
      var tree = parser.document();
      var container = Descendants(tree).Select(Container).FirstOrDefault(value => value != null);
      if (container == null) return text;
      var (expression, body, type, collection) = container.Value;
      var anchor = "node_" + Guid.NewGuid().ToString("N");
      var start = expression.Start.StartIndex; var end = body.Stop.StopIndex + 1;
      var raw = text.Substring(body.Start.StartIndex, end - body.Start.StartIndex);
      var content = body is HalParser.TableContext ? raw.Substring(1, raw.Length - 2).Trim() : raw;
      var reference = (collection ? "= " : "") + "ref(" + JsonConvert.ToString(anchor) + ")";
      text = text.Substring(0, start) + reference + text.Substring(end);
      text = text.TrimEnd() + "\n\n--- " + type + " &" + anchor + "\n" + content + "\n";
    }
  }
  private static (ParserRuleContext Expression, ParserRuleContext Body, string Type, bool Collection)? Container(IParseTree node) {
    if (node is HalParser.ValueContext value) {
      ParserRuleContext body = (ParserRuleContext)value.table() ?? value.list() ?? (ParserRuleContext)value.typedContainer()?.table() ?? value.typedContainer()?.list();
      if (body != null) return (value, body, value.IDENTIFIER()?.GetText() ?? (body is HalParser.TableContext ? "table" : "tuple"), false);
    }
    if (node is HalParser.CollectionValueContext collection) {
      ParserRuleContext body = (ParserRuleContext)collection.typedContainer().table() ?? collection.typedContainer().list();
      return (collection, body, collection.IDENTIFIER()?.GetText() ?? (body is HalParser.TableContext ? "table" : "tuple"), true);
    }
    return null;
  }
  private static IEnumerable<IParseTree> Descendants(IParseTree node) {
    yield return node;
    for (var i = 0; i < node.ChildCount; i++) foreach (var child in Descendants(node.GetChild(i))) yield return child;
  }
}
