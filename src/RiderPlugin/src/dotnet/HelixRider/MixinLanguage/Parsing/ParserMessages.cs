#if RIDER
namespace HelixRider.MixinLanguage.Parsing;

public static class ParserMessages
{
    public static string GetString(string id) => id;
    public static string GetUnexpectedTokenMessage() => "Unexpected token";
    public static string GetExpectedMessage(string token) => token + " expected";
    public static string GetExpectedMessage(string first, string second) => first + " or " + second + " expected";
}
#endif
