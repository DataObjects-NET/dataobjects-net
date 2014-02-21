// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Linq.Model;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal sealed class AggregateOptimizer : ExtendedExpressionVisitor
  {
    private sealed class GroupByItemWrapper<TKey, TElement>
    {
      public TKey Key { get; set; }
      public TElement Element { get; set; }
    }

    private sealed class ProjectionWrapper<TItem, TProjected>
    {
      public TItem Item { get; set; }
      public TProjected Projected { get; set; }
    }

    private sealed class AggregateProjectionFinder : ExpressionVisitor
    {
      private readonly List<LambdaExpression> result = new List<LambdaExpression>();
      private ParameterExpression aggregatedSequence;

      public static List<LambdaExpression> Execute(LambdaExpression expression)
      {
        // Find projection for aggregates calculated over sequence
        // specified by aggregatedSequence.
        //
        // Example:
        //   x => new {
        //     CountActive = x.Count(i => i.Active),
        //     SumActive = x.Sum(i => i.Active ? i.Value : 0),
        //   };
        //
        // The following expressions are returned:
        //
        //   i => i.Active
        //   i => i.Acitve ? i.Value : 0

        var finder = new AggregateProjectionFinder();
        finder.aggregatedSequence = expression.Parameters.Count==2
          ? expression.Parameters[1]  // Result selector parameter in GroupBy
          : expression.Parameters[0]; // Selector parameter in Select after GroupBy
        finder.Visit(expression);
        return finder.result;
      }

      protected override Expression VisitMethodCall(MethodCallExpression mc)
      {
        if (IsAggregateWithProjection(mc)) {
          var sequence = mc.Arguments[0];
          if (sequence==aggregatedSequence) {
            var projection = mc.Arguments[1].StripQuotes();
            result.Add(projection);
            return mc;
          }
        }
        return base.VisitMethodCall(mc);
      }

      private static bool IsAggregateWithProjection(MethodCallExpression call)
      {
        if (call.Arguments.Count!=2)
          return false; // Aggregates with projection have exactly 2 parameters
        var kind = QueryableVisitor.GetQueryableMethod(call);
        if (kind==null)
          return false; // Not a queryable method
        switch (kind.Value) {
        case QueryableMethodKind.Max:
        case QueryableMethodKind.Min:
        case QueryableMethodKind.Sum:
        case QueryableMethodKind.Average:
        case QueryableMethodKind.Count:
          return true;
        default:
          return false;
        }
      }

      private AggregateProjectionFinder()
      {
      }
    }

    private sealed class ReferenceFieldAccessExtractor : ExpressionVisitor
    {
      private Expression result;
      private ParameterExpression parameter;

      public static LambdaExpression Execute(LambdaExpression expression)
      {
        // Transform lamba expression as follows:
        // if original lambda contains sequence of reference field traversals,
        // extract this traversal sequence and create lambda with it as body, otherwise return null.
        // Longest possible expression is returned.
      
        // Example 1: e => e.Ref.Value > 0 transforms into e => e.Foo
        // Example 2: e => e.Value transforms into null
        // Example 3: e => e.Ref1.Ref2!=null transforms into e => e.Ref1.Ref

        var extractor = new ReferenceFieldAccessExtractor();
        extractor.parameter = expression.Parameters[0];
        extractor.Visit(expression);
        return extractor.result==null
          ? null
          : FastExpression.Lambda(extractor.result, extractor.parameter);
      }

      protected override Expression VisitMemberAccess(MemberExpression m)
      {
        if (IsReferenceFieldAccess(m)) {
          var fieldAccess = m;
          while (IsReferenceOrStructureFieldAccess(fieldAccess)) {
            var parent = fieldAccess.Expression as MemberExpression;
            if (parent==null)
              break;
            fieldAccess = parent;
          }
          if (IsReferenceOrStructureFieldAccess(fieldAccess) && fieldAccess.Expression==parameter) {
            result = m;
            return m;
          }
        }
        return base.VisitMemberAccess(m);
      }

      private static bool IsReferenceFieldAccess(MemberExpression m)
      {
        var property = m.Member as PropertyInfo;
        return property!=null && typeof (IEntity).IsAssignableFrom(property.PropertyType);
      }

      private static bool IsReferenceOrStructureFieldAccess(MemberExpression m)
      {
        var property = m.Member as PropertyInfo;
        return property!=null && (typeof (IEntity).IsAssignableFrom(property.PropertyType)
          || typeof (Structure).IsAssignableFrom(property.PropertyType));
      }

      private ReferenceFieldAccessExtractor()
      {
      }
    }

    protected override Expression VisitUnknown(Expression expression)
    {
      return expression;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      var methodKind = QueryableVisitor.GetQueryableMethod(mc);
      if (methodKind==QueryableMethodKind.Select) {
        var selectSource = mc.Arguments[0] as MethodCallExpression;
        if (selectSource!=null) {
          var sourceMethodKind = QueryableVisitor.GetQueryableMethod(selectSource);
          if (sourceMethodKind==QueryableMethodKind.GroupBy) {
            var projection = mc.Arguments[1].StripQuotes();
            var newSelectSource = VisitGroupBy(selectSource, projection);
            if (newSelectSource!=selectSource)
              return QueryFactory.Select(newSelectSource, projection);
          }
        }
      }
      else if (methodKind==QueryableMethodKind.GroupBy) {
        var newGroupBy = VisitGroupBy(mc, null);
        if (newGroupBy!=mc)
          return newGroupBy;
      }
      return base.VisitMethodCall(mc);
    }

    private MethodCallExpression VisitGroupBy(MethodCallExpression groupByCall, LambdaExpression selectProjection)
    {
      var groupBy = QueryParser.ParseGroupBy(groupByCall);
      var projection = groupBy.ResultSelector ?? selectProjection;
      if (projection!=null) {
        var referenceFieldAccessors = AggregateProjectionFinder.Execute(projection)
          .Select(ReferenceFieldAccessExtractor.Execute)
          .Where(item => item!=null)
          .ToList();
        if (referenceFieldAccessors.Count > 0) {
          groupBy.Source = Visit(groupBy.Source);
          if (groupBy.ElementSelector!=null)
            AddGroupByItemWrapper(groupBy, referenceFieldAccessors);
          groupBy.Source = referenceFieldAccessors.Aggregate(groupBy.Source, AddProjectionPrefetch);
          return QueryFactory.GroupBy(groupBy);
        }
      }
      return groupByCall;
    }

    public static Expression Rewrite(Expression expression)
    {
      // Optimize translation of aggregates by pre-evaluting
      // reference field access before GroupBy.
      // This would help Translator to translate GroupBy+Aggregate
      // into single AggregateProvider.

      return new AggregateOptimizer().Visit(expression);
    }

    private static void AddGroupByItemWrapper(GroupByQuery groupBy, List<LambdaExpression> elementProjections)
    {
      var keySelector = groupBy.KeySelector;
      var elementSelector = groupBy.ElementSelector;

      var keyType = keySelector.Body.Type;
      var elementType = elementSelector.Body.Type;
      var wrapperType = typeof (GroupByItemWrapper<,>).MakeGenericType(keyType, elementType);
      var wrapperKeyProperty = wrapperType.GetProperty("Key");
      var wrapperElementProperty = wrapperType.GetProperty("Element");

      var projectionParameter = keySelector.Parameters[0];
      var projectionLambda = FastExpression.Lambda(
        Expression.MemberInit(
          Expression.New(wrapperType),
          Expression.Bind(wrapperKeyProperty, keySelector.Body),
          Expression.Bind(wrapperElementProperty,
            elementSelector.BindParameters(projectionParameter))),
        projectionParameter);

      groupBy.Source = QueryFactory.Select(groupBy.Source, projectionLambda);

      var wrapperParameter = Expression.Parameter(wrapperType, "wrapper");
      var wrapperKeyAccess = Expression.MakeMemberAccess(wrapperParameter, wrapperKeyProperty);
      var wrapperElementAccess = Expression.MakeMemberAccess(wrapperParameter, wrapperElementProperty);
      groupBy.KeySelector = FastExpression.Lambda(wrapperKeyAccess, wrapperParameter);
      groupBy.ElementSelector = FastExpression.Lambda(wrapperElementAccess, wrapperParameter);

      for (int i = 0; i < elementProjections.Count; i++) {
        elementProjections[i] = FastExpression.Lambda(
          elementProjections[i].BindParameters(wrapperElementAccess),
          wrapperParameter);
      }
    }

    private static MethodCallExpression AddProjectionPrefetch(Expression source, LambdaExpression projection)
    {
      var itemType = QueryHelper.GetSequenceElementType(source.Type);
      var projectionType = projection.Body.Type;

      var wrapperType = typeof (ProjectionWrapper<,>).MakeGenericType(itemType, projectionType);
      var wrapperItemProperty = wrapperType.GetProperty("Item");
      var wrapperProjectedProperty = wrapperType.GetProperty("Projected");

      var wrapperProjectionParameter = projection.Parameters[0];
      var wrapperProjectionLambda = FastExpression.Lambda(
        Expression.MemberInit(
          Expression.New(wrapperType),
          Expression.Bind(wrapperItemProperty, wrapperProjectionParameter),
          Expression.Bind(wrapperProjectedProperty, projection.Body)
          ),
        wrapperProjectionParameter);
      var wrapperProjection = QueryFactory.Select(source, wrapperProjectionLambda);

      var resultProjectionParameter = Expression.Parameter(wrapperType, "wrapper");
      var resultProjectionLambda = FastExpression.Lambda(
        Expression.MakeMemberAccess(resultProjectionParameter, wrapperItemProperty),
        resultProjectionParameter);
      var resultProjection = QueryFactory.Select(wrapperProjection, resultProjectionLambda);

      return resultProjection;
    }

    private AggregateOptimizer()
    {
    }
  }
}