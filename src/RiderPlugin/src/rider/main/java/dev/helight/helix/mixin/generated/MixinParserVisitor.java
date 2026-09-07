// Generated from MixinParser.g4 by ANTLR 4.13.2
package dev.helight.helix.mixin.generated;
import org.antlr.v4.runtime.tree.ParseTreeVisitor;

/**
 * This interface defines a complete generic visitor for a parse tree produced
 * by {@link MixinParser}.
 *
 * @param <T> The return type of the visit operation. Use {@link Void} for
 * operations with no return type.
 */
public interface MixinParserVisitor<T> extends ParseTreeVisitor<T> {
	/**
	 * Visit a parse tree produced by {@link MixinParser#compilationUnit}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitCompilationUnit(MixinParser.CompilationUnitContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#mixinDeclaration}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMixinDeclaration(MixinParser.MixinDeclarationContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#mixinBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMixinBody(MixinParser.MixinBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#expressionDeclaration}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitExpressionDeclaration(MixinParser.ExpressionDeclarationContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#funcDeclaration}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFuncDeclaration(MixinParser.FuncDeclarationContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#functionMetadata}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionMetadata(MixinParser.FunctionMetadataContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#functionSignatureVariant}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionSignatureVariant(MixinParser.FunctionSignatureVariantContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#signature}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitSignature(MixinParser.SignatureContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#tableSignature}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTableSignature(MixinParser.TableSignatureContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#tableSignatureEntry}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTableSignatureEntry(MixinParser.TableSignatureEntryContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#statementBlock}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitStatementBlock(MixinParser.StatementBlockContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#statement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitStatement(MixinParser.StatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#invocationStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitInvocationStatement(MixinParser.InvocationStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenConditionStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenConditionStatement(MixinParser.WhenConditionStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenElseBranch}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenElseBranch(MixinParser.WhenElseBranchContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenResult}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenResult(MixinParser.WhenResultContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenChainCondition}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenChainCondition(MixinParser.WhenChainConditionContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenChainStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenChainStatement(MixinParser.WhenChainStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenChainBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenChainBody(MixinParser.WhenChainBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenChainBranch}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenChainBranch(MixinParser.WhenChainBranchContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenValueStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenValueStatement(MixinParser.WhenValueStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenValueBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenValueBody(MixinParser.WhenValueBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenValueBranch}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenValueBranch(MixinParser.WhenValueBranchContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#whenValueCondition}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenValueCondition(MixinParser.WhenValueConditionContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#assignmentStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitAssignmentStatement(MixinParser.AssignmentStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#assignedValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitAssignedValue(MixinParser.AssignedValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#controlflowStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitControlflowStatement(MixinParser.ControlflowStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#contentBlock}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitContentBlock(MixinParser.ContentBlockContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#contentBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitContentBody(MixinParser.ContentBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#contentInterpolate}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitContentInterpolate(MixinParser.ContentInterpolateContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#value}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitValue(MixinParser.ValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#valueExpression}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitValueExpression(MixinParser.ValueExpressionContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#nonArgumentValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitNonArgumentValue(MixinParser.NonArgumentValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#primaryValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPrimaryValue(MixinParser.PrimaryValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#prefixOperators}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPrefixOperators(MixinParser.PrefixOperatorsContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#postfixOperators}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPostfixOperators(MixinParser.PostfixOperatorsContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#valueStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitValueStatement(MixinParser.ValueStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#tailValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTailValue(MixinParser.TailValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#tupleValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTupleValue(MixinParser.TupleValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#tableValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTableValue(MixinParser.TableValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#tableKeyedEntry}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTableKeyedEntry(MixinParser.TableKeyedEntryContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#valueList}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitValueList(MixinParser.ValueListContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#argumentValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitArgumentValue(MixinParser.ArgumentValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#argumentBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitArgumentBody(MixinParser.ArgumentBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#inlineValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitInlineValue(MixinParser.InlineValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#inlineTransformation}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitInlineTransformation(MixinParser.InlineTransformationContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#derivation}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitDerivation(MixinParser.DerivationContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#derivationRoot}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitDerivationRoot(MixinParser.DerivationRootContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#elvisValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitElvisValue(MixinParser.ElvisValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#transformationPart}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTransformationPart(MixinParser.TransformationPartContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#functionChainType}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionChainType(MixinParser.FunctionChainTypeContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#labelIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitLabelIdentifier(MixinParser.LabelIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#memberIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMemberIdentifier(MixinParser.MemberIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#mixinIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMixinIdentifier(MixinParser.MixinIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#variableIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitVariableIdentifier(MixinParser.VariableIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#functionIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionIdentifier(MixinParser.FunctionIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#expressionModifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitExpressionModifier(MixinParser.ExpressionModifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#mixinModifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMixinModifier(MixinParser.MixinModifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#variableSpecifiers}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitVariableSpecifiers(MixinParser.VariableSpecifiersContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#funcModifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFuncModifier(MixinParser.FuncModifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#trivia}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTrivia(MixinParser.TriviaContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#comment}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitComment(MixinParser.CommentContext ctx);
	/**
	 * Visit a parse tree produced by {@link MixinParser#escaped}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitEscaped(MixinParser.EscapedContext ctx);
}