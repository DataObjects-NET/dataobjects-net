// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.30

using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A service, that provides SQL query execution functionality.
  /// </summary>
  public interface IQueryExecutor
  {
    /// <summary>
    /// Executes the specified request.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <returns><see cref="IEnumerator{Tuple}"/> that contains result of execution.</returns>
    IEnumerator<Tuple> ExecuteTupleReader(QueryRequest request);

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
    
    /// <summary>
    /// Stores the specified tuples in specified temporary table.
    /// </summary>
    /// <param name="descriptor">The descriptor of temporary table.</param>
    /// <param name="tuples">The tuples to store.</param>
    void Store(TemporaryTableDescriptor descriptor, IEnumerable<Tuple> tuples);

    /// <summary>
    /// Clears the specified temporary table.
    /// </summary>
    /// <param name="descriptor">The descriptor of temporary table.</param>
    void Clear(TemporaryTableDescriptor descriptor);
  }
}