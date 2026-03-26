// Copyright (C) 2013-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;

namespace Xtensive.Orm.Linq
{
  internal static class LocalCollectionKeyTypeExtractor
  {
    public static Type Extract(BinaryExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      if (expression.Right.StripMarkers() is KeyExpression key) {
        return key.EntityType.UnderlyingType;
      }

      var expr = VisitBinaryExpression(expression);
      if (expr.Type.IsSubclassOf(WellKnownOrmTypes.Entity)) {
        return expr.Type;
      }
      throw new NotSupportedException(string.Format(Strings.ExCurrentTypeXIsNotSupported, expr.Type));
    }

    private static Expression VisitBinaryExpression(BinaryExpression binaryExpression)
    {
      if (!(binaryExpression.Right is MemberExpression memberExpression)) {
        throw new InvalidOperationException(string.Format(Strings.ExCantConvertXToY, binaryExpression.Type, typeof(MemberExpression)));
      }
      return VisitMemberExpression(memberExpression);
    }

    private static Expression VisitMemberExpression(MemberExpression memberExpression)
    {
      if (memberExpression.Expression is ParameterExpression parameter) {
        return parameter;
      }
      if (memberExpression.Expression is MemberExpression member) {
        return member;
      }
      throw new NotSupportedException(string.Format(Strings.ExCurrentTypeOfExpressionXIsNotSupported, memberExpression.Expression.Type));
    }
  }
}
