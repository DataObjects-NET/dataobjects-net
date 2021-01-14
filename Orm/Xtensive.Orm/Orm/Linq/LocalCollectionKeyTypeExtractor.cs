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

namespace Xtensive.Orm.Linq
{
  internal static class LocalCollectionKeyTypeExtractor
  {
    public static Type Extract(BinaryExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      var expr = VisitBinaryExpession(expression);
      if (expr.Type.IsSubclassOf(WellKnownOrmTypes.Entity))
        return expr.Type;
      throw new NotSupportedException(string.Format(Strings.ExCurrentTypeXIsNotSupported, expr.Type));
    }

    private static Expression VisitBinaryExpession(BinaryExpression binaryExpression)
    {
      var memberExpression = binaryExpression.Right as MemberExpression;
      if (memberExpression==null)
        throw new InvalidOperationException(string.Format(Strings.ExCantConvertXToY, binaryExpression.Type, typeof (MemberExpression)));
      return VisitMemberExpression(memberExpression);
    }

    private static Expression VisitMemberExpression(MemberExpression memberExpression)
    {
      var parameter = memberExpression.Expression as ParameterExpression;
      if (parameter!=null)
        return parameter;
      var member = memberExpression.Expression as MemberExpression;
      if (member!=null)
        return member;
      throw new NotSupportedException(string.Format(Strings.ExCurrentTypeOfExpressionXIsNotSupported, memberExpression.Expression.Type));
    }
  }
}
