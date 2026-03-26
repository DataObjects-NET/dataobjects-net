// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq.Expressions;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal sealed class EnumRewriter : ExpressionVisitor
  {
    private readonly static System.Collections.Concurrent.ConcurrentDictionary<Type, Delegate> NullableValueCreators = new();

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
          underlyingType = WellKnownTypes.NullableOfT.CachedMakeGenericType(underlyingType);
        return Expression.Convert(expression, underlyingType);
      }
      return expression;
    }

    private Expression ConvertEnumConstant(ConstantExpression c)
    {
      if (c.Type.StripNullable().IsEnum) {
        var underlyingType = Enum.GetUnderlyingType(c.Type.StripNullable());

        object underlyingTypeValue;
        if (c.Type.IsNullable()) {
          underlyingTypeValue = (c.Value is null)
            ? null
            : CreateNullableUnderlyingValue(c.Value, underlyingType);
        }
        else {
          underlyingTypeValue = Convert.ChangeType(c.Value, underlyingType);
        }
        var constantExpression = Expression.Constant(underlyingTypeValue);
        return Expression.Convert(constantExpression, c.Type);
      }
      return c;
    }

    private static object CreateNullableUnderlyingValue(object value, Type underlyingType)
    {
      var instanceCreator = NullableValueCreators.GetOrAdd(underlyingType, (t) => {
        var parameter = Expression.Parameter(underlyingType);
        var type = WellKnownTypes.NullableOfT.MakeGenericType(underlyingType);
        var ctor = type.GetConstructors()[0];
        var body = Expression.New(ctor, parameter);
        var instanceCreatorLambda = Expression.Lambda(body, parameter);
        return instanceCreatorLambda.Compile();
      });

      return instanceCreator.DynamicInvoke(value);
    }

    private EnumRewriter()
    {
    }
  }
}
