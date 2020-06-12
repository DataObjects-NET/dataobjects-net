// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm
{
  /// <summary>
  /// Future returning a scalar result.
  /// </summary>
  /// <typeparam name="T">The type of the result.</typeparam>
  [Serializable]
  public sealed class Delayed<T> : DelayedQueryResult
  {
    private ParameterContext parameterContext;
    private Materializer materializer;
    private Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings;

    /// <summary>
    /// Gets the result.
    /// </summary>
    public T Value => Materialize(Session);

    /// <summary>
    /// Asynchronously gets value.
    /// </summary>
    /// <returns>Task running this operation</returns>
    [Obsolete("Use AsAsync() method instead.")]
    public Task<T> AsAsyncTask()
    {
      return AsAsync(CancellationToken.None);
    }

    /// <summary>
    /// Asynchronously gets value.
    /// </summary>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task running this operation.</returns>
    [Obsolete("Use AsAsync(CancellationToken) method instead.")]
    public Task<T> AsAsyncTask(CancellationToken token)
    {
      return AsAsync(token);
    }

    /// <summary>
    /// Asynchronously gets value.
    /// </summary>
    /// <returns>Task running this operation</returns>
    public Task<T> AsAsync()
    {
      return AsAsync(CancellationToken.None);
    }

    /// <summary>
    /// Asynchronously gets value.
    /// </summary>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task running this operation.</returns>
    public async Task<T> AsAsync(CancellationToken token)
    {
      if (!LifetimeToken.IsActive)
        throw new InvalidOperationException(Strings.ExThisInstanceIsExpiredDueToTransactionBoundaries);
      if (Task.Result==null) {
        token.ThrowIfCancellationRequested();
        await Session.ExecuteDelayedUserQueriesAsync(false, token).ConfigureAwait(false);
      }
      return Materialize(Session);
    }


    private T Materialize(Session session)
    {
      if (!LifetimeToken.IsActive)
        throw new InvalidOperationException(Strings.ExThisInstanceIsExpiredDueToTransactionBoundaries);
      if (Task.Result==null)
        session.ExecuteUserDefinedDelayedQueries(false);
      var tupleReader = TupleReader.Create(Task.Result);
      var result = materializer.Invoke<T>(tupleReader, session, tupleParameterBindings, parameterContext);
      // TODO: Call correct Result selection method
      return result.First();
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="translatedQuery">The translated query.</param>
    /// <param name="parameterContext">The parameter context.</param>
    internal Delayed(Session session, TranslatedQuery translatedQuery, ParameterContext parameterContext) :
      base(session, translatedQuery, parameterContext)
    {
      materializer = translatedQuery.Materializer;
      tupleParameterBindings = translatedQuery.TupleParameterBindings;
      this.parameterContext = parameterContext;
    }
  }
}