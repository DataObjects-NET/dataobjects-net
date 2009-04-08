// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.08

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq
{
  internal static class ExpressionHelper
  {
    public static Expression KeyAccess(Expression target)
    {
      return Expression.Property(target, "Key");
    }

    public static Expression TupleAccess(Expression target, Type accessorType, int index)
    {
      return Expression.Call(
        target,
        WellKnownMembers.TupleGenericAccessor.MakeGenericMethod(accessorType),
        Expression.Constant(index)
        );
    }

    public static Expression IsNullCondition(Expression target, Expression ifNull, Expression ifNotNull)
    {
      return Expression.Condition(
        Expression.Equal(target, Expression.Constant(null, target.Type)),
        ifNull, ifNotNull
        );
    }
  }
}