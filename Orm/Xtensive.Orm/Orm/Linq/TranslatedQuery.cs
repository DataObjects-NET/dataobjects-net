// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// Abstract base class describing LINQ query translation result.
  /// </summary>
  internal class TranslatedQuery
  {
    /// <summary>
    /// The <see cref="ExecutableProvider"/> acting as source for further materialization.
    /// </summary>
    public readonly ExecutableProvider DataSource;

    /// <summary>
    /// Materializer.
    /// </summary>
    public readonly Func<object, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, object> Materializer;

    /// <summary>
    /// Gets the tuple parameter bindings.
    /// </summary>
    public Dictionary<Parameter<Tuple>, Tuple> TupleParameterBindings { get; private set; }

    /// <summary>
    /// Gets the tuple parameters.
    /// </summary>
    public List<Parameter<Tuple>> TupleParameters { get; private set; }

    /// <summary>
    /// Gets the untyped materializer.
    /// </summary>
    public Delegate UntypedMaterializer => Materializer;

    /// <summary>
    /// Executes the query in specified parameter context.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <returns>Query execution result.</returns>
    public TResult Execute<TResult>(Session session, ParameterContext parameterContext)
    {
      var recordSet = DataSource.GetRecordSet(session, parameterContext);
      return (TResult) Materializer(recordSet, session, TupleParameterBindings, parameterContext);
    }

    /// <summary>
    /// Asynchronously executes the query in specified parameter context.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <param name="token">The token to cancel this operation</param>
    /// <returns><see cref="Task{TResult}"/> performing this operation.</returns>
    public async Task<TResult> ExecuteAsync<TResult>(
      Session session, ParameterContext parameterContext, CancellationToken token)
    {
      var recordSet = await DataSource.GetRecordSetAsync(session, parameterContext, token).ConfigureAwait(false);
      return (TResult) Materializer(recordSet, session, TupleParameterBindings, parameterContext);

      // var enumerable = (await recordSet.GetEnumeratorAsync(token).ConfigureAwait(false)).ToEnumerable();
      // enumerable.GetEnumerator().Dispose();
      // return (TResult) Materializer(enumerable, session, TupleParameterBindings, parameterContext, false);
    }


    // Constructors

    /// <summary>
    ///	Initializes a new instance of this class.
    /// </summary>
    /// <param name="dataSource">The data source.</param>
    /// <param name="materializer">The materializer.</param>
    public TranslatedQuery(ExecutableProvider dataSource,
      Func<object, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, object> materializer)
      : this(dataSource, materializer, new Dictionary<Parameter<Tuple>, Tuple>(), Enumerable.Empty<Parameter<Tuple>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="dataSource">The data source.</param>
    /// <param name="materializer">The materializer.</param>
    /// <param name="tupleParameterBindings">The tuple parameter bindings.</param>
    /// <param name="tupleParameters">The tuple parameters.</param>
   public TranslatedQuery(ExecutableProvider dataSource,
      Func<object, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, object> materializer,
        Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      DataSource = dataSource;
      Materializer = materializer;
      TupleParameterBindings = new Dictionary<Parameter<Tuple>, Tuple>(tupleParameterBindings);
      TupleParameters = tupleParameters.ToList();
    }
  }
}