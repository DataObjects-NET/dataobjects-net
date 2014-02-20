// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xtensive.Core;

namespace Xtensive.Orm.Linq
{
  internal static class LocalCollectionKeyTypeExtractor
  {
    public static Type Extract(BinaryExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      var expr = VisitBinaryExpession(expression);
      if (expr.Type.IsSubclassOf(typeof (Entity)))
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
