// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Reflection;
using Xtensive.Orm.Linq.Expressions;


namespace Xtensive.Orm.Linq
{
  internal static class ExpressionExtensions
  {
    public static void EnsureKeyExpressionCompatible(this KeyExpression leftKeyExpression, KeyExpression rightKeyExpression, Expression expressionPart)
    {
        if (leftKeyExpression!=null && rightKeyExpression!=null) {
          if (leftKeyExpression.EntityType.IsInterface || rightKeyExpression.EntityType.IsInterface) {
            if (leftKeyExpression.EntityType.Key!=rightKeyExpression.EntityType.Key)
              throw new InvalidOperationException(String.Format(Strings.ExKeysOfXAndXNotCompatible, expressionPart.ToString(true), leftKeyExpression.EntityType, rightKeyExpression.EntityType));
          }
          else if (leftKeyExpression.EntityType.Hierarchy!=rightKeyExpression.EntityType.Hierarchy)
            throw new InvalidOperationException(String.Format(Strings.ExEntitiesXAndXBelongToDifferentHierarchies, expressionPart.ToString(true), leftKeyExpression.EntityType, rightKeyExpression.EntityType));
        }
    }

    public static bool IsAnonymousConstructor(this Expression expression)
    {
      expression = expression.StripMarkers();
      return expression.NodeType == ExpressionType.New && expression.GetMemberType() == MemberType.Anonymous;
    }

    public static bool IsConversionOperation(this Expression expression)
    {
      expression = expression.StripMarkers();
      return expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.TypeAs;
    }

    public static bool IsQuery(this Expression expression)
    {
      return expression.Type.IsOfGenericInterface(typeof(IQueryable<>));
    }

    public static bool IsLocalCollection(this Expression expression, TranslatorContext context)
    {
      return expression!=null
        && !expression.IsProjection()
        && !expression.IsGroupingExpression()
        && !expression.IsEntitySet()
        && !expression.IsSubqueryExpression()
        && expression.Type!=typeof(string)
        && !expression.Type.IsOfGenericInterface(typeof (IQueryable<>))
        && context.Evaluator.CanBeEvaluated(expression)
          && expression.Type.IsOfGenericInterface(typeof (IEnumerable<>));
    }

    public static bool IsItemProjector(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType)expression.NodeType == ExtendedExpressionType.ItemProjector;
    }

    public static bool IsProjection(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType==ExtendedExpressionType.Projection;
    }

    public static bool IsEntitySetProjection(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType)expression.NodeType == ExtendedExpressionType.EntitySet;
    }

    public static bool IsGroupingExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType)expression.NodeType==ExtendedExpressionType.Grouping;
    }

    public static bool IsSubqueryExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType)expression.NodeType==ExtendedExpressionType.SubQuery;
    }

    public static bool IsFullTextMatchExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType)expression.NodeType == ExtendedExpressionType.FullText;
    }

    public static bool IsEntityExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType)expression.NodeType==ExtendedExpressionType.Entity;
    }

    public static bool IsEntitySetExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType)expression.NodeType==ExtendedExpressionType.EntitySet;
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

    public static Expression StripMarkers(this Expression e)
    {
      var ee = e as ExtendedExpression;
      if (ee != null && ee.ExtendedType == ExtendedExpressionType.Marker) {
        var marker = (MarkerExpression) ee;
        return marker.Target;
      }
      return e;
    }

    public static bool IsMarker(this Expression e)
    {
      e = e.StripCasts();
      return (ExtendedExpressionType)e.NodeType == ExtendedExpressionType.Marker;
    }

    public static bool TryGetMarker(this Expression e, out MarkerType markerType)
    {
      e = e.StripCasts();
      markerType = MarkerType.None;
      var result = (ExtendedExpressionType)e.NodeType == ExtendedExpressionType.Marker;
      if (result) {
        var marker = (MarkerExpression) e;
        markerType = marker.MarkerType;
      }
      return result;
    }

    public static MemberType GetMemberType(this Expression e)
    {
      e = e.StripMarkers();
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
      if (e.IsFullTextMatchExpression())
        return MemberType.FullTextMatch;
      if (type.IsArray)
        return MemberType.Array;

      if ((ExtendedExpressionType)e.NodeType==ExtendedExpressionType.Field 
        || (ExtendedExpressionType)e.NodeType==ExtendedExpressionType.Column)
        return MemberType.Primitive;

      return MemberType.Unknown;
    }

    public static ParameterInfo[] GetConstructorParameters(this NewExpression expression)
    {
      // Note for stupid ReSharper:
      // NewExpression.Constructor is perfectly valid null
      // when value type is created using default (parameterless) constructor

      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      if (expression.Constructor==null)
        return new ParameterInfo[0];
      // ReSharper restore ConditionIsAlwaysTrueOrFalse
      return expression.Constructor.GetParameters();
    }
  }
}