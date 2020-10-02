// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.29

using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Provides simple execution API for SQL queries.
  /// </summary>
  public interface ISqlExecutor
  {
    /// <summary>
    /// Executes the specified query statement. This method is similar to <see cref="DbCommand.ExecuteReader()"/>.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    CommandWithDataReader ExecuteReader(ISqlCompileUnit statement);

    /// <summary>
    /// Asynchronously executes the specified query statement.
    /// This method is similar to <see cref="DbCommand.ExecuteReaderAsync()"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="statement">The statement to execute.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>Result of execution.</returns>
    Task<CommandWithDataReader> ExecuteReaderAsync(ISqlCompileUnit statement, CancellationToken token = default);

    /// <summary>
    /// Executes the specified query statement. This method is similar to <see cref="DbCommand.ExecuteReader()"/>.
    /// </summary>
    /// <param name="commandText">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    CommandWithDataReader ExecuteReader(string commandText);

    /// <summary>
    /// Asynchronously executes the specified query statement.
    /// This method is similar to <see cref="DbCommand.ExecuteReaderAsync()"/>.
    /// </summary>
    /// <param name="commandText">The statement to execute.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>Result of execution.</returns>
    Task<CommandWithDataReader> ExecuteReaderAsync(string commandText, CancellationToken token = default);

    /// <summary>
    /// Executes the specified scalar statement. This method is similar to <see cref="DbCommand.ExecuteScalar"/>.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    object ExecuteScalar(ISqlCompileUnit statement);

    /// <summary>
    /// Asynchronously executes the specified scalar statement.
    /// This method is similar to <see cref="DbCommand.ExecuteScalarAsync()"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="statement">The statement to execute.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>Result of execution.</returns>
    Task<object> ExecuteScalarAsync(ISqlCompileUnit statement, CancellationToken token = default);

    /// <summary>
    /// Executes the specified scalar statement. This method is similar to <see cref="DbCommand.ExecuteScalar"/>.
    /// </summary>
    /// <param name="commandText">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    object ExecuteScalar(string commandText);

    /// <summary>
    /// Asynchronously executes the specified scalar statement.
    /// This method is similar to <see cref="DbCommand.ExecuteScalarAsync()"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="commandText">The statement to execute.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>Result of execution.</returns>
    Task<object> ExecuteScalarAsync(string commandText, CancellationToken token = default);

    /// <summary>
    /// Executes the specified non query statement. This method is similar to <see cref="DbCommand.ExecuteNonQuery"/>.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    int ExecuteNonQuery(ISqlCompileUnit statement);

    /// <summary>
    /// Asynchronously executes the specified non query statement.
    /// This method is similar to <see cref="DbCommand.ExecuteNonQueryAsync()"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="statement">The statement to execute.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>Result of execution.</returns>
    Task<int> ExecuteNonQueryAsync(ISqlCompileUnit statement, CancellationToken token = default);

    /// <summary>
    /// Executes the specified non query statement. This method is similar to <see cref="DbCommand.ExecuteNonQuery"/>.
    /// </summary>
    /// <param name="commandText">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    int ExecuteNonQuery(string commandText);

    /// <summary>
    /// Asynchronously executes the specified non query statement.
    /// This method is similar to <see cref="DbCommand.ExecuteNonQueryAsync()"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="commandText">The statement to execute.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>Result of execution.</returns>
    Task<int> ExecuteNonQueryAsync(string commandText, CancellationToken token = default);

    /// <summary>
    /// Executes group of DDL statements via <see cref="ExecuteNonQuery(System.String)"/>.
    /// </summary>
    /// <param name="statements">Statements to execute</param>
    void ExecuteMany(IEnumerable<string> statements);

    /// <summary>
    /// Asynchronously executes group of DDL statements
    /// via <see cref="ExecuteNonQueryAsync(System.String, System.Threading.CancellationToken)"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <param name="statements">Statements to execute</param>
    Task ExecuteManyAsync(IEnumerable<string> statements, CancellationToken token = default);

    /// <summary>
    /// Executes specified extraction tasks.
    /// </summary>
    /// <param name="tasks">Tasks to execute.</param>
    /// <returns>Extraction result.</returns>
    SqlExtractionResult Extract(IEnumerable<SqlExtractionTask> tasks);

    /// <summary>
    /// Asynchronously executes the specified extraction tasks.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="tasks">Tasks to execute.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>Extraction result.</returns>
    Task<SqlExtractionResult> ExtractAsync(IEnumerable<SqlExtractionTask> tasks, CancellationToken token = default);
  }
}