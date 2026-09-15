using Antlr4.Runtime;
using Hix.Compiler;
using Hix.Hal;
using Hix.Hal.Generated;
using Newtonsoft.Json;

namespace Hix.Standalone.Analysis;

public sealed record HalGraphPort(string Id, string Name, string Type, string Text, bool Optional, bool HasDefault);
public sealed record HalGraphPosition(double X, double Y);
public sealed record HalGraphPlacement(string Node, double X, double Y);
public sealed record HalGraphNode(string Id, string Type, string Kind, string SectionId, IReadOnlyList<HalGraphPort> Ports, HalGraphPosition Position = null);
public sealed record HalGraphEdge(string Id, string Source, string Target, string TargetPort);
public sealed record HalGraphType(string Name, string Kind, IReadOnlyList<HalGraphPort> Fields);
public sealed record HalGraphCandidate(string Type, string Kind, string Port, string Label);
public sealed record HalGraphSnapshot(string Session, string Path, string Workspace, long Revision, bool Dirty,
  string Text, IReadOnlyList<HalGraphNode> Nodes, IReadOnlyList<HalGraphEdge> Edges,
  IReadOnlyList<HalGraphType> Types, IReadOnlyList<string> Diagnostics, bool CanUndo, bool CanRedo);
public sealed record HalGraphEdit(string Operation, string Node = null, string Port = null,
  string Value = null, string Source = null, string Direction = null, string ReceiverPort = null, double? X = null, double? Y = null, IReadOnlyList<HalGraphPlacement> Positions = null);

/// Revisioned HAL documents. Graph identities are structural paths within a revision; clients must
/// submit the revision with every mutation. Source-range edits preserve unrelated trivia verbatim.
public sealed class HalGraphService {
  private readonly object gate = new();
  private readonly Dictionary<string, Session> sessions = new();
  public HalGraphSnapshot Open(string path) {
    lock (gate) {
      path = Path.GetFullPath(path);
      if (!path.EndsWith(".hal", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Expected a .hal asset");
      if (!Path.GetFileName(path).Equals("manifest.hal", StringComparison.OrdinalIgnoreCase) && !HixWorkspace.Find(path).IncludesAsset(path))
        throw new ArgumentException("Asset is outside manifest sources.assets: " + path);
      var session = new Session(path, File.ReadAllText(path));
      if (!Path.GetFileName(path).Equals("manifest.hal", StringComparison.OrdinalIgnoreCase))
        Commit(session, HalGraphSections.Extract(session.Text));
      sessions.Add(session.Id, session);
      return Snapshot(session);
    }
  }
  public string PathOf(string id) { lock (gate) return Find(id).Path; }
  public HalGraphSnapshot Get(string id) { lock (gate) return Snapshot(Find(id)); }
  public void Close(string id, long revision) { lock (gate) { Check(id, revision); sessions.Remove(id); } }
  public HalGraphSnapshot Change(string id, long revision, string text) {
    lock (gate) { var s = Check(id, revision); Commit(s, Normalize(s, text ?? "")); return Snapshot(s); }
  }
  public HalGraphSnapshot Undo(string id, long revision, bool redo = false) {
    lock (gate) {
      var s = Check(id, revision); var from = redo ? s.Redo : s.Undo; var to = redo ? s.Undo : s.Redo;
      if (from.Count == 0) throw new ArgumentException("No history available");
      to.Push(s.Text); s.Text = from.Pop(); s.Revision++; return Snapshot(s);
    }
  }
  public HalGraphSnapshot Save(string id, long revision) {
    lock (gate) {
      var s = Check(id, revision);
      if (File.ReadAllText(s.Path) != s.Saved) throw new InvalidOperationException("File changed on disk; reopen before saving");
      var temporary = s.Path + "." + Guid.NewGuid().ToString("N") + ".tmp";
      try { File.WriteAllText(temporary, s.Text); File.Replace(temporary, s.Path, null); }
      finally { if (File.Exists(temporary)) File.Delete(temporary); }
      s.Saved = s.Text;
      if (!Path.GetFileName(s.Path).Equals("manifest.hal", StringComparison.OrdinalIgnoreCase)) {
        using var database = AssetDatabase.Open(s.Path); database.Index(s.Path);
      }
      return Snapshot(s);
    }
  }
  public IReadOnlyList<HalGraphCandidate> Candidates(string id, long revision, string node, string port, string direction) {
    lock (gate) { var s = Check(id, revision); return Candidates(Build(s), node, port, direction); }
  }
  private static IReadOnlyList<HalGraphCandidate> Candidates(Graph graph, string node, string port, string direction) {
    if (!graph.NodePatterns.TryGetValue(node ?? "", out var output)) throw new ArgumentException("Unknown node");
    var result = new List<HalGraphCandidate>();
    if (direction == "supplier") {
      if (!graph.Ports.TryGetValue(PortKey(node, port ?? ""), out var input)) throw new ArgumentException("Unknown input");
      foreach (var type in Catalog(graph))
        if (Compatible(HixPatterns.Named(type.Name), input.Pattern, graph))
          result.Add(new(type.Name, type.Kind, null, type.Name == "tuple" ? "List" : type.Name == "table" ? "Table" : type.Name));
    } else if (direction == "receiver") {
      foreach (var type in Catalog(graph)) {
        var pattern = HixPatterns.Named(type.Name);
        if (type.Kind == "list") {
          if (Compatible(output, Element(pattern, graph.Patterns, 0), graph)) result.Add(new(type.Name, "list", "+", type.Name + " · append item"));
        } else {
          foreach (var field in Fields(pattern, graph.Patterns))
            if (Compatible(output, field.Pattern, graph)) result.Add(new(type.Name, type.Kind, field.Name, type.Name + " · " + field.Name));
          if (type.Name == "table") result.Add(new("table", "table", "value", "Table · new field"));
        }
      }
    } else throw new ArgumentException("Expected supplier or receiver direction");
    return result;
  }
  public HalGraphSnapshot Edit(string id, long revision, HalGraphEdit edit) {
    lock (gate) {
      var s = Check(id, revision); var graph = Build(s);
      if (graph.SyntaxError && !(edit.Operation == "add" && string.IsNullOrWhiteSpace(s.Text))) throw new ArgumentException("Correct source syntax before editing the graph");
      var candidate = Apply(s, graph, edit);
      // All containers introduced by graph actions become independently referenced sections.
      candidate = Normalize(s, candidate);
      Commit(s, candidate);
      return Snapshot(s);
    }
  }
  private static string Apply(Session s, Graph graph, HalGraphEdit edit) {
    if (edit.Operation == "layout") {
      var positions = edit.Positions ?? throw new ArgumentException("Missing node positions");
      if (positions.Select(p => p.Node).Distinct().Count() != positions.Count || positions.Any(p => !graph.Spans.ContainsKey(p.Node))) throw new ArgumentException("Invalid layout nodes");
      var text = s.Text;
      foreach (var position in positions.OrderByDescending(p => graph.Spans[p.Node].Start))
        text = Apply(new Session(s.Path, text), graph, new("move", position.Node, X: position.X, Y: position.Y));
      return text;
    }
    if (edit.Operation is "add" or "createConnected") {
      HixPattern context = null;
      if (edit.Operation == "createConnected") {
        var allowed = Candidates(graph, edit.Node, edit.Port, edit.Direction);
        if (!allowed.Any(c => c.Type == edit.Value && c.Port == edit.ReceiverPort)) throw new ArgumentException("This node cannot supply or receive the selected port");
        if (edit.Direction == "supplier") context = graph.Ports[PortKey(edit.Node, edit.Port)].Pattern;
      }
      var type = Catalog(graph).FirstOrDefault(t => t.Name == edit.Value) ?? throw new ArgumentException("Unknown section type");
      var anchor = "node_" + Guid.NewGuid().ToString("N");
      var metadata = edit.X.HasValue && edit.Y.HasValue ? PositionMetadata(edit.X.Value, edit.Y.Value) : "";
      var body = type.Kind == "list" ? "[]\n" : "";
      var text = s.Text.TrimEnd() + "\n\n" + metadata + "--- " + type.Name + " &" + anchor + "\n" + body;
      if (edit.Operation == "add") return text;
      var draft = new Session(s.Path, text); var next = Build(draft); var created = NodeId(anchor);
      // A generic supplier adopts the receiving shape for connection checking and its inspector.
      if (context != null && type.Name is "table" or "tuple") next.NodePatterns[created] = context;
      if (edit.Direction == "supplier") return Apply(draft, next, new("connect", edit.Node, edit.Port, Source: created));
      if (type.Name == "table") {
        text = Apply(draft, next, new("addField", created, edit.ReceiverPort, "null"));
        draft = new Session(s.Path, text); next = Build(draft);
      }
      return Apply(draft, next, new("connect", created, edit.ReceiverPort, Source: edit.Node));
    }
    if (!graph.Spans.TryGetValue(edit.Node ?? "", out var node)) throw new ArgumentException("Unknown node");
    if (edit.Operation == "move") {
      if (!edit.X.HasValue || !edit.Y.HasValue) throw new ArgumentException("A position requires x and y");
      var metadata = PositionMetadata(edit.X.Value, edit.Y.Value);
      var text = s.Text;
      foreach (var span in graph.PositionMetadata[edit.Node].OrderByDescending(span => span.Start)) text = Replace(text, span.Start, span.End, "");
      return Replace(text, node.Start, node.Start, metadata);
    }
    if (edit.Operation == "remove") {
      if (graph.Edges.Any(edge => edge.Source == edit.Node)) throw new ArgumentException("Disconnect incoming references before removing this section");
      return Replace(s.Text, node.Start, node.End, "");
    }
    if (edit.Operation == "addField") {
      if (node.Kind != "table" || string.IsNullOrWhiteSpace(edit.Port)) throw new ArgumentException("Choose a table and a field name");
      if (graph.Ports.ContainsKey(PortKey(edit.Node, edit.Port))) throw new ArgumentException("Field already exists");
      graph.Ports[PortKey(edit.Node, edit.Port)] = new(node.End, node.End, edit.Port, true, "", HixPattern.Any);
      edit = edit with { Operation = "set", Value = edit.Value ?? "null" };
    }
    if (!graph.Ports.TryGetValue(PortKey(edit.Node, edit.Port ?? ""), out var port)) throw new ArgumentException("Unknown port");
    if (edit.Operation == "removeItem") {
      if (node.Kind != "list" || port.Insert) throw new ArgumentException("Choose an existing list item");
      var removed = Replace(s.Text, port.Start, port.End, "");
      if (graph.Commas.TryGetValue(PortKey(edit.Node, edit.Port), out var comma))
        removed = removed.Remove(comma >= port.End ? comma - (port.End - port.Start) : comma, 1);
      return removed;
    }
    string value;
    if (edit.Operation == "connect") {
      if (!graph.Spans.TryGetValue(edit.Source ?? "", out var source)) throw new ArgumentException("Unknown source");
      if (!Compatible(graph.NodePatterns[edit.Source], port.Pattern, graph)) throw new ArgumentException("Incompatible port types");
      var pending = new Stack<string>(); var visited = new HashSet<string>(); pending.Push(edit.Source);
      while (pending.Count != 0) {
        var current = pending.Pop(); if (current == edit.Node) throw new ArgumentException("Cyclic section reference");
        if (!visited.Add(current)) continue;
        foreach (var edge in graph.Edges.Where(e => e.Target == current)) pending.Push(edge.Source);
      }
      value = "ref(" + JsonConvert.ToString(source.SectionId) + ")";
    } else if (edit.Operation == "disconnect") value = "null";
    else if (edit.Operation == "set") value = edit.Value ?? "null";
    else throw new ArgumentException("Unknown graph operation");
    var probe = HalDocumentSyntax.Parse("--- table\nvalue = " + value + "\n");
    if (probe.Diagnostics.Count != 0 || probe.Sections.Count != 1 || probe.Sections[0].Fields.Count != 1)
      throw new ArgumentException("Expected one HAL value");
    var replacement = port.Insert ? (node.Kind == "list" ? port.Separator + value : "\n" + JsonConvert.ToString(port.Name) + " = " + value + "\n") : value;
    var candidate = Replace(s.Text, port.Start, port.End, replacement);
    if (HalDocumentSyntax.Parse(candidate).Diagnostics.Count != 0) throw new ArgumentException("Invalid field syntax");
    return candidate;
  }
  private static string PositionMetadata(double x, double y) {
    if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y)) throw new ArgumentException("Position coordinates must be finite");
    return "%graphPosition(x=" + x.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + ", y=" + y.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + ")\n";
  }
  private static bool Compatible(HixPattern actual, HixPattern expected, Graph graph) =>
    HixPatternRelations.Relate(actual, expected, graph.Patterns) != HixPatternRelation.Never;
  private static string NodeId(string anchor) => "section/" + Uri.EscapeDataString(anchor);
  private static string Normalize(Session s, string text) => Path.GetFileName(s.Path).Equals("manifest.hal", StringComparison.OrdinalIgnoreCase) ? text : HalGraphSections.Extract(text);
  private static string PortKey(string node, string port) => node + "/port/" + Uri.EscapeDataString(port);
  private Session Find(string id) => sessions.TryGetValue(id ?? "", out var s) ? s : throw new ArgumentException("Unknown session");
  private Session Check(string id, long revision) { var s = Find(id); if (s.Revision != revision) throw new InvalidOperationException("Stale revision; refresh the document"); return s; }
  private static void Commit(Session s, string text) { if (s.Text == text) return; s.Undo.Push(s.Text); s.Redo.Clear(); s.Text = text; s.Revision++; }
  private static string Replace(string text, int start, int end, string value) => text.Substring(0, start) + value + text.Substring(end);
  private static HalGraphSnapshot Snapshot(Session s) {
    var g = Build(s);
    return new(s.Id, s.Path, g.Workspace, s.Revision, s.Text != s.Saved, s.Text, g.Nodes, g.Edges,
      Catalog(g),
      g.Diagnostics, s.Undo.Count > 0, s.Redo.Count > 0);
  }
  private static IReadOnlyList<HixPatternField> Fields(HixPattern pattern, IReadOnlyDictionary<string, HixPattern> patterns) =>
    HixPatterns.TryStructural(pattern, patterns, out var shape) && shape is TableHixPattern table ? table.Fields : [];
  private static HalGraphPort Port(string name, HixPatternField field, string text) =>
    new(name, name, field?.Pattern.Display ?? "any", text, field?.AllowsMissing ?? true, field?.HasDefault ?? false);
  private static Graph Build(Session s) {
    var g = new Graph();
    try {
      using var database = AssetDatabase.Open(s.Path);
      var analysis = database.Analyze();
      g.Workspace = analysis.Workspace;
      g.Patterns = analysis.Patterns;
      g.Diagnostics.AddRange(analysis.Diagnostics);
    } catch (Exception e) when (e is ArgumentException or IOException or NotSupportedException) { g.Diagnostics.Add("Workspace: " + e.Message); }
    HalDocumentSyntax syntax;
    try { syntax = HalDocumentSyntax.Parse(s.Text); }
    catch (Exception e) when (e is NullReferenceException or ArgumentException) { g.SyntaxError = true; g.Diagnostics.Add("Incomplete HAL syntax: " + e.Message); return g; }
    if (syntax.Diagnostics.Count != 0) { g.SyntaxError = true; g.Diagnostics.AddRange(syntax.Diagnostics.Select(d => $"{d.Line}:{d.Column}: {d.Message}")); return g; }
    try {
      var references = AssetDatabase.References(syntax).Distinct().ToArray();
      var values = new Dictionary<HalAssetReference, AssetBuild>();
      if (references.Length != 0) {
        using var database = AssetDatabase.Open(s.Path);
        foreach (var path in references) values[path] = database.Resolve(s.Path, path);
      }
      var failure = HalFunctions.Validate(syntax, g.Patterns, reference => values[reference].Values.TryGetValue(reference.Section ?? values[reference].PrimarySection, out var value)
        ? value : throw new ArgumentException("Unknown external section: " + reference.Value + " &" + reference.Section));
      if (failure != null) g.Diagnostics.Add(failure);
    } catch (Exception e) when (e is ArgumentException or IOException) { g.Diagnostics.Add(e.Message); }
    var parser = new HalParser(new CommonTokenStream(new HalLexer(new AntlrInputStream(s.Text))));
    parser.RemoveErrorListeners(); var blocks = parser.document().sectionBlock();
    var ids = new Dictionary<string, string>();
    for (var i = 0; i < blocks.Length; i++) {
      var anchor = syntax.Sections[i].Id ?? "missing-" + i;
      ids[anchor] = NodeId(anchor); g.NodePatterns[NodeId(anchor)] = HixPatterns.Named(syntax.Sections[i].Type);
    }
    // Infer the element/field schema of primitive collection sections from their receiving ports.
    for (var pass = 0; pass < blocks.Length; pass++) {
      for (var i = 0; i < blocks.Length; i++) {
        var info = syntax.Sections[i]; var pattern = g.NodePatterns[ids[info.Id ?? "missing-" + i]];
        var schema = Fields(pattern, g.Patterns);
        if (info.List != null) {
          for (var j = 0; j < info.List.Values.Count; j++) Infer(info.List.Values[j], Element(pattern, g.Patterns, j));
        } else foreach (var field in info.Fields) Infer(field.Value, schema.FirstOrDefault(f => f.Name == field.Name)?.Pattern ?? HixPattern.Any);
      }
    }
    for (var i = 0; i < blocks.Length; i++) {
      var section = blocks[i].section(); var info = syntax.Sections[i]; var anchor = info.Id ?? "missing-" + i; var id = ids[anchor];
      var end = i + 1 < blocks.Length ? blocks[i + 1].Start.StartIndex : s.Text.Length;
      var kind = section.list() == null ? "table" : "list";
      g.Spans[id] = new(blocks[i].Start.StartIndex, end, kind, anchor);
      var pattern = g.NodePatterns[id]; var ports = new List<HalGraphPort>();
      if (section.list() is { } list) {
        var values = list.value();
        for (var j = 0; j < values.Length; j++) {
          AddPort(id, j.ToString(), new(j.ToString(), Element(pattern, g.Patterns, j)), values[j], ports);
          var comma = list.COMMA().FirstOrDefault(c => c.Symbol.StartIndex > values[j].Stop.StopIndex && (j + 1 == values.Length || c.Symbol.StartIndex < values[j + 1].Start.StartIndex))
            ?? list.COMMA().LastOrDefault(c => c.Symbol.StartIndex < values[j].Start.StartIndex && (j == 0 || c.Symbol.StartIndex > values[j - 1].Stop.StopIndex));
          if (comma != null) g.Commas[PortKey(id, j.ToString())] = comma.Symbol.StartIndex;
        }
        var element = Element(pattern, g.Patterns, values.Length);
        ports.Add(Port("+", new("+", element, true), null));
        var insert = list.RBRACKET().Symbol.StartIndex;
        var trailing = values.Length != 0 && list.COMMA().Any(c => c.Symbol.StartIndex > values.Last().Stop.StopIndex);
        g.Ports[PortKey(id, "+")] = new(insert, insert, "+", true, values.Length == 0 || trailing ? "" : ", ", element);
      } else {
        var schema = Fields(pattern, g.Patterns);
        foreach (var entry in section.sectionEntry()) {
          var field = entry.field(); var name = field.fieldKey().STRING() == null ? field.fieldKey().GetText() : JsonConvert.DeserializeObject<string>(field.fieldKey().GetText());
          AddPort(id, name, schema.FirstOrDefault(f => f.Name == name), (ParserRuleContext)field.value() ?? field.collectionValue(), ports);
        }
        foreach (var field in schema.Where(f => ports.All(p => p.Name != f.Name))) {
          ports.Add(Port(field.Name, field, null)); g.Ports[PortKey(id, field.Name)] = new(end, end, field.Name, true, "", field.Pattern);
        }
      }
      HalGraphPosition position = null;
      var metadata = blocks[i].metadata().Where(m => m.IDENTIFIER()?.GetText() == "graphPosition").ToArray();
      g.PositionMetadata[id] = metadata.Select(m => new Span(m.Start.StartIndex, m.Stop.StopIndex + 1, "metadata", null)).ToArray();
      foreach (var item in metadata) {
        var coordinates = new Dictionary<string, double>();
        foreach (var argument in item.metadataArguments()?.argumentList()?.argument() ?? [])
          if (argument.EQUALS() != null && double.TryParse(argument.value()?.GetText(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var coordinate) && !double.IsInfinity(coordinate) && !double.IsNaN(coordinate))
            coordinates[argument.IDENTIFIER().GetText()] = coordinate;
        if (coordinates.TryGetValue("x", out var x) && coordinates.TryGetValue("y", out var y)) position = new(x, y);
      }
      g.Nodes.Add(new(id, info.Type, kind, anchor, ports, position));
    }
    return g;
    void Infer(HalValueSyntax value, HixPattern expected) {
      if (Reference(value) is { } target && ids.TryGetValue(target, out var node) && expected is not AnyHixPattern &&
          g.NodePatterns[node] is KindHixPattern {ValueKind: HixValueKind.Table or HixValueKind.Tuple}) g.NodePatterns[node] = expected;
    }
    void AddPort(string owner, string name, HixPatternField field, ParserRuleContext value, List<HalGraphPort> ports) {
      var start = value.Start.StartIndex; var end = value.Stop.StopIndex + 1; var raw = s.Text.Substring(start, end - start);
      ports.Add(Port(name, field, raw)); g.Ports[PortKey(owner, name)] = new(start, end, name, false, "", field?.Pattern ?? HixPattern.Any);
      var parsed = HalDocumentSyntax.Parse("--- table\nx = " + raw + "\n").Sections[0].Fields[0].Value;
      if (Reference(parsed) is { } target && ids.TryGetValue(target, out var node)) g.Edges.Add(new(PortKey(owner, name), node, owner, name));
    }
  }
  private static string Reference(HalValueSyntax value) => value is HalCallSyntax {Name: "ref", Arguments.Count: 1} call && call.Arguments[0].Value is HalScalarSyntax scalar
    ? Convert.ToString(scalar.Value, System.Globalization.CultureInfo.InvariantCulture) : null;
  private static HixPattern Resolve(HixPattern pattern, IReadOnlyDictionary<string, HixPattern> patterns) {
    var seen = new HashSet<string>();
    while (true) switch (pattern) {
      case NamedHixPattern named when seen.Add(named.Name) && patterns.TryGetValue(named.Name, out var resolved): pattern = resolved; break;
      case DocumentedHixPattern documented: pattern = documented.Underlying; break;
      case ConstrainedHixPattern constrained: pattern = constrained.Underlying; break;
      default: return pattern;
    }
  }
  private static HixPattern Element(HixPattern pattern, IReadOnlyDictionary<string, HixPattern> patterns, int index) => Resolve(pattern, patterns) switch {
    ManyHixPattern many => many.Element,
    TupleHixPattern tuple when index < tuple.Fields.Count => tuple.Fields[index].Pattern,
    _ => HixPattern.Any
  };
  private static string Kind(HixPattern pattern, IReadOnlyDictionary<string, HixPattern> patterns) => Resolve(pattern, patterns) switch {
    ManyHixPattern or TupleHixPattern or KindHixPattern {ValueKind: HixValueKind.Tuple} => "list",
    TableHixPattern or MapHixPattern or KindHixPattern {ValueKind: HixValueKind.Table} => "table",
    var other when HixPatterns.TryStructural(other, patterns, out var shape) => shape is TableHixPattern ? "table" : "list",
    _ => null
  };
  private static HalGraphType[] Catalog(Graph g) => new[] { "table", "tuple" }.Concat(g.Patterns.Keys).Distinct()
    .Where(name => !HixPatterns.TryTrait(HixPatterns.Named(name), g.Patterns, out _) && Kind(HixPatterns.Named(name), g.Patterns) != null)
    .Select(name => new HalGraphType(name, Kind(HixPatterns.Named(name), g.Patterns), Fields(HixPatterns.Named(name), g.Patterns).Select(f => Port(f.Name, f, null)).ToArray())).ToArray();
  private sealed class Session(string path, string text) {
    public readonly string Id = Guid.NewGuid().ToString("N"), Path = path;
    public string Text = text, Saved = text;
    public long Revision = 1;
    public readonly Stack<string> Undo = new(), Redo = new();
  }
  private sealed record Span(int Start, int End, string Kind, string SectionId);
  private sealed record PortSpan(int Start, int End, string Name, bool Insert, string Separator, HixPattern Pattern);
  private sealed class Graph {
    public string Workspace;
    public bool SyntaxError;
    public Dictionary<string, HixPattern> Patterns = new();
    public readonly Dictionary<string, HixPattern> NodePatterns = new();
    public readonly Dictionary<string, Span[]> PositionMetadata = new();
    public readonly List<string> Diagnostics = new();
    public readonly List<HalGraphNode> Nodes = new();
    public readonly List<HalGraphEdge> Edges = new();
    public readonly Dictionary<string, int> Commas = new();
    public readonly Dictionary<string, Span> Spans = new();
    public readonly Dictionary<string, PortSpan> Ports = new();
  }
}
