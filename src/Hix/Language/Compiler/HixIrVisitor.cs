namespace Hix.Compiler;

/// <summary>Read-only structural traversal. Semantic visitors can attach facts to compilation-owned nodes.</summary>
public class HixIrVisitor {
  public void Visit(HixModuleIr module) {
    foreach (var function in module.Functions) Visit(function);
    foreach (var expression in module.Prelude) Visit(expression);
    foreach (var expression in module.Late) Visit(expression);
  }
  public virtual void Visit(HixIrNode node) {
    switch (node) {
      case null: return;
      case FunctionDeclarationIr function: VisitFunction(function); break;
      case ExpressionDeclarationIr expression: VisitExpressionDeclaration(expression); break;
      case BlockStatementIr block: VisitBlock(block); break;
      case AssignmentStatementIr assignment: VisitAssignment(assignment); break;
      case CallExpressionIr call: VisitCall(call); break;
      case RootExpressionIr root: VisitRoot(root); break;
      case MemberExpressionIr member: VisitMember(member); break;
      case SelectionExpressionIr selection: VisitSelection(selection); break;
      default: VisitChildren(node); break;
    }
  }
  protected virtual void VisitChildren(HixIrNode node) {
    foreach (var child in node.SemanticChildren) Visit(child);
  }
  protected virtual void VisitFunction(FunctionDeclarationIr function) => Visit(function.Body);
  protected virtual void VisitExpressionDeclaration(ExpressionDeclarationIr expression) => Visit(expression.Body);
  protected virtual void VisitBlock(BlockStatementIr block) => VisitChildren(block);
  protected virtual void VisitAssignment(AssignmentStatementIr assignment) => Visit(assignment.Value);
  protected virtual void VisitCall(CallExpressionIr call) => VisitChildren(call);
  protected virtual void VisitRoot(RootExpressionIr root) { }
  protected virtual void VisitMember(MemberExpressionIr member) => Visit(member.Receiver);
  protected virtual void VisitSelection(SelectionExpressionIr selection) => VisitChildren(selection);
}
