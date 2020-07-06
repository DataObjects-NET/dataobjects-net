// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
  /// <typeparam name="T">The type of the result.</typeparam>
  [Serializable]
  public sealed class DelayedScalarQuery<T> : DelayedQuery
  {
    private readonly ResultAccessMethod resultAccessMethod;

    /// <summary>
    /// Gets the result.
    /// </summary>
    public T Value => Materialize<T>().ToScalar(resultAccessMethod);

    /// <summary>
    /// Asynchronously gets value.
    /// </summary>
    /// <returns>Task running this operation</returns>
    [Obsolete("AsAsync Method is obsolete. Use ExecuteAsync method instead")]
    public Task<T> AsAsync() => AsAsync(CancellationToken.None);

    /// <summary>
    /// Asynchronously gets value.
    /// </summary>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task running this operation.</returns>
    [Obsolete("AsAsync Method is obsolete. Use ExecuteAsync method instead")]
    public async Task<T> AsAsync(CancellationToken token) => await ExecuteAsync(token).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously executes delayed scalar query.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Value representing scalar query execution result.</returns>
    public async ValueTask<T> ExecuteAsync(CancellationToken token = default) =>
      (await MaterializeAsync<T>(token).ConfigureAwait(false)).ToScalar(resultAccessMethod);

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