// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.30

using System.Collections.Generic;
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