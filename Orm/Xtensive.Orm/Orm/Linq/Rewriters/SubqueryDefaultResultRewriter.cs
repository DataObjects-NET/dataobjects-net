// Copyright (C) 2013-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal sealed class SubqueryDefaultResultRewriter : ExpressionVisitor
  {
    protected override Expression VisitUnknown(Expression e) => e;

    protected override Expression VisitBinary(BinaryExpression b)
    {
      var left = ApplyCorrection(Visit(b.Left));
      var right = ApplyCorrection(Visit(b.Right));

      if (b.Left == left && b.Right == right) {
        return b;
      }

      return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
    }

    private static Expression ApplyCorrection(Expression originalExpression)
    {
      var expression = originalExpression;
      if (expression.NodeType == ExpressionType.Convert) {
        expression = ((UnaryExpression) expression).Operand;
      }
      if (expression.NodeType is not ExpressionType.Call)
        return originalExpression;

      var methodCall = expression as MethodCallExpression;
      if (!IsDefaultingMethod(methodCall.Method) || !HasNonNullDefault(methodCall.Type)) {
        return originalExpression;
      }

      var methodReturnType = methodCall.Type;
      expression = Expression.Coalesce(
        Expression.Convert(expression, methodReturnType.ToNullable()),
        Expression.Constant(Activator.CreateInstance(methodReturnType)));

      if (expression.Type != originalExpression.Type) {
        expression = Expression.Convert(expression, originalExpression.Type);
      }

      return expression;
    }

    private static bool HasNonNullDefault(Type type) => type.IsValueType && !type.IsNullable();

    private static bool IsDefaultingMethod(MethodInfo method) =>
      method.DeclaringType == WellKnownTypes.Queryable
      && (method.Name == nameof(Queryable.FirstOrDefault)
        || method.Name == nameof(Queryable.SingleOrDefault));

    public static Expression Rewrite(Expression expression) =>
      new SubqueryDefaultResultRewriter().Visit(expression);

    private SubqueryDefaultResultRewriter()
    {
    }
  }
}