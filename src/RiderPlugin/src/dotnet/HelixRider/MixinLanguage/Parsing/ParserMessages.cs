#if RIDER
namespace HelixRider.MixinLanguage.Parsing;

public static class ParserMessages
{
    public const string IDS_SYNTAX_NODE = "syntax node";
    public const string IDS_REFERENCE_NODE = "reference";
    public const string IDS_PARENTHESIZED_REFERENCE_NODE = "parenthesized reference";
    public const string IDS_PATH_NODE = "path";
    public const string IDS_FUNCTION_CALL_NODE = "function call";
    public const string IDS_LITERAL_ARGUMENT_NODE = "literal argument";
    public const string IDS_EXPRESSION_ARGUMENT_NODE = "expression argument";
    public static string GetString(string id) => id;
    public static string GetUnexpectedTokenMessage() => "Unexpected token";
    public static string GetExpectedMessage(string token) => token + " expected";
    public static string GetExpectedMessage(string first, string second) => first + " or " + second + " expected";
}
#endif
