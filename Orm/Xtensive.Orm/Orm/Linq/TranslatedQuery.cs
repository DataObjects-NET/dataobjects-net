// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Linq.Expressions;
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
    public static IReadOnlyDictionary<Parameter<Tuple>, Tuple> EmptyTupleParameterBindings { get; } = new Dictionary<Parameter<Tuple>, Tuple>();

    internal readonly ResultAccessMethod ResultAccessMethod;

    /// <summary>
    /// The <see cref="ExecutableProvider"/> acting as source for further materialization.
    /// </summary>
    public readonly ExecutableProvider DataSource;

    /// <summary>
    /// Materializer.
    /// </summary>
    public readonly Materializer Materializer;

    public bool IsScalar => ResultAccessMethod != ResultAccessMethod.All;

    /// <summary>
    /// Gets the tuple parameter bindings.
    /// </summary>
    public IReadOnlyDictionary<Parameter<Tuple>, Tuple> TupleParameterBindings { get; }

    /// <summary>
    /// Gets the tuple parameters.
    /// </summary>
    public IEnumerable<Parameter<Tuple>> TupleParameters { get; }

    /// <summary>
    /// Executes the query in specified parameter context.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <returns>Query execution result.</returns>
    public TResult ExecuteScalar<TResult>(Session session, ParameterContext parameterContext)
    {
      var sequenceResult = ExecuteSequence<TResult>(session, parameterContext);
      return sequenceResult.ToScalar(ResultAccessMethod);
    }

    /// <summary>
    /// Executes the query in specified parameter context.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <returns>Query execution result.</returns>
    public QueryResult<T> ExecuteSequence<T>(Session session, ParameterContext parameterContext)
    {
      var newParameterContext = new ParameterContext(parameterContext, TupleParameterBindings);
      var recordSetReader = DataSource.GetRecordSetReader(session, newParameterContext);
      return Materializer.Invoke<T>(recordSetReader, session, newParameterContext);
    }

    /// <summary>
    /// Asynchronously executes the query in specified parameter context.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <param name="token">The token to cancel this operation</param>
    /// <returns><see cref="Task{TResult}"/> performing this operation.</returns>
    public async Task<TResult> ExecuteScalarAsync<TResult>(
      Session session, ParameterContext parameterContext, CancellationToken token)
    {
      var sequenceResult = await ExecuteSequenceAsync<TResult>(session, parameterContext, token).ConfigureAwaitFalse();
      return sequenceResult.ToScalar(ResultAccessMethod);
    }

    /// <summary>
    /// Asynchronously executes the query in specified parameter context.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <param name="token">The token to cancel this operation</param>
    /// <returns><see cref="Task{TResult}"/> performing this operation.</returns>
    public async Task<QueryResult<T>> ExecuteSequenceAsync<T>(
      Session session, ParameterContext parameterContext, CancellationToken token)
    {
      var newParameterContext = new ParameterContext(parameterContext, TupleParameterBindings);
      var recordSetReader =
        await DataSource.GetRecordSetReaderAsync(session, newParameterContext, token).ConfigureAwaitFalse();
      return Materializer.Invoke<T>(recordSetReader, session, newParameterContext);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="dataSource">The data source.</param>
    /// <param name="materializer">The materializer.</param>
    /// <param name="resultAccessMethod">The value describing how it is supposed to access query result.</param>
    public TranslatedQuery(ExecutableProvider dataSource, Materializer materializer, ResultAccessMethod resultAccessMethod)
      : this(dataSource, materializer, resultAccessMethod, EmptyTupleParameterBindings, Array.Empty<Parameter<Tuple>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="dataSource">The data source.</param>
    /// <param name="materializer">The materializer.</param>
    /// <param name="resultAccessMethod">The value describing how it is supposed to access query result.</param>
    /// <param name="tupleParameterBindings">The tuple parameter bindings.</param>
    /// <param name="tupleParameters">The tuple parameters.</param>
    public TranslatedQuery(ExecutableProvider dataSource,
      Materializer materializer,
      ResultAccessMethod resultAccessMethod,
      IReadOnlyDictionary<Parameter<Tuple>, Tuple> tupleParameterBindings, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      DataSource = dataSource;
      Materializer = materializer;
      ResultAccessMethod = resultAccessMethod;
      TupleParameterBindings = tupleParameterBindings;
      TupleParameters = tupleParameters;
    }
  }
}
