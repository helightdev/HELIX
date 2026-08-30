using System.Linq;
using HelixRider.MixinLanguage;
using HelixRider.MixinLanguage.Parsing;
using HelixRider.MixinLanguage.Highlighting;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Text;
using NUnit.Framework;

namespace HelixRider.Tests.MixinLanguage;

[TestFixture]
public sealed class HelixMixinParserTests
{
    [Test]
    public void BuildsTraversableLosslessPsiFromSharedSyntax()
    {
        const string source = "@LOCAL<Value> @local#Prop:matches<\\S>\r\n@# comment\r\n";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();

        Assert.That(file.GetText(), Is.EqualTo(source));
        var nodes = AllNodes(file).ToArray();
        Assert.That(nodes.OfType<IDirectiveNode>().Count(), Is.EqualTo(1));
        Assert.That(nodes.OfType<IDirectiveArgumentNode>().Count(), Is.EqualTo(1));
        Assert.That(nodes.OfType<IReferenceNode>().Count(), Is.EqualTo(1));
        Assert.That(nodes.OfType<IFunctionCallNode>().Count(), Is.EqualTo(1));
        Assert.That(nodes.OfType<ILiteralArgumentNode>().Single().GetText(),
            Is.EqualTo("<\\S>"));
        Assert.That(nodes.OfType<HelixRider.MixinLanguage.ICommentNode>().Single().GetText(),
            Is.EqualTo("@# comment\r\n"));
    }

    [Test]
    public void LiteralCSharpDoesNotBecomeExpressionPsi()
    {
        const string source = "@MIXIN<$PostConstruct><0> global::UnityEngine.Call(this)\n";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();

        var operand = AllNodes(file).OfType<IOperandNode>().Single();
        Assert.That(operand.GetText(), Does.Contain("global::UnityEngine"));
        Assert.That(AllNodes(operand).OfType<IReferenceNode>(), Is.Empty);
    }

    [Test]
    public void IncompleteArgumentRemainsLosslessAndStructured()
    {
        const string source = "@LOCAL<Name @this:name";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();

        Assert.That(file.GetText(), Is.EqualTo(source));
        var nodes = AllNodes(file).ToArray();
        Assert.That(nodes.OfType<IDirectiveNode>().Count(), Is.EqualTo(1));
        Assert.That(nodes.OfType<IInvalidNode>().Count(), Is.EqualTo(1));
    }

    [Test]
    public void DelimiterHighlightingComesFromPsiContext()
    {
        const string source = "@LOCAL<Value> @local:matches<\\S> @(var#CompanionName)";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();
        var openingAngles = AllNodes(file).OfType<ITokenNode>().Where(token => token.GetText() == "<").ToArray();

        Assert.That(openingAngles, Has.Length.EqualTo(2));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(openingAngles[0]),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Directive));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(openingAngles[1]),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Function));

        var referenceParentheses = AllNodes(file).OfType<ITokenNode>()
            .Where(token => token.GetText() is "(" or ")" && token.Parent is IParenthesizedReferenceNode)
            .ToArray();
        Assert.That(referenceParentheses, Has.Length.EqualTo(2));
        Assert.That(referenceParentheses.All(token =>
            HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(token) == HelixMixinHighlightingAttributeIds.Value),
            Is.True);

        const string dynamicSource = "@RETURN @table:put<name><(@local#Name)>";
        var dynamicFile = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(dynamicSource))).ParseFile();
        var dynamicParentheses = AllNodes(dynamicFile).OfType<ITokenNode>()
            .Where(token => token.GetText() is "(" or ")" && token.Parent is IExpressionArgumentNode)
            .ToArray();
        Assert.That(dynamicParentheses, Has.Length.EqualTo(2));
        Assert.That(dynamicParentheses.All(token =>
                HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(token) ==
                HelixMixinHighlightingAttributeIds.Value),
            Is.True);
    }

    [Test]
    public void ReparseAfterIncompleteEditKeepsStableStructureAndRanges()
    {
        const string incomplete = "@RETURN @table:put<name><(@(var#CompanionName)";
        const string completed = "@RETURN @table:put<name><(@(var#CompanionName))>";

        var incompleteFile = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(incomplete))).ParseFile();
        var completedFile = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(completed))).ParseFile();

        Assert.That(incompleteFile.GetText(), Is.EqualTo(incomplete));
        Assert.That(AllNodes(incompleteFile).OfType<IInvalidNode>(), Is.Not.Empty);
        Assert.That(completedFile.GetText(), Is.EqualTo(completed));
        Assert.That(AllNodes(completedFile).OfType<IInvalidNode>(), Is.Empty);
        var nested = AllNodes(completedFile).OfType<IParenthesizedReferenceNode>().Single();
        Assert.That(nested.GetText(), Is.EqualTo("@(var#CompanionName)"));
        Assert.That(nested.GetTreeTextRange().StartOffset.Offset,
            Is.EqualTo(completed.IndexOf("@(var#CompanionName)", System.StringComparison.Ordinal)));
    }

    [Test]
    public void NestedLiteralAnglesDoNotBecomeLanguageDelimiters()
    {
        const string source = "@ANNOTATION<global::Provider<Type>>";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();
        var angles = AllNodes(file).OfType<ITokenNode>()
            .Where(token => token.GetText() is "<" or ">")
            .ToArray();

        Assert.That(angles, Has.Length.EqualTo(4));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(angles[0]),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Directive));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(angles[1]),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Argument));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(angles[2]),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Argument));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(angles[3]),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Directive));
    }

    [Test]
    public void EscapedAtSignHasDedicatedPsiAndHighlighting()
    {
        const string source = "@RETURN left@@right @this:name";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();
        var escape = AllNodes(file).OfType<IEscapeNode>().Single();

        Assert.That(escape.GetText(), Is.EqualTo("@@"));
        var tokens = AllNodes(escape).OfType<ITokenNode>().ToArray();
        Assert.That(tokens, Is.Not.Empty);
        Assert.That(tokens.All(token =>
                HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(token) ==
                HelixMixinHighlightingAttributeIds.Escape),
            Is.True);
    }

    [Test]
    public void CompleteSyntaxSpecimenBuildsEveryRequiredCompositeNode()
    {
        const string source =
            "@LOCAL<Value> @local#Prop#nested:put<literal><(@(var#CompanionName))> @@\r\n" +
            "@+ @this:type\n" +
            "@# comment\n";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();
        var nodes = AllNodes(file).ToArray();

        Assert.That(file.GetText(), Is.EqualTo(source));
        Assert.That(nodes.OfType<IDirectiveNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IDirectiveArgumentNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IOperandNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IReferenceNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IParenthesizedReferenceNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IRootNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IMemberNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IPathNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IFunctionCallNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<ILiteralArgumentNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IExpressionArgumentNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<HelixRider.MixinLanguage.ICommentNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IContinuationNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IEscapeNode>(), Is.Not.Empty);
        Assert.That(nodes.OfType<IInvalidNode>(), Is.Empty);
    }

    [Test]
    public void ContinuationFunctionsAndMemberSeparatorsUseExpressionColors()
    {
        const string source = "@RETURN @table#base\n  @+#entry:put<name><(@local#Name)>\n";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();
        var nodes = AllNodes(file).ToArray();
        var continuationFunction = nodes.OfType<IFunctionCallNode>().Single();
        var continuationPath = nodes.OfType<IPathNode>().Single();
        var openingAngle = AllNodes(continuationFunction).OfType<ITokenNode>()
            .First(token => token.GetText() == "<");
        var memberSeparator = nodes.OfType<ITokenNode>().Single(token =>
            token.GetText() == "#" && token.Parent is IReferenceNode);
        var memberTokens = nodes.OfType<IMemberNode>().SelectMany(AllNodes)
            .OfType<ITokenNode>().ToArray();
        var pathTokens = nodes.OfType<IPathNode>().SelectMany(AllNodes)
            .OfType<ITokenNode>().ToArray();

        Assert.That(continuationFunction.GetText(), Is.EqualTo(":put<name><(@local#Name)>") );
        Assert.That(continuationPath.GetText(), Is.EqualTo("#entry"));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(openingAngle),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Function));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(memberSeparator),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Path));
        Assert.That(memberTokens, Is.Not.Empty);
        Assert.That(pathTokens, Is.Not.Empty);
        Assert.That(memberTokens.Concat(pathTokens).All(token =>
                HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(token) ==
                HelixMixinHighlightingAttributeIds.Path),
            Is.True);
    }

    [Test]
    public void MultilineDynamicArgumentOwnsBothKeywordColoredParentheses()
    {
        const string source =
            "@RETURN @table:put<symbol><(@param)>:put<value><(@table\r\n" +
            "  @+:put<name><(@local#Name)>\r\n" +
            "  @+:put<hash><(@local#Hash)>)>\r\n";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();
        var nodes = AllNodes(file).ToArray();
        var multilineArgument = nodes.OfType<IExpressionArgumentNode>()
            .Single(node => node.GetText().Contains("@+:put<hash>"));
        var argumentRange = multilineArgument.GetTreeTextRange();
        var boundaryParentheses = AllNodes(multilineArgument).OfType<ITokenNode>()
            .Where(token => {
                var range = token.GetTreeTextRange();
                return token.GetText() == "("
                    ? range.StartOffset.Offset == argumentRange.StartOffset.Offset + 1
                    : token.GetText() == ")" &&
                      range.EndOffset.Offset == argumentRange.EndOffset.Offset - 1;
            })
            .ToArray();

        Assert.That(file.GetText(), Is.EqualTo(source));
        Assert.That(boundaryParentheses, Has.Length.EqualTo(2));
        Assert.That(boundaryParentheses.All(token =>
                HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(token) ==
                HelixMixinHighlightingAttributeIds.Value),
            Is.True);
        Assert.That(nodes.OfType<IFunctionCallNode>().Count(), Is.EqualTo(4));
        Assert.That(nodes.OfType<IFunctionCallNode>().Single(node =>
            node.GetText().StartsWith(":put<hash>")), Is.Not.Null);
        Assert.That(boundaryParentheses.Single(token => token.GetText() == ")").Parent,
            Is.SameAs(multilineArgument));
        Assert.That(AllNodes(multilineArgument).OfType<IContinuationNode>().Count(), Is.EqualTo(2));
    }

    [Test]
    public void OrdinaryParenthesesInsideDynamicLiteralRemainUncolored()
    {
        const string source = "@RETURN @table:put<code><(Call((value)))>";
        var file = new HelixMixinParser(new HelixMixinLexer(new StringBuffer(source))).ParseFile();
        var argument = AllNodes(file).OfType<IExpressionArgumentNode>().Single();
        var parentheses = AllNodes(argument).OfType<ITokenNode>()
            .Where(token => token.GetText() is "(" or ")")
            .ToArray();

        Assert.That(parentheses, Has.Length.EqualTo(6));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(parentheses[0]),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Value));
        Assert.That(HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(parentheses[5]),
            Is.EqualTo(HelixMixinHighlightingAttributeIds.Value));
        Assert.That(parentheses.Skip(1).Take(4).All(token =>
            HelixMixinSyntaxHighlightingProcessor.GetMixinAttributeId(token) == null), Is.True);
    }

    private static System.Collections.Generic.IEnumerable<ITreeNode> AllNodes(ITreeNode root)
    {
        for (var child = root.FirstChild; child != null; child = child.NextSibling)
        {
            yield return child;
            foreach (var descendant in AllNodes(child)) yield return descendant;
        }
    }
}
