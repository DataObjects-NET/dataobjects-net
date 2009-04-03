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
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Internals;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal static class RangeSetExpressionBuilder
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

    public static RangeSetInfo BuildConstructor(TupleFieldInfo originTuple, IndexInfo indexInfo)
    {
      Expression firstEndpoint;
      Expression secondEndpoint;
      singleValueCash.Clear();
      singleValueCash.Add(0, originTuple.Comparison.Value);
      CreateRangeEndpoints(out firstEndpoint, out secondEndpoint, singleValueCash,
        originTuple.Comparison.Operation, indexInfo);
      return BuildConstructor(firstEndpoint, secondEndpoint, originTuple.Comparison.Operation, originTuple);
    }

    public static RangeSetInfo BuildConstructor(Dictionary<int, Expression> indexKeyValues,
      TupleFieldInfo originTuple, IndexInfo indexInfo)
    {
      ArgumentValidator.EnsureArgumentIsInRange(indexKeyValues.Count, 2, int.MaxValue, "indexKeyValues.Count");
      Expression firstEndpoint;
      Expression secondEndpoint;
      CreateRangeEndpoints(out firstEndpoint, out secondEndpoint, indexKeyValues,
        originTuple.Comparison.Operation, indexInfo);
      return BuildConstructor(firstEndpoint, secondEndpoint, originTuple.Comparison.Operation, null);
    }

    private static RangeSetInfo BuildConstructor(Expression firstEndpoint, Expression secondEndpoint,
      ComparisonOperation comparisonOperation, TupleFieldInfo origin)
    {
      NewExpression rangeConstruction = Expression.New(rangeContructor, firstEndpoint, secondEndpoint);
      //TODO:A comparer from index must be passed here.
      RangeSetInfo result = CreateNotFullExpression(
        Expression.New(rangeSetConstructor, rangeConstruction,
          Expression.Constant(AdvancedComparer<Entire<Tuple>>.Default)),
        origin);
      if (comparisonOperation == ComparisonOperation.NotEqual)
        return BuildInvert(result);
      return result;
    }

    public static RangeSetInfo BuildFullRangeSetConstructor(TupleFieldInfo origin)
    {
      return new RangeSetInfo(
        //TODO:A comparer from index must be passed here.
        Expression.New(rangeSetConstructor, Expression.Constant(Range<Entire<Tuple>>.Full),
          Expression.Constant(AdvancedComparer<Entire<Tuple>>.Default)),
        origin, true);
    }

    public static RangeSetInfo BuildIntersect(RangeSetInfo target, RangeSetInfo other)
    {
      var intersectionResult = Expression.Call(target.Source, intersectMethod, other.Source);
      target.Intersect(intersectionResult, other);
      return target;
    }

    public static RangeSetInfo BuildUnite(RangeSetInfo target, RangeSetInfo other)
    {
      var unionResult = Expression.Call(target.Source, uniteMethod, other.Source);
      target.Unite(unionResult, other);
      return target;
    }

    public static RangeSetInfo BuildFullOrEmpty(Expression booleanExp)
    {
      //TODO:A comparer from index must be passed here.
      return CreateNotFullExpression(Expression.Call(
        fullOrEmptyMethod, booleanExp, Expression.Constant(AdvancedComparer<Entire<Tuple>>.Default)), null);
    }

    private static RangeSetInfo BuildInvert(RangeSetInfo target)
    {
      var invertionResult = Expression.Call(target.Source, invertMethod);
      target.Invert(invertionResult);
      return target;
    }

    private static void CreateRangeEndpoints(out Expression first, out Expression second,
      Dictionary<int, Expression> indexKeyValues, ComparisonOperation comparsionType, IndexInfo indexInfo)
    {
      if (comparsionType == ComparisonOperation.Equal || comparsionType == ComparisonOperation.NotEqual) {
        first = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, false);
        second = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, true);
        return;
      }
      if(comparsionType == ComparisonOperation.LessThan) {
        first = BuildInfiniteEntire(false);
        second = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, false);
        return;
      }
      if (comparsionType == ComparisonOperation.LessThanOrEqual) {
        first = BuildInfiniteEntire(false);
        second = BuildEntireConstructor(indexKeyValues, indexInfo);
        return;
      }
      if (comparsionType == ComparisonOperation.GreaterThan) {
        first = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, true);
        second = BuildInfiniteEntire(true);
        return;
      }
      if (comparsionType == ComparisonOperation.GreaterThanOrEqual) {
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
          Expression.Convert(indexKeyValue.Value, typeof (object)));
      }
      result = Expression.Property(result, wrappedTupleProperty);
      return result;
    }

    private static RangeSetInfo CreateNotFullExpression(Expression source, TupleFieldInfo origin)
    {
      return new RangeSetInfo(source, origin, false);
    }

    // Constructors

    static RangeSetExpressionBuilder()
    {
      tupleCreateMethod = typeof (Tuple).GetMethod("Create", new[] {typeof (TupleDescriptor)});
      tupleUpdaterConstructor = typeof(TupleUpdater).GetConstructor(new[] { typeof(Tuple) });
      tupleUpdateMethod = typeof(TupleUpdater).GetMethod("UpdateField");
      wrappedTupleProperty = typeof (TupleUpdater).GetProperty("Tuple");
      entireConstructor = typeof (Entire<Tuple>).GetConstructor(new[]{typeof(Tuple)});
      shiftedEntireConstutor = typeof (Entire<Tuple>).GetConstructor(new[] {typeof (Tuple),
                                                                            typeof (Direction)});
      infiniteEntireConstructor = typeof (Entire<Tuple>).GetConstructor(new[] {typeof (InfinityType)});
      rangeContructor = typeof (Range<Entire<Tuple>>)
        .GetConstructor(new[] {typeof (Entire<Tuple>), typeof (Entire<Tuple>)});
      rangeSetConstructor = typeof (RangeSet<Entire<Tuple>>)
        .GetConstructor(new[] { typeof(Range<Entire<Tuple>>), typeof(AdvancedComparer<Entire<Tuple>>) });
      intersectMethod = typeof (RangeSet<Entire<Tuple>>)
        .GetMethod("Intersect", new[] {typeof (RangeSet<Entire<Tuple>>)});
      uniteMethod = typeof (RangeSet<Entire<Tuple>>)
        .GetMethod("Unite", new[] {typeof (RangeSet<Entire<Tuple>>)});
      invertMethod = typeof(RangeSet<Entire<Tuple>>).GetMethod("Invert");
      fullOrEmptyMethod = typeof (RangeSet<Entire<Tuple>>).GetMethod("CreateFullOrEmpty");
    }
  }
}