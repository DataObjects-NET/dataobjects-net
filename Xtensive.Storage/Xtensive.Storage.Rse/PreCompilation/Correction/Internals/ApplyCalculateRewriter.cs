// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.21

using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Expressions;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class ApplyCalculateRewriter : ExpressionVisitor
  {
    private ParameterExpression substitute;
    private int fieldIndexOffset;

    public LambdaExpression Rewrite(LambdaExpression expression,
      int substituteParameterIndex, int fieldIndexOffset)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      substitute = expression.Parameters[substituteParameterIndex];
      this.fieldIndexOffset = fieldIndexOffset;
      return (LambdaExpression) Visit(expression);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if(IsApplyParameter(m.Expression))
        return substitute;
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      var visited = (MethodCallExpression)base.VisitMethodCall(mc);
      if(mc.Object.NodeType == ExpressionType.Parameter
        && mc.Object.Type == typeof(Tuple)) {
        var fieldIndex = visited.GetTupleAccessArgument();
        fieldIndex += fieldIndexOffset;
        return Expression.Call(visited.Object, visited.Method, Expression.Constant(fieldIndex));
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