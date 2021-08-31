// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Rse;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Xtensive.Tuples.Transform;
using EnumerationContext = Xtensive.Orm.Providers.EnumerationContext;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  internal static class MaterializationHelper
  {
    public static readonly MethodInfo MaterializeMethodInfo;
    public static readonly MethodInfo GetDefaultMethodInfo;
    public static readonly MethodInfo CreateItemMaterializerMethodInfo;
    public static readonly MethodInfo CreateNullableItemMaterializerMethodInfo;
    public static readonly MethodInfo IsNullMethodInfo;
    public static readonly MethodInfo ThrowEmptySequenceExceptionMethodInfo;
    public static readonly MethodInfo PrefetchEntitySetMethodInfo;

    public static int[] CreateSingleSourceMap(int targetLength, Pair<int>[] remappedColumns)
    {
      var map = new int[targetLength];
      Array.Fill(map, MapTransform.NoMapping);

      for (var i = 0; i < remappedColumns.Length; i++) {
        var remappedColumn = remappedColumns[i];
        var targetIndex = remappedColumn.First;
        var sourceIndex = remappedColumn.Second;
        map[targetIndex] = sourceIndex;
      }

      return map;
    }

    public static T GetDefault<T>() => default;

    public static bool IsNull(Tuple tuple, int[] columns) =>
      columns.All(column => tuple.GetFieldState(column).IsNull());

    public static object ThrowEmptySequenceException() =>
      throw new InvalidOperationException(Strings.ExSequenceContainsNoElements);

    /// <summary>
    /// Wraps <see cref="RecordSetReader"/> by adding <see cref="Tuple"/> to <typeparamref name="TResult"/>
    /// conversion for individual records.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="recordSetReader">The reader over raw data source.</param>
    /// <param name="context">The context.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <param name="itemMaterializer">The materialization delegate performing
    /// <see cref="Tuple"/> instance to instance of <typeparamref name="TResult"/> type conversion.</param>
    public static object Materialize<TResult>(
      RecordSetReader recordSetReader,
      MaterializationContext context,
      ParameterContext parameterContext,
      IItemMaterializer<TResult> itemMaterializer)
    {
      var enumerationContext = (EnumerationContext) recordSetReader.Context;
      if (enumerationContext!=null) {
        enumerationContext.MaterializationContext = context;
      }

      return new MaterializingReader<TResult>(recordSetReader, context, parameterContext, itemMaterializer);
    }

    public static IItemMaterializer<TResult> CreateItemMaterializer<TResult>(
      Expression<Func<Tuple, ItemMaterializationContext, TResult>> itemMaterializerLambda, AggregateType? aggregateType)
    {
      var materializationDelegate = itemMaterializerLambda.CachingCompile();
      return aggregateType.HasValue
        ? new AggregateResultMaterializer<TResult>(materializationDelegate, aggregateType.Value)
        : new ItemMaterializer<TResult>(materializationDelegate);
    }

    public static IItemMaterializer<TResult?> CreateNullableItemMaterializer<TResult>(
      Expression<Func<Tuple, ItemMaterializationContext, TResult?>> itemMaterializerLambda, AggregateType? aggregateType)
      where TResult : struct
    {
      var materializationDelegate = itemMaterializerLambda.CachingCompile();
      return aggregateType.HasValue
        ? new NullableAggregateResultMaterializer<TResult>(materializationDelegate, aggregateType.Value)
        : new ItemMaterializer<TResult?>(materializationDelegate);
    }

    public static TEntitySet PrefetechEntitySet<TEntitySet>(TEntitySet entitySet, ItemMaterializationContext context)
      where TEntitySet : EntitySetBase
    {
      context.Session.Handler.Prefetch(
        entitySet.Owner.Key, 
        entitySet.Owner.TypeInfo, 
        new List<PrefetchFieldDescriptor>{new PrefetchFieldDescriptor(entitySet.Field, WellKnown.EntitySetPreloadCount)});
      return entitySet;
    }

    // Type initializer

    static MaterializationHelper()
    {
      MaterializeMethodInfo = typeof (MaterializationHelper)
        .GetMethod(nameof(Materialize), BindingFlags.Public | BindingFlags.Static);
      CreateItemMaterializerMethodInfo = typeof (MaterializationHelper)
        .GetMethod(nameof(CreateItemMaterializer), BindingFlags.Public | BindingFlags.Static);
      CreateNullableItemMaterializerMethodInfo = typeof (MaterializationHelper)
        .GetMethod(nameof(CreateNullableItemMaterializer), BindingFlags.Public | BindingFlags.Static);
      GetDefaultMethodInfo = typeof(MaterializationHelper)
        .GetMethod(nameof(GetDefault), BindingFlags.Public | BindingFlags.Static);
      IsNullMethodInfo = typeof(MaterializationHelper)
        .GetMethod(nameof(IsNull), BindingFlags.Public | BindingFlags.Static);
      ThrowEmptySequenceExceptionMethodInfo = typeof(MaterializationHelper)
        .GetMethod(nameof(ThrowEmptySequenceException), BindingFlags.Public | BindingFlags.Static);
      PrefetchEntitySetMethodInfo = typeof(MaterializationHelper)
        .GetMethod(nameof(PrefetechEntitySet), BindingFlags.Public | BindingFlags.Static);
    }
  }
}