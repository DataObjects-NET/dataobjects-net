// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Info;

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

    /// <summary>
    /// Quotes the specified identifier with square brackets (i.e. ``).
    /// </summary>
    /// <returns>Quoted indentifier.</returns>
    public static string QuoteIdentifierWithBackTick(string[] names)
    {
      return Quote("`", "`", ".", "``", names);
    }

    private static string Quote(string openingBracket, string closingBracket, string delimiter,
      string escapedClosingBracket, IEnumerable<string> names)
    {
      var tokens = names.Where(name => !string.IsNullOrEmpty(name)).ToArray();
      var builder = new StringBuilder();
      for (int i = 0; i < tokens.Length - 1; i++) {
        builder.Append(openingBracket);
        builder.Append(tokens[i].Replace(closingBracket, escapedClosingBracket));
        builder.Append(closingBracket);
        builder.Append(delimiter);
      }
      builder.Append(openingBracket);
      builder.Append(tokens[tokens.Length - 1].Replace(closingBracket, escapedClosingBracket));
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
    /// <param name="queryText">The query text.</param>
    /// <param name="connection">The connection to use.</param>
    /// <param name="transaction">The transaction to use.</param>
    /// <returns><see cref="DefaultSchemaInfo"/> instance.</returns>
    public static DefaultSchemaInfo ReadDatabaseAndSchema(string queryText,
      DbConnection connection, DbTransaction transaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(queryText, "queryText");

      using (var command = connection.CreateCommand()) {
        command.CommandText = queryText;
        command.Transaction = transaction;
        using (var reader = command.ExecuteReader()) {
          if (!reader.Read())
            throw new InvalidOperationException(Strings.ExCanNotReadDatabaseAndSchemaNames);
          return new DefaultSchemaInfo(reader.GetString(0), reader.GetString(1));
        }
      }
    }

    /// <summary>
    /// Executes <see cref="SqlDriverConfiguration.ConnectionInitializationSql"/> (if any).
    /// </summary>
    /// <param name="connection">Connection to initialize.</param>
    /// <param name="configuration">Driver configuration.</param>
    public static void ExecuteInitializationSql(DbConnection connection, SqlDriverConfiguration configuration)
    {
      if (string.IsNullOrEmpty(configuration.ConnectionInitializationSql))
        return;
      using (var command = connection.CreateCommand()) {
        command.CommandText = configuration.ConnectionInitializationSql;
        command.ExecuteNonQuery();
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

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static bool ShouldRetryOn([NotNull] Exception ex)
    {
      SqlException sqlException = ex as SqlException;
      if (sqlException!=null) {
        foreach (SqlError err in sqlException.Errors) {
          switch (err.Number) {
            // SQL Error Code: 49920
            // Cannot process request. Too many operations in progress for subscription "%ld".
            // The service is busy processing multiple requests for this subscription.
            // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for operation status.
            // Wait until pending requests are complete or delete one of your pending requests and retry your request later.
            case 49920:
            // SQL Error Code: 49919
            // Cannot process create or update request. Too many create or update operations in progress for subscription "%ld".
            // The service is busy processing multiple create or update requests for your subscription or server.
            // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for pending operations.
            // Wait till pending create or update requests are complete or delete one of your pending requests and
            // retry your request later.
            case 49919:
            // SQL Error Code: 49918
            // Cannot process request. Not enough resources to process request.
            // The service is currently busy.Please retry the request later.
            case 49918:
            // SQL Error Code: 41839
            // Transaction exceeded the maximum number of commit dependencies.
            case 41839:
            // SQL Error Code: 41325
            // The current transaction failed to commit due to a serializable validation failure.
            case 41325:
            // SQL Error Code: 41305
            // The current transaction failed to commit due to a repeatable read validation failure.
            case 41305:
            // SQL Error Code: 41302
            // The current transaction attempted to update a record that has been updated since the transaction started.
            case 41302:
            // SQL Error Code: 41301
            // Dependency failure: a dependency was taken on another transaction that later failed to commit.
            case 41301:
            // SQL Error Code: 40613
            // Database XXXX on server YYYY is not currently available. Please retry the connection later.
            // If the problem persists, contact customer support, and provide them the session tracing ID of ZZZZZ.
            case 40613:
            // SQL Error Code: 40501
            // The service is currently busy. Retry the request after 10 seconds. Code: (reason code to be decoded).
            case 40501:
            // SQL Error Code: 40197
            // The service has encountered an error processing your request. Please try again.
            case 40197:
            // SQL Error Code: 10929
            // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d.
            // However, the server is currently too busy to support requests greater than %d for this database.
            // For more information, see http://go.microsoft.com/fwlink/?LinkId=267637. Otherwise, please try again.
            case 10929:
            // SQL Error Code: 10928
            // Resource ID: %d. The %s limit for the database is %d and has been reached. For more information,
            // see http://go.microsoft.com/fwlink/?LinkId=267637.
            case 10928:
            // SQL Error Code: 10060
            // A network-related or instance-specific error occurred while establishing a connection to SQL Server.
            // The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server
            // is configured to allow remote connections. (provider: TCP Provider, error: 0 - A connection attempt failed
            // because the connected party did not properly respond after a period of time, or established connection failed
            // because connected host has failed to respond.)"}
            case 10060:
            // SQL Error Code: 10054
            // A transport-level error has occurred when sending the request to the server.
            // (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
            case 10054:
            // SQL Error Code: 10053
            // A transport-level error has occurred when receiving results from the server.
            // An established connection was aborted by the software in your host machine.
            case 10053:
            // SQL Error Code: 1205
            // Deadlock
            case 1205:
            // SQL Error Code: 233
            // The client was unable to establish a connection because of an error during connection initialization process before login.
            // Possible causes include the following: the client tried to connect to an unsupported version of SQL Server;
            // the server was too busy to accept new connections; or there was a resource limitation (insufficient memory or maximum
            // allowed connections) on the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by
            // the remote host.)
            case 233:
            // SQL Error Code: 121
            // The semaphore timeout period has expired
            case 121:
            // SQL Error Code: 64
            // A connection was successfully established with the server, but then an error occurred during the login process.
            // (provider: TCP Provider, error: 0 - The specified network name is no longer available.)
            case 64:
            // DBNETLIB Error Code: 20
            // The instance of SQL Server you attempted to connect to does not support encryption.
            case 20:
              return true;
            // This exception can be thrown even if the operation completed succesfully, so it's safer to let the application fail.
            // DBNETLIB Error Code: -2
            // Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding. The statement has been terminated.
            //case -2:
          }
        }

        return false;
      }

      return ex is TimeoutException;
    }
  }
}