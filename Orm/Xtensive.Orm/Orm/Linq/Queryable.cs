// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.07.01

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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
      var result = provider.Execute<IEnumerable<T>>(expression);
      return result.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public async IAsyncEnumerator<T> GetAsyncEnumerator(
      [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
      var result = await provider.ExecuteAsync<IAsyncEnumerable<T>>(expression, cancellationToken);
      await foreach (var element in result.WithCancellation(cancellationToken)) {
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