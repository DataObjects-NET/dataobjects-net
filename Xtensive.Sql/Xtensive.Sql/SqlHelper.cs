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
    /// Determines whether the specified expression is a null reference.
    /// Use this method instead of comparison with null,
    /// because equality operator is overloaded for <see cref="SqlExpression"/>
    /// to yield equality comparison expression.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>
    /// <see langword="true"/> if argument is a null reference; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNullReference(this SqlExpression expression)
    {
      return ReferenceEquals(expression, null);
    }

    /// <summary>
    /// Converts the specified <see cref="SqlType"/> to corresponding .NET type.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    /// <returns>Converter type.</returns>
    public static Type ToClrType(this SqlType type)
    {
      switch (type) {
      case SqlType.Boolean:
        return typeof (bool);
      case SqlType.Int8:
        return typeof (sbyte);
      case SqlType.UInt8:
        return typeof (byte);
      case SqlType.Int16:
        return typeof (short);
      case SqlType.UInt16:
        return typeof (ushort);
      case SqlType.Int32:
        return typeof (int);
      case SqlType.UInt32:
        return typeof (uint);
      case SqlType.Int64:
        return typeof (long);
      case SqlType.UInt64:
        return typeof (ulong);
      case SqlType.Decimal:
        return typeof (decimal);
      case SqlType.Float:
        return typeof (float);
      case SqlType.Double:
        return typeof (double);
      case SqlType.DateTime:
        return typeof (DateTime);
      case SqlType.Interval:
        return typeof (TimeSpan);
      case SqlType.Char:
      case SqlType.VarChar:
      case SqlType.VarCharMax:
        return typeof (string);
      case SqlType.Binary:
      case SqlType.VarBinary:
      case SqlType.VarBinaryMax:
        return typeof (byte[]);
      case SqlType.Guid:
        return typeof (Guid);
      default:
        throw new ArgumentOutOfRangeException("type");
      }
    }
  }
}