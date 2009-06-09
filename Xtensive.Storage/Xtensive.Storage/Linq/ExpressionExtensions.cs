// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;

namespace Xtensive.Storage.Linq
{
  internal static class ExpressionExtensions
  {
    public static Expression MakeTupleAccess(this Expression target, Type accessorType, int index)
    {
      return Expression.Call(
        target,
        WellKnownMembers.TupleGenericAccessor.MakeGenericMethod(accessorType),
        Expression.Constant(index)
        );
    }

    public static Expression MakeIsNullCondition(this Expression target, Expression ifNull, Expression ifNotNull)
    {
      return Expression.Condition(
        Expression.Equal(target, Expression.Constant(null, target.Type)),
        ifNull, ifNotNull
        );
    }

    public static bool IsAnonymousConstructor(this Expression expression)
    {
      if (expression.NodeType != ExpressionType.New) return false;
      return expression.GetMemberType() == MemberType.Anonymous;
    }

    public static bool IsConversionOperation(this Expression expression)
    {
      return expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.TypeAs;
    }

    public static bool IsQuery(this Expression expression)
    {
      return expression.Type.IsOfGenericInterface(typeof(IQueryable<>));
    }

    public static bool IsItemProjector(this Expression expression)
    {
      return (ExtendedExpressionType)expression.NodeType == ExtendedExpressionType.ItemProjector;
    }

    public static bool IsProjection(this Expression expression)
    {
      return (ExtendedExpressionType) expression.NodeType==ExtendedExpressionType.Projection;
    }

    public static bool IsEntitySetProjection(this Expression expression)
    {
      return (ExtendedExpressionType)expression.NodeType == ExtendedExpressionType.EntitySet;
    }

    public static bool IsGroupingExpression(this Expression expression)
    {
      return (ExtendedExpressionType)expression.NodeType==ExtendedExpressionType.Grouping;
    }

    public static bool IsSubqueryExpression(this Expression expression)
    {
      return (ExtendedExpressionType)expression.NodeType==ExtendedExpressionType.SubQuery;
    }

    public static bool IsEntitySet(this Expression expression)
    {
      return expression.Type.IsGenericType 
        && expression.Type.GetGenericTypeDefinition() == typeof(EntitySet<>);
    }

    public static Type GetGroupingKeyType(this Expression expression)
    {
      var newExpression = (NewExpression)expression.StripCasts();
      return newExpression.Type.GetGenericArguments()[0];
    }

//    public static Type GetGroupingElementType(this Expression expression)
//    {
//      var newExpression = (NewExpression)expression.StripCasts();
//      return newExpression.Type.GetGenericArguments()[1];
//    }
//
//    public static Parameter<Tuple> GetGroupingParameter(this Expression expression)
//    {
//      var newExpression = (NewExpression)expression.StripCasts();
//      return (Parameter<Tuple>) ((ConstantExpression) newExpression.Arguments[3]).Value;
//    }


//    public static Parameter<Tuple> GetSubqueryParameter(this Expression expression)
//    {
//      var newExpression = (NewExpression)expression.StripCasts();
//      return (Parameter<Tuple>) ((ConstantExpression) newExpression.Arguments[2]).Value;
//    }
//
//    public static ProjectionExpression GetGroupingItemsResult(this Expression expression)
//    {
//      var newExpression = (NewExpression)expression.StripCasts();
//      return (ProjectionExpression) ((ConstantExpression) newExpression.Arguments[2]).Value;
//    }

//    public static ProjectionExpression GetSubqueryItemsResult(this Expression expression)
//    {
//      var newExpression = (NewExpression)expression.StripCasts();
//      return (ProjectionExpression) ((ConstantExpression) newExpression.Arguments[0]).Value;
//    }

    public static MemberType GetMemberType(this Expression e)
    {
      var type = e.Type;
      if (typeof (Key).IsAssignableFrom(type))
        return MemberType.Key;
      if (typeof (IEntity).IsAssignableFrom(type))
        return MemberType.Entity;
      if (typeof (Structure).IsAssignableFrom(type))
        return MemberType.Structure;
      if (typeof (EntitySetBase).IsAssignableFrom(type))
        return MemberType.EntitySet;
      if (type.IsAnonymous())
        return MemberType.Anonymous;
      if (e.IsGroupingExpression())
        return MemberType.Grouping;
      if (e.IsSubqueryExpression())
        return MemberType.Subquery;
      if (type.IsArray)
        return MemberType.Array;

      if ((ExtendedExpressionType)e.NodeType==ExtendedExpressionType.Field 
        || (ExtendedExpressionType)e.NodeType==ExtendedExpressionType.Column)
        return MemberType.Primitive;

      return MemberType.Unknown;
    }
 }
}