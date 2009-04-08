// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.18

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// <see cref="string"/> related extension methods.
  /// </summary>
  public static class StringExtensions
  {
    /// <summary>
    /// Indicates whether the specified  <paramref name="value"/> 
    /// is <see langword="null"/> or is <see cref="string.Empty"/>.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <paramref name="value"/> 
    /// is <see langword="null"/> or is <see cref="string.Empty"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNullOrEmpty(this string value)
    {
      return string.IsNullOrEmpty(value);
    }
    
    /// <summary>
    /// Cuts the specified <paramref name="suffix"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The original string value.</param>
    /// <param name="suffix">The suffix to cut.</param>
    /// <returns>String without <paramref name="suffix"/> if it was found; 
    /// otherwise, original <paramref name="value"/>.</returns>
    public static string TryCutSuffix(this string value, string suffix)
    {
      if (!value.EndsWith(suffix))
        return value;
      return value.Substring(0, value.Length - suffix.Length);        
    }

    /// <summary>
    /// Cuts the specified <paramref name="suffix"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The original string value.</param>
    /// <param name="suffix">The suffix to cut.</param>
    /// <param name="isCut">Upon return contains <see langword="true"/>
    /// if suffix was cut, otherwise <see langword="false"/></param>
    /// <returns>String without <paramref name="suffix"/> if it was found; 
    /// otherwise, original <paramref name="value"/>.</returns>
    public static string TryCutSuffix(this string value, string suffix, out bool isCut)
    {
      if (!value.EndsWith(suffix)) {
        isCut = false;
        return value;
      }
      isCut = true;
      return value.Substring(0, value.Length - suffix.Length);       
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
      if (!value.StartsWith(prefix))
        return value;
      return value.Substring(prefix.Length);        
    }

    /// <summary>
    /// Cuts the specified <paramref name="prefix"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The original string value.</param>
    /// <param name="prefix">The prefix to cut.</param>
    /// <param name="isCut">Upon return contains <see langword="true"/>
    /// if prefix was cut, otherwise <see langword="false"/></param>
    /// <returns>String without <paramref name="prefix"/> if it was found; 
    /// otherwise, original <paramref name="value"/>.</returns>
    public static string TryCutPrefix(this string value, string prefix, out bool isCut)
    {
      if (!value.StartsWith(prefix)) {
        isCut = false;
        return value;
      }
      isCut = true;
      return value.Substring(prefix.Length); 
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
  }
}