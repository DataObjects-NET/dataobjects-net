// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Resources;
using Xtensive.Core.Linq;
using System.Linq;

namespace Xtensive.Storage.Linq.Materialization
{
  internal static class MaterializationHelper
  {
    public readonly static int BatchFastFirstCount = 0;
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
        scope.DisposeSafely();
        while (materializationQueue.Count > 0) {
          var materializeSelf = materializationQueue.Dequeue();
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
      var result = true;
      for (int i = 0; i < columns.Length; i++) {
        var column = columns[i];
        result &= tuple.GetFieldState(column).IsNull();
      }
      return result;
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
    /// <param name="itemMaterializer">The item materializer.</param>
    /// <param name="tupleParameterBindings">The tuple parameter bindings.</param>
    /// <returns></returns>
    public static IEnumerable<TResult> Materialize<TResult>(IEnumerable<Tuple> dataSource, MaterializationContext context, Func<Tuple, ItemMaterializationContext, TResult> itemMaterializer, Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings)
    {
      var parameterContext = new ParameterContext();
      using (parameterContext.Activate())
        foreach (var tupleParameterBinding in tupleParameterBindings)
          tupleParameterBinding.Key.Value = tupleParameterBinding.Value;
      var session = Session.Demand();
      var materializedSequence = dataSource
        .Select(tuple => itemMaterializer.Invoke(tuple, new ItemMaterializationContext(context, session)));
      return context.MaterializationQueue == null 
        ? BatchMaterialize(materializedSequence, context, parameterContext, session) 
        : SubqueryMaterialize(materializedSequence, parameterContext);
    }

    private static IEnumerable<TResult> BatchMaterialize<TResult>(IEnumerable<TResult> materializedSequence, MaterializationContext context, ParameterContext parameterContext, Session session)
    {
      var materializationQueue = new Queue<Action>();
      var batchActivator = new BatchActivator(materializationQueue, parameterContext);
      context.MaterializationQueue = materializationQueue;
      var batchSequence = materializedSequence
        .Batch(BatchFastFirstCount, BatchMinSize, BatchMaxSize)
        .ApplyBeforeAndAfter(batchActivator.Activate, batchActivator.Deactivate)
        .ToTransactional();
      foreach (var batch in batchSequence)
        foreach (var result in batch)
          yield return result;
    }

    private static IEnumerable<TResult> SubqueryMaterialize<TResult>(IEnumerable<TResult> materializedSequence, ParameterContext parameterContext)
    {
      ParameterScope scope = null;
      var batchSequence = materializedSequence
        .Batch(BatchFastFirstCount, BatchMinSize, BatchMaxSize)
        .ApplyBeforeAndAfter(
          () => scope = parameterContext.Activate(),
          () => scope.DisposeSafely())
        .ToTransactional();
      foreach (var batch in batchSequence)
        foreach (var result in batch)
          yield return result;
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
        entitySet.Owner.Type, 
        new FieldDescriptorCollection(
          new PrefetchFieldDescriptor(entitySet.Field, WellKnown.EntitySetPreloadCount)));
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