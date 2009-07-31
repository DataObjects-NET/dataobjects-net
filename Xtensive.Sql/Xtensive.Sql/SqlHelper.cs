// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql
{
  /// <summary>
  /// Various helper methods related to this namespace.
  /// </summary>
  public static class SqlHelper
  {
    /// <summary>
    /// Validates the specified URL againts charactes that usually forbidden inside connection strings.
    /// </summary>
    /// <param name="url">The URL.</param>
    public static void ValidateConnectionUrl(UrlInfo url)
    {
      var forbiddenChars = new[] {'=', ';'};
      bool isBadUrl = url.Host.IndexOfAny(forbiddenChars) >= 0
        || url.Resource.IndexOfAny(forbiddenChars) >= 0
        || url.User.IndexOfAny(forbiddenChars) >= 0
        || url.Password.IndexOfAny(forbiddenChars) >= 0;
      if (isBadUrl)
        throw new ArgumentException(
          Strings.ExPartOfUrlContainsForbiddenCharacters + forbiddenChars.ToCommaDelimitedString(), "url");
    }

    /// <summary>
    /// Quotes the specified identifier with quotes (i.e. "").
    /// </summary>
    /// <returns>Quoted identifier.</returns>
    public static string QuoteIdentifierWithQuotes(string[] names)
    {
      return Quote("\"", "\"", ".", "\"\"", names);
    }

    /// <summary>
    /// Quotes the specified identifier with square brackets (i.e. []).
    /// </summary>
    /// <returns>Quoted indentifier.</returns>
    public static string QuoteIdentifierWithBrackets(string[] names)
    {
      return Quote("[", "]", ".", "]]", names);
    }

    private static string Quote(string openingBracket, string closingBracket, string delimiter,
      string escapedClosingBracket, string[] names)
    {
      var builder = new StringBuilder();
      for (int i = 0; i < names.Length-1; i++) {
        builder.Append(openingBracket);
        builder.Append(names[i].Replace(closingBracket, escapedClosingBracket));
        builder.Append(closingBracket);
        builder.Append(delimiter);
      }
      builder.Append(openingBracket);
      builder.Append(names[names.Length-1].Replace(closingBracket, escapedClosingBracket));
      builder.Append(closingBracket);
      return builder.ToString();
    }

    /// <summary>
    /// Converts the specified interval expression to expression
    /// that represents number of milliseconds in that interval.
    /// This is a generic implementation via <see cref="SqlExtract"/>s.
    /// It's suitable for any server, but can be inefficient.
    /// </summary>
    /// <param name="interval">The interval to convert.</param>
    /// <returns>Result of conversion.</returns>
    public static SqlExpression IntervalToMilliseconds(SqlExpression interval)
    {
      var days = SqlDml.Extract(SqlIntervalPart.Day, interval);
      var hours = SqlDml.Extract(SqlIntervalPart.Hour, interval);
      var minutes = SqlDml.Extract(SqlIntervalPart.Minute, interval);
      var seconds = SqlDml.Extract(SqlIntervalPart.Second, interval);
      var milliseconds = SqlDml.Extract(SqlIntervalPart.Millisecond, interval);

      return (((days * 24L + hours) * 60L + minutes) * 60L + seconds) * 1000L + milliseconds;
    }

    /// <summary>
    /// Converts the specified interval expression to expression
    /// that represents absolute value (duration) of the specified interval.
    /// This is a generic implementation that uses comparison with zero interval.
    /// It's suitable for any server, but can be inefficent.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>Result of conversion.</returns>
    public static SqlExpression IntervalAbs(SqlExpression source)
    {
      var result = SqlDml.Case();
      result.Add(source >= SqlDml.Literal(new TimeSpan(0)), source);
      result.Else = SqlDml.IntervalNegate(source);
      return result;
    }
  }
}