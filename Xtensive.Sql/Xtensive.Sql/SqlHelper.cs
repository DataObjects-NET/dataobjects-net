// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Info;
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
      var tokens = names.Where(name => !string.IsNullOrEmpty(name)).ToArray();
      var builder = new StringBuilder();
      for (int i = 0; i < tokens.Length-1; i++) {
        builder.Append(openingBracket);
        builder.Append(tokens[i].Replace(closingBracket, escapedClosingBracket));
        builder.Append(closingBracket);
        builder.Append(delimiter);
      }
      builder.Append(openingBracket);
      builder.Append(tokens[tokens.Length-1].Replace(closingBracket, escapedClosingBracket));
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
    /// that represents number of milliseconds in that interval.
    /// This is a generic implementation via <see cref="SqlExtract"/>s.
    /// It's suitable for any server, but can be inefficient.
    /// </summary>
    /// <param name="interval">The interval to convert.</param>
    /// <returns>Result of conversion.</returns>
    public static SqlExpression IntervalToNanoseconds(SqlExpression interval)
    {
      var nanoseconds = SqlDml.Extract(SqlIntervalPart.Nanosecond, interval);

      return IntervalToMilliseconds(interval) * 1000000L + nanoseconds;
    }

    /// <summary>
    /// Converts the specified interval expression to expression
    /// that represents absolute value (duration) of the specified interval.
    /// This is a generic implementation that uses comparison with zero interval.
    /// It's suitable for any server, but can be inefficient.
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

    /// <summary>
    /// Performs banker's rounding on the specified argument.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>Result of rounding.</returns>
    public static SqlExpression BankersRound(SqlExpression value)
    {
      var mainPart = 2 * SqlDml.Floor((value + 0.5) / 2);
      var extraPart = SqlDml.Case();
      extraPart.Add(value - mainPart > 0.5, 1);
      extraPart.Else = 0;
      return mainPart + extraPart;
    }

    /// <summary>
    /// Performs banker's rounding on the speicified argument
    /// to a specified number of fractional digits.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="digits">The digits.</param>
    /// <returns>Result of rounding.</returns>
    public static SqlExpression BankersRound(SqlExpression value, SqlExpression digits)
    {
      var scale = SqlDml.Power(10, digits);
      return BankersRound(value * scale) / scale;
    }

    /// <summary>
    /// Performs "rounding as tought in school" on the specified argument.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>Result of rounding.</returns>
    public static SqlExpression RegularRound(SqlExpression value)
    {
      var result = SqlDml.Case();
      result.Add(value > 0, SqlDml.Truncate(value + 0.5));
      result.Else = SqlDml.Truncate(value - 0.5);
      return result;
    }

    /// <summary>
    /// Performs "rounding as tought in school" on the specified argument
    /// to a specified number of fractional digits.
    /// </summary>
    /// <param name="argument">The value to round.</param>
    /// <param name="digits">The digits.</param>
    /// <returns>Result of rounding.</returns>
    public static SqlExpression RegularRound(SqlExpression argument, SqlExpression digits)
    {
      var scale = SqlDml.Power(10, digits);
      return RegularRound(argument * scale) / scale;
    }

    public static string GuidToString(Guid guid)
    {
      return guid.ToString("N");
    }

    public static Guid GuidFromString(string value)
    {
      return new Guid(value);
    }

    /// <summary>
    /// Converts the specified <see cref="TimeSpan"/> to string using the specified format string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format string.</param>
    /// <returns>Formatted representation of the <paramref name="value"/>.</returns>
    /// <remarks>
    /// Format string can contain any of these placeholders:
    /// <list type="table">
    /// <item><term>{0}</term><description>negative sign, if argument represents a negative <see cref="TimeSpan"/>; <see cref="string.Empty"/>, otherwise.</description></item>
    /// <item><term>{1}</term><description>absolute value of <see cref="TimeSpan.Days"/> property.</description></item>
    /// <item><term>{2}</term><description>absolute value of <see cref="TimeSpan.Hours"/> property.</description></item>
    /// <item><term>{3}</term><description>absolute value of <see cref="TimeSpan.Minutes"/> property.</description></item>
    /// <item><term>{4}</term><description>absolute value of <see cref="TimeSpan.Seconds"/> property.</description></item>
    /// <item><term>{5}</term><description>absolute value of <see cref="TimeSpan.Milliseconds"/> property.</description></item>
    /// </list>
    /// </remarks>
    public static string TimeSpanToString(TimeSpan value, string format)
    {
      int days = value.Days;
      int hours = value.Hours;
      int minutes = value.Minutes;
      int seconds = value.Seconds;
      int milliseconds = value.Milliseconds;

      bool negative = false;

      if (days < 0) {
        days = -days;
        negative = true;
      }

      if (hours < 0) {
        hours = -hours;
        negative = true;
      }

      if (minutes < 0) {
        minutes = -minutes;
        negative = true;
      }

      if (seconds < 0) {
        seconds = -seconds;
        negative = true;
      }

      if (milliseconds < 0) {
        milliseconds = -milliseconds;
        negative = true;
      }

      return String.Format(format, negative ? "-" : string.Empty,
          days, hours, minutes, seconds, milliseconds);
    }

    public static SqlExpression GenericPad(SqlFunctionCall node)
    {
      string paddingFunction;
      switch (node.FunctionType) {
      case SqlFunctionType.PadLeft:
        paddingFunction = "lpad";
        break;
      case SqlFunctionType.PadRight:
        paddingFunction = "rpad";
        break;
      default:
        throw new InvalidOperationException();
      }
      var operand = node.Arguments[0];
      var result = SqlDml.Case();
      result.Add(
        SqlDml.CharLength(operand) < node.Arguments[1],
        SqlDml.FunctionCall(paddingFunction, node.Arguments));
      result.Else = operand;
      return result;
    }

    /// <summary>
    /// Reads the database and schema using the specified query.
    /// By contract query should return database in first column and schema in second.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="queryText">The query text.</param>
    /// <param name="coreServerInfo">The core server info.</param>
    public static void ReadDatabaseAndSchema(DbConnection connection, string queryText,
      CoreServerInfo coreServerInfo)
    {
      using (var command = connection.CreateCommand()) {
        command.CommandText = queryText;
        using (var reader = command.ExecuteReader()) {
          if (!reader.Read())
            throw new InvalidOperationException(Strings.ExCanNotReadDatabaseAndSchemaNames);
          coreServerInfo.DatabaseName = reader.GetString(0);
          coreServerInfo.DefaultSchemaName = reader.GetString(1);
        }
      }
    }

    /// <summary>
    /// Reduces the isolation level to the most commonly supported ones.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <returns>Converted isolation level.</returns>
    public static IsolationLevel ReduceIsolationLevel(IsolationLevel level)
    {
      switch (level) {
      case IsolationLevel.ReadUncommitted:
      case IsolationLevel.ReadCommitted:
        return IsolationLevel.ReadCommitted;
      case IsolationLevel.RepeatableRead:
      case IsolationLevel.Serializable:
      case IsolationLevel.Snapshot:
        return IsolationLevel.Serializable;
      default:
        throw new NotSupportedException(string.Format(Strings.ExIsolationLevelXIsNotSupported, level));
      }
    }

    /// <summary>
    /// Quotes the string using standard SQL quoting rules.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Quoted string.</returns>
    public static string QuoteString(string value)
    {
      return "'" + value.Replace("'", "''").Replace("\0", string.Empty) + "'";
    }

    /// <summary>
    /// Creates a <see cref="NotSupportedException"/> with message that says that <paramref name="feature"/>
    /// is not supported by current storage.
    /// </summary>
    /// <param name="feature">The feature.</param>
    /// <returns>Created exception.</returns>
    public static NotSupportedException NotSupported(string feature)
    {
      return new NotSupportedException(string.Format(Strings.ExThisStorageDoesNotSupportX, feature));
    }

    /// <summary>
    /// Creates a <see cref="NotSupportedException"/> with message that says that <paramref name="feature"/>
    /// is not supported by current storage.
    /// </summary>
    /// <param name="feature">The feature.</param>
    /// <returns>Created exception.</returns>
    public static NotSupportedException NotSupported(QueryFeatures feature)
    {
      return NotSupported(feature.ToString());
    }

    /// <summary>
    /// Creates a <see cref="NotSupportedException"/> with message that says that <paramref name="feature"/>
    /// is not supported by current storage.
    /// </summary>
    /// <param name="feature">The feature.</param>
    /// <returns>Created exception.</returns>
    public static NotSupportedException NotSupported(ServerFeatures feature)
    {
      return NotSupported(feature.ToString());
    }
  }
}