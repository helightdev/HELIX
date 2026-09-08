using System;
using System.Collections.Generic;
using Mixins.Runtime;

namespace Mixins.Compiler;

/// <summary>Executable blocks lowered once from canonical syntax; operations contain no invocation state.</summary>
internal sealed class LanguageProgramBindings {
  private readonly Dictionary<BlockStatementAst, Action<LanguageExecution>> blocks = new();

  internal LanguageProgramBindings(IEnumerable<HixAst> roots) {
    var visited = new HashSet<HixAst>();
    foreach (var root in roots) Visit(root, visited);
  }

  internal void Execute(BlockStatementAst block, LanguageExecution execution) => blocks[block](execution);

  private void Visit(HixAst node, HashSet<HixAst> visited) {
    if (!visited.Add(node)) return;
    if (node is BlockStatementAst block) {
      LanguageExecution.LowerBlock(block, blocks);
      return;
    }
    foreach (var child in node.SemanticChildren) Visit(child, visited);
  }
}
