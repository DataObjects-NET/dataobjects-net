using System;
using System.Linq.Expressions;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal sealed class EnumRewriter : ExpressionVisitor
  {
    public static Expression Rewrite(Expression target)
    {
      return new EnumRewriter().Visit(target);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return ConvertEnum(e);
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      return ConvertEnum(c);
    }

    protected override Expression VisitNewArray(NewArrayExpression na)
    {
      var initializers = VisitExpressionList(na.Expressions);
      if (initializers==na.Expressions)
        return na;
      var currentType = na.Type.GetElementType();
      if (currentType.IsEnum)
        currentType = Enum.GetUnderlyingType(currentType);
      if (na.NodeType==ExpressionType.NewArrayInit)
        return Expression.NewArrayInit(currentType, initializers);
      return Expression.NewArrayBounds(currentType, initializers);
    }

    private Expression ConvertEnum(Expression expression)
    {
      if (expression.Type.StripNullable().IsEnum) {
        var underlyingType = Enum.GetUnderlyingType(expression.Type.StripNullable());
        if (expression.Type.IsNullable())
          underlyingType = typeof (Nullable<>).MakeGenericType(underlyingType);
        return Expression.Convert(expression, underlyingType);
      }
      return expression;
    }

    private EnumRewriter()
    {
    }
  }
}
