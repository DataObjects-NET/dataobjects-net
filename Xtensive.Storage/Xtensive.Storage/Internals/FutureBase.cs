// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections.Generic;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Abstract base for a future query and future scalar implementation.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  [Serializable]
  public abstract class FutureBase<TResult>
  {
    private readonly Func<IEnumerable<Tuple>, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, TResult> materializer;
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
        transaction.Session.ExecuteDelayedQueries();
      return materializer.Invoke(Task.Result, tupleParameterBindings, new ParameterContext());
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
      using (parameterContext.ActivateSafely()) {
        var executableProvider = CompilationContext.Current.Compile(translatedQuery.DataSource.Provider);
        Task = new QueryTask(executableProvider, parameterContext);
      }
    }
  }
}