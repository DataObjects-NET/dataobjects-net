using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ExpressionVisitor = Xtensive.Core.Linq.ExpressionVisitor;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  internal sealed class EnumRewriter : ExpressionVisitor
  {
    protected override Expression VisitUnknown(Expression e)
    {
      return ConvertEnum(e);
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      return ConvertEnum(c);
    }

    /// <inheritdoc/>
    public new Expression Visit(Expression e)
    {
      return base.Visit(e);
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
  }
}
