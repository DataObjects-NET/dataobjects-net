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
using Xtensive.Orm.Rse.Providers;
using Xtensive.Tuples;
using Xtensive.Tuples.Transform;
using EnumerationContext = Xtensive.Orm.Providers.EnumerationContext;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  internal static class MaterializationHelper
  {
    private const int BatchFastFirstCount = 2;
    private const int BatchMinSize = 16;
    private const int BatchMaxSize = 1024;

    public static readonly MethodInfo MaterializeMethodInfo;
    public static readonly MethodInfo GetDefaultMethodInfo;
    public static readonly MethodInfo CompileItemMaterializerMethodInfo;
    public static readonly MethodInfo IsNullMethodInfo;
    public static readonly MethodInfo ThrowEmptySequenceExceptionMethodInfo;
    public static readonly MethodInfo PrefetchEntitySetMethodInfo;

    #region Nested type: BatchActivator

    private class BatchActivator
    {
      private readonly Queue<Action> materializationQueue;
      private readonly ParameterContext parameterContext;

      public void Activate() {}

      public void Deactivate()
      {
        while (materializationQueue.Count > 0) {
          var materializeSelf = materializationQueue.Dequeue();
          // TODO: Probably we need to pass parameter context into materializeSelf
          materializeSelf.Invoke();
        }
      }

      public BatchActivator(Queue<Action> materializationQueue, ParameterContext parameterContext)
      {
        this.materializationQueue = materializationQueue;
        this.parameterContext = parameterContext;
      }
    }

    #endregion

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
    /// Materializes the specified data source.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="dataSource">The data source.</param>
    /// <param name="context">The context.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <param name="itemMaterializer">The item materializer.</param>
    /// <param name="tupleParameterBindings">The tuple parameter bindings.</param>
    public static object Materialize<TResult>(
      ExecutableProvider.RecordSet recordSet,
      MaterializationContext context,
      ParameterContext parameterContext,
      Func<Tuple, ItemMaterializationContext, TResult> itemMaterializer,
      Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings)
    {
      foreach (var (parameter, tuple) in tupleParameterBindings) {
        parameterContext.SetValue(parameter, tuple);
      }

      var enumerationContext = (EnumerationContext) recordSet.Context;
      if (enumerationContext!=null) {
        enumerationContext.MaterializationContext = context;
      }

      if (context.IsAsync) {
        return EnumerateDataSourceAsync((IAsyncEnumerable<Tuple>) dataSource, context, parameterContext,
          itemMaterializer);
      }

      return EnumerateDataSource((IEnumerable<Tuple>) dataSource, context, parameterContext, itemMaterializer);
    }

    private static object EnumerateDataSource<TResult>(IEnumerable<Tuple> dataSource, MaterializationContext context,
      ParameterContext parameterContext, Func<Tuple, ItemMaterializationContext, TResult> itemMaterializer)
    {
      var materializedSequence = dataSource
        .Select(tuple => itemMaterializer.Invoke(tuple, new ItemMaterializationContext(context, parameterContext)));
      return context.MaterializationQueue == null
        ? BatchMaterialize(materializedSequence, context, parameterContext)
        : SubqueryMaterialize(materializedSequence, parameterContext);
    }

    private static async IAsyncEnumerable<TResult> EnumerateDataSourceAsync<TResult>(
      IAsyncEnumerable<Tuple> dataSource,
      MaterializationContext context,
      ParameterContext parameterContext,
      Func<Tuple, ItemMaterializationContext, TResult> itemMaterializer)
    {
      Queue<Action> materializationQueue = null;
      context.MaterializationQueue ??= materializationQueue = new Queue<Action>();

      await foreach (var tuple in dataSource) {
        yield return itemMaterializer.Invoke(tuple, new ItemMaterializationContext(context, parameterContext));
      }

      if (materializationQueue == null) {
        yield break;
      }

      while (materializationQueue.Count > 0) {
        var materializeSelf = materializationQueue.Dequeue();
        // TODO: Probably we need to pass ParameterContext into materializeSelf
        materializeSelf.Invoke();
      }
    }

    private static IEnumerable<TResult> BatchMaterialize<TResult>(IEnumerable<TResult> materializedSequence,
      MaterializationContext context, ParameterContext parameterContext)
    {
      var materializationQueue = new Queue<Action>();
      var batchActivator = new BatchActivator(materializationQueue, parameterContext);
      context.MaterializationQueue = materializationQueue;
      var batchSequence = materializedSequence
        .Batch(BatchFastFirstCount, BatchMinSize, BatchMaxSize)
        .ApplyBeforeAndAfter(batchActivator.Activate, batchActivator.Deactivate);
      return batchSequence.SelectMany(batch => batch);
    }

    private static IEnumerable<TResult> SubqueryMaterialize<TResult>(
      IEnumerable<TResult> materializedSequence, ParameterContext parameterContext)
    {
      var batchSequence = materializedSequence
        .Batch(BatchFastFirstCount, BatchMinSize, BatchMaxSize);
      return batchSequence.SelectMany(batch => batch);
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