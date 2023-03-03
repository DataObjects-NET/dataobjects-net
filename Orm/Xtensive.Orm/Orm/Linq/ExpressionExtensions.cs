// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Reflection;

namespace Xtensive.Orm.Linq
{
  internal static class ExpressionExtensions
  {
    public static void EnsureKeyExpressionCompatible(this KeyExpression left, KeyExpression right, Expression expressionPart)
    {
      if (left==null || right==null) {
        return;
      }

      if (left.EntityType.IsInterface || right.EntityType.IsInterface) {
        if (left.EntityType.Key.EqualityIdentifier!=right.EntityType.Key.EqualityIdentifier) {
          throw new InvalidOperationException(string.Format(
            Strings.ExKeysOfXAndXNotCompatible, expressionPart.ToString(true), left.EntityType, right.EntityType));
        }
      }
      else {
        if (left.EntityType.Hierarchy!=right.EntityType.Hierarchy) {
          throw new InvalidOperationException(string.Format(
            Strings.ExEntitiesXAndXBelongToDifferentHierarchies, expressionPart.ToString(true), left.EntityType, right.EntityType));
        }
      }
    }

    public static bool IsAnonymousConstructor(this Expression expression)
    {
      expression = expression.StripMarkers();
      return expression.NodeType==ExpressionType.New && expression.GetMemberType()==MemberType.Anonymous;
    }

    public static bool IsNewExpressionSupportedByStorage(this Expression expression) =>
      expression.NodeType == ExpressionType.New
      && expression.Type switch { var t =>
        t == WellKnownTypes.TimeSpan || t == WellKnownTypes.DateTime || t == WellKnownTypes.DateTimeOffset
#if NET6_0_OR_GREATER
          || t == WellKnownTypes.DateOnly || t == WellKnownTypes.TimeOnly
#endif
      };

    public static bool IsQuery(this Expression expression) =>
      expression.Type.IsOfGenericInterface(WellKnownInterfaces.QueryableOfT);

    public static bool IsLocalCollection(this Expression expression, TranslatorContext context) =>
      expression != null
      && !expression.IsProjection()
      && !expression.IsGroupingExpression()
      && !expression.IsEntitySet()
      && !expression.IsSubqueryExpression()
      && expression.Type != WellKnownTypes.String
      && expression.Type.IsOfGenericInterface(WellKnownInterfaces.EnumerableOfT)
      && (IsEvaluableCollection(context, expression) || IsForeignQuery(expression));

    private static bool IsEvaluableCollection(TranslatorContext context, Expression expression) =>
      !expression.Type.IsOfGenericInterface(WellKnownInterfaces.QueryableOfT)
      && context.Evaluator.CanBeEvaluated(expression);

    private static bool IsForeignQuery(Expression expression)
    {
      // Check for EnumerableQuery<T> and similar things
      if (expression.NodeType == ExpressionType.Constant) {
        if (((ConstantExpression) expression).Value is IQueryable value) {
          var type = value.GetType();
          if (type.IsGenericType) {
            var definition = type.GetGenericTypeDefinition();
            return definition != WellKnownInterfaces.QueryableOfT && definition != WellKnownTypes.QueryableOfT;
          }
        }
      }

      return false;
    }

    public static bool IsItemProjector(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType == ExtendedExpressionType.ItemProjector;
    }

    public static bool IsProjection(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType == ExtendedExpressionType.Projection;
    }

    public static bool IsEntitySetProjection(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType == ExtendedExpressionType.EntitySet;
    }

    public static bool IsGroupingExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType == ExtendedExpressionType.Grouping;
    }

    public static bool IsSubqueryExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType == ExtendedExpressionType.SubQuery;
    }

    public static bool IsFullTextMatchExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType == ExtendedExpressionType.FullText;
    }

    public static bool IsEntityExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType == ExtendedExpressionType.Entity;
    }

    public static bool IsEntitySetExpression(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType == ExtendedExpressionType.EntitySet;
    }

    public static bool IsEntitySet(this Expression expression) =>
      expression.Type.IsGenericType
      && expression.Type.GetGenericTypeDefinition() == WellKnownOrmTypes.EntitySetOfT;

    public static Expression StripMarkers(this Expression e)
    {
      if (e is ExtendedExpression ee && ee.ExtendedType == ExtendedExpressionType.Marker) {
        var marker = (MarkerExpression) ee;
        return marker.Target;
      }

      return e;
    }

    public static bool IsMarker(this Expression e)
    {
      e = e.StripCasts();
      return (ExtendedExpressionType) e.NodeType == ExtendedExpressionType.Marker;
    }

    public static bool TryGetMarker(this Expression e, out MarkerType markerType)
    {
      e = e.StripCasts();
      markerType = MarkerType.None;
      var result = (ExtendedExpressionType) e.NodeType == ExtendedExpressionType.Marker;
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
      if (WellKnownOrmTypes.Key.IsAssignableFrom(type)) {
        return MemberType.Key;
      }

      if (WellKnownOrmInterfaces.Entity.IsAssignableFrom(type)) {
        return MemberType.Entity;
      }

      if (WellKnownOrmTypes.Structure.IsAssignableFrom(type)) {
        return MemberType.Structure;
      }

      if (WellKnownOrmTypes.EntitySetBase.IsAssignableFrom(type)) {
        return MemberType.EntitySet;
      }

      if (type.IsAnonymous()) {
        return MemberType.Anonymous;
      }

      if (e.IsGroupingExpression()) {
        return MemberType.Grouping;
      }

      if (e.IsSubqueryExpression()) {
        return MemberType.Subquery;
      }

      if (e.IsFullTextMatchExpression()) {
        return MemberType.FullTextMatch;
      }

      if (type.IsArray) {
        return MemberType.Array;
      }

      if ((ExtendedExpressionType) e.NodeType == ExtendedExpressionType.Field
        || (ExtendedExpressionType) e.NodeType == ExtendedExpressionType.Column) {
        return MemberType.Primitive;
      }

      return MemberType.Unknown;
    }

    public static ParameterInfo[] GetConstructorParameters(this NewExpression expression)
    {
      // Note for stupid ReSharper:
      // NewExpression.Constructor is perfectly valid null
      // when value type is created using default (parameterless) constructor

      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      // ReSharper disable HeuristicUnreachableCode
      if (expression.Constructor == null) {
        return new ParameterInfo[0];
      }
      // ReSharper restore ConditionIsAlwaysTrueOrFalse
      // ReSharper restore HeuristicUnreachableCode

      return expression.Constructor.GetParameters();
    }
  }
}