using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Impl;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.Match;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Features.Intellisense.CodeCompletion.CSharp.Rules;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Resources;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.UI.Icons;

namespace HelixRider;

[Language(typeof(CSharpLanguage))]
public sealed class EventHandlerMemberCompletionProvider : CSharpItemsProviderBase<CSharpCodeCompletionContext>
{
    private const string Shortcut = "eventHandler";

    protected override bool AddLookupItems(CSharpCodeCompletionContext context, IItemsCollector collector)
    {
        if (!HelixProjectAvailability.IsAvailable(context.PsiModule))
            return false;
        if (!TryGetContext(context, out var typeUsage, out var typeElement, out var typeDeclaration))
            return false;

        var replaceRange = typeUsage.GetDocumentRange()
            .JoinRight(context.BasicContext.SelectedRange);
        if (!replaceRange.IsValid())
            return false;

        var ranges = CodeCompletionContextProviderBase.GetTextLookupRanges(context.BasicContext, replaceRange);
        collector.Add(new EventHandlerLookupItem(typeElement, typeDeclaration, ranges));
        return true;
    }

    private static bool TryGetContext(
        CSharpCodeCompletionContext context,
        out IUserTypeUsage typeUsage,
        out ITypeElement typeElement,
        out IClassLikeDeclaration typeDeclaration)
    {
        typeUsage = null;
        typeElement = null;
        typeDeclaration = null;

        if (context.UnterminatedContext.TreeNode is not ICSharpIdentifier identifier)
            return false;
        var typedPrefix = identifier.Name.Replace(SyntheticComments.CodeCompletionIdentifierToken, string.Empty);
         if (!Shortcut.StartsWith(typedPrefix))
             return false;

        typeUsage = identifier.GetContainingNode<IUserTypeUsage>();
        typeDeclaration = identifier.GetContainingNode<IClassLikeDeclaration>();
        if (typeUsage == null || typeDeclaration == null ||
            identifier.GetContainingNode<IMethodDeclaration>() != null ||
            identifier.GetContainingNode<IAccessorDeclaration>() != null)
            return false;

        var referenceName = typeUsage.ScalarTypeName;
        var qualifier = referenceName?.Qualifier;
        typeElement = qualifier?.Reference.Resolve().DeclaredElement as ITypeElement;
        return typeElement != null;
    }

    private sealed class EventHandlerLookupItem : TextLookupItemBase, IMLSortingAwareItem
    {
        private readonly ITypeElement _eventType;
        private readonly IClassLikeDeclaration _containingType;

        public EventHandlerLookupItem(
            ITypeElement eventType,
            IClassLikeDeclaration containingType,
            TextLookupRanges ranges)
        {
            _eventType = eventType;
            _containingType = containingType;
            Text = Shortcut;
            Ranges = ranges;
        }

        public override IconId Image => PsiSymbolsThemedIcons.Method.Id;

        public override MatchingResult Match(PrefixMatcher prefixMatcher) => prefixMatcher.Match(Shortcut);

        public override void Accept(
            ITextControl textControl,
            DocumentRange nameRange,
            LookupItemInsertType insertType,
            Suffix suffix,
            ISolution solution,
            bool keepCaretStill)
        {
            var factory = CSharpElementFactory.GetInstance(_containingType, false);
            var eventType = TypeFactory.CreateType(_eventType);
            var methodName = "On" + _eventType.ShortName;
            var declaration = factory.CreateTypeMemberDeclaration(
                "[HELIX.Context.EventHandler] private void $0($1 evt) {\n}",
                methodName,
                eventType);
            var text = declaration.GetText();
            var range = Ranges.ReplaceRange;
            textControl.Document.ReplaceText(range.TextRange, text);
            textControl.Caret.MoveTo(range.StartOffset + text.Length - 1, CaretVisualPlacement.DontScrollIfVisible);
        }

        public bool UseMLSort() => false;
    }
}
