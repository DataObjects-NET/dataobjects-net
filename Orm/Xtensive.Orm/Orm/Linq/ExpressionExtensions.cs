// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.12.02

using System;
using System.Collections.Concurrent;
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
    private static readonly ConcurrentDictionary<Type, MemberType> memberTypeByType = new ConcurrentDictionary<Type, MemberType>();

    private static readonly Func<Type, MemberType> memberTypeFactory = type =>
      WellKnownOrmTypes.Key.IsAssignableFrom(type) ? MemberType.Key
      : WellKnownOrmInterfaces.Entity.IsAssignableFrom(type) ? MemberType.Entity
      : WellKnownOrmTypes.Structure.IsAssignableFrom(type) ? MemberType.Structure
      : WellKnownOrmTypes.EntitySetBase.IsAssignableFrom(type) ? MemberType.EntitySet
      : type.IsAnonymous() ? MemberType.Anonymous
      : MemberType.Unknown;

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

    public static bool IsLocalCollection(this Expression expression, TranslatorContext context)
    {
      if (expression == null) {
        return false;
      }
      expression = expression.StripMarkers();

      return !expression.IsProjection()
      && !expression.IsGroupingExpression()
      && !expression.IsEntitySet()
      && !expression.IsSubqueryExpression()
      && expression.Type != WellKnownTypes.String
      && expression.Type.IsOfGenericInterface(WellKnownInterfaces.EnumerableOfT)
      && (IsEvaluableCollection(context, expression) || IsForeignQuery(expression));
    }

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
            var definition = type.CachedGetGenericTypeDefinition();
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

    public static bool IsProjection(this Expression strippedMarkerExpression) =>
      (ExtendedExpressionType) strippedMarkerExpression.NodeType == ExtendedExpressionType.Projection;

    public static bool IsEntitySetProjection(this Expression expression)
    {
      expression = expression.StripMarkers();
      return (ExtendedExpressionType) expression.NodeType == ExtendedExpressionType.EntitySet;
    }

    public static bool IsGroupingExpression(this Expression strippedMarkerExpression) =>
      (ExtendedExpressionType) strippedMarkerExpression.NodeType == ExtendedExpressionType.Grouping;

    public static bool IsSubqueryExpression(this Expression strippedMarkerExpression) =>
      (ExtendedExpressionType) strippedMarkerExpression.NodeType == ExtendedExpressionType.SubQuery;

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

    public static bool IsEntitySetExpression(this Expression strippedMarkerExpression) =>
      (ExtendedExpressionType) strippedMarkerExpression.NodeType == ExtendedExpressionType.EntitySet;

    public static bool IsEntitySet(this Expression expression) =>
      expression.Type switch {
        var type => type.IsGenericType && type.CachedGetGenericTypeDefinition() == WellKnownOrmTypes.EntitySetOfT
      };

    public static Expression StripMarkers(this Expression e) =>
      e is MarkerExpression marker ? marker.Target : e;

    public static bool IsMarker(this Expression e)
    {
      e = e.StripCasts();
      return (ExtendedExpressionType) e.NodeType == ExtendedExpressionType.Marker;
    }

    public static bool TryGetMarker(this Expression e, out MarkerType markerType)
    {
      if (e.StripCasts() is MarkerExpression marker) {
        markerType = marker.MarkerType;
        return true;
      }
      markerType = MarkerType.None;
      return false;
    }

    public static MemberType GetMemberType(this Expression e)
    {
      e = e.StripMarkers();
      var type = e.Type;

      if (memberTypeByType.GetOrAdd(type, memberTypeFactory) is var memberType && memberType != MemberType.Unknown) {
        return memberType;
      }

      return (ExtendedExpressionType) e.NodeType switch {
        ExtendedExpressionType.Grouping => MemberType.Grouping,
        ExtendedExpressionType.SubQuery => MemberType.Subquery,
        ExtendedExpressionType.FullText => MemberType.FullTextMatch,
        _ when type.IsArray => MemberType.Array,
        ExtendedExpressionType.Field or ExtendedExpressionType.Column => MemberType.Primitive,
        _ => MemberType.Unknown
      };
    }

    public static ParameterInfo[] GetConstructorParameters(this NewExpression expression)
    {
      // Note for stupid ReSharper:
      // NewExpression.Constructor is perfectly valid null
      // when value type is created using default (parameterless) constructor

      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      // ReSharper disable HeuristicUnreachableCode
      if (expression.Constructor == null) {
        return Array.Empty<ParameterInfo>();
      }
      // ReSharper restore ConditionIsAlwaysTrueOrFalse
      // ReSharper restore HeuristicUnreachableCode

      return expression.Constructor.GetParameters();
    }
  }
}