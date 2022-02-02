// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    DataReader ExecuteTupleReader(QueryRequest request, ParameterContext parameterContext);

    /// <summary>
    /// Asynchronously executes the specified request.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="request">The request to execute.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing the operation.</returns>
    Task<DataReader> ExecuteTupleReaderAsync(QueryRequest request,
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
    /// Asynchronously stores the specified tuples in specified table.
    /// </summary>
    /// <param name="descriptor">Persist descriptor.</param>
    /// <param name="tuples">The tuples to store.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing the operation.</returns>
    Task StoreAsync(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples, ParameterContext parameterContext, CancellationToken token);

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
