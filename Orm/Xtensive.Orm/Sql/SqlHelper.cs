// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
    public readonly struct EscapeSetup
    {
      public static readonly EscapeSetup WithQuotes = 
        new EscapeSetup('.', '\"', '\"', '\"', '\"');
      public static readonly EscapeSetup WithBrackets = 
        new EscapeSetup('.', '[', ']', ']', ']');
      public static readonly EscapeSetup WithBackTick = 
        new EscapeSetup('.', '`', '`', '`', '`');

      public readonly char Delimiter;
      public readonly char Opener; 
      public readonly char Closer;
      public readonly char EscapeCloser1;
      public readonly char EscapeCloser2;

      public EscapeSetup(char delimiter, char opener, char closer, char escapeCloser1, char escapeCloser2)
      {
        Delimiter = delimiter;
        Opener = opener;
        Closer = closer;
        EscapeCloser1 = escapeCloser1;
        EscapeCloser2 = escapeCloser2;
      }
    }

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string QuoteIdentifierWithQuotes(string[] names) 
      => Quote(EscapeSetup.WithQuotes, names);

    /// <summary>
    /// Quotes the specified identifier with square brackets (i.e. []).
    /// </summary>
    /// <returns>Quoted indentifier.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string QuoteIdentifierWithBrackets(string[] names) 
      => Quote(EscapeSetup.WithBrackets, names);

    /// <summary>
    /// Quotes the specified identifier with square brackets (i.e. ``).
    /// </summary>
    /// <returns>Quoted identifier.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string QuoteIdentifierWithBackTick(string[] names) 
      => Quote(EscapeSetup.WithBackTick, names);

    public static unsafe string Quote(in EscapeSetup setup, string[] names)
    {
      // That's one of frequently called methods, so it's optimized for speed. 

      // 1. Find resultLength
      var resultLength = 0;
      foreach (var name in names) {
        if (string.IsNullOrEmpty(name))
          continue;
        if (resultLength != 0)
          resultLength++;
        resultLength += 2 + name.Length;
        var start = 0;
        while (true) {
          start = 1 + name.IndexOf(setup.Closer, start);
          if (start == 0)
            break;
          resultLength++;
        }
      }

      // 2. Create the resulting string at once
      var result = new string(setup.Delimiter, resultLength);
      fixed (char* pResult = result) {
        var p = pResult;
        foreach (var name in names) {
          if (string.IsNullOrEmpty(name))
            continue;
          if (p != pResult)
            p++; // Skip the delimiter (initial result value is a repeating delimiter)
          *p++ = setup.Opener;
          fixed (char* pName = name) {
            var pNameEnd = pName + name.Length;
            for (var pn = pName; pn < pNameEnd; pn++) {
              var c = *pn;
              if (c == setup.Closer) {
                *p++ = setup.EscapeCloser1;
                *p++ = setup.EscapeCloser2;
                continue;
              }
              *p++ = c;
            }
          }
          *p++ = setup.Closer;
        }
      }
      return result;
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
    /// Checks if <paramref name="expressionToCheck"/> is indeed a representation of TimeSpan.Ticks
    /// created by <see cref="Xtensive.Orm.Providers.TimeSpanCompilers.TimeSpanTicks"/>
    /// </summary>
    /// <param name="expressionToCheck">Expression to check</param>
    /// <param name="sourceInterval">Source interval expression</param>
    /// <returns></returns>
    public static bool IsTimeSpanTicks(SqlExpression expressionToCheck, out SqlExpression sourceInterval)
    {
      sourceInterval = null;

      if (expressionToCheck is SqlCast sqlCast
        && (sqlCast.Type.Type == SqlType.Int64 || sqlCast.Type.Type == SqlType.Decimal)) {
        var operand = sqlCast.Operand;
        if (operand is SqlBinary sqlBinary && sqlBinary.NodeType == SqlNodeType.Divide) {
          var left = sqlBinary.Left;
          if (left is SqlFunctionCall functionCall && functionCall.FunctionType == SqlFunctionType.IntervalToNanoseconds) {
            sourceInterval = functionCall.Arguments[0];
            return true;
          }
        }
      }
      return false;
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
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(queryText, nameof(queryText));

      using var command = connection.CreateCommand();
      command.CommandText = queryText;
      command.Transaction = transaction;
      using var reader = command.ExecuteReader();
      if (!reader.Read()) {
        throw new InvalidOperationException(Strings.ExCanNotReadDatabaseAndSchemaNames);
      }

      return new DefaultSchemaInfo(reader.GetString(0), reader.GetString(1));
    }

    /// <summary>
    /// Asynchronously reads the database and schema using the specified query.
    /// By contract query should return database in first column and schema in second.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="queryText">The query text.</param>
    /// <param name="connection">The connection to use.</param>
    /// <param name="transaction">The transaction to use.</param>
    /// <param name="token">The token to cancel async operation if needed.</param>
    /// <returns><see cref="DefaultSchemaInfo"/> instance.</returns>
    public static async Task<DefaultSchemaInfo> ReadDatabaseAndSchemaAsync(string queryText,
      DbConnection connection, DbTransaction transaction, CancellationToken token)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(queryText, nameof(queryText));

      var command = connection.CreateCommand();
      await using (command.ConfigureAwait(false)) {
        command.CommandText = queryText;
        command.Transaction = transaction;
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          if (!await reader.ReadAsync(token).ConfigureAwait(false)) {
            throw new InvalidOperationException(Strings.ExCanNotReadDatabaseAndSchemaNames);
          }

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
      if (string.IsNullOrEmpty(configuration.ConnectionInitializationSql)) {
        return;
      }

      using var command = connection.CreateCommand();
      command.CommandText = configuration.ConnectionInitializationSql;
      command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes <paramref name="initializationSql"/> (if any).
    /// </summary>
    /// <param name="connection">Connection to initialize.</param>
    /// <param name="initializationSql">Sql expression.</param>
    public static void ExecuteInitializationSql(DbConnection connection, string initializationSql)
    {
      if (string.IsNullOrEmpty(initializationSql)) {
        return;
      }
      using var command = connection.CreateCommand();
      command.CommandText = initializationSql;
      _ = command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes <see cref="SqlDriverConfiguration.ConnectionInitializationSql"/> (if any).
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connection">Connection to initialize.</param>
    /// <param name="configuration">Driver configuration.</param>
    /// <param name="token">The token to cancel async operation if needed.</param>
    public static async Task ExecuteInitializationSqlAsync(
      DbConnection connection, SqlDriverConfiguration configuration, CancellationToken token)
    {
      if (string.IsNullOrEmpty(configuration.ConnectionInitializationSql)) {
        return;
      }

      var command = connection.CreateCommand();
      await using (command.ConfigureAwait(false)) {
        command.CommandText = configuration.ConnectionInitializationSql;
        await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Executes <paramref name="initializationSql"/> (if any).
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connection">Connection to initialize.</param>
    /// <param name="initializationSql">Sql expression.</param>
    /// <param name="token">The token to cancel async operation if needed.</param>
    public static async Task ExecuteInitializationSqlAsync(
      DbConnection connection, string initializationSql, CancellationToken token)
    {
      if (string.IsNullOrEmpty(initializationSql)) {
        return;
      }

      var command = connection.CreateCommand();
      await using (command.ConfigureAwait(false)) {
        command.CommandText = initializationSql;
        _ = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
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

    #region Notifications

    /// <summary>
    /// Notifies all the <paramref name="connectionAccessors"/> that
    /// <paramref name="connection"/> is about to be opened.
    /// </summary>
    /// <param name="connectionAccessors">The accessors that should be notified.</param>
    /// <param name="connection">The connection that is opening.</param>
    /// <param name="reconnect"><see langword="true"/> if event happened on attemp to restore connection, otherwise <see langword="false"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotifyConnectionOpening(
      IEnumerable<IDbConnectionAccessor> connectionAccessors, DbConnection connection, bool reconnect = false)
    {
      foreach (var accessor in connectionAccessors) {
        accessor.ConnectionOpening(new ConnectionEventData(connection, reconnect));
      }
    }

    /// <summary>
    /// Notifies all the <paramref name="connectionAccessors"/> that
    /// <paramref name="connection"/> is about to be opened.
    /// </summary>
    /// <param name="connectionAccessors">The accessors that should be notified.</param>
    /// <param name="connection">The connection that is opening.</param>
    /// <param name="reconnect"><see langword="true"/> if event happened on attemp to restore connection, otherwise <see langword="false"/>.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task performing operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task NotifyConnectionOpeningAsync(
      IEnumerable<IDbConnectionAccessor> connectionAccessors, DbConnection connection, bool reconnect = false, CancellationToken token = default)
    {
      foreach (var accessor in connectionAccessors) {
        await accessor.ConnectionOpeningAsync(
          new ConnectionEventData(connection, reconnect), token)
          .ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Notifies all the <paramref name="connectionAccessors"/> that
    /// opened connection is about to be initialized with <paramref name="initializationScript"/>.
    /// </summary>
    /// <param name="connectionAccessors">The accessors that should be notified.</param>
    /// <param name="connection">Opened but not initialized connection</param>
    /// <param name="initializationScript">The script that will run to initialize connection</param>
    /// <param name="reconnect"><see langword="true"/> if event happened on attemp to restore connection, otherwise <see langword="false"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotifyConnectionInitializing(
      IEnumerable<IDbConnectionAccessor> connectionAccessors, DbConnection connection, string initializationScript, bool reconnect = false)
    {
      foreach (var accessor in connectionAccessors) {
        accessor.ConnectionInitialization(new ConnectionInitEventData(initializationScript, connection, reconnect));
      }
    }

    /// <summary>
    /// Notifies all the <paramref name="connectionAccessors"/> that
    /// opened connection is about to be initialized with <paramref name="initializationScript"/>.
    /// </summary>
    /// <param name="connectionAccessors">The accessors that should be notified.</param>
    /// <param name="connection">Opened but not initialized connection</param>
    /// <param name="initializationScript">The script that will run to initialize connection</param>
    /// <param name="reconnect"><see langword="true"/> if event happened on attemp to restore connection, otherwise <see langword="false"/>.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task performing operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task NotifyConnectionInitializingAsync(
      IEnumerable<IDbConnectionAccessor> connectionAccessors, DbConnection connection, string initializationScript,
      bool reconnect = false, CancellationToken token = default)
    {
      foreach (var accessor in connectionAccessors) {
        await accessor.ConnectionInitializationAsync(
          new ConnectionInitEventData(initializationScript, connection, reconnect), token)
          .ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Notifies all the <paramref name="connectionAccessors"/> about
    /// successful connection opening.
    /// </summary>
    /// <param name="connectionAccessors">The accessors that should be notified.</param>
    /// <param name="connection">The connection that is completely opened and initialized.</param>
    /// <param name="reconnect"><see langword="true"/> if event happened on attemp to restore connection, otherwise <see langword="false"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotifyConnectionOpened(
      IEnumerable<IDbConnectionAccessor> connectionAccessors, DbConnection connection, bool reconnect = false)
    {
      foreach (var accessor in connectionAccessors) {
        accessor.ConnectionOpened(new ConnectionEventData(connection, reconnect));
      }
    }

    /// <summary>
    /// Notifies all the <paramref name="connectionAccessors"/> about
    /// successful connection opening.
    /// </summary>
    /// <param name="connectionAccessors">The accessors that should be notified.</param>
    /// <param name="connection">The connection that is completely opened and initialized.</param>
    /// <param name="reconnect"><see langword="true"/> if event happened on attemp to restore connection, otherwise <see langword="false"/>.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task performing operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task NotifyConnectionOpenedAsync(
      IEnumerable<IDbConnectionAccessor> connectionAccessors, DbConnection connection, bool reconnect = false, CancellationToken token = default)
    {
      foreach (var accessor in connectionAccessors) {
        await accessor.ConnectionOpenedAsync(
          new ConnectionEventData(connection, reconnect), token)
          .ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Notifies all the <paramref name="connectionAccessors"/> about
    /// connection opening failure.
    /// </summary>
    /// <param name="connectionAccessors">The accessors that should be notified.</param>
    /// <param name="connection">Connection that failed to be opened or properly initialized.</param>
    /// <param name="exception">The exception which appeared.</param>
    /// <param name="reconnect"><see langword="true"/> if event happened on attemp to restore connection, otherwise <see langword="false"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotifyConnectionOpeningFailed(
      IEnumerable<IDbConnectionAccessor> connectionAccessors, DbConnection connection, Exception exception, bool reconnect = false)
    {
      foreach (var accessor in connectionAccessors) {
        accessor.ConnectionOpeningFailed(new ConnectionErrorEventData(exception, connection, reconnect));
      }
    }

    /// <summary>
    /// Notifies all the <paramref name="connectionAccessors"/> about
    /// connection opening failure.
    /// </summary>
    /// <param name="connectionAccessors">The accessors that should be notified.</param>
    /// <param name="connection">Connection that failed to be opened or properly initialized.</param>
    /// <param name="exception">The exception which appeared.</param>
    /// <param name="reconnect"><see langword="true"/> if event happened on attemp to restore connection, otherwise <see langword="false"/>.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Task performing operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task NotifyConnectionOpeningFailedAsync(
      IEnumerable<IDbConnectionAccessor> connectionAccessors, DbConnection connection, Exception exception,
      bool reconnect = false, CancellationToken token = default)
    {
      foreach (var accessor in connectionAccessors) {
        await accessor.ConnectionOpeningFailedAsync(
          new ConnectionErrorEventData(exception, connection, reconnect), token)
          .ConfigureAwait(false);
      }
    }

    #endregion
  }
}
