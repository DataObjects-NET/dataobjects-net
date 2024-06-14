// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Expressions;

namespace Xtensive.Orm
{
  /// <summary>
  /// Future returning a scalar result.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  [Serializable]
  public sealed class DelayedScalarQuery<TResult> : DelayedQuery
  {
    private readonly ResultAccessMethod resultAccessMethod;

    /// <summary>
    /// Gets the result.
    /// </summary>
    public TResult Value => Materialize<TResult>().ToScalar(resultAccessMethod);

    /// <summary>
    /// Asynchronously executes delayed scalar query.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Value representing scalar query execution result.</returns>
    public async ValueTask<TResult> ExecuteAsync(CancellationToken token = default) =>
      (await MaterializeAsync<TResult>(token).ConfigureAwaitFalse()).ToScalar(resultAccessMethod);

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="translatedQuery">The translated query.</param>
    /// <param name="parameterContext">The parameter context.</param>
    internal DelayedScalarQuery(Session session, TranslatedQuery translatedQuery, ParameterContext parameterContext)
      : base(session, translatedQuery, parameterContext)
    {
      resultAccessMethod = translatedQuery.ResultAccessMethod;
    }
  }
}