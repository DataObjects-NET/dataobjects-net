// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.24

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Collections;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class ExpressionMap
  {
    private readonly Dictionary<Expression, HashSet<Expression>> childrenMap
      = new Dictionary<Expression, HashSet<Expression>>();

    public IEnumerable<Expression> GetChildren(Expression parent)
    {
      HashSet<Expression> children;
      return childrenMap.TryGetValue(parent, out children)
        ? children
        : EnumerableUtils<Expression>.Empty;
    }

    public void RegisterChild(Expression parent, Expression child)
    {
      HashSet<Expression> children;
      if (!childrenMap.TryGetValue(parent, out children)) {
        children = new HashSet<Expression>();
        childrenMap.Add(parent, children);
      }
      children.Add(child);
    }
  }
}