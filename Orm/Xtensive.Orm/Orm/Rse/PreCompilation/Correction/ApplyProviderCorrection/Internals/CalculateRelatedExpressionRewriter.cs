// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.21

using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Rse.Helpers;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Rse.PreCompilation.Correction.ApplyProviderCorrection
{
  internal sealed class CalculateRelatedExpressionRewriter : ExpressionVisitor
  {
    private ParameterExpression substitute;
    private ColumnCollection sourceColumns;
    private ColumnCollection targetColumns;

    public LambdaExpression Rewrite(LambdaExpression expression,
      ParameterExpression substituteParameter, ColumnCollection sourceColumns,
      ColumnCollection targetColumns)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentNotNull(substituteParameter, "substituteParameter");
      ArgumentValidator.EnsureArgumentNotNull(sourceColumns, "sourceColumns");
      ArgumentValidator.EnsureArgumentNotNull(targetColumns, "targetColumns");
      substitute = substituteParameter;
      this.sourceColumns = sourceColumns;
      this.targetColumns = targetColumns;
      return (LambdaExpression) Visit(expression);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (IsApplyParameter(m.Expression))
        return substitute;
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      var visited = (MethodCallExpression)base.VisitMethodCall(mc);
      if (mc.Object.NodeType == ExpressionType.Parameter
        && mc.Object.Type == typeof(Tuple)) {
        var sourceIndex = visited.GetTupleAccessArgument();
        var name = sourceColumns.Single(column => column.Index == sourceIndex).Name;
        var currentIndex = targetColumns[name].Index;
        return Expression.Call(visited.Object, visited.Method, Expression.Constant(currentIndex));
      }
      return visited;
    }

    private static bool IsApplyParameter(Expression exp)
    {
      return exp.NodeType == ExpressionType.Constant
        && exp.Type == typeof(ApplyParameter);
    }
  }
}