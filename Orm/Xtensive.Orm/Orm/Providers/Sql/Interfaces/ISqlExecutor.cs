// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.29

using System.Data.Common;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers.Sql
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
    DbDataReader ExecuteReader(ISqlCompileUnit statement);

    /// <summary>
    /// Executes the specified query statement. This method is similar to <see cref="DbCommand.ExecuteReader()"/>.
    /// </summary>
    /// <param name="commandText">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    DbDataReader ExecuteReader(string commandText);

    /// <summary>
    /// Executes the specified scalar statement. This method is similar to <see cref="DbCommand.ExecuteScalar"/>.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    object ExecuteScalar(ISqlCompileUnit statement);

    /// <summary>
    /// Executes the specified scalar statement. This method is similar to <see cref="DbCommand.ExecuteScalar"/>.
    /// </summary>
    /// <param name="commandText">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    object ExecuteScalar(string commandText);

    /// <summary>
    /// Executes the specified non query statement. This method is similar to <see cref="DbCommand.ExecuteNonQuery"/>.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    int ExecuteNonQuery(ISqlCompileUnit statement);

    /// <summary>
    /// Executes the specified non query statement. This method is similar to <see cref="DbCommand.ExecuteNonQuery"/>.
    /// </summary>
    /// <param name="commandText">The statement to execute.</param>
    /// <returns>Result of execution.</returns>
    int ExecuteNonQuery(string commandText);
  }
}