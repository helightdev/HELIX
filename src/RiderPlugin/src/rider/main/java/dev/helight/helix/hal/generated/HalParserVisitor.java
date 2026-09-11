// Generated from HalParser.g4 by ANTLR 4.13.2
package dev.helight.helix.hal.generated;
import org.antlr.v4.runtime.tree.ParseTreeVisitor;

/**
 * This interface defines a complete generic visitor for a parse tree produced
 * by {@link HalParser}.
 *
 * @param <T> The return type of the visit operation. Use {@link Void} for
 * operations with no return type.
 */
public interface HalParserVisitor<T> extends ParseTreeVisitor<T> {
	/**
	 * Visit a parse tree produced by {@link HalParser#document}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitDocument(HalParser.DocumentContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#sectionBlock}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitSectionBlock(HalParser.SectionBlockContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#metadata}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMetadata(HalParser.MetadataContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#metadataArguments}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMetadataArguments(HalParser.MetadataArgumentsContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#metadataArgument}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitMetadataArgument(HalParser.MetadataArgumentContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#section}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitSection(HalParser.SectionContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#sectionEntry}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitSectionEntry(HalParser.SectionEntryContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#field}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitField(HalParser.FieldContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#fieldKey}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitFieldKey(HalParser.FieldKeyContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#value}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitValue(HalParser.ValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#typedContainer}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTypedContainer(HalParser.TypedContainerContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#table}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTable(HalParser.TableContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#tableEntry}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitTableEntry(HalParser.TableEntryContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#list}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitList(HalParser.ListContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#collectionValue}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitCollectionValue(HalParser.CollectionValueContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#call}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitCall(HalParser.CallContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#argumentList}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitArgumentList(HalParser.ArgumentListContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#argument}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitArgument(HalParser.ArgumentContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#selection}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitSelection(HalParser.SelectionContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#looseScalar}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitLooseScalar(HalParser.LooseScalarContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#scalarAtom}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitScalarAtom(HalParser.ScalarAtomContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#lineEnd}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitLineEnd(HalParser.LineEndContext ctx);
	/**
	 * Visit a parse tree produced by {@link HalParser#newlines}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitNewlines(HalParser.NewlinesContext ctx);
}