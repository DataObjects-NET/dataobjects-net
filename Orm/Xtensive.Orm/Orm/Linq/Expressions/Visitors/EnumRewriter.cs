﻿using System;
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
      return ConvertEnumConstant(c);
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

    private Expression ConvertEnumConstant(ConstantExpression c)
    {
      if (c.Type.StripNullable().IsEnum) {
        var underlyingType = Enum.GetUnderlyingType(c.Type.StripNullable());
        if (c.Type.IsNullable())
          underlyingType = typeof (Nullable<>).MakeGenericType(underlyingType);
        var underlyingTypeValue = Convert.ChangeType(c.Value, underlyingType);
        var constantExpression = Expression.Constant(underlyingTypeValue);
        return Expression.Convert(constantExpression, c.Type);
      }
      return c;
    }

    private EnumRewriter()
    {
    }
  }
}
