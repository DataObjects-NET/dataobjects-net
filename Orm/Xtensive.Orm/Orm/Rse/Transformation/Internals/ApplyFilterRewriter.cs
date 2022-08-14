// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.05.15

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Tuple = Xtensive.Tuples.Tuple;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class ApplyFilterRewriter : ExpressionVisitor
  {
    private static readonly ParameterExpression LeftTupleParameter = Expression.Parameter(WellKnownOrmTypes.Tuple, "leftTuple");
    private ColumnCollection sourceColumns;
    private ColumnCollection targetColumns;

    public Expression<Func<Tuple, Tuple, bool>> Rewrite(Expression<Func<Tuple, bool>> predicate,
      ColumnCollection predicateColumns, ColumnCollection currentColumns)
    {
      Initialize(predicate, predicateColumns, currentColumns);
      var visited = Visit(predicate.Body);
      return (Expression<Func<Tuple, Tuple, bool>>) FastExpression
        .Lambda(visited, LeftTupleParameter, predicate.Parameters[0]);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.AsTupleAccess() != null) {
        var sourceIndex = mc.GetTupleAccessArgument();
        if (mc.GetApplyParameter() != null)
          return Expression.Call(LeftTupleParameter, mc.Method, mc.Arguments[0]);
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