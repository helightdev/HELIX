using System;
using System.Linq;
using MixinLanguage.Compiler;
using NUnit.Framework;

namespace HelixRider.Tests.MixinLanguage;

[TestFixture]
public sealed class MixinEditorModelTests
{
    [Test]
    public void EditorLexerIsLosslessAndClassifiesLanguagePunctuation()
    {
        const string source = "@# comment\n@LOCAL<Name> @param#symbol:name:replace<Old><New>\n@+ :members";
        var tokens = MixinLexer.Lex(source).ToArray();

        Assert.That(tokens.First().Start, Is.Zero);
        Assert.That(tokens.Last().End, Is.EqualTo(source.Length));
        for (var index = 1; index < tokens.Length; index++)
            Assert.That(tokens[index].Start, Is.EqualTo(tokens[index - 1].End));
        Assert.That(tokens.Select(token => token.Kind), Does.Contain(MixinTokenKind.Comment));
        Assert.That(tokens.Select(token => token.Kind), Does.Contain(MixinTokenKind.Directive));
        Assert.That(tokens.Select(token => token.Kind), Does.Contain(MixinTokenKind.Path));
        Assert.That(tokens.Select(token => token.Kind), Does.Contain(MixinTokenKind.Function));
        Assert.That(tokens.Select(token => token.Kind), Does.Contain(MixinTokenKind.Continuation));
    }

    [Test]
    public void EditorLexerDoesNotParseOpaqueCSharpAsMixinExpressions()
    {
        const string source =
            "@MIXIN<$PostConstruct><0> global::UnityEngine.UIElements.VisualElementExtensions.Call(this)";
        var tokens = MixinLexer.Lex(source);

        Assert.That(tokens.Any(token => token.Kind == MixinTokenKind.Function &&
            Slice(source, token.Start, token.End) == "UnityEngine"), Is.False);
        Assert.That(tokens.Count(token => token.Kind == MixinTokenKind.DirectiveArgumentDelimiter),
            Is.EqualTo(4));
    }

    [Test]
    public void EditorSyntaxProjectsCompilerReferencesOntoOriginalSource()
    {
        const string source = "@LOCAL<Value> @local#Prop#HashCodeSyntax:unwrap:matches<\\S>\n";
        var tree = MixinParser.Parse(source);
        var nodes = DescendantsAndSelf(tree.Root).ToArray();

        Assert.That(Slice(source, tree.Root.Children[0].Children[0]), Is.EqualTo("@LOCAL"));
        Assert.That(nodes.Single(node => node.Kind == MixinSyntaxKind.Path).Children, Is.Empty);
        Assert.That(nodes.Any(node => node.Kind == MixinSyntaxKind.LiteralArgument &&
            Slice(source, node) == "<\\S>"), Is.True);
        AssertContained(tree.Root);
    }

    [Test]
    public void EditorSyntaxAssignsSemanticDirectiveArgumentKinds()
    {
        const string source =
            "@FUNC<Build>\n@GOTO<done>\n@ANNOTATION<Example.Attribute>\n@MIXIN<target><0> code\n";
        var arguments = MixinParser.Parse(source).Root.Children
            .SelectMany(node => node.Children)
            .Where(node => node.Kind is MixinSyntaxKind.DirectiveArgument or
                MixinSyntaxKind.DeclarationDirectiveArgument or
                MixinSyntaxKind.ReferenceDirectiveArgument or
                MixinSyntaxKind.DeclarationReferenceDirectiveArgument)
            .Select(node => node.Kind)
            .ToArray();

        Assert.That(arguments, Does.Contain(MixinSyntaxKind.DeclarationDirectiveArgument));
        Assert.That(arguments, Does.Contain(MixinSyntaxKind.ReferenceDirectiveArgument));
        Assert.That(arguments, Does.Contain(MixinSyntaxKind.DeclarationReferenceDirectiveArgument));
        Assert.That(arguments, Does.Contain(MixinSyntaxKind.DirectiveArgument));
    }

    [Test]
    public void CatalogMirrorsRuntimeFunctionsAndDirectives()
    {
        Assert.That(MixinLanguageCatalog.Directives.Any(item =>
            item.Name == "PUT" && item.MaximumArguments == 2 && item.OperandKind == MixinOperandKind.Value),
            Is.True);
        Assert.That(MixinLanguageCatalog.Functions.Any(item =>
            item.Name == "reduce" && item.ArgumentRoles.SequenceEqual(new[] {
                MixinArgumentRole.Value, MixinArgumentRole.Function
            })), Is.True);
        Assert.That(MixinLanguageCatalog.Functions.Any(item =>
            item.Name == "members" && item.ReceiverKind == MixinReceiverKind.Type), Is.True);
    }

    [Test]
    public void AnalysisFindsScopedDeclarationsReferencesAndDiagnostics()
    {
        const string source =
            "@FUNC<Build>\n@LABEL<retry>\n@GOTO<missing>\n@END\n@CALL<Build> value\n";
        var analysis = MixinEditorAnalyzer.Analyze(source);

        Assert.That(analysis.Declarations.Any(item => item.Kind == MixinEditorSymbolKind.Function &&
            Slice(source, item.Range.Start, item.Range.End) == "Build"), Is.True);
        Assert.That(analysis.References.Any(item => item.Kind == MixinEditorReferenceKind.Function &&
            Slice(source, item.Range.Start, item.Range.End) == "Build"), Is.True);
        var diagnostic = analysis.Diagnostics.Single(item => item.Message.Contains("unresolved label"));
        Assert.That(Slice(source, diagnostic.Range.Start, diagnostic.Range.End), Is.EqualTo("missing"));
    }

    [TestCase("@", MixinEditorCompletionKind.Directive)]
    [TestCase("@RETURN @", MixinEditorCompletionKind.Root)]
    [TestCase("@RETURN @table:", MixinEditorCompletionKind.Function)]
    [TestCase("@GOTO<ret", MixinEditorCompletionKind.Label)]
    [TestCase("@CALL<Result><Bui", MixinEditorCompletionKind.DeclaredFunction)]
    [TestCase("@ANNOTATION<HELIX.Pro", MixinEditorCompletionKind.CSharpType)]
    [TestCase("@RETURN @local#Na", MixinEditorCompletionKind.Local)]
    [TestCase("@RETURN @table:reduce<@null><Com", MixinEditorCompletionKind.DeclaredFunction)]
    public void AnalysisProvidesIncompleteCompletionContexts(string source,
        MixinEditorCompletionKind expected)
    {
        Assert.That(MixinEditorAnalyzer.GetCompletionContext(source, source.Length).Kind,
            Is.EqualTo(expected));
    }

    [Test]
    public void CompletionReplacesWholeNameWithoutOperator()
    {
        const string source = "@RETURN @table:put";
        var context = MixinEditorAnalyzer.GetCompletionContext(source, source.Length - 1);

        Assert.That(context.Kind, Is.EqualTo(MixinEditorCompletionKind.Function));
        Assert.That(Slice(source, context.ReplacementRange.Start, context.ReplacementRange.End),
            Is.EqualTo("put"));
        Assert.That(context.Prefix, Is.EqualTo("pu"));
    }

    [Test]
    public void AnalysisRecognizesImplicitAndExplicitScopeTermination()
    {
        var implicitScope = MixinEditorAnalyzer.Analyze(
            "@FUNC<Flow>\n@SCOPE\n@MATCH @true\n@LABEL<next>\n@RETURN @null\n@END\n");
        var explicitScope = MixinEditorAnalyzer.Analyze(
            "@ANNOTATION<Example.Attribute>\n@SCOPE\n@MATCH @true\n@END\n@END\n");

        Assert.That(implicitScope.Diagnostics.Any(item => item.Message.Contains("unterminated") ||
            item.Message == "unmatched @END"), Is.False);
        Assert.That(explicitScope.Diagnostics.Any(item => item.Message.Contains("unterminated") ||
            item.Message == "unmatched @END"), Is.False);
    }

    [Test]
    public void TypingFactsPairOnlyLanguageDelimiters()
    {
        Assert.That(MixinEditorTypingFacts.CanOpenPair("@LOCAL", 6, '<'), Is.True);
        Assert.That(MixinEditorTypingFacts.CanOpenPair("@RETURN @table:put", 18, '<'), Is.True);
        Assert.That(MixinEditorTypingFacts.CanOpenPair("@RETURN @", 9, '('), Is.True);
        Assert.That(MixinEditorTypingFacts.CanOpenPair("@CODE value.Call", 16, '('), Is.False);
        Assert.That(MixinEditorTypingFacts.IsEmptyPair("@LOCAL<>", 7), Is.True);
        Assert.That(MixinEditorTypingFacts.TryGetContinuationIndent(
            "  @RETURN @table:put<name", 25, out var indent), Is.True);
        Assert.That(indent, Is.EqualTo("    "));
    }

    [Test]
    public void MultilineDynamicArgumentsMapBackWithoutCrossingRanges()
    {
        const string source =
            "@RETURN @table:put<symbol><(@param)>:put<value><(@table\r\n" +
            "  @+:put<name><(@local#Name)>\r\n" +
            "  @+:put<hash><(@local#Hash)>)>\r\n";
        var tree = MixinParser.Parse(source);
        var nodes = DescendantsAndSelf(tree.Root).ToArray();
        var multiline = nodes.Single(node => node.Kind == MixinSyntaxKind.ExpressionArgument &&
            Slice(source, node).Contains("@+:put<hash>"));

        Assert.That(DescendantsAndSelf(multiline).Count(node =>
            node.Kind == MixinSyntaxKind.Continuation), Is.EqualTo(2));
        AssertNoCrossingRanges(nodes);
    }

    private static string Slice(string source, int start, int end) =>
        source.Substring(start, end - start);

    private static string Slice(string source, MixinAst node) =>
        Slice(source, node.SourceRange.Start, node.SourceRange.End);

    private static System.Collections.Generic.IEnumerable<MixinAst> DescendantsAndSelf(
        MixinAst node)
    {
        yield return node;
        foreach (var child in node.Children)
            foreach (var descendant in DescendantsAndSelf(child)) yield return descendant;
    }

    private static void AssertContained(MixinAst node)
    {
        foreach (var child in node.Children)
        {
            Assert.That(child.SourceRange.Start, Is.InRange(node.SourceRange.Start, node.SourceRange.End));
            Assert.That(child.SourceRange.End, Is.InRange(child.SourceRange.Start, node.SourceRange.End));
            AssertContained(child);
        }
    }

    private static void AssertNoCrossingRanges(MixinAst[] nodes)
    {
        for (var leftIndex = 0; leftIndex < nodes.Length; leftIndex++)
        for (var rightIndex = leftIndex + 1; rightIndex < nodes.Length; rightIndex++)
        {
            var left = nodes[leftIndex].SourceRange;
            var right = nodes[rightIndex].SourceRange;
            if (left.Start >= right.End || right.Start >= left.End) continue;
            Assert.That(left.Start <= right.Start && left.End >= right.End ||
                right.Start <= left.Start && right.End >= left.End, Is.True,
                $"crossing editor ranges {left} and {right}");
        }
    }
}
