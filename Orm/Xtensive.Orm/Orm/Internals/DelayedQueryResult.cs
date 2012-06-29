// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Abstract base for a future query and future scalar implementation.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  [Serializable]
  public abstract class DelayedQueryResult<TResult>
  {
    private readonly Func<IEnumerable<Tuple>, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, TResult> materializer;
    private readonly Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings;
    protected readonly Transaction transaction;

    /// <summary>
    /// Gets the task for this future.
    /// </summary>
    public QueryTask Task { get; private set; }

    /// <summary>
    /// Materializes a result.
    /// </summary>
    /// <param name="session"></param>
    /// <returns>The materialized result.</returns>
    protected TResult Materialize(Session session)
    {
      if (transaction != session.Transaction)
        throw new InvalidOperationException(
          Strings.ExCurrentTransactionIsDifferentFromTransactionBoundToThisInstance);
      if (Task.Result==null)
        session.ExecuteDelayedQueries(false);
      return materializer.Invoke(Task.Result, session, tupleParameterBindings, new ParameterContext());
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="translatedQuery">The translated query.</param>
    /// <param name="parameterContext">The parameter context.</param>
    internal DelayedQueryResult(Session session, TranslatedQuery<TResult> translatedQuery, ParameterContext parameterContext)
    {
      transaction = session.Transaction;
      if (transaction == null)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      materializer = translatedQuery.Materializer;
      tupleParameterBindings = translatedQuery.TupleParameterBindings;
      using (parameterContext.ActivateSafely())
        Task = new QueryTask(translatedQuery.DataSource, parameterContext);
    }
  }
}