#if RIDER
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes.Idea;
using JetBrains.TextControl.DocumentMarkup;

namespace HelixRider.MixinLanguage.Highlighting;

[RegisterHighlighterGroup(GroupId, "HELIX Mixin",
    HighlighterGroupPriority.LANGUAGE_SETTINGS,
    Language = typeof(HelixMixinLanguage),
    DemoText = "@LOCAL<Name> @(this:type)\n@RETURN @param#symbol:members")]
[RegisterHighlighter(Directive, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = DefaultLanguageAttributeIds.KEYWORD, Layer = HighlighterLayer.SYNTAX)]
[RegisterHighlighter(Value, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = DefaultLanguageAttributeIds.KEYWORD, Layer = HighlighterLayer.SYNTAX)]
[RegisterHighlighter(Path, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = IdeaHighlightingAttributeIds.INSTANCE_FIELD, Layer = HighlighterLayer.SYNTAX)]
[RegisterHighlighter(Function, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = IdeaHighlightingAttributeIds.INSTANCE_METHOD, Layer = HighlighterLayer.SYNTAX)]
[RegisterHighlighter(FunctionArgumentDelimiter, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = IdeaHighlightingAttributeIds.INSTANCE_METHOD,
    Layer = HighlighterLayer.ADDITIONAL_SYNTAX + 1)]
[RegisterHighlighter(Argument, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = IdeaHighlightingAttributeIds.MARKUP_ATTRIBUTE, Layer = HighlighterLayer.SYNTAX)]
[RegisterHighlighter(Operator, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = IdeaHighlightingAttributeIds.FUNCTION_CALL, Layer = HighlighterLayer.SYNTAX)]
[RegisterHighlighter(Parenthesis, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = IdeaHighlightingAttributeIds.PARENTHESES, Layer = HighlighterLayer.SYNTAX)]
[RegisterHighlighter(Escape, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = IdeaHighlightingAttributeIds.VALID_STRING_ESCAPE, Layer = HighlighterLayer.SYNTAX)]
[RegisterHighlighter(Comment, GroupId = GroupId, EffectType = EffectType.TEXT,
    FallbackAttributeId = DefaultLanguageAttributeIds.LINE_COMMENT, Layer = HighlighterLayer.SYNTAX)]
internal static class HelixMixinHighlightingAttributeIds
{
    public const string GroupId = "HELIX Mixin";
    public const string Directive = "HELIX_MIXIN_DIRECTIVE";
    public const string Value = "HELIX_MIXIN_VALUE";
    public const string Path = "HELIX_MIXIN_PATH";
    public const string Function = "HELIX_MIXIN_FUNCTION";
    public const string FunctionArgumentDelimiter = "HELIX_MIXIN_FUNCTION_ARGUMENT_DELIMITER";
    public const string Argument = "HELIX_MIXIN_ARGUMENT";
    public const string Operator = "HELIX_MIXIN_OPERATOR";
    public const string Parenthesis = "HELIX_MIXIN_PARENTHESIS";
    public const string Escape = "HELIX_MIXIN_ESCAPE";
    public const string Comment = "HELIX_MIXIN_COMMENT";
}
#endif
