using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Hix.Runtime;

namespace Hix.Compiler;

/// <summary>Structural source generation and reconstruction of compiler-produced bytecode.</summary>
public static class HixDecompiler {
  /// <summary>Render an IR node without consulting its original source or tokens.</summary>
  public static string ToSource(HixIrNode node) {
    if (node == null) throw new ArgumentNullException(nameof(node));
    return new SourceWriter(node).Render(node);
  }

  /// <summary>Recover declarations and expression trees from an executable image.</summary>
  public static CompilationUnitIr Decompile(HixProgramImage image, string mixinName = "Decompiled") {
    if (image == null) throw new ArgumentNullException(nameof(image));
    if (string.IsNullOrEmpty(mixinName)) throw new ArgumentException("A mixin name is required", nameof(mixinName));
    return new ImageReader(image).Read(mixinName);
  }

  private sealed class SourceWriter {
    private readonly StringBuilder text = new();
    private readonly HashSet<string> names = new(StringComparer.Ordinal);
    private HashSet<string> declared = new(StringComparer.Ordinal);
    private int depth;
    private int sequence;
    public SourceWriter(HixIrNode node) {
      foreach (var child in Descendants(node)) {
        if (child is AssignmentStatementIr assignment) names.Add(assignment.Name);
        if (child is FunctionDeclarationIr function) names.Add(function.Name);
      }
    }
    private string Temporary() {
      string name;
      do name = "__decompiled_" + sequence++; while (!names.Add(name));
      return name;
    }
    private void Line(string value = "") => text.Append(' ', depth * 2).Append(value).Append('\n');
    public string Render(HixIrNode node) {
      if (node is ExpressionIr expression) return Value(expression);
      if (node is MetadataIr metadata) return Metadata(metadata);
      Write(node);
      return text.ToString();
    }
    private void Metadatas(IEnumerable<MetadataIr> values) {
      foreach (var metadata in values) Line(Metadata(metadata));
    }
    private string Metadata(MetadataIr metadata) => metadata.Name == null
      ? "%[" + Value(metadata.Values.Single()) + "]"
      : "%" + metadata.Name + (metadata.Values.Count == 0 ? "" : "(" + string.Join(", ", metadata.Values.Select(Value)) + ")");
    private void Write(HixIrNode node) {
      switch (node) {
        case CompilationUnitIr unit:
          if (unit.Metadata.Count != 0) { Metadatas(unit.Metadata); Line("---"); }
          foreach (var declaration in unit.Declarations) Write(declaration);
          break;
        case TypeDeclarationIr type:
          // Pattern metadata has already been incorporated in Pattern by the parser.
          Metadatas(type.Metadata.Where(item => !PatternMetadata.Contains(item.Name ?? "")));
          var typePattern = TypePattern(type.Pattern);
          Line("type " + type.Name + " = " + Pattern(typePattern)); break;
        case MixinDeclarationIr mixin:
          Metadatas(mixin.Metadata);
          Line((mixin.IsDerivation ? "derivation " : "") + "mixin " + DeclarationName(mixin.Name, true) +
            (mixin.Parameters.Count == 0 ? "" : Fields(mixin.Parameters)) + " {");
          depth++; foreach (var declaration in mixin.Declarations) Write(declaration); depth--; Line("}"); break;
        case FunctionDeclarationIr function:
          var signatures = function.Signatures.Count == 0 ? new FunctionSignature[] {null} : function.Signatures;
          foreach (var signature in signatures) {
            Metadatas(function.Metadata);
            var header = (function.IsPure ? "pure " : "") + (function.IsInline ? "inline " : "") +
              (function.IsNoinline ? "noinline " : "") + "func " + DeclarationName(function.Name);
            if (signature != null) {
              header += (signature.Inputs == null ? signature.InputPattern == null ? "" : " [" + Pattern(signature.InputPattern) + "]" : Fields(signature.Inputs)) + " -> " +
                (signature.Outputs == null ? Pattern(signature.OutputPattern ?? HixPattern.Any) : Fields(signature.Outputs));
            }
            Line(header + " {"); var previous = declared; declared = new(StringComparer.Ordinal);
            depth++; Statements(function.Body); depth--; Line("}"); declared = previous;
          }
          break;
        case ExpressionDeclarationIr expression:
          Line((expression.IsPrelude ? "prelude " : "") + (expression.IsStrict ? "strict " : "") + "expression {");
          var previousExpressionLocals = declared; declared = new(StringComparer.Ordinal);
          depth++; Statements(expression.Body); depth--; Line("}"); declared = previousExpressionLocals; break;
        case StatementIr statement: Statement(statement); break;
        default: throw new NotSupportedException("Cannot render " + node.GetType().Name);
      }
    }
    private HixPattern TypePattern(HixPattern pattern) {
      if (pattern is TaggedHixPattern tagged) {
        var underlying = TypePattern(tagged.Underlying); Line("%tagged(" + Quote(tagged.Discriminator) + ")"); return underlying;
      }
      if (pattern is DocumentedHixPattern documented) {
        var underlying = TypePattern(documented.Underlying);
        if (documented.Title != null) Line("%title(" + Quote(documented.Title) + ")");
        if (documented.Description != null) Line("%description(" + Quote(documented.Description) + ")");
        return underlying;
      }
      return pattern;
    }
    private string Fields(IReadOnlyList<SignatureField> fields) => "(" + string.Join(", ", fields.Select(field => {
      var extra = field.Metadata.Where(item => !PatternMetadata.Contains(item.Name ?? ""));
      return string.Concat(extra.Select(item => Metadata(item) + " ")) + (field.Optional ? "%optional " : "") +
        (field.Variadic ? "%variadic " : "") + Pattern(field.Pattern) + " " + field.Name +
        (field.DefaultValue == null ? "" : " = [" + Value(field.DefaultValue) + "]");
    })) + ")";
    private void Statements(BlockStatementIr block) { foreach (var statement in block.Statements) Statement(statement); }
    private void Statement(StatementIr statement) {
      // Prefixes belong before metadata so marker offsets describe the actual statement.
      switch (statement) {
        case AssignmentStatementIr assignment:
          if (assignment.Storage != StorageSpace.Local) throw new NotSupportedException("Non-local assignment IR is not supported by the bytecode compiler");
          var value = assignment.Value is SelectionExpressionIr assignedSelection ? Selection(assignedSelection) :
            assignment.Value == null ? null : Value(assignment.Value);
          var declaration = declared.Add(assignment.Name);
          Metadatas(statement.Metadata);
          Line((assignment.IsCarried ? "carry " : "") + (declaration ? "local " +
            (assignment.DeclaredPattern == null ? "" : Pattern(assignment.DeclaredPattern) + " ") : "$") +
            assignment.Name + (value == null ? "" : " = " + value)); break;
        case BlockStatementIr block:
          Metadatas(statement.Metadata); Line((block.Label == null ? "" : ":" + block.Label + " ") + "{");
          depth++; Statements(block); depth--; Line("}"); break;
        case InvocationStatementIr invocation:
          var call = Value(invocation.Call); Metadatas(statement.Metadata);
          if (invocation.Call.CoerceBoolean || Constraint(invocation.Call).Length != 0) Discard(call); else Line(call); break;
        case ExpressionStatementIr expression:
          if (expression.Expression is CallExpressionIr {CoerceBoolean: false} direct) {
            var source = Value(direct); Metadatas(statement.Metadata);
            if (Constraint(direct).Length != 0) Discard(source); else Line(source);
          } else { var source = Value(expression.Expression); Metadatas(statement.Metadata); Discard(source); }
          break;
        case SelectionStatementIr selection:
          var selected = Selection(selection.Selection); Metadatas(statement.Metadata); Line(selected); break;
        case ControlFlowStatementIr flow:
          var values = flow.Values.Select(Value).ToArray(); Metadatas(statement.Metadata);
          Line(flow.Operation switch {
            ControlFlowKind.Label => ":" + flow.Label,
            ControlFlowKind.Goto => "goto " + flow.Label,
            ControlFlowKind.Break => "break",
            ControlFlowKind.Continue => "continue",
            ControlFlowKind.Return => "return(" + string.Join(", ", values) + ")",
            _ => throw new NotSupportedException("Unknown control flow")
          }); break;
        default: throw new NotSupportedException("Cannot render " + statement.GetType().Name);
      }
    }
    private void Discard(string value) {
      // Expression statements discard the value without exposing a synthetic local.
      Line("@= " + value + ";");
    }
    private string Fragment(Action action) {
      var start = text.Length; var savedDepth = depth;
      depth = 0; action(); var result = text.ToString(start, text.Length - start).TrimEnd();
      text.Length = start; depth = savedDepth; return result;
    }
    private string Selection(SelectionExpressionIr selection) => Fragment(() => {
      Line("when" + (selection.Selector == null ? "" : " [" + Value(selection.Selector) + "]") + " {"); depth++;
      foreach (var branch in selection.Branches) {
        var conditions = branch.Conditions.Select(condition => branch.IsTransformation
          ? Transformation(condition) : "[" + Value(condition) + "]").ToArray();
        var conditionSource = selection.Selector == null ? "(" + string.Join(", ", branch.Conditions.Select(Value)) + ")"
          : conditions.Single();
        Line(conditionSource + " -> " + Result(branch.Result));
      }
      if (selection.Fallback != null) Line("else -> " + Result(selection.Fallback));
      depth--; Line("}");
    });
    private string Result(HixIrNode result) => result is ExpressionIr expression ? Value(expression) :
      result is StatementIr statement ? Fragment(() => Statement(statement)) : "null";
    private string Transformation(ExpressionIr expression) {
      if (expression is SelectorExpressionIr) return "[";
      if (expression is MemberExpressionIr member) return Transformation(member.Receiver).TrimEnd(']') + "#" + member.Member + "]";
      if (expression is CallExpressionIr call && call.Arguments.Count != 0) {
        var receiver = Transformation(call.Arguments[0]).TrimEnd(']');
        return receiver + (call.CoerceBoolean ? ":?" : ":") + call.Name + "(" +
          string.Join(", ", call.Arguments.Skip(1).Select(Value)) + ")" + Constraint(call) + "]";
      }
      throw new NotSupportedException("Invalid selection transformation " + expression.GetType().Name);
    }
    private static string Constraint(CallExpressionIr call) {
      var kind = call.DispatchConstraint?.Kind ?? call.Binding.Kind;
      return kind == HixCallKind.Static ? " @static(" + Quote(call.DispatchConstraint?.Signature ?? call.Binding.Signature.Display) + ")" :
        kind == HixCallKind.Dynamic ? " @dynamic" : "";
    }
    private string Chain(ExpressionIr expression) => expression switch {
      RootExpressionIr root => root.IsSmart ? "$" + root.Name : RootName(root.Name),
      MemberExpressionIr member => Chain(member.Receiver) + "#" + member.Member,
      _ => Value(expression)
    };
    private string Value(ExpressionIr expression) {
      switch (expression) {
        case StringExpressionIr literal: return Quote(literal.Value);
        case NumberExpressionIr number: return Number(number.Value);
        case BooleanExpressionIr boolean: return boolean.Value ? "true" : "false";
        case NullExpressionIr: return "null";
        case MissingExpressionIr: return "[missing]";
        case RootExpressionIr root: return "[" + (root.IsSmart ? "$" + root.Name : RootName(root.Name)) + "]";
        case MemberExpressionIr member: return "[" + Chain(member) + "]";
        case TupleExpressionIr tuple: return "@[" + string.Join(", ", tuple.Values.Select(Value)) + "]";
        case TableExpressionIr table: return "@{" + string.Join(", ", table.Entries.Select(entry => {
          var metadata = table.FieldMetadata.FirstOrDefault(item => item.Key == entry.Key).Value ?? [];
          return string.Concat(metadata.Select(item => Metadata(item) + " ")) + entry.Key + "=" + Value(entry.Value);
        })) + "}";
        case CallExpressionIr call:
          var args = call.BoundArguments ?? call.Arguments;
          var invocation = call.Name + "(" + string.Join(", ", args.Select((arg, index) =>
            (call.BoundArguments == null && call.ArgumentNames[index] != null ? call.ArgumentNames[index] + " = " : "") + Value(arg))) + ")";
          return Constraint(call).Length == 0 && !call.CoerceBoolean ? invocation :
            "[" + invocation + Constraint(call) + (call.CoerceBoolean ? " @bool" : "") + "]";
        case UnaryExpressionIr unary: return unary.Operation == UnaryOperation.Not ? "![" + Value(unary.Value) + "]" : "[" + Value(unary.Value) + "?]";
        case FallbackExpressionIr fallback: return "[" + Value(fallback.Value) + " ?: " + Value(fallback.Fallback) + "]";
        case InterpolationExpressionIr interpolation: return "<" + string.Concat(interpolation.Parts.Select(part => "[" + Value(part) + "]")) + ">";
        case LambdaExpressionIr lambda: return "func " + Fragment(() => { Line("{"); depth++; Statements(lambda.Body); depth--; Line("}"); });
        case InlineExpressionIr inline:
          // The compiler's inline body already owns collision-free locals and labels.
          return "[do " + Fragment(() => { Line("{"); depth++; Statements(inline.Body); depth--; Line("}"); }) + " => local#" + inline.ResultLocal + "]";
        case SelectionExpressionIr selection:
          var temporary = Temporary();
          return "[do " + Fragment(() => { Line("{"); depth++; Line("local " + temporary + " = " + Selection(selection)); depth--; Line("}"); }) + " => local#" + temporary + "]";
        default: throw new NotSupportedException("Cannot render " + expression.GetType().Name);
      }
    }
  }

  private static IEnumerable<HixIrNode> Descendants(HixIrNode node) {
    yield return node;
    foreach (var child in node.Children) foreach (var descendant in Descendants(child)) yield return descendant;
  }
  private static readonly HashSet<string> PatternMetadata = new(StringComparer.Ordinal) {
    "optional", "variadic", "default", "graph", "many", "map", "union", "tagged", "const", "enum", "title", "description", "min", "max", "length", "matches"
  };
  private static bool Identifier(string name) => name.Length != 0 && (name[0] == '_' || name[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z') &&
    name.All(character => character == '_' || character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9');
  private static string RootName(string name) => Identifier(name) && name is not ("null" or "missing" or "true" or "false" or "delegate")
    ? name : "@root(" + Quote(name) + ")";
  private static string DeclarationName(string name, bool namespaces = false) =>
    (namespaces ? name.Split('.').All(Identifier) : Identifier(name)) ? name : "<[" + Quote(name) + "]>";
  private static string Quote(string value) {
    var result = new StringBuilder("\"");
    foreach (var character in value) result.Append(character switch {
      '\\' => "\\\\", '"' => "\\\"", '\n' => "\\n", '\r' => "\\r", '\t' => "\\t", '\a' => "\\a", '\b' => "\\b", '\f' => "\\f", '\v' => "\\v",
      _ when character < 32 || character == 127 => "\\x" + ((int)character).ToString("X2", CultureInfo.InvariantCulture),
      _ => character.ToString()
    });
    return result.Append('"').ToString();
  }
  private static string Number(double value) {
    if (double.IsNaN(value) || double.IsInfinity(value)) throw new NotSupportedException("Non-finite numbers cannot be represented by Hix numeric literals");
    // Hix literals do not accept exponent notation. Expand the round-trip representation.
    var source = value.ToString("R", CultureInfo.InvariantCulture);
    var exponentIndex = source.IndexOf('E');
    if (exponentIndex < 0) return source;
    var negative = source[0] == '-'; var mantissa = source.Substring(negative ? 1 : 0, exponentIndex - (negative ? 1 : 0));
    var dot = mantissa.IndexOf('.'); var point = (dot < 0 ? mantissa.Length : dot) + int.Parse(source.Substring(exponentIndex + 1), CultureInfo.InvariantCulture);
    var digits = mantissa.Replace(".", "");
    return (negative ? "-" : "") + (point <= 0 ? "0." + new string('0', -point) + digits :
      point >= digits.Length ? digits + new string('0', point - digits.Length) : digits.Insert(point, "."));
  }
  private static string Pattern(HixPattern pattern) => pattern switch {
    AnyHixPattern or KindHixPattern or NamedHixPattern => pattern.Display,
    UnionHixPattern union => string.Join(" | ", union.Patterns.Select(Pattern)),
    TupleHixPattern tuple => "@[" + string.Join(", ", tuple.Fields.Select(PatternField)) + "]",
    TableHixPattern table => "@{" + string.Join(", ", table.Fields.Select(PatternField)) + "}",
    DelegateHixPattern callable => "delegate(" + string.Join(", ", callable.Parameters.Select(PatternField)) + ") -> " + Pattern(callable.Result),
    ManyHixPattern many => "%many " + Pattern(many.Element),
    MapHixPattern map => "%map<[" + Quote(Pattern(map.Key)) + "]><[" + Quote(Pattern(map.Value)) + "]>",
    TaggedHixPattern tagged => "%tagged(" + Quote(tagged.Discriminator) + ") " + Pattern(tagged.Underlying),
    ConstantHixPattern constant => "%const(" + ObjectSource(constant.Value) + ") " + Pattern(constant.Underlying),
    EnumHixPattern enumeration => "%enum(" + string.Join(", ", enumeration.Values.Select(ObjectSource)) + ") " + Pattern(enumeration.Underlying),
    DocumentedHixPattern documented => (documented.Title == null ? "" : "%title(" + Quote(documented.Title) + ") ") +
      (documented.Description == null ? "" : "%description(" + Quote(documented.Description) + ") ") + Pattern(documented.Underlying),
    ConstrainedHixPattern constrained => "%" + (constrained.Constraint == HixPatternConstraintKind.Minimum ? "min" : constrained.Constraint == HixPatternConstraintKind.Maximum ? "max" : constrained.Constraint.ToString().ToLowerInvariant()) + "(" + ObjectSource(constrained.Argument) +
      (constrained.Exclusive ? ", true" : "") + ") " + Pattern(constrained.Underlying),
    _ => throw new NotSupportedException("Cannot render pattern " + pattern.GetType().Name)
  };
  private static string PatternField(HixPatternField field) => (field.Optional ? "%optional " : "") +
    (field.Graph == null ? "" : "%graph(" + Quote(field.Graph.Value.ToString().ToLowerInvariant()) + ") ") + Pattern(field.Pattern) +
    (string.IsNullOrEmpty(field.Name) ? "" : " " + field.Name) + (field.HasDefault ? " = [" + ObjectSource(field.DefaultValue) + "]" : "");
  private static string ObjectSource(object value) => value switch {
    null or NullHixValue => "null", MissingHixValue => "missing", string literal => Quote(literal),
    bool boolean => boolean ? "true" : "false", double number => Number(number),
    IHixValue hix => ToSource(Literal(hix, null)),
    IReadOnlyDictionary<string, object> table => "@{" + string.Join(", ", table.Select(item => item.Key + "=" + ObjectSource(item.Value))) + "}",
    IEnumerable<object> tuple => "@[" + string.Join(", ", tuple.Select(ObjectSource)) + "]",
    IConvertible convertible => Number(convertible.ToDouble(CultureInfo.InvariantCulture)),
    _ => throw new NotSupportedException("Cannot render constant " + value.GetType().Name)
  };
  private static ExpressionIr Literal(IHixValue value, HixStringPool strings) => value switch {
    NullHixValue => new NullExpressionIr(), MissingHixValue => new MissingExpressionIr(),
    BooleanHixValue boolean => new BooleanExpressionIr(boolean.Value), NumberHixValue number => new NumberExpressionIr(number.Value),
    LiteralHixValue literal => new StringExpressionIr(literal.Value.Resolve(strings)),
    TupleHixValue tuple => new TupleExpressionIr(tuple.Values.Select(item => Literal(item, strings)).ToArray()),
    HixTableValue table => new TableExpressionIr(table.Entries.Select(item => new KeyValuePair<string, ExpressionIr>(item.Key.Resolve(strings), Literal(item.Value, strings))).ToArray()),
    _ => throw new NotSupportedException("Cannot reconstruct constant " + value.GetType().Name)
  };

  private sealed class ImageReader {
    private readonly HixProgramImage image;
    private readonly Dictionary<int, HixInstruction> instructions = new();
    private readonly Dictionary<int, int> previousInstructions = new();
    private readonly HashSet<int> activeBlocks = [];
    private HashSet<string> declared = new(StringComparer.Ordinal);
    private readonly Dictionary<HixIrNode, int> nodeOffsets = new();
    private readonly Dictionary<int, string> labels = new();
    public ImageReader(HixProgramImage image) {
      this.image = image;
      for (var offset = 0; offset < image.Bytecode.Count;) {
        try { var instruction = HixInstruction.Decode(image.Bytecode, offset); instructions.Add(offset, instruction); previousInstructions.Add(offset + instruction.Size, offset); offset += instruction.Size; }
        catch (ArgumentException error) { throw Error(offset, error.Message); }
      }
      foreach (var item in instructions) {
        var instruction = item.Value;
        if (instruction.IsRelative && !Boundary(item.Key + instruction.A)) throw Error(item.Key, "target is not an instruction boundary");
        if (instruction.UsesStringPool && instruction.A >= image.StringPool.Count) throw Error(item.Key, "string pool index is out of range");
        if (instruction.Opcode is HixOpcode.LoadConst or HixOpcode.Call && instruction.A >= image.ConstantPool.Count)
          throw Error(item.Key, "constant pool index is out of range");
        if (instruction.Opcode == HixOpcode.Call && image.ConstantPool[instruction.A] is not FunctionReferenceHixValue)
          throw Error(item.Key, "Call requires a function reference constant");
        if (instruction.Opcode == HixOpcode.Goto && !labels.ContainsKey(item.Key + instruction.A)) {
          labels.Add(item.Key + instruction.A, "__label_" + labels.Count);
        }
      }
      foreach (var marker in image.Markers.Values)
        if (!Boundary(marker.Offset)) throw Error(marker.Offset, "marker is not an instruction boundary");
    }
    private bool Boundary(int offset) => offset == image.Bytecode.Count || instructions.ContainsKey(offset);
    private static ArgumentException Error(int offset, string message) => new("Bytecode at offset " + offset + ": " + message);
    private HixInstruction Instruction(int offset) => instructions.TryGetValue(offset, out var instruction) ? instruction : throw Error(offset, "missing instruction");
    private int Next(int offset) => offset + Instruction(offset).Size;
    private string Name(int index) => image.StringPool[index];
    private T Located<T>(T node, int offset) where T : HixIrNode {
      nodeOffsets[node] = offset;
      node.SourceRange = new HixSourceRange(0, 0, image.SourceLines.TryGetValue(offset, out var line) ? line : 0, 0);
      return node;
    }
    public CompilationUnitIr Read(string name) {
      var declarations = new List<HixIrNode>();
      foreach (var pattern in image.Patterns) declarations.Add(new TypeDeclarationIr(pattern.Key, pattern.Value));
      var constructors = image.Patterns.SelectMany(item => PatternConstructorCompiler.Compile(item.Key, item.Value, image.Patterns)).ToArray();
      bool Constructor(BytecodeFunction function) => constructors.Any(candidate => candidate.Name == function.Name &&
        candidate.Signatures.Count == function.Signatures.Count && candidate.Signatures.Select(signature => signature.Constant(candidate.Name).Display)
          .SequenceEqual(function.Signatures.Select(signature => signature.Constant(function.Name).Display)));
      foreach (var function in image.Scope.DisassemblyFunctions("mixin").Where(item => item.Scope == "global").OrderBy(item => item.Function.Body))
        if (!Constructor(function.Function)) declarations.Add(Function(function.Function));
      foreach (var derivation in image.Derivations) {
        var body = derivation.Expressions.Select(Expression).Cast<HixIrNode>().Concat(
          derivation.Scope.DisassemblyFunctions("derivation", false).OrderBy(item => item.Function.Body).Select(item => Function(item.Function))).ToArray();
        declarations.Add(new MixinDeclarationIr(derivation.Name, true, body));
      }
      var local = image.Scope.DisassemblyFunctions("mixin", false).OrderBy(item => item.Function.Body).Select(item => Function(item.Function)).Cast<HixIrNode>().ToList();
      local.AddRange(image.Expressions.Select(Expression));
      while (image.Derivations.Any(derivation => derivation.Name == name)) name += "_";
      declarations.Add(new MixinDeclarationIr(name, false, local, parameters: Fields(image.Parameters)));
      var unit = new CompilationUnitIr("", declarations, [], []);
      foreach (var node in Descendants(unit))
        if (node is EqualityExpression or RenderedExpression) throw Error(nodeOffsets[node], "unconsumed value operation");
      return unit;
    }
    private SignatureField[] Fields(IReadOnlyList<BytecodeField> fields) => fields?.Select(field =>
      new SignatureField(field.Name, field.Pattern, field.Variadic, optional: field.Optional,
        defaultValue: field.DefaultValue == null ? null : Literal(field.DefaultValue, image.StringPool))).ToArray();
    private FunctionDeclarationIr Function(BytecodeFunction function) => Located(new FunctionDeclarationIr(function.Name, function.IsPure,
      false, true, function.Signatures.Select(signature => new FunctionSignature(signature.InputPattern, Fields(signature.Inputs),
        signature.OutputPattern, Fields(signature.Outputs))).ToArray(), Block(function.Body)), function.Body);
    private ExpressionDeclarationIr Expression(BytecodeExpression expression) => Located(new ExpressionDeclarationIr(expression.IsPrelude, false, Block(expression.Function.Body)), expression.Function.Body);
    private BlockStatementIr Block(int offset) {
      var instruction = Instruction(offset);
      if (instruction.Opcode != HixOpcode.Enter || instruction.A <= instruction.Size) throw Error(offset, "expected a bounded Enter block");
      var end = offset + instruction.A;
      var last = previousInstructions.TryGetValue(end, out var previous) ? previous : -1;
      if (last < offset || Instruction(last).Opcode != HixOpcode.End || Next(last) != end) throw Error(offset, "block must end with End");
      if (!activeBlocks.Add(offset)) throw Error(offset, "recursive block reference");
      try {
        if (activeBlocks.Count == 1) declared = new(StringComparer.Ordinal);
        var stack = new List<ExpressionIr>(); var statements = new List<StatementIr>(); var pc = Next(offset);
        Region(ref pc, last, stack, statements);
        if (pc != last || stack.Count != 0) throw Error(pc, "unbalanced block stack or control sequence");
        AddLabel(last, statements);
        return Located(new BlockStatementIr(statements), offset);
      } finally { activeBlocks.Remove(offset); }
    }
    private void AddLabel(int offset, List<StatementIr> statements) {
      if (labels.TryGetValue(offset, out var label)) statements.Add(Located(new ControlFlowStatementIr(ControlFlowKind.Label, label, []), offset));
    }
    private ExpressionIr Pop(List<ExpressionIr> stack, int offset) {
      if (stack.Count == 0) throw Error(offset, "expression stack underflow");
      var value = stack[stack.Count - 1]; stack.RemoveAt(stack.Count - 1); return value;
    }
    private ExpressionIr[] PopMany(List<ExpressionIr> stack, int count, int offset) {
      if (count > stack.Count) throw Error(offset, "expression stack underflow");
      var result = stack.Skip(stack.Count - count).ToArray(); stack.RemoveRange(stack.Count - count, count); return result;
    }
    private void Add(StatementIr statement, int offset, List<StatementIr> statements) {
      var metadata = image.Markers.Where(item => item.Value.Offset == offset)
        .Select(item => new MetadataIr("marker", [new StringExpressionIr(item.Key), new StringExpressionIr(item.Value.Type)])).ToArray();
      if (metadata.Length != 0) statement.SetMetadata(metadata);
      statements.Add(Located(statement, offset));
    }
    // Stops apply only at this region's level: nested selections, checks and blocks consume their own branches.
    private void Region(ref int pc, int end, List<ExpressionIr> stack, List<StatementIr> statements,
      bool stopAtBranch = false) {
      var statementStart = pc; var carry = false;
      while (pc < end) {
        var offset = pc; var instruction = Instruction(pc); var next = Next(pc);
        if (stopAtBranch && (instruction.Opcode == HixOpcode.JumpFalse ||
          instruction.Opcode == HixOpcode.Jump && Instruction(next).Opcode != HixOpcode.Enter)) return;
        if (stack.Count == 0 && !carry) { statementStart = offset; AddLabel(offset, statements); }
        pc = next;
        ExpressionIr value = null;
        switch (instruction.Opcode) {
          case HixOpcode.LoadString: value = new StringExpressionIr(Name(instruction.A)); break;
          case HixOpcode.LoadConst:
            try { value = Literal(image.ConstantPool[instruction.A], image.StringPool); }
            catch (NotSupportedException error) { throw Error(offset, error.Message); }
            break;
          case HixOpcode.LoadNull: value = new NullExpressionIr(); break;
          case HixOpcode.LoadMissing: value = new MissingExpressionIr(); break;
          case HixOpcode.LoadTrue: value = new BooleanExpressionIr(true); break;
          case HixOpcode.LoadFalse: value = new BooleanExpressionIr(false); break;
          case HixOpcode.LoadTuple: value = new TupleExpressionIr([]); break;
          case HixOpcode.LoadTable: value = new TableExpressionIr([]); break;
          case HixOpcode.LoadRoot: value = new RootExpressionIr(Name(instruction.A)); break;
          case HixOpcode.Member:
            var receiver = Pop(stack, offset); var memberName = Name(instruction.A);
            value = receiver is RootExpressionIr {Name: "args", IsSmart: false} && int.TryParse(memberName, out var parameterIndex) && parameterIndex >= 0
              ? new RootExpressionIr(memberName, true) : new MemberExpressionIr(receiver, memberName); break;
          case HixOpcode.Equal:
            var right = Pop(stack, offset); value = new EqualityExpression(Pop(stack, offset), right); break;
          case HixOpcode.Not: value = new UnaryExpressionIr(UnaryOperation.Not, Pop(stack, offset)); break;
          case HixOpcode.Call:
            var reference = (FunctionReferenceHixValue)image.ConstantPool[instruction.A];
            value = new CallExpressionIr(reference.Signature.Name, PopMany(stack, instruction.B, offset)) {DispatchConstraint = new(HixCallKind.Static, reference.Signature.Display)}; break;
          case HixOpcode.CallDynamic: value = new CallExpressionIr(Name(instruction.A), PopMany(stack, instruction.B, offset)) {DispatchConstraint = new(HixCallKind.Dynamic)}; break;
          case HixOpcode.CastBoolean:
            if (Pop(stack, offset) is not CallExpressionIr call) throw Error(offset, "CastBoolean must follow a call");
            value = new CallExpressionIr(call.Name, call.Arguments, true) {DispatchConstraint = call.DispatchConstraint}; break;
          case HixOpcode.CastString:
            // Only Interpolate can consume these compiler-emitted casts.
            value = new RenderedExpression(Pop(stack, offset)); break;
          case HixOpcode.Interpolate:
            var parts = PopMany(stack, instruction.A, offset);
            if (parts.Any(part => part is not RenderedExpression)) throw Error(offset, "Interpolate requires rendered parts");
            value = new InterpolationExpressionIr(parts.Cast<RenderedExpression>().Select(part => part.Value).ToArray()); break;
          case HixOpcode.PackTuple: value = new TupleExpressionIr(PopMany(stack, instruction.A, offset)); break;
          case HixOpcode.PackTable:
            var pairs = PopMany(stack, instruction.A * 2, offset); var entries = new List<KeyValuePair<string, ExpressionIr>>();
            for (var index = 0; index < pairs.Length; index += 2) {
              if (pairs[index] is not StringExpressionIr key) throw Error(offset, "table key must be a string literal");
              entries.Add(new(key.Value, pairs[index + 1]));
            }
            value = new TableExpressionIr(entries); break;
          case HixOpcode.Pack:
            if (Instruction(pc).Opcode != HixOpcode.Return) throw Error(offset, "Pack must precede Return");
            var returned = PopMany(stack, instruction.A, offset);
            Add(new ControlFlowStatementIr(ControlFlowKind.Return, null, returned), statementStart, statements); pc = Next(pc); break;
          case HixOpcode.Check:
            var target = offset + instruction.A;
            var checkedStart = pc;
            var endCheck = previousInstructions.TryGetValue(target, out var previousCheck) ? previousCheck : -1;
            if (endCheck < pc || Instruction(endCheck).Opcode != HixOpcode.EndCheck || Next(endCheck) != target)
              throw Error(offset, "checked expression must terminate with EndCheck");
            var checkedStack = new List<ExpressionIr>(); var checkedStatements = new List<StatementIr>();
            Region(ref pc, endCheck, checkedStack, checkedStatements);
            if (checkedStatements.Count != 0 || checkedStack.Count != 1) throw Error(offset, "checked expression must produce one value");
            value = new UnaryExpressionIr(UnaryOperation.Check, checkedStack[0]); pc = target; break;
          case HixOpcode.JumpNotNull:
            var original = Pop(stack, offset); var fallbackEnd = offset + instruction.A;
            if (Instruction(pc).Opcode != HixOpcode.Pop || fallbackEnd <= pc) throw Error(offset, "invalid fallback sequence");
            pc = Next(pc); var fallbackStack = new List<ExpressionIr>(); var fallbackStatements = new List<StatementIr>();
            Region(ref pc, fallbackEnd, fallbackStack, fallbackStatements);
            if (fallbackStatements.Count != 0 || fallbackStack.Count != 1) throw Error(offset, "fallback must produce one value");
            value = new FallbackExpressionIr(original, fallbackStack[0]); break;
          case HixOpcode.CheckStoreCarry:
            if (carry) throw Error(offset, "duplicate carry check"); carry = true; break;
          case HixOpcode.StoreLocal:
          case HixOpcode.StoreCarry:
            var assigned = Pop(stack, offset); var name = Name(instruction.A);
            if (instruction.Opcode == HixOpcode.StoreLocal && SelectorName(name)) {
              var selectionPc = pc; var previousDeclarations = new HashSet<string>(declared, StringComparer.Ordinal);
              try { value = Selection(ref selectionPc, end, assigned, name); pc = selectionPc; break; }
              catch (ArgumentException) { declared = previousDeclarations; }
              // A user local may use the same prefix. Only a complete selection sequence identifies a temporary.
            }
            if ((instruction.Opcode == HixOpcode.StoreCarry) != carry) throw Error(offset, "carry store/check mismatch");
            Add(new AssignmentStatementIr(StorageSpace.Local, name, assigned, carry, declaration: declared.Add(name)), statementStart, statements); carry = false; break;
          case HixOpcode.Pop:
            var discarded = Pop(stack, offset);
            Add(discarded is SelectionExpressionIr selected ? new SelectionStatementIr(selected) :
              discarded is CallExpressionIr invocation ? new InvocationStatementIr(invocation) : new ExpressionStatementIr(discarded), statementStart, statements); break;
          case HixOpcode.Return: Add(new ControlFlowStatementIr(ControlFlowKind.Return, null, [Pop(stack, offset)]), statementStart, statements); break;
          case HixOpcode.Break: Add(new ControlFlowStatementIr(ControlFlowKind.Break, null, []), statementStart, statements); break;
          case HixOpcode.Continue: Add(new ControlFlowStatementIr(ControlFlowKind.Continue, null, []), statementStart, statements); break;
          case HixOpcode.Goto:
            Add(new ControlFlowStatementIr(ControlFlowKind.Goto, labels[offset + instruction.A], []), statementStart, statements); break;
          case HixOpcode.InvalidGoto:
            const string missingLabel = "__unresolved_goto";
            Add(new ControlFlowStatementIr(ControlFlowKind.Goto, missingLabel, []), statementStart, statements); break;
          case HixOpcode.Throw:
            var message = Name(instruction.A);
            const string unknown = "unknown local or parameter '";
            const string duplicate = "duplicate table key '";
            if (message.StartsWith(unknown, StringComparison.Ordinal) && message.EndsWith("'", StringComparison.Ordinal))
              value = new RootExpressionIr(message.Substring(unknown.Length, message.Length - unknown.Length - 1), true);
            else if (message.StartsWith(duplicate, StringComparison.Ordinal) && message.EndsWith("'", StringComparison.Ordinal)) {
              var key = message.Substring(duplicate.Length, message.Length - duplicate.Length - 1);
              value = new TableExpressionIr([new(key, new NullExpressionIr()), new(key, new NullExpressionIr())]);
            } else throw Error(offset, "unrecognized compiler Throw sequence");
            break;
          case HixOpcode.Jump:
            if (Instruction(pc).Opcode != HixOpcode.Enter) throw Error(offset, "unrecognized Jump sequence");
            var body = Block(pc); var blockCall = offset + instruction.A;
            if (Instruction(blockCall).Opcode != HixOpcode.Block || blockCall + Instruction(blockCall).A != pc ||
              pc + Instruction(pc).A != blockCall) throw Error(offset, "invalid nested block sequence");
            pc = Next(blockCall);
            var resultPc = pc;
            if (pc < end && Instruction(pc).Opcode == HixOpcode.LoadRoot && Name(Instruction(pc).A) == "local" &&
              Instruction(Next(pc)).Opcode == HixOpcode.Member && Descendants(body).OfType<AssignmentStatementIr>()
                .Any(assignment => assignment.Name == Name(Instruction(Next(resultPc)).A))) {
              value = new InlineExpressionIr(body, Name(Instruction(Next(pc)).A)); pc = Next(Next(pc));
            } else Add(body, statementStart, statements);
            break;
          default: throw Error(offset, "unsupported instruction sequence " + instruction.Opcode);
        }
        if (value != null) stack.Add(Located(value, offset));
        if (pc > end) throw Error(offset, "instruction sequence crosses its enclosing region");
      }
      if (carry) throw Error(pc, "carry check without a store");
    }
    private static bool SelectorName(string name) => name.StartsWith("__selector_", StringComparison.Ordinal) && name.Length > 11 &&
      name.Substring(11).All(character => character is >= '0' and <= '9');
    private SelectionExpressionIr Selection(ref int pc, int end, ExpressionIr selector, string local) {
      var branches = new List<SelectionBranchIr>(); var merge = -1;
      while (pc < end && (merge < 0 || pc < merge)) {
        var conditionStack = new List<ExpressionIr>(); var conditionStatements = new List<StatementIr>();
        var start = pc; Region(ref pc, merge < 0 ? end : merge, conditionStack, conditionStatements, true);
        if (pc >= end || Instruction(pc).Opcode != HixOpcode.JumpFalse || conditionStack.Count != 1 || conditionStatements.Count != 0) {
          if (merge < 0) throw Error(start, "selection requires a conditional branch");
          var implicitSelector = selector is BooleanExpressionIr {Value: true} && branches.All(branch => branch.IsTransformation) && !branches.SelectMany(branch => branch.Conditions)
            .SelectMany(Descendants).Any(node => node is SelectorExpressionIr);
          return new SelectionExpressionIr(implicitSelector ? null : selector, implicitSelector
            ? branches.Select(branch => new SelectionBranchIr(branch.Conditions, branch.Result)).ToArray() : branches,
            RegionResult(conditionStack, conditionStatements, start));
        }
        var branchEnd = pc + Instruction(pc).A; var conditions = new List<ExpressionIr>(); var transformation = false;
        while (true) {
          var condition = conditionStack.Single();
          if (condition is EqualityExpression equality && IsSelectionLocal(equality.Left, local)) conditions.Add(equality.Right);
          else { transformation = true; conditions.Add(new SelectorRewriter(local).Rewrite(condition)); }
          pc = Next(pc); conditionStack.Clear(); conditionStatements.Clear();
          Region(ref pc, branchEnd, conditionStack, conditionStatements, true);
          if (pc < branchEnd && Instruction(pc).Opcode == HixOpcode.JumpFalse && pc + Instruction(pc).A == branchEnd &&
              conditionStack.Count == 1 && conditionStatements.Count == 0) continue;
          if (pc >= branchEnd || Instruction(pc).Opcode != HixOpcode.Jump) throw Error(pc, "selection branch must end with Jump");
          var target = pc + Instruction(pc).A;
          if (merge >= 0 && merge != target) throw Error(pc, "selection branches must share a merge target");
          merge = target;
          branches.Add(new SelectionBranchIr(conditions, RegionResult(conditionStack, conditionStatements, start), transformation));
          if (Next(pc) != branchEnd) throw Error(pc, "selection false target must follow its result branch");
          pc = branchEnd; break;
        }
        // The next iteration distinguishes another condition from the final fallback.
      }
      throw Error(pc, "selection has no fallback result");
    }
    private HixIrNode RegionResult(List<ExpressionIr> stack, List<StatementIr> statements, int offset) {
      if (stack.Count != 1) throw Error(offset, "selection result must produce one value");
      if (statements.Count == 0) return stack[0];
      if (stack[0] is not NullExpressionIr) throw Error(offset, "statement selection result must end with null");
      return statements.Count == 1 ? statements[0] : new BlockStatementIr(statements);
    }
    private static bool IsSelectionLocal(ExpressionIr expression, string name) => expression is MemberExpressionIr {
      Receiver: RootExpressionIr {Name: "local", IsSmart: false}} member && member.Member == name;
    private sealed class SelectorRewriter(string name) : HixIrRewriter {
      protected override HixIrNode RewriteNode(HixIrNode node) => node is ExpressionIr expression && IsSelectionLocal(expression, name)
        ? new SelectorExpressionIr() : base.RewriteNode(node);
    }
    private sealed class RenderedExpression(ExpressionIr value) : ExpressionIr([value]) {
      public ExpressionIr Value { get; } = value;
    }
    private sealed class EqualityExpression(ExpressionIr left, ExpressionIr right) : ExpressionIr([left, right]) {
      public ExpressionIr Left { get; } = left;
      public ExpressionIr Right { get; } = right;
    }
  }
}
