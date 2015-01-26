// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.30

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    /// <returns><see cref="IEnumerator{Tuple}"/> that contains result of execution.</returns>
    IEnumerator<Tuple> ExecuteTupleReader(QueryRequest request);

#if NET45
    Task<IEnumerator<Tuple>> ExecuteTupleReaderAsync(QueryRequest request, CancellationToken token); 
#endif

    /// <summary>
    /// Stores the specified tuples in specified table.
    /// </summary>
    /// <param name="descriptor">Persist descriptor.</param>
    /// <param name="tuples">The tuples to store.</param>
    void Store(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples);

    /// <summary>
    /// Clears the specified table.
    /// </summary>
    /// <param name="descriptor">Persist descriptor.</param>
    void Clear(IPersistDescriptor descriptor);

    /// <summary>
    /// Executes <see cref="Store"/> and <see cref="Clear"/> via single batch.
    /// </summary>
    /// <param name="descriptor">Persist descriptor</param>
    /// <param name="tuples">Tuples to store</param>
    void Overwrite(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples);
  }
}