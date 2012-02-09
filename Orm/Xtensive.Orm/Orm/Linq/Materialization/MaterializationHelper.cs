// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Parameters;
using Xtensive.Tuples;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Orm.Internals.Prefetch;

using System.Linq;

namespace Xtensive.Orm.Linq.Materialization
{
  internal static class MaterializationHelper
  {
    public readonly static int BatchFastFirstCount = 2;
    public readonly static int BatchMinSize = 16;
    public readonly static int BatchMaxSize = 1024;

    public static readonly MethodInfo MaterializeMethodInfo;
    public static readonly MethodInfo GetDefaultMethodInfo;
    public static readonly MethodInfo CompileItemMaterializerMethodInfo;
    public static readonly MethodInfo IsNullMethodInfo;
    public static readonly MethodInfo ThrowEmptySequenceExceptionMethodInfo;
    public static readonly MethodInfo PrefetchEntitySetMethodInfo;

    #region Nested type: BatchActivator

    class BatchActivator
    {
      private readonly Queue<Action> materializationQueue;
      private readonly ParameterContext parameterContext;
      private ParameterScope scope;

      public void Activate()
      {
        scope = parameterContext.Activate();
      }

      public void Deactivate()
      {
        try { 
          while (materializationQueue.Count > 0){
            var materializeSelf = materializationQueue.Dequeue();
            materializeSelf.Invoke();
          }
        }
        finally {
          scope.DisposeSafely();
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
      for (int i = 0; i < map.Length; i++)
        map[i] = MapTransform.NoMapping;

      for (int i = 0; i < remappedColumns.Length; i++) {
        var targetIndex = remappedColumns[i].First;
        var sourceIndex = remappedColumns[i].Second;
        map[targetIndex] = sourceIndex;
      }
      return map;
    }

// ReSharper disable UnusedMember.Global

    public static T GetDefault<T>()
    {
      return default(T);
    }

    public static bool IsNull(Tuple tuple, int[] columns)
    {
      return columns.Aggregate(true, (current, column) => current & tuple.GetFieldState(column).IsNull());
    }

    public static object ThrowEmptySequenceException()
    {
      throw new InvalidOperationException(Strings.ExSequenceContainsNoElements);
    }

    /// <summary>
    /// Materializes the specified data source.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="dataSource">The data source.</param>
    /// <param name="context">The context.</param>
    /// <param name="parameterContext">The parameter context.</param>
    /// <param name="itemMaterializer">The item materializer.</param>
    /// <param name="tupleParameterBindings">The tuple parameter bindings.</param>
    public static IEnumerable<TResult> Materialize<TResult>(IEnumerable<Tuple> dataSource, MaterializationContext context, ParameterContext parameterContext, Func<Tuple, ItemMaterializationContext, TResult> itemMaterializer, Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings)
    {
      using (parameterContext.Activate())
        foreach (var tupleParameterBinding in tupleParameterBindings)
          tupleParameterBinding.Key.Value = tupleParameterBinding.Value;
      var session = context.Session;
      var recordSet = dataSource as RecordSet;
      if (recordSet != null) {
        var enumerationContext = (EnumerationContext)recordSet.Context;
        enumerationContext.MaterializationContext = context;
      }
      var materializedSequence = dataSource
        .Select(tuple => itemMaterializer.Invoke(tuple, new ItemMaterializationContext(context, session)));
      return context.MaterializationQueue == null 
        ? BatchMaterialize(materializedSequence, context, parameterContext) 
        : SubqueryMaterialize(materializedSequence, parameterContext);
    }

    private static IEnumerable<TResult> BatchMaterialize<TResult>(IEnumerable<TResult> materializedSequence, MaterializationContext context, ParameterContext parameterContext)
    {
      var materializationQueue = new Queue<Action>();
      var batchActivator = new BatchActivator(materializationQueue, parameterContext);
      context.MaterializationQueue = materializationQueue;
      var batchSequence = materializedSequence
        .Batch(BatchFastFirstCount, BatchMinSize, BatchMaxSize)
        .ApplyBeforeAndAfter(batchActivator.Activate, batchActivator.Deactivate);
      return batchSequence.SelectMany(batch => batch);
    }

    private static IEnumerable<TResult> SubqueryMaterialize<TResult>(IEnumerable<TResult> materializedSequence, ParameterContext parameterContext)
    {
      ParameterScope scope = null;
      var batchSequence = materializedSequence
        .Batch(BatchFastFirstCount, BatchMinSize, BatchMaxSize)
        .ApplyBeforeAndAfter(
          () => scope = parameterContext.Activate(),
          () => scope.DisposeSafely());
      return batchSequence.SelectMany(batch => batch);
    }

    public static Func<Tuple, ItemMaterializationContext, TResult> CompileItemMaterializer<TResult>(Expression<Func<Tuple, ItemMaterializationContext, TResult>> itemMaterializerLambda)
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

// ReSharper restore UnusedMember.Global


    // Type initializer

    static MaterializationHelper()
    {
      MaterializeMethodInfo = typeof (MaterializationHelper)
        .GetMethod("Materialize", BindingFlags.Public | BindingFlags.Static);
      CompileItemMaterializerMethodInfo = typeof (MaterializationHelper)
        .GetMethod("CompileItemMaterializer", BindingFlags.Public | BindingFlags.Static);
      GetDefaultMethodInfo = typeof(MaterializationHelper)
        .GetMethod("GetDefault", BindingFlags.Public | BindingFlags.Static);
      IsNullMethodInfo = typeof(MaterializationHelper)
        .GetMethod("IsNull", BindingFlags.Public | BindingFlags.Static);
      ThrowEmptySequenceExceptionMethodInfo = typeof(MaterializationHelper)
        .GetMethod("ThrowEmptySequenceException", BindingFlags.Public | BindingFlags.Static);
      PrefetchEntitySetMethodInfo = typeof(MaterializationHelper)
        .GetMethod("PrefetechEntitySet", BindingFlags.Public | BindingFlags.Static);
    }
  }
}