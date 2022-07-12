// Copyright (C) 2008-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.07.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="string"/> related extension methods.
  /// </summary>
  public static class StringExtensions
  {
    /// <summary>
    /// Cuts the specified <paramref name="suffix"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The original string value.</param>
    /// <param name="suffix">The suffix to cut.</param>
    /// <returns>String without <paramref name="suffix"/> if it was found;
    /// otherwise, original <paramref name="value"/>.</returns>
    public static string TryCutSuffix(this string value, string suffix)
    {
      if (!value.EndsWith(suffix, StringComparison.Ordinal))
        return value;
      return value.Substring(0, value.Length - suffix.Length);
    }

    /// <summary>
    /// Cuts the specified <paramref name="suffix"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The original value.</param>
    /// <param name="suffix">The suffix to cut.</param>
    /// <returns>Span without <paramref name="suffix"/> if it was found;
    /// otherwise, original <paramref name="value"/>.</returns>
    public static ReadOnlySpan<char>TryCutSuffix(this in ReadOnlySpan<char> value, in ReadOnlySpan<char> suffix)
    {
      if (!value.EndsWith(suffix, StringComparison.Ordinal))
        return value;
      return value.Slice(0, value.Length - suffix.Length);
    }

    /// <summary>
    /// Cuts the specified <paramref name="prefix"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The original string value.</param>
    /// <param name="prefix">The prefix to cut.</param>
    /// <returns>String without <paramref name="prefix"/> if it was found;
    /// otherwise, original <paramref name="value"/>.</returns>
    public static string TryCutPrefix(this string value, string prefix)
    {
      if (!value.StartsWith(prefix, StringComparison.Ordinal))
        return value;
      return value.Substring(prefix.Length);
    }

    /// <summary>
    /// Cuts the specified <paramref name="prefix"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The original value.</param>
    /// <param name="prefix">The suffix to cut.</param>
    /// <returns>Span without <paramref name="prefix"/> if it was found;
    /// otherwise, original <paramref name="value"/>.</returns>
    public static ReadOnlySpan<char> TryCutPrefix(this in ReadOnlySpan<char> value, in ReadOnlySpan<char> prefix)
    {
      if (!value.StartsWith(prefix, StringComparison.Ordinal))
        return value;
      return value.Slice(prefix.Length);
    }

    /// <summary>
    /// Indents the specified string value.
    /// </summary>
    /// <param name="value">The value to indent.</param>
    /// <param name="indentSize">Size of the indent (in space characters).</param>
    /// <returns>Indented <paramref name="value"/>.</returns>
    public static string Indent(this string value, int indentSize)
    {
      return value.Indent(indentSize, true);
    }

    /// <summary>
    /// Indents the specified string value.
    /// </summary>
    /// <param name="value">The value to indent.</param>
    /// <param name="indentSize">Size of the indent (in space characters).</param>
    /// <param name="indentFirstLine">If set to <see langword="true"/>, first line must be indented;
    /// otherwise, <see langword="false"/>.</param>
    /// <returns>Indented <paramref name="value"/>.</returns>
    public static string Indent(this string value, int indentSize, bool indentFirstLine)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      var indent = new string(' ', indentSize);
      var sb = new StringBuilder();
      if (indentFirstLine)
        sb.Append(indent);
      int start = 0;
      int next;
      while ((next = value.IndexOf('\n', start)) >= 0) {
        next++;
        sb.Append(value.Substring(start, next - start));
        sb.Append(indent);
        start = next;
      }
      sb.Append(value.Substring(start, value.Length - start));
      return sb.ToString();
    }

    /// <summary>
    /// Determines whether <paramref name="x"/> <see cref="string"/> is less than <paramref name="y"/> <see cref="string"/>.
    /// </summary>
    /// <param name="x">The first argument.</param>
    /// <param name="y">The second argument.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="x"/> is less than <paramref name="y"/>; otherwise <see langword="false" />.
    /// </returns>
    public static bool LessThan(this string x, string y)
    {
      if (x == y)
        return false;
      if (x == null)
        return true;
      if (y == null)
        return false;
      return x.CompareTo(y) < 0;
    }

    /// <summary>
    /// Determines whether <paramref name="x"/> <see cref="string"/> is less than or equals to <paramref name="y"/> <see cref="string"/>.
    /// </summary>
    /// <param name="x">The first argument.</param>
    /// <param name="y">The second argument.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="x"/> is less than or equals to <paramref name="y"/>; otherwise <see langword="false" />.
    /// </returns>
    public static bool LessThanOrEqual(this string x, string y)
    {
      if (x == y)
        return true;
      if (x == null)
        return true;
      if (y == null)
        return false;
      return x.CompareTo(y) <= 0;
    }

    /// <summary>
    /// Determines whether <paramref name="x"/> <see cref="string"/> is greater than <paramref name="y"/> <see cref="string"/>.
    /// </summary>
    /// <param name="x">The first argument.</param>
    /// <param name="y">The second argument.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="x"/> is greater than <paramref name="y"/>; otherwise <see langword="false" />.
    /// </returns>
    public static bool GreaterThan(this string x, string y)
    {
      if (x == y)
        return false;
      if (x == null)
        return false;
      if (y == null)
        return true;
      return x.CompareTo(y) > 0;
    }

    /// <summary>
    /// Determines whether <paramref name="x"/> <see cref="string"/> is less than <paramref name="y"/> <see cref="string"/>.
    /// </summary>
    /// <param name="x">The first argument.</param>
    /// <param name="y">The second argument.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="x"/> is less than <paramref name="y"/>; otherwise <see langword="false" />.
    /// </returns>
    public static bool GreaterThanOrEqual(this string x, string y)
    {
      if (x == y)
        return true;
      if (x == null)
        return false;
      if (y == null)
        return true;
      return x.CompareTo(y) >= 0;
    }

    /// <summary>
    /// Converts the <paramref name="source"/> to a separated string
    /// using "escape separator" syntax to encode inner separators in
    /// <paramref name="source"/> parts.
    /// </summary>
    /// <param name="source">The sequence of strings to join.</param>
    /// <param name="escape">The escape character.</param>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns>
    /// Comma-separated string of all the items
    /// from <paramref name="source"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="escape"/>==<paramref name="delimiter"/>.</exception>
    public static string RevertibleJoin(this IEnumerable<string> source, char escape, char delimiter)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (escape==delimiter)
        throw new ArgumentException(
          Strings.ExEscapeCharacterMustDifferFromDelimiterCharacter);

      var sb = new StringBuilder();
      bool needDelimiter = false;
      foreach (var part in source) {
        if (needDelimiter)
          sb.Append(delimiter);
        else
          needDelimiter = true;
        if (part==null)
          continue;
        for (int i = 0; i<part.Length; i++) {
          char c = part[i];
          if (c==delimiter || c==escape)
            sb.Append(escape);
          sb.Append(c);
        }
      }
      return sb.ToString();
    }

    /// <summary>
    /// Reverts the result of <see cref="RevertibleJoin"/>.
    /// </summary>
    /// <param name="source">The source string to split.</param>
    /// <param name="escape">The escape character.</param>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns>
    /// The array of values that were previously joined
    /// by <see cref="RevertibleJoin"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="escape"/>==<paramref name="delimiter"/>.</exception>
    public static IEnumerable<string> RevertibleSplit(this string source, char escape, char delimiter)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (escape==delimiter)
        throw new ArgumentException(
          Strings.ExEscapeCharacterMustDifferFromDelimiterCharacter);

      var sb = new StringBuilder();
      bool previousCharIsEscape = false;
      for (int i = 0; i<source.Length; i++) {
        char c = source[i];
        if (previousCharIsEscape) {
          sb.Append(c);
          previousCharIsEscape = false;
        }
        else if (c==escape) {
          previousCharIsEscape = true;
        }
        else if (c==delimiter) {
          yield return sb.ToString();
          sb.Length = 0;
        }
        else
          sb.Append(c);
      }
      yield return sb.ToString();
    }

    /// <summary>
    /// Reverts the result of <see cref="RevertibleJoin"/>.
    /// </summary>
    /// <param name="source">The source string to split.</param>
    /// <param name="escape">The escape character.</param>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns>
    /// The array of values that were previously joined
    /// by <see cref="RevertibleJoin"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="escape"/>==<paramref name="delimiter"/>.</exception>
    public static Pair<string> RevertibleSplitFirstAndTail(this string source, char escape, char delimiter)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (escape==delimiter)
        throw new ArgumentException(
          Strings.ExEscapeCharacterMustDifferFromDelimiterCharacter);

      var sb = new StringBuilder();
      bool previousCharIsEscape = false;
      for (int i = 0; i<source.Length; i++) {
        char c = source[i];
        if (previousCharIsEscape) {
          sb.Append(c);
          previousCharIsEscape = false;
        }
        else if (c==escape) {
          previousCharIsEscape = true;
        }
        else if (c==delimiter)
          return new Pair<string>(sb.ToString(), source.Substring(i + 1));
        else
          sb.Append(c);
      }
      return new Pair<string>(sb.ToString(), null);
    }

    /// <summary>
    /// Escapes the specified source string.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="escape">The escape char.</param>
    /// <param name="escapedChars">Chars to escape.</param>
    public static string Escape(this string source, char escape, char[] escapedChars)
    {
      if (source==null)
        throw new ArgumentNullException("source");
      if (escapedChars==null)
        throw new ArgumentNullException("escapedChars");
      var chars = escapedChars.Append(escape);
      var sb = new StringBuilder();
      foreach (var c in source) {
        var found = false;
        for (int i = 0; i < chars.Length && !found; i++)
          found = chars[i] == c;
        if (found)
          sb.Append(escape);
        sb.Append(c);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Unescapes the specified source string.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="escape">The escape char.</param>
    public static string Unescape(this string source, char escape)
    {
      if (source==null)
        throw new ArgumentNullException("source");
      var sb = new StringBuilder(source.Length);
      var previousCharIsEscape = false;
      foreach (var c in source) {
        if (previousCharIsEscape) {
          sb.Append(c);
          previousCharIsEscape = false;
        }
        else if (c==escape)
          previousCharIsEscape = true;
        else
          sb.Append(c);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Compares <paramref name="value"/> with <paramref name="sqlLikePattern"/>
    /// </summary>
    /// <param name="value">Value to compare.</param>
    /// <param name="sqlLikePattern">SQL-Like pattern</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="value"/> fits the <paramref name="sqlLikePattern"/>; otherwise <see langword="false" />
    /// </returns>
    public static bool Like(this string value, string sqlLikePattern)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentNotNull(sqlLikePattern, "sqlLikePattern");
      var regexPattern = Regex.Replace(sqlLikePattern,
        @"[%_]|[^%_]+",
        match => {
          if (match.Value == "%") {
            return ".*";
          }
          if (match.Value == "_") {
            return ".";
          }
          return Regex.Escape(match.Value);
        });
      return Regex.IsMatch(value, $"^{regexPattern}$");
    }

    /// <summary>
    /// Compares <paramref name="value"/> with <paramref name="sqlLikePattern"/>
    /// </summary>
    /// <param name="value">Value to compare.</param>
    /// <param name="sqlLikePattern">SQL-Like pattern</param>
    /// <param name="escapeCharacter">Character to escape special symbols like '%' or '_'</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="value"/> fits the <paramref name="sqlLikePattern"/>; otherwise <see langword="false" />
    /// </returns>
    public static bool Like(this string value, string sqlLikePattern, char escapeCharacter)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentNotNull(sqlLikePattern, "sqlLikePattern");
      const string regExSpecialChars = @"[]\/^$.|?*+(){}";

      if(char.IsControl(escapeCharacter))
        throw new ArgumentException(Strings.ExControlCharacterUsedAsEscapeCharacter, "escapeCharacter");
      if(escapeCharacter=='%' || escapeCharacter== '_')
        throw new ArgumentException(string.Format(Strings.ExSpecialCharacterXUsedAsEscapeCharacter, escapeCharacter), "escapeCharacter");

      var escChar = new string(escapeCharacter, 1);
      if (regExSpecialChars.Contains(escapeCharacter))
        escChar = @"\" + escChar;
      var pattern = escChar + escChar + "|" + escChar + @"[%_]|[%_]|[^%" + escapeCharacter + "_]+";

      var regexPattern = Regex.Replace(sqlLikePattern,
        pattern,
        match => {
          if (match.Value=="%") {
            return ".*";
          }
          if (match.Value=="_") {
            return ".";
          }
          if(match.Value.StartsWith(escapeCharacter.ToString(), StringComparison.Ordinal)) {
            return match.Value[1].ToString();
          }
          return Regex.Escape(match.Value);
        });
      return Regex.IsMatch(value, $"^{regexPattern}$");
    }

    /// <summary>
    /// Strips '(' and ')' symetrically.
    /// </summary>
    /// <param name="value">A string to strip round brackets.</param>
    /// <returns>Stripped string.</returns>
    public static string StripRoundBrackets(this string value)
    {
      int start;
      var end = value.Length - 1;
      var actualLength = value.Length;
      for (start = 0; value[start++] == '(' && value[end--] == ')'; actualLength -= 2) { }

      return start == 1
        ? value
        : value.Substring(start - 1, actualLength);
    }

    internal static bool Contains(this string str, string value, StringComparison comparison)
    {
      ArgumentValidator.EnsureArgumentNotNull(str, nameof(str));
      ArgumentValidator.EnsureArgumentNotNull(value, nameof(value));

      return str.IndexOf(value, comparison) >= 0;
    }
  }
}