// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.05.21

using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Tuple = Xtensive.Tuples.Tuple;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Rse.Transformation
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
        && mc.Object.Type == WellKnownOrmTypes.Tuple) {
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
        && exp.Type == WellKnownOrmTypes.ApplyParameter;
    }
  }
}