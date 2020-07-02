// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    public static readonly MethodInfo CompileItemMaterializerMethodInfo;
    public static readonly MethodInfo IsNullMethodInfo;
    public static readonly MethodInfo ThrowEmptySequenceExceptionMethodInfo;
    public static readonly MethodInfo PrefetchEntitySetMethodInfo;

    public static int[] CreateSingleSourceMap(int targetLength, Pair<int>[] remappedColumns)
    {
      var map = new int[targetLength];
      for (var i = 0; i < map.Length; i++) {
        map[i] = MapTransform.NoMapping;
      }

      for (var i = 0; i < remappedColumns.Length; i++) {
        var targetIndex = remappedColumns[i].First;
        var sourceIndex = remappedColumns[i].Second;
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
      Func<Tuple, ItemMaterializationContext, TResult> itemMaterializer)
    {
      var enumerationContext = (EnumerationContext) recordSetReader.Context;
      if (enumerationContext!=null) {
        enumerationContext.MaterializationContext = context;
      }

      return new MaterializingReader<TResult>(recordSetReader, context, parameterContext, itemMaterializer);
    }

    public static Func<Tuple, ItemMaterializationContext, TResult> CompileItemMaterializer<TResult>(
      Expression<Func<Tuple, ItemMaterializationContext, TResult>> itemMaterializerLambda)
    {
      return itemMaterializerLambda.CachingCompile();
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
      CompileItemMaterializerMethodInfo = typeof (MaterializationHelper)
        .GetMethod(nameof(CompileItemMaterializer), BindingFlags.Public | BindingFlags.Static);
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