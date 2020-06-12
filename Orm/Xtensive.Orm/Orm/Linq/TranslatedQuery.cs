// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

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
    public readonly Materializer Materializer;

    /// <summary>
    /// Gets the tuple parameter bindings.
    /// </summary>
    public Dictionary<Parameter<Tuple>, Tuple> TupleParameterBindings { get; private set; }

    /// <summary>
    /// Gets the tuple parameters.
    /// </summary>
    public List<Parameter<Tuple>> TupleParameters { get; private set; }

    /// <summary>
    /// Executes the query in specified parameter context.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <returns>Query execution result.</returns>
    public TResult ExecuteScalar<TResult>(Session session, ParameterContext parameterContext)
    {
      var sequenceResult = ExecuteSequence<TResult>(session, parameterContext);
      // TODO: Call proper method specified by projection.ResultType
      // if (projection.IsScalar) {
      //   var materializerResultType = typeof(IEnumerable<>).MakeGenericType(elementType);
      //   var scalarMethodName = projection.ResultType.ToString();
      //   var enumerableMethod = typeof (Enumerable)
      //     .GetMethods(BindingFlags.Static | BindingFlags.Public)
      //     .First(m => m.Name==scalarMethodName && m.GetParameters().Length==1)
      //     .MakeGenericMethod(elementType);
      //   body = Expression.Call(enumerableMethod, Expression.Convert(body, materializerResultType));
      // }

      return sequenceResult.First();
    }

    /// <summary>
    /// Executes the query in specified parameter context.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <returns>Query execution result.</returns>
    public SequenceQueryResult<T> ExecuteSequence<T>(Session session, ParameterContext parameterContext)
    {
      var recordSet = DataSource.GetRecordSet(session, parameterContext);
      return Materializer.Invoke<T>(recordSet, session, TupleParameterBindings, parameterContext);
    }

    /// <summary>
    /// Asynchronously executes the query in specified parameter context.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <param name="token">The token to cancel this operation</param>
    /// <returns><see cref="Task{TResult}"/> performing this operation.</returns>
    public async Task<TResult> ExecuteScalarAsync<TResult>(
      Session session, ParameterContext parameterContext, CancellationToken token)
    {
      var sequenceResult = await ExecuteSequenceAsync<TResult>(session, parameterContext, token);
      return sequenceResult.First();
    }

    /// <summary>
    /// Asynchronously executes the query in specified parameter context.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <param name="token">The token to cancel this operation</param>
    /// <returns><see cref="Task{TResult}"/> performing this operation.</returns>
    public async Task<SequenceQueryResult<T>> ExecuteSequenceAsync<T>(
      Session session, ParameterContext parameterContext, CancellationToken token)
    {
      var recordSet = await DataSource.GetRecordSetAsync(session, parameterContext, token).ConfigureAwait(false);
      return Materializer.Invoke<T>(recordSet, session, TupleParameterBindings, parameterContext);
    }


    // Constructors

    /// <summary>
    ///	Initializes a new instance of this class.
    /// </summary>
    /// <param name="dataSource">The data source.</param>
    /// <param name="materializer">The materializer.</param>
    public TranslatedQuery(ExecutableProvider dataSource, Materializer materializer)
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
      Materializer materializer,
        Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      DataSource = dataSource;
      Materializer = materializer;
      TupleParameterBindings = new Dictionary<Parameter<Tuple>, Tuple>(tupleParameterBindings);
      TupleParameters = tupleParameters.ToList();
    }
  }
}