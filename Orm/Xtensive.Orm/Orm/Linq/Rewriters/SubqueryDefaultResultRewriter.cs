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
    private readonly Expression root;

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      var left = ApplyCorrection(Visit(b.Left));
      var right = ApplyCorrection(Visit(b.Right));

      if (b.Left==left && b.Right==right)
        return b;

      return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
    }

    private Expression ApplyCorrection(Expression originalExpression)
    {
      var expression = originalExpression;
      if (expression.NodeType==ExpressionType.Convert)
        expression = ((UnaryExpression) expression).Operand;

      var methodCall = expression as MethodCallExpression;
      if (methodCall==null || !IsDefaultingMethod(methodCall.Method) || !HasNonNullDefault(methodCall.Type))
        return originalExpression;

      var methodReturnType = methodCall.Type;
      expression = Expression.Coalesce(
        Expression.Convert(expression, methodReturnType.ToNullable()),
        Expression.Constant(Activator.CreateInstance(methodReturnType)));

      if (expression.Type!=originalExpression.Type)
        expression = Expression.Convert(expression, originalExpression.Type);

      return expression;
    }

    private bool HasNonNullDefault(Type type)
    {
      return type.IsValueType && !type.IsNullable();
    }

    private bool IsDefaultingMethod(MethodInfo method)
    {
      return method.DeclaringType==typeof (Queryable)
        && (method.Name==Reflection.WellKnown.Queryable.FirstOrDefault
        || method.Name==Reflection.WellKnown.Queryable.SingleOrDefault);
    }

    public static Expression Rewrite(Expression expression)
    {
      return new SubqueryDefaultResultRewriter(expression).Visit(expression);
    }

    private SubqueryDefaultResultRewriter(Expression root)
    {
      this.root = root;
    }
  }
}