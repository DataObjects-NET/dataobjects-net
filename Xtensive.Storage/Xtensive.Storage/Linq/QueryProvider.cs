// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.26

using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  public abstract class QueryProvider : IQueryProvider
  {
    public DomainModel Model { get; private set; }

    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
      Type elementType = TypeHelper.GetElementType(expression.Type);
      try {
        var query = (IQueryable)typeof (Query<>).Activate(new[] {elementType}, new object[] {this, expression});
        return query;
      }
      catch (TargetInvocationException tie) {
        throw tie.InnerException;
      }
    }

    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
    {
      return new Query<TElement>(this, expression);
    }

    object IQueryProvider.Execute(Expression expression)
    {
      return Execute(expression);
    }

    TResult IQueryProvider.Execute<TResult>(Expression expression)
    {
      object execute = Execute(expression);
      execute = execute ?? default(TResult);
      return (TResult)execute;
    }

    protected abstract object Execute(Expression expression);


    // Constructor

    protected QueryProvider(DomainModel model)
    {
      Model = model;
    }
  }
}