// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.07.01

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// An implementation of <see cref="IQueryable{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of the content item of the data source.</typeparam>
  public sealed class Queryable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
  {
    private readonly QueryProvider provider;
    private readonly Expression expression;

    /// <inheritdoc/>
    public Expression Expression => expression;

    /// <inheritdoc/>
    public Type ElementType => typeof (T);

    /// <inheritdoc/>
    IQueryProvider IQueryable.Provider => provider;

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      var result = provider.ExecuteSequence<T>(expression);
      return result.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
      var result = await provider.ExecuteSequenceAsync<T>(expression, cancellationToken).ConfigureAwaitFalse();
      var asyncSource = result.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwaitFalse();
      await foreach (var element in asyncSource) {
        yield return element;
      }
    }

    /// <inheritdoc/>
    public override string ToString() => expression.ToString();

    // Constructors

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="expression"/>  is out of range.</exception>
    public Queryable(QueryProvider provider, Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, nameof(expression));
      if (!typeof (IQueryable<T>).IsAssignableFrom(expression.Type)) {
        throw new ArgumentOutOfRangeException(nameof(expression));
      }

      this.provider = provider;
      this.expression = expression;
    }
  }
}