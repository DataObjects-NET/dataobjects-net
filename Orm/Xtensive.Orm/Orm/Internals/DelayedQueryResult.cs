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
    private readonly ParameterContext parameterContext;
    private readonly Func<IEnumerable<Tuple>, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, TResult> materializer;
    private readonly Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings;

    /// <summary>
    /// Gets <see cref="Session"/> this instance is bound to.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets <see cref="StateLifetimeToken"/> this instance is bound to.
    /// </summary>
    public StateLifetimeToken LifetimeToken { get; private set; }

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
      if (!LifetimeToken.IsActive)
        throw new InvalidOperationException(Strings.ExThisInstanceIsExpiredDueToTransactionBoundaries);
      if (Task.Result==null)
        session.ExecuteDelayedQueries(false);
      return materializer.Invoke(Task.Result, session, tupleParameterBindings, parameterContext);
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
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      ArgumentValidator.EnsureArgumentNotNull(translatedQuery, "translatedQuery");
      ArgumentValidator.EnsureArgumentNotNull(parameterContext, "parameterContext");

      Session = session;
      LifetimeToken = session.GetLifetimeToken();

      materializer = translatedQuery.Materializer;
      tupleParameterBindings = translatedQuery.TupleParameterBindings;
      this.parameterContext = parameterContext;
      Task = new QueryTask(translatedQuery.DataSource, LifetimeToken, parameterContext);
    }
  }
}