using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

internal static class HixRoundTripAssertions {
  internal static void Equal(HixProgramImage expected, HixProgramImage actual) {
    var first = Canonical(expected); var second = Canonical(actual);
    Assert.Equal(first, second);
  }
  internal static string Canonical(HixProgramImage image) {
    var decoded = HixInstruction.ReadAll(image.Bytecode).ToArray();
    var positions = decoded.Select((item, index) => (item.Offset, index)).ToDictionary(item => item.Offset, item => item.index);
    positions.Add(image.Bytecode.Count, decoded.Length);
    IEnumerable<HixIrNode> Descendants(HixIrNode node) => new[] {node}.Concat(node.Children.SelectMany(Descendants));
    var declaredLocals = Descendants(HixDecompiler.Decompile(image)).OfType<AssignmentStatementIr>().Select(assignment => assignment.Name).ToHashSet(StringComparer.Ordinal);
    var temporaries = new Dictionary<string, string>(StringComparer.Ordinal);
    string Name(string name) {
      if (!name.StartsWith("__selector_", StringComparison.Ordinal) || declaredLocals.Contains(name)) return name;
      if (!temporaries.TryGetValue(name, out var normalized)) temporaries.Add(name, normalized = "__selector_" + temporaries.Count);
      return normalized;
    }
    var text = new StringBuilder();
    foreach (var item in decoded) {
      var instruction = item.Instruction;
      text.Append(instruction.Opcode).Append('|');
      if (instruction.IsRelative) text.Append(positions[item.Offset + instruction.A]);
      else if (instruction.UsesStringPool) text.Append(instruction.Opcode is HixOpcode.StoreLocal or HixOpcode.Member
        ? Name(image.StringPool[instruction.A]) : image.StringPool[instruction.A]);
      else if (instruction.Opcode is HixOpcode.LoadConst or HixOpcode.Call) {
        var value = image.ConstantPool[instruction.A];
        text.Append(value.Kind).Append(':').Append(value is FunctionReferenceHixValue reference ? reference.Signature.Display :
          value is NumberHixValue number ? number.Value.ToString("R", CultureInfo.InvariantCulture) : value.ToString());
      } else text.Append(instruction.A);
      text.Append('|').Append(instruction.B).Append('\n');
    }
    void Fields(IReadOnlyList<BytecodeField> fields) {
      text.Append(fields == null ? -1 : fields.Count).Append('|');
      if (fields == null) return;
      foreach (var field in fields) text.Append(field.Name).Append(':').Append(field.Kind).Append(':').Append(field.Variadic)
        .Append(':').Append(field.Optional).Append(':').Append(field.DefaultValue?.Kind).Append(':').Append(field.DefaultValue).Append('|');
    }
    void Scope(LanguageFunctionScope scope, string name, bool parents) {
      foreach (var item in scope.DisassemblyFunctions(name, parents)) {
        text.Append(item.Scope).Append(':').Append(item.Function.Name).Append(':').Append(item.Function.IsPure).Append(':').Append(positions[item.Function.Body]).Append('|');
        foreach (var signature in item.Function.Signatures) {
          text.Append(signature.InputKind).Append('|'); Fields(signature.Inputs);
          text.Append(signature.OutputKind).Append('|'); Fields(signature.Outputs);
        }
        text.Append('\n');
      }
    }
    Scope(image.Scope, "mixin", true);
    foreach (var expression in image.Expressions) text.Append("entry|").Append(positions[expression.Function.Body]).Append('|').Append(expression.IsPrelude).Append('\n');
    foreach (var derivation in image.Derivations) {
      text.Append("derive|").Append(derivation.Name).Append('\n'); Scope(derivation.Scope, "derivation", false);
      foreach (var expression in derivation.Expressions) text.Append(positions[expression.Function.Body]).Append('|').Append(expression.IsPrelude).Append('\n');
    }
    foreach (var pattern in image.Patterns.OrderBy(item => item.Key, StringComparer.Ordinal)) text.Append("pattern|").Append(pattern.Key).Append('|').Append(pattern.Value.Display).Append('\n');
    foreach (var marker in image.Markers.OrderBy(item => item.Key, StringComparer.Ordinal)) text.Append("marker|").Append(marker.Key).Append('|').Append(positions[marker.Value.Offset]).Append('|').Append(marker.Value.Type).Append('\n');
    Fields(image.Parameters);
    return text.ToString();
  }
}
