// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Delayed query task. 
  /// Used internally to support delayed (future) queries.
  /// </summary>
  [Serializable]
  public sealed class QueryTask : IEnumerable<Tuple>
  {
    /// <summary>
    /// The data surce (data provider).
    /// </summary>
    public readonly ExecutableProvider DataSource;

    /// <summary>
    /// The parameter value context.
    /// </summary>
    public readonly ParameterContext ParameterContext;

    /// <summary>
    /// Gets <see cref="StateLifetimeToken"/> this instance is bound to.
    /// </summary>
    public readonly StateLifetimeToken LifetimeToken;

    /// <summary>
    /// Gets or sets the result of execution of this query task.
    /// </summary>
    public List<Tuple> Result { get; set; }

    /// <exception cref="InvalidOperationException">Query task is not executed yet.</exception>
    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator()
    {
      if (Result == null)
        throw new InvalidOperationException(Strings.ExQueryTaskIsNotExecutedYet);
      return Result.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="dataSource">The data source.</param>
    /// <param name="parameterContext">The parameter value context.</param>
    public QueryTask(ExecutableProvider dataSource, StateLifetimeToken lifetimeToken, ParameterContext parameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(dataSource, "dataSource");
      DataSource = dataSource;
      LifetimeToken = lifetimeToken;
      ParameterContext = parameterContext;
    }
  }
}