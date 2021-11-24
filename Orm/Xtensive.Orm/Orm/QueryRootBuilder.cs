// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.23

using System;
using System.Linq.Expressions;
using Xtensive.Reflection;

namespace Xtensive.Orm
{
  /// <summary>
  /// A simple implementation of <see cref="IQueryRootBuilder"/>
  /// that allows query construction via generic methods.
  /// </summary>
  public abstract class QueryRootBuilder : IQueryRootBuilder
  {
    private static readonly Type InvokerOfTType = typeof (Invoker<>);

    private abstract class Invoker
    {
      public abstract Expression Invoke(QueryRootBuilder builder);
    }

    private sealed class Invoker<TEntity> : Invoker
      where TEntity : class, IEntity
    {
      public override Expression Invoke(QueryRootBuilder builder)
      {
        return builder.BuildRootExpression<TEntity>();
      }
    }

    /// <inheritdoc/>
    public Expression BuildRootExpression(Type entityType)
    {
      return CreateInvoker(entityType).Invoke(this);
    }

    private Invoker CreateInvoker(Type entityType)
    {
      return (Invoker) Activator.CreateInstance(InvokerOfTType.CachedMakeGenericType(entityType));
    }

    /// <summary>
    /// Creates root expression for specified <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <returns>Root expression for <typeparamref name="TEntity"/> query.</returns>
    protected abstract Expression BuildRootExpression<TEntity>()
      where TEntity : class, IEntity;
  }
}