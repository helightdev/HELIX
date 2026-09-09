// Generated from HixParser.g4 by ANTLR 4.13.2
package dev.helight.helix.hix.generated;
import org.antlr.v4.runtime.tree.ParseTreeVisitor;

/**
 * This interface defines a complete generic visitor for a parse tree produced
 * by {@link HixParser}.
 *
 * @param <T> The return type of the visit operation. Use {@link Void} for
 * operations with no return type.
 */
public interface HixParserVisitor<T> extends ParseTreeVisitor<T> {
	/**
	 * Visit a parse tree produced by {@link HixParser#compilationUnit}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitCompilationUnit(HixParser.CompilationUnitContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#fileMetadataSection}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFileMetadataSection(HixParser.FileMetadataSectionContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#topLevelDeclaration}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTopLevelDeclaration(HixParser.TopLevelDeclarationContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#metadataList}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMetadataList(HixParser.MetadataListContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#metadata}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMetadata(HixParser.MetadataContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#metadataValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMetadataValue(HixParser.MetadataValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#mixinDeclaration}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMixinDeclaration(HixParser.MixinDeclarationContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#typeDeclaration}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTypeDeclaration(HixParser.TypeDeclarationContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#mixinBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMixinBody(HixParser.MixinBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#expressionDeclaration}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitExpressionDeclaration(HixParser.ExpressionDeclarationContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#funcDeclaration}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFuncDeclaration(HixParser.FuncDeclarationContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#directFunctionSignature}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitDirectFunctionSignature(HixParser.DirectFunctionSignatureContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#functionBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionBody(HixParser.FunctionBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#functionMetadata}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionMetadata(HixParser.FunctionMetadataContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#functionSignatureVariant}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionSignatureVariant(HixParser.FunctionSignatureVariantContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#patternExpression}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPatternExpression(HixParser.PatternExpressionContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#patternPrimary}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPatternPrimary(HixParser.PatternPrimaryContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#tablePattern}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTablePattern(HixParser.TablePatternContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#tuplePattern}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTuplePattern(HixParser.TuplePatternContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#delegatePattern}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitDelegatePattern(HixParser.DelegatePatternContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#patternParameterList}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPatternParameterList(HixParser.PatternParameterListContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#patternField}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPatternField(HixParser.PatternFieldContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#patternIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPatternIdentifier(HixParser.PatternIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#signature}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitSignature(HixParser.SignatureContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#tableSignature}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTableSignature(HixParser.TableSignatureContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#tableSignatureEntry}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTableSignatureEntry(HixParser.TableSignatureEntryContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#statementBlock}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitStatementBlock(HixParser.StatementBlockContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#statement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitStatement(HixParser.StatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#invocationStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitInvocationStatement(HixParser.InvocationStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenConditionStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenConditionStatement(HixParser.WhenConditionStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenElseBranch}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenElseBranch(HixParser.WhenElseBranchContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenResult}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenResult(HixParser.WhenResultContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenChainCondition}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenChainCondition(HixParser.WhenChainConditionContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenChainStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenChainStatement(HixParser.WhenChainStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenChainBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenChainBody(HixParser.WhenChainBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenChainBranch}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenChainBranch(HixParser.WhenChainBranchContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenValueStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenValueStatement(HixParser.WhenValueStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenValueBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenValueBody(HixParser.WhenValueBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenValueBranch}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenValueBranch(HixParser.WhenValueBranchContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#whenValueCondition}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitWhenValueCondition(HixParser.WhenValueConditionContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#assignmentStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitAssignmentStatement(HixParser.AssignmentStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#assignedValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitAssignedValue(HixParser.AssignedValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#controlflowStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitControlflowStatement(HixParser.ControlflowStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#contentBlock}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitContentBlock(HixParser.ContentBlockContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#contentBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitContentBody(HixParser.ContentBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#contentInterpolate}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitContentInterpolate(HixParser.ContentInterpolateContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#value}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitValue(HixParser.ValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#valueExpression}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitValueExpression(HixParser.ValueExpressionContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#nonArgumentValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitNonArgumentValue(HixParser.NonArgumentValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#primaryValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPrimaryValue(HixParser.PrimaryValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#lambdaValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitLambdaValue(HixParser.LambdaValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#prefixOperators}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPrefixOperators(HixParser.PrefixOperatorsContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#postfixOperators}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitPostfixOperators(HixParser.PostfixOperatorsContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#valueStatement}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitValueStatement(HixParser.ValueStatementContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#tailValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTailValue(HixParser.TailValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#tupleValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTupleValue(HixParser.TupleValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#tableValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTableValue(HixParser.TableValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#tableKeyedEntry}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTableKeyedEntry(HixParser.TableKeyedEntryContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#valueList}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitValueList(HixParser.ValueListContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#argumentValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitArgumentValue(HixParser.ArgumentValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#argumentBody}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitArgumentBody(HixParser.ArgumentBodyContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#inlineValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitInlineValue(HixParser.InlineValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#inlineTransformation}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitInlineTransformation(HixParser.InlineTransformationContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#derivation}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitDerivation(HixParser.DerivationContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#derivationRoot}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitDerivationRoot(HixParser.DerivationRootContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#elvisValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitElvisValue(HixParser.ElvisValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#transformationPart}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTransformationPart(HixParser.TransformationPartContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#functionChainType}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionChainType(HixParser.FunctionChainTypeContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#labelIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitLabelIdentifier(HixParser.LabelIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#memberIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMemberIdentifier(HixParser.MemberIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#mixinIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMixinIdentifier(HixParser.MixinIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#functionDeclarationIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionDeclarationIdentifier(HixParser.FunctionDeclarationIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#variableIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitVariableIdentifier(HixParser.VariableIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#functionIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFunctionIdentifier(HixParser.FunctionIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#kindIdentifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitKindIdentifier(HixParser.KindIdentifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#expressionModifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitExpressionModifier(HixParser.ExpressionModifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#mixinModifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMixinModifier(HixParser.MixinModifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#variableSpecifiers}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitVariableSpecifiers(HixParser.VariableSpecifiersContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#funcModifier}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFuncModifier(HixParser.FuncModifierContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#trivia}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTrivia(HixParser.TriviaContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#comment}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitComment(HixParser.CommentContext ctx);
	/**
	 * Visit a parse tree produced by {@link HixParser#escaped}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitEscaped(HixParser.EscapedContext ctx);
}