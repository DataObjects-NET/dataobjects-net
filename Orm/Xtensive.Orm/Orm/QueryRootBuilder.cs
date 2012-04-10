// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.23

using System;
using System.Linq.Expressions;

namespace Xtensive.Orm
{
  /// <summary>
  /// A simple implementation of <see cref="IQueryRootBuilder"/>
  /// that allows query construction via generic methods.
  /// </summary>
  public abstract class QueryRootBuilder : IQueryRootBuilder
  {
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

    
    public Expression BuildRootExpression(Type entityType)
    {
      return CreateInvoker(entityType).Invoke(this);
    }

    private Invoker CreateInvoker(Type entityType)
    {
      return (Invoker) Activator.CreateInstance(typeof (Invoker<>).MakeGenericType(entityType));
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