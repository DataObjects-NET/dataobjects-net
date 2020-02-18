// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.15

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Tuple = Xtensive.Tuples.Tuple;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class ApplyFilterRewriter : ExpressionVisitor
  {
    private ColumnCollection sourceColumns;
    private ColumnCollection targetColumns;
    private ParameterExpression leftTupleParameter;

    public Expression<Func<Tuple, Tuple, bool>> Rewrite(Expression<Func<Tuple, bool>> predicate,
      ColumnCollection predicateColumns, ColumnCollection currentColumns)
    {
      Initialize(predicate, predicateColumns, currentColumns);
      leftTupleParameter = Expression.Parameter(typeof (Tuple), "leftTuple");
      var visited = Visit(predicate.Body);
      return (Expression<Func<Tuple, Tuple, bool>>) FastExpression
        .Lambda(visited, leftTupleParameter, predicate.Parameters[0]);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.AsTupleAccess() != null) {
        var sourceIndex = mc.GetTupleAccessArgument();
        if (mc.GetApplyParameter() != null)
          return Expression.Call(leftTupleParameter, mc.Method, mc.Arguments[0]);
        var name = sourceColumns.Single(column => column.Index == sourceIndex).Name;
        var currentIndex = targetColumns[name].Index;
        return Expression.Call(mc.Object, mc.Method, Expression.Constant(currentIndex));
      }
      return base.VisitMethodCall(mc);
    }

    private void Initialize(Expression<Func<Tuple, bool>> predicate,
      ColumnCollection predicateColumns, ColumnCollection currentColumns)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      ArgumentValidator.EnsureArgumentNotNull(predicateColumns, "predicateColumns");
      ArgumentValidator.EnsureArgumentNotNull(currentColumns, "currentColumns");
      if (predicateColumns.Count == 0)
        throw Exceptions.CollectionIsEmpty("predicateColumns");
      if (currentColumns.Count == 0)
        throw Exceptions.CollectionIsEmpty("currentColumns");
      sourceColumns = predicateColumns;
      targetColumns = currentColumns;
    }
  }
}