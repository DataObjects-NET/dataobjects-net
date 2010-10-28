// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections.Generic;
using Xtensive.Aspects;
using Xtensive.Internals.DocTemplates;
using Xtensive.Parameters;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Linq;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Abstract base for a future query and future scalar implementation.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  [Serializable]
  public abstract class FutureBase<TResult>
  {
    private readonly Func<IEnumerable<Tuple>, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, TResult> materializer;
    protected readonly Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings;

    private readonly Transaction transaction;

    /// <summary>
    /// Gets the task for this future.
    /// </summary>
    public QueryTask Task { get; private set; }

    /// <summary>
    /// Materializes a result.
    /// </summary>
    /// <returns>The materialized result.</returns>
    protected TResult Materialize()
    {
      if (transaction != Transaction.Current)
        throw new InvalidOperationException(
          Strings.ExCurrentTransactionIsDifferentFromTransactionBoundToThisInstance);
      if (Task.Result==null)
        transaction.Session.ExecuteDelayedQueries(false);
      return materializer.Invoke(Task.Result, transaction.Session, tupleParameterBindings, new ParameterContext());
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="translatedQuery">The translated query.</param>
    /// <param name="parameterContext">The parameter context.</param>
    protected FutureBase(TranslatedQuery<TResult> translatedQuery, ParameterContext parameterContext)
    {
      transaction = Transaction.Current;
      if (transaction == null)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      materializer = translatedQuery.Materializer;
      tupleParameterBindings = translatedQuery.TupleParameterBindings;
      using (parameterContext.ActivateSafely())
        Task = new QueryTask(translatedQuery.DataSource, parameterContext);
    }
  }
}