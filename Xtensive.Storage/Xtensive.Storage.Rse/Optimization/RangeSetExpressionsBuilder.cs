// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Internals;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization
{
  internal static class RangeSetExpressionsBuilder
  {
    private static readonly MethodInfo tupleCreateMethod;
    private static readonly ConstructorInfo tupleUpdaterConstructor;
    private static readonly MethodInfo tupleUpdateMethod;
    private static readonly PropertyInfo wrappedTupleProperty;
    private static readonly ConstructorInfo entireConstructor;
    private static readonly ConstructorInfo shiftedEntireConstutor;
    private static readonly ConstructorInfo infiniteEntireConstructor;
    private static readonly ConstructorInfo rangeContructor;
    private static readonly ConstructorInfo rangeSetConstructor;
    private static readonly MethodInfo intersectMethod;
    private static readonly MethodInfo uniteMethod;
    private static readonly MethodInfo invertMethod;
    private static readonly MethodInfo fullOrEmptyMethod;

    private static readonly Dictionary<int, Expression> singleValueCash = new Dictionary<int, Expression>(1);

    public static RangeSetExpression BuildConstructor(Expression indexKeyValue, int tupleField,
      ExpressionType comparisonType, IndexInfo indexInfo)
    {
      Expression firstEndpoint;
      Expression secondEndpoint;
      singleValueCash.Clear();
      singleValueCash.Add(0, indexKeyValue);
      CreateRangeEndpoints(out firstEndpoint, out secondEndpoint, singleValueCash, comparisonType,
                           indexInfo);
      return BuildConstructor(firstEndpoint, secondEndpoint, comparisonType,
                              new RangeSetOriginInfo(comparisonType == ExpressionType.NotEqual ?
                                                                               ExpressionType.Equal
                                                                               :comparisonType,
                                                     tupleField,
                                                     indexKeyValue));
    }

    public static RangeSetExpression BuildConstructor(Dictionary<int, Expression> indexKeyValues,
      ExpressionType comparisonType, IndexInfo indexInfo)
    {
      ArgumentValidator.EnsureArgumentIsInRange(indexKeyValues.Count, 2, int.MaxValue, "indexKeyValues.Count");
      Expression firstEndpoint;
      Expression secondEndpoint;
      CreateRangeEndpoints(out firstEndpoint, out secondEndpoint, indexKeyValues, comparisonType,
                           indexInfo);
      return BuildConstructor(firstEndpoint, secondEndpoint, comparisonType, null);
    }

    private static RangeSetExpression BuildConstructor(Expression firstEndpoint, Expression secondEndpoint,
      ExpressionType comparisonType, RangeSetOriginInfo origin)
    {
      NewExpression rangeConstruction = Expression.New(rangeContructor, firstEndpoint, secondEndpoint);
      //TODO:A comparer from index must be passed here.
      RangeSetExpression result = CreateNotFullExpression(
                                    Expression.New(rangeSetConstructor, rangeConstruction,
                                    Expression.Constant(AdvancedComparer<Entire<Tuple>>.Default)),
                                    origin);
      if (comparisonType == ExpressionType.NotEqual)
        return BuildInvert(result);
      return result;
    }

    public static RangeSetExpression BuildFullRangeSetConstructor(RangeSetOriginInfo origin)
    {
      return new RangeSetExpression(
        //TODO:A comparer from index must be passed here.
                         Expression.New(rangeSetConstructor, Expression.Constant(Range<Entire<Tuple>>.Full),
                                        Expression.Constant(AdvancedComparer<Entire<Tuple>>.Default)),
                         origin, true);
    }

    public static RangeSetExpression BuildIntersect(RangeSetExpression target, RangeSetExpression other)
    {
      var intersectionResult = Expression.Call(target.Source, intersectMethod, other.Source);
      target.Intersect(intersectionResult, other);
      return target;
    }

    public static RangeSetExpression BuildUnite(RangeSetExpression target, RangeSetExpression other)
    {
      var unionResult = Expression.Call(target.Source, uniteMethod, other.Source);
      target.Unite(unionResult, other);
      return target;
    }

    public static RangeSetExpression BuildInvert(RangeSetExpression target)
    {
      var invertionResult = Expression.Call(target.Source, invertMethod);
      target.Invert(invertionResult);
      return target;
    }

    public static RangeSetExpression BuildFullOrEmpty(Expression booleanExp)
    {
      //TODO:A comparer from index must be passed here.
      return CreateNotFullExpression(Expression.Call(
                                       fullOrEmptyMethod, booleanExp,
                                       Expression.Constant(AdvancedComparer<Entire<Tuple>>.Default)),
                                       null);
    }

    private static void CreateRangeEndpoints(out Expression first, out Expression second,
      Dictionary<int, Expression> indexKeyValues, ExpressionType comparsionType, IndexInfo indexInfo)
    {
      if (comparsionType == ExpressionType.Equal || comparsionType == ExpressionType.NotEqual) {
        first = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, false);
        second = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, true);
        return;
      }
      if(comparsionType == ExpressionType.LessThan) {
        first = BuildInfiniteEntire(false);
        second = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, false);
        return;
      }
      if (comparsionType == ExpressionType.LessThanOrEqual) {
        first = BuildInfiniteEntire(false);
        second = BuildEntireConstructor(indexKeyValues, indexInfo);
        return;
      }
      if (comparsionType == ExpressionType.GreaterThan) {
        first = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, true);
        second = BuildInfiniteEntire(true);
        return;
      }
      if (comparsionType == ExpressionType.GreaterThanOrEqual) {
        first = BuildEntireConstructor(indexKeyValues, indexInfo);
        second = BuildInfiniteEntire(true);
        return;
      }
      throw Exceptions.InvalidArgument(comparsionType, "comparsionType");
    }

    private static NewExpression BuildEntireConstructor(Dictionary<int, Expression> indexKeyValues,
      IndexInfo indexInfo)
    {
      Expression tupleCreation = BuildTupleCreation(indexKeyValues, indexInfo);
      return Expression.New(entireConstructor, tupleCreation);
    }

    private static NewExpression BuildShiftedEntireConstructor(Dictionary<int, Expression> indexKeyValues,
      IndexInfo indexInfo, bool positiveShift)
    {
      Expression tupleCreation = BuildTupleCreation(indexKeyValues, indexInfo);
      var shiftDirection = positiveShift ? Direction.Positive : Direction.Negative;
      return Expression.New(shiftedEntireConstutor, tupleCreation, Expression.Constant(shiftDirection));
    }

    private static NewExpression BuildInfiniteEntire(bool positiveInfinity)
    {
      var infinityType = positiveInfinity ? InfinityType.Positive : InfinityType.Negative;
      return Expression.New(infiniteEntireConstructor, Expression.Constant(infinityType));
    }

    private static Expression BuildTupleCreation(Dictionary<int, Expression> indexKeyValues,
      IndexInfo indexInfo)
    {
      if (indexKeyValues.Count == 0)
        throw new ArgumentException(Resources.Strings.ExCollectionMustNotBeEmpty);
      MethodCallExpression tupleCreation = Expression.Call(tupleCreateMethod,
        Expression.Constant(indexInfo.KeyTupleDescriptor));
      Expression result = Expression.New(tupleUpdaterConstructor, tupleCreation);
      foreach (var indexKeyValue in indexKeyValues) {
        result = Expression.Call(result, tupleUpdateMethod, Expression.Constant(indexKeyValue.Key),
                                 Expression.Convert(indexKeyValue.Value,typeof(object)));
      }
      result = Expression.Property(result, wrappedTupleProperty);
      return result;
    }

    private static RangeSetExpression CreateNotFullExpression(Expression source, RangeSetOriginInfo origin)
    {
      return new RangeSetExpression(source, origin, false);
    }

    static RangeSetExpressionsBuilder()
    {
      tupleCreateMethod = typeof (Tuple).GetMethod("Create", new[] {typeof (TupleDescriptor)});
      tupleUpdaterConstructor = typeof(TupleUpdater).GetConstructor(new[] { typeof(Tuple) });
      tupleUpdateMethod = typeof(TupleUpdater).GetMethod("UpdateField");
      wrappedTupleProperty = typeof (TupleUpdater).GetProperty("Tuple");
      entireConstructor = typeof (Entire<Tuple>).GetConstructor(new[]{typeof(Tuple)});
      shiftedEntireConstutor = typeof (Entire<Tuple>).GetConstructor(new[] {typeof (Tuple),
                                                                            typeof (Direction)});
      infiniteEntireConstructor = typeof (Entire<Tuple>).GetConstructor(new[] {typeof (InfinityType)});
      rangeContructor = typeof (Range<Entire<Tuple>>).GetConstructor(new[] {typeof (Entire<Tuple>), typeof (Entire<Tuple>)});
      rangeSetConstructor = typeof (RangeSet<Entire<Tuple>>).
        GetConstructor(new[] { typeof(Range<Entire<Tuple>>), typeof(AdvancedComparer<Entire<Tuple>>) });
      intersectMethod = typeof (RangeSet<Entire<Tuple>>).
        GetMethod("Intersect",
                  new[]
                  {typeof (RangeSet<Entire<Tuple>>)});
      uniteMethod = typeof(RangeSet<Entire<Tuple>>).
        GetMethod("Unite",
                  new[] { typeof(RangeSet<Entire<Tuple>>) });
      invertMethod = typeof(RangeSet<Entire<Tuple>>).
        GetMethod("Invert");
      fullOrEmptyMethod = typeof (RangeSet<Entire<Tuple>>).GetMethod("CreateFullOrEmpty");
    }
  }
}