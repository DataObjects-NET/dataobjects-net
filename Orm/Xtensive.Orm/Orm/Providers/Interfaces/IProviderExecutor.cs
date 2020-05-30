// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.30

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Provides query features for <see cref="SqlProvider"/>s.
  /// </summary>
  public interface IProviderExecutor
  {
    /// <summary>
    /// Executes the specified request.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    /// <returns><see cref="IEnumerator{Tuple}"/> that contains result of execution.</returns>
    TupleReader ExecuteTupleReader(QueryRequest request, ParameterContext parameterContext);

    /// <summary>
    /// Asynchronously executes the specified request.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing the operation.</returns>
    Task<TupleReader> ExecuteTupleReaderAsync(QueryRequest request,
      ParameterContext parameterContext, CancellationToken token);

    /// <summary>
    /// Stores the specified tuples in specified table.
    /// </summary>
    /// <param name="descriptor">Persist descriptor.</param>
    /// <param name="tuples">The tuples to store.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    void Store(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples, ParameterContext parameterContext);

    /// <summary>
    /// Clears the specified table.
    /// </summary>
    /// <param name="descriptor">Persist descriptor.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    void Clear(IPersistDescriptor descriptor, ParameterContext parameterContext);

    /// <summary>
    /// Executes <see cref="Store"/> and <see cref="Clear"/> via single batch.
    /// </summary>
    /// <param name="descriptor">Persist descriptor</param>
    /// <param name="tuples">Tuples to store</param>
    void Overwrite(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples);
  }
}