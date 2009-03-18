// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Internals;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimisation
{
  internal static class RangeSetExpressionsBuilder
  {
    private static readonly MethodInfo tupleDescriptorCreator;
    private static readonly MethodInfo tupleCreateMethod;
    private static readonly ConstructorInfo tupleUpdaterConstructor;
    private static readonly MethodInfo tupleUpdateMethod;
    private static readonly ConstructorInfo entireConstructor;
    private static readonly ConstructorInfo shiftedEntireConstutor;
    private static readonly ConstructorInfo infiniteEntireConstructor;
    private static readonly ConstructorInfo rangeContructor;
    private static readonly ConstructorInfo rangeSetConstructor;
    private static readonly MethodInfo intersectMethod;
    private static readonly MethodInfo uniteMethod;
    private static readonly MethodInfo invertMethod;

    public static NewExpression BuildConstructor(Expression indexKeyValue, int keyFieldIndex,
      ExpressionType comparisonType, Type[] indexedFieldTypes, IndexInfo indexInfo)
    {
      Expression firstEndpoint;
      Expression secondEndpoint;
      CreateRangeEndpoints(out firstEndpoint, out secondEndpoint, indexKeyValue, keyFieldIndex, comparisonType,
                           indexedFieldTypes, indexInfo);
      NewExpression rangeConstruction = Expression.New(rangeContructor, firstEndpoint, secondEndpoint);
      //TODO:A comparer from index must be passed here.
      return Expression.New(rangeSetConstructor, rangeConstruction,
                            Expression.Constant(AdvancedComparer<Entire<Tuple>>.Default));
    }

    public static NewExpression BuildFullRangeSetConstructor()
    {
      return Expression.New(rangeSetConstructor, Expression.Constant(Range<Entire<Tuple>>.Full));
    }

    public static MethodCallExpression BuildIntersect(Expression targetRangeSet, Expression otherRangeSet)
    {
      return Expression.Call(targetRangeSet, intersectMethod, otherRangeSet);
    }

    public static MethodCallExpression BuildUnite(Expression targetRangeSet, Expression otherRangeSet)
    {
      return Expression.Call(targetRangeSet, uniteMethod, otherRangeSet);
    }

    public static MethodCallExpression BuildInvert(Expression targetRangeSet)
    {
      return Expression.Call(targetRangeSet, invertMethod);
    }

    private static void CreateRangeEndpoints(out Expression first, out Expression second,
      Expression indexKeyValue, int fieldIndex, ExpressionType comparsionType, Type[] indexedFieldTypes,
      IndexInfo indexInfo)
    {
      if(comparsionType == ExpressionType.Equal) {
        first = BuildShiftedEntireConstructor(indexKeyValue, fieldIndex, indexedFieldTypes, indexInfo, false);
        second = BuildShiftedEntireConstructor(indexKeyValue, fieldIndex, indexedFieldTypes, indexInfo, true);
        return;
      }
      if(comparsionType == ExpressionType.LessThan) {
        first = BuildInfiniteEntire(false);
        second = BuildShiftedEntireConstructor(indexKeyValue, fieldIndex, indexedFieldTypes, indexInfo, false);
        return;
      }
      if (comparsionType == ExpressionType.LessThanOrEqual) {
        first = BuildInfiniteEntire(false);
        second = BuildEntireConstructor(indexKeyValue, fieldIndex, indexedFieldTypes, indexInfo);
        return;
      }
      if (comparsionType == ExpressionType.GreaterThan) {
        first = BuildShiftedEntireConstructor(indexKeyValue, fieldIndex, indexedFieldTypes, indexInfo, true);
        second = BuildInfiniteEntire(true);
        return;
      }
      if (comparsionType == ExpressionType.GreaterThanOrEqual) {
        first = BuildEntireConstructor(indexKeyValue, fieldIndex, indexedFieldTypes, indexInfo);
        second = BuildInfiniteEntire(true);
        return;
      }
      throw Exceptions.InvalidArgument(comparsionType, "comparsionType");
    }

    private static NewExpression BuildEntireConstructor(Expression tupleFieldValue, int fieldIndex,
      Type[] indexedFieldTypes, IndexInfo indexInfo)
    {
      MethodCallExpression tupleCreation = BuildTupleCreation(tupleFieldValue, fieldIndex, indexedFieldTypes,
        indexInfo);
      return Expression.New(entireConstructor, tupleCreation);
    }

    private static NewExpression BuildShiftedEntireConstructor(Expression tupleFieldValue,
      int fieldIndex, Type[] indexedFieldTypes, IndexInfo indexInfo, bool positiveShift)
    {
      MethodCallExpression tupleCreation = BuildTupleCreation(tupleFieldValue, fieldIndex, indexedFieldTypes,
        indexInfo);
      var shiftDirection = positiveShift ? Direction.Positive : Direction.Negative;
      return Expression.New(shiftedEntireConstutor, tupleCreation, Expression.Constant(shiftDirection));
    }

    private static NewExpression BuildInfiniteEntire(bool positiveInfinity)
    {
      var infinityType = positiveInfinity ? InfinityType.Positive : InfinityType.Negative;
      return Expression.New(infiniteEntireConstructor, Expression.Constant(infinityType));
    }

    private static MethodCallExpression BuildTupleCreation(Expression tupleFieldValue, int fieldIndex,
      Type[] indexedFieldTypes, IndexInfo indexInfo)
    {
      MethodCallExpression tupleCreation = Expression.Call(tupleCreateMethod,
        Expression.Constant(indexInfo.KeyTupleDescriptor));
      NewExpression tupleUpdaterConstruction = Expression.New(tupleUpdaterConstructor, tupleCreation);
      return Expression.Call(tupleUpdaterConstruction, tupleUpdateMethod,
                                                                Expression.Constant(fieldIndex),
                                                                Expression.Convert(tupleFieldValue,
                                                                                   typeof(object)));
    }

    static RangeSetExpressionsBuilder()
    {
      tupleDescriptorCreator = typeof(TupleDescriptor).GetMethod("Create", new[] { typeof(Type[]) });
      tupleCreateMethod = typeof (Tuple).GetMethod("Create", new[] {typeof (TupleDescriptor)});
      tupleUpdaterConstructor = typeof(TupleUpdater).GetConstructor(new[] { typeof(Tuple) });
      tupleUpdateMethod = typeof(TupleUpdater).GetMethod("UpdateField");
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
    }
  }
}