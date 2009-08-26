// Copyright (C) 2009 Xtensive LLC.
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
using Xtensive.Storage.Linq;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Base class for a future implementation.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  [Serializable]
  public abstract class FutureBase<TResult> : SessionBound
  {
    private readonly Func<IEnumerable<Tuple>, Dictionary<Parameter<Tuple>, Tuple>, TResult> materializer;
    private readonly Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings;

    /// <summary>
    /// Gets the task for this future.
    /// </summary>
    [Infrastructure]
    public QueryTask Task { get; private set; }

    /// <summary>
    /// Materializes a result.
    /// </summary>
    /// <returns>The materialized result.</returns>
    protected TResult Materialize()
    {
      if (Task.Result == null)
          Session.ExecuteAllDelayedQueries(false);
      return materializer.Invoke(Task.Result, tupleParameterBindings);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="translatedQuery">The translated query.</param>
    /// <param name="parameterContext">The parameter context.</param>
    protected FutureBase(TranslatedQuery<TResult> translatedQuery, ParameterContext parameterContext)
    {
      materializer = translatedQuery.Materializer;
      tupleParameterBindings = translatedQuery.TupleParameterBindings;
      var executableProvider = CompilationContext.Current.Compile(translatedQuery.DataSource.Provider);
      Task = new QueryTask(executableProvider, parameterContext);
    }
  }
}