using System;
using System.Collections.Generic;
using System.Text;

namespace HELIX {
  public static class Casing {

    public enum CaseStyle {
      PascalCase,
      CamelCase,
      SnakeCase,
      ScreamingSnakeCase,
      KebabCase,
      ScreamingKebabCase,
      SentenceCase,
      TitleCase,
      LowerCase,
      UpperCase,
      DomainCase
    }

    public static string Convert(string str, CaseStyle style) {
      var words = SplitWords(str);

      if (words.Count == 0) {
        return string.Empty;
      }

      switch (style) {
        case CaseStyle.PascalCase:
          return JoinPascal(words);

        case CaseStyle.CamelCase:
          return JoinCamel(words);

        case CaseStyle.SnakeCase:
          return Join(words, "_", false);

        case CaseStyle.ScreamingSnakeCase:
          return Join(words, "_", true);

        case CaseStyle.KebabCase:
          return Join(words, "-", false);

        case CaseStyle.ScreamingKebabCase:
          return Join(words, "-", true);

        case CaseStyle.SentenceCase:
          return JoinSentence(words);

        case CaseStyle.TitleCase:
          return JoinTitle(words);

        case CaseStyle.LowerCase:
          return Join(words, " ", false);

        case CaseStyle.UpperCase:
          return Join(words, " ", true);

        case CaseStyle.DomainCase:
          return Join(words, ".", false);

        default:
          throw new ArgumentOutOfRangeException(nameof(style), style, null);
      }
    }

    public static string ToPascalCase(string str) {
      return Convert(str, CaseStyle.PascalCase);
    }

    public static string ToCamelCase(string str) {
      return Convert(str, CaseStyle.CamelCase);
    }

    public static string ToSnakeCase(string str) {
      return Convert(str, CaseStyle.SnakeCase);
    }

    public static string ToScreamingSnakeCase(string str) {
      return Convert(str, CaseStyle.ScreamingSnakeCase);
    }

    public static string ToKebabCase(string str) {
      return Convert(str, CaseStyle.KebabCase);
    }

    public static string ToScreamingKebabCase(string str) {
      return Convert(str, CaseStyle.ScreamingKebabCase);
    }

    public static string ToSentenceCase(string str) {
      return Convert(str, CaseStyle.SentenceCase);
    }

    public static string ToTitleCase(string str) {
      return Convert(str, CaseStyle.TitleCase);
    }

    public static string ToLowerCase(string str) {
      return Convert(str, CaseStyle.LowerCase);
    }

    public static string ToUpperCase(string str) {
      return Convert(str, CaseStyle.UpperCase);
    }

    private static List<string> SplitWords(string str) {
      var words = new List<string>();

      if (string.IsNullOrWhiteSpace(str)) {
        return words;
      }

      var current = new StringBuilder();

      for (var i = 0; i < str.Length; i++) {
        var c = str[i];

        if (!char.IsLetterOrDigit(c)) {
          AddWord(words, current);
          continue;
        }

        if (current.Length > 0) {
          var previous = str[i - 1];

          var lowerToUpper =
            char.IsLower(previous) &&
            char.IsUpper(c);

          var letterToDigit =
            char.IsLetter(previous) &&
            char.IsDigit(c);

          var digitToLetter =
            char.IsDigit(previous) &&
            char.IsLetter(c);

          var acronymBoundary =
            char.IsUpper(previous) &&
            char.IsUpper(c) &&
            i + 1 < str.Length &&
            char.IsLower(str[i + 1]);

          if (
            lowerToUpper ||
            letterToDigit ||
            digitToLetter ||
            acronymBoundary
          ) {
            AddWord(words, current);
          }
        }

        current.Append(c);
      }

      AddWord(words, current);

      return words;
    }

    private static void AddWord(
      List<string> words,
      StringBuilder current
    ) {
      if (current.Length == 0) {
        return;
      }

      words.Add(current.ToString().ToLowerInvariant());
      current.Clear();
    }

    private static string JoinPascal(List<string> words) {
      var result = new StringBuilder();

      foreach (var word in words) {
        result.Append(Capitalize(word));
      }

      return result.ToString();
    }

    private static string JoinCamel(List<string> words) {
      if (words.Count == 0) {
        return string.Empty;
      }

      var result = new StringBuilder(words[0].ToLowerInvariant());

      for (var i = 1; i < words.Count; i++) {
        result.Append(Capitalize(words[i]));
      }

      return result.ToString();
    }

    private static string JoinSentence(List<string> words) {
      if (words.Count == 0) {
        return string.Empty;
      }

      var result = new StringBuilder(Capitalize(words[0]));

      for (var i = 1; i < words.Count; i++) {
        result.Append(' ');
        result.Append(words[i].ToLowerInvariant());
      }

      return result.ToString();
    }

    private static string JoinTitle(List<string> words) {
      var result = new StringBuilder();

      for (var i = 0; i < words.Count; i++) {
        if (i > 0) {
          result.Append(' ');
        }

        result.Append(Capitalize(words[i]));
      }

      return result.ToString();
    }

    private static string Join(
      List<string> words,
      string separator,
      bool upper
    ) {
      var result = new StringBuilder();

      for (var i = 0; i < words.Count; i++) {
        if (i > 0) {
          result.Append(separator);
        }

        result.Append(
          upper
            ? words[i].ToUpperInvariant()
            : words[i].ToLowerInvariant()
        );
      }

      return result.ToString();
    }

    private static string Capitalize(string word) {
      if (string.IsNullOrEmpty(word)) {
        return word;
      }

      if (word.Length == 1) {
        return word.ToUpperInvariant();
      }

      return char.ToUpperInvariant(word[0]) +
             word.Substring(1).ToLowerInvariant();
    }

  }
}