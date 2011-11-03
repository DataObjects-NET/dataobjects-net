// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Internals;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Resources;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  internal static class RangeSetExpressionBuilder
  {
    private static readonly MethodInfo tupleCreateMethod;
    private static readonly ConstructorInfo tupleUpdaterConstructor;
    private static readonly MethodInfo setValueMethod;
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
    private static readonly MethodInfo concatMethod;

    public static RangeSetInfo BuildConstructor(TupleExpressionInfo originTuple,
      IndexInfo indexInfo, AdvancedComparer<Entire<Tuple>> comparer)
    {
      Expression firstEndpoint;
      Expression secondEndpoint;
      var singleValueCash = new Dictionary<int, Expression>(1);
      singleValueCash.Clear();
      singleValueCash.Add(0, originTuple.Comparison.Value);
      if (!CanBuildNonFullRangeSet(originTuple.Comparison.Operation))
        return BuildFullRangeSetConstructor(null, comparer);
      CreateRangeEndpoints(out firstEndpoint, out secondEndpoint, singleValueCash,
        originTuple.Comparison.Operation, indexInfo);
      return BuildConstructor(firstEndpoint, secondEndpoint, originTuple.Comparison.Operation, originTuple,
        comparer);
    }

    public static RangeSetInfo BuildConstructorForMultiColumnIndex(Dictionary<int, Expression> indexKeyValues,
      TupleExpressionInfo originTuple, IndexInfo indexInfo, AdvancedComparer<Entire<Tuple>> comparer)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(indexKeyValues.Count, 1, "indexKeyValues.Count");
      if (!CanBuildNonFullRangeSet(originTuple.Comparison.Operation))
        return BuildFullRangeSetConstructor(null, comparer);
      var firstBoundary = BuildFirstBoundaryOfMuliColumnIndex(indexKeyValues, originTuple.Comparison.Operation,
        indexInfo, comparer);
      if (IsEqualityComparison(originTuple))
        return firstBoundary;
      var valuesForSecondBoundary = indexKeyValues.Take(indexKeyValues.Count - 1);
      var reversedOperation = ReverseWithIgnoringOfEuqality(originTuple.Comparison.Operation);
      var secondBoundary = BuildSecondBoundaryOfMultiColumnIndex(valuesForSecondBoundary,
        reversedOperation, indexInfo, comparer);
      return BuildIntersect(firstBoundary, secondBoundary);
    }

    public static RangeSetInfo BuildFullRangeSetConstructor(TupleExpressionInfo origin,
      AdvancedComparer<Entire<Tuple>> comparer)
    {
      return new RangeSetInfo(
        Expression.New(rangeSetConstructor, Expression.Constant(Range<Entire<Tuple>>.Full),
          Expression.Constant(comparer)),
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

    public static RangeSetInfo BuildFullOrEmpty(Expression booleanExp,
      AdvancedComparer<Entire<Tuple>> comparer)
    {
      return CreateNotFullExpression(Expression.Call(
        fullOrEmptyMethod, booleanExp, Expression.Constant(comparer)), null);
    }

    public static RangeSetInfo BuildInvert(RangeSetInfo target)
    {
      var invertionResult = Expression.Call(target.Source, invertMethod);
      target.Invert(invertionResult);
      return target;
    }

    #region Private \ internal methods
    private static RangeSetInfo BuildFirstBoundaryOfMuliColumnIndex(
      IEnumerable<KeyValuePair<int, Expression>> indexKeyValues, ComparisonOperation operation,
      IndexInfo indexInfo, AdvancedComparer<Entire<Tuple>> comparer)
    {
      Expression firstEndpoint;
      Expression secondEndpoint;
      CreateRangeEndpoints(out firstEndpoint, out secondEndpoint, indexKeyValues,
        operation, indexInfo);
      return BuildConstructor(firstEndpoint, secondEndpoint,
        operation, null, comparer);
    }

    private static RangeSetInfo BuildSecondBoundaryOfMultiColumnIndex(
      IEnumerable<KeyValuePair<int, Expression>> indexKeyValues, ComparisonOperation operation,
      IndexInfo indexInfo, AdvancedComparer<Entire<Tuple>> comparer)
    {
      Expression firstEndpoint;
      Expression secondEndpoint;
      switch (operation) {
        case ComparisonOperation.GreaterThan:
          firstEndpoint = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, false);
          secondEndpoint = BuildInfiniteEntire(true);
          break;
        case ComparisonOperation.LessThan:
          firstEndpoint = BuildInfiniteEntire(false);
          secondEndpoint = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, true);
          break;
        default:
          throw Exceptions.InvalidArgument(operation, "operation");
      }
      return BuildConstructor(firstEndpoint, secondEndpoint, operation, null, comparer);
    }

    private static bool IsEqualityComparison(TupleExpressionInfo originTuple)
    {
      return originTuple.Comparison.Operation==ComparisonOperation.Equal
        || originTuple.Comparison.Operation==ComparisonOperation.NotEqual
        || originTuple.Comparison.Operation==ComparisonOperation.LikeStartsWith
        || originTuple.Comparison.Operation==ComparisonOperation.NotLikeStartsWith
        || originTuple.Comparison.Operation==ComparisonOperation.LikeEndsWith
        || originTuple.Comparison.Operation==ComparisonOperation.NotLikeEndsWith;
    }

    private static ComparisonOperation ReverseWithIgnoringOfEuqality(ComparisonOperation operation)
    {
      switch (operation) {
        case ComparisonOperation.GreaterThan:
        case ComparisonOperation.GreaterThanOrEqual:
          return ComparisonOperation.LessThan;
        case ComparisonOperation.LessThan:
        case ComparisonOperation.LessThanOrEqual:
          return ComparisonOperation.GreaterThan;
        default:
          throw Exceptions.InvalidArgument(operation, "operation");
      }
    }

    private static bool CanBuildNonFullRangeSet(ComparisonOperation comparisonOperation)
    {
      return comparisonOperation!=ComparisonOperation.LikeEndsWith
        && comparisonOperation!=ComparisonOperation.NotLikeEndsWith;
    }

    private static RangeSetInfo BuildConstructor(Expression firstEndpoint, Expression secondEndpoint,
      ComparisonOperation comparisonOperation, TupleExpressionInfo origin,
      AdvancedComparer<Entire<Tuple>> comparer)
    {
      NewExpression rangeConstruction = Expression.New(rangeContructor, firstEndpoint, secondEndpoint);
      RangeSetInfo result = CreateNotFullExpression(
        Expression.New(rangeSetConstructor, rangeConstruction,
          Expression.Constant(comparer)),
        origin);
      if (comparisonOperation == ComparisonOperation.NotEqual
        || comparisonOperation == ComparisonOperation.NotLikeStartsWith)
        return BuildInvert(result);
      return result;
    }

    private static void CreateRangeEndpoints(out Expression first, out Expression second,
      IEnumerable<KeyValuePair<int, Expression>> indexKeyValues,
      ComparisonOperation comparisonType, IndexInfo indexInfo)
    {
      if (comparisonType == ComparisonOperation.Equal || comparisonType == ComparisonOperation.NotEqual) {
        first = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, false);
        second = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, true);
        return;
      }
      if (comparisonType == ComparisonOperation.LessThan) {
        first = BuildInfiniteEntire(false);
        second = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, false);
        return;
      }
      if (comparisonType == ComparisonOperation.LessThanOrEqual) {
        first = BuildInfiniteEntire(false);
        second = BuildEntireConstructor(indexKeyValues, indexInfo);
        return;
      }
      if (comparisonType == ComparisonOperation.GreaterThan) {
        first = BuildShiftedEntireConstructor(indexKeyValues, indexInfo, true);
        second = BuildInfiniteEntire(true);
        return;
      }
      if (comparisonType == ComparisonOperation.GreaterThanOrEqual) {
        first = BuildEntireConstructor(indexKeyValues, indexInfo);
        second = BuildInfiniteEntire(true);
        return;
      }
      if (comparisonType == ComparisonOperation.LikeStartsWith
        || comparisonType == ComparisonOperation.NotLikeStartsWith)
      {
        first = BuildEntireConstructor(indexKeyValues, indexInfo);
        second = BuildSecondValueForLikeStartsWith(indexKeyValues, indexInfo);
        return;
      }
      throw Exceptions.InvalidArgument(comparisonType, "comparisonType");
    }

    private static Expression BuildSecondValueForLikeStartsWith(
      IEnumerable<KeyValuePair<int, Expression>> indexKeyValues, IndexInfo indexInfo)
    {
      if (indexKeyValues.Any(pair => pair.Value.Type != typeof(string)))
        throw new ArgumentException(String.Format(Strings.ExTypeOfExpressionReturnValueIsNotX,
          typeof (string)));
      var nearestKeyValues = indexKeyValues.Select(
        pair => new KeyValuePair<int, Expression>(pair.Key,
          Expression.Call(null, concatMethod, pair.Value,
            Expression.Constant(
              indexInfo.Columns[pair.Key].CultureInfo==null
              ? Comparison.WellKnown.OrdinalMaxChar : Comparison.WellKnown.CultureSensitiveMaxChar))));
      return BuildEntireConstructor(nearestKeyValues, indexInfo);
    }

    private static NewExpression BuildEntireConstructor(
      IEnumerable<KeyValuePair<int, Expression>> indexKeyValues,
      IndexInfo indexInfo)
    {
      Expression tupleCreation = BuildTupleCreation(indexKeyValues, indexInfo);
      return Expression.New(entireConstructor, tupleCreation);
    }

    private static NewExpression BuildShiftedEntireConstructor(
      IEnumerable<KeyValuePair<int, Expression>> indexKeyValues,
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

    private static Expression BuildTupleCreation(IEnumerable<KeyValuePair<int, Expression>> indexKeyValues,
      IndexInfo indexInfo)
    {
      var tupleDescriptor = indexInfo.KeyTupleDescriptor;
      var count = indexKeyValues.Last().Key + 1;
      if (count < tupleDescriptor.Count)
        tupleDescriptor = tupleDescriptor.Head(count);
      MethodCallExpression tupleCreation = Expression.Call(tupleCreateMethod,
        Expression.Constant(tupleDescriptor));
      Expression result = Expression.New(tupleUpdaterConstructor, tupleCreation);
      foreach (var indexKeyValue in indexKeyValues) {
        result = Expression.Call(result, setValueMethod, Expression.Constant(indexKeyValue.Key),
          Expression.Convert(indexKeyValue.Value, typeof (object)));
      }
      result = Expression.Property(result, wrappedTupleProperty);
      return result;
    }

    private static RangeSetInfo CreateNotFullExpression(Expression source, TupleExpressionInfo origin)
    {
      return new RangeSetInfo(source, origin, false);
    }
    #endregion

    // Constructors

    static RangeSetExpressionBuilder()
    {
      tupleCreateMethod = typeof (Tuple).GetMethod("Create", new[] {typeof (TupleDescriptor)});
      tupleUpdaterConstructor = typeof (TupleUpdater).GetConstructor(new[] {typeof (Tuple)});
      setValueMethod = typeof(TupleUpdater).GetMethod("SetValue");
      wrappedTupleProperty = typeof (TupleUpdater).GetProperty("Tuple");
      entireConstructor = typeof (Entire<Tuple>).GetConstructor(new[] {typeof (Tuple)});
      shiftedEntireConstutor = typeof (Entire<Tuple>)
        .GetConstructor(new[] {typeof (Tuple), typeof (Direction)});
      infiniteEntireConstructor = typeof (Entire<Tuple>).GetConstructor(new[] {typeof (InfinityType)});
      rangeContructor = typeof (Range<Entire<Tuple>>)
        .GetConstructor(new[] {typeof (Entire<Tuple>), typeof (Entire<Tuple>)});
      rangeSetConstructor = typeof (RangeSet<Entire<Tuple>>)
        .GetConstructor(new[] {typeof (Range<Entire<Tuple>>), typeof (AdvancedComparer<Entire<Tuple>>)});
      intersectMethod = typeof (RangeSet<Entire<Tuple>>)
        .GetMethod("Intersect", new[] {typeof (RangeSet<Entire<Tuple>>)});
      uniteMethod = typeof (RangeSet<Entire<Tuple>>)
        .GetMethod("Unite", new[] {typeof (RangeSet<Entire<Tuple>>)});
      invertMethod = typeof(RangeSet<Entire<Tuple>>).GetMethod("Invert");
      fullOrEmptyMethod = typeof (RangeSet<Entire<Tuple>>).GetMethod("CreateFullOrEmpty");
      concatMethod = typeof (string).GetMethod("Concat", new[] {typeof (string), typeof (string)});
    }
  }
}