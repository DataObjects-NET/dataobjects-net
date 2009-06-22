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
using Xtensive.Storage.Rse;
using Xtensive.Core.Linq;
using System.Linq;

namespace Xtensive.Storage.Linq.Materialization
{
  internal static class MaterializationHelper
  {
    public static readonly MethodInfo MaterializeMethodInfo;
    public static readonly MethodInfo GetDefaultMethodInfo;
    public static readonly MethodInfo CompileItemMaterializerMethodInfo;
    public static readonly MethodInfo IsNullMethodInfo;
    public static readonly MethodInfo ThrowSequenceExceptionMethodInfo;

    public static int[] GetColumnMap(int targetLength, Pair<int>[] columns)
    {
      var columnMap = new int[targetLength];
      for (int i = 0; i < columnMap.Length; i++)
        columnMap[i] = MapTransform.NoMapping;

      for (int i = 0; i < columns.Length; i++) {
        var targetIndex = columns[i].First;
        var sourceIndex = columns[i].Second;
        columnMap[targetIndex] = sourceIndex;
      }
      return columnMap;
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
        result &= tuple.IsNull(column);
      }
      return result;
    }

    public static object ThrowSequenceException()
    {
      throw new InvalidOperationException("Sequence contains no elements.");
    }

    public static IEnumerable<TResult> Materialize<TResult>(RecordSet rs, MaterializationContext context, Func<Tuple, ItemMaterializationContext, TResult> itemMaterializer, IDictionary<Parameter<Tuple>, Tuple> tupleParameterBindings)
    {
      ParameterContext ctx;
      var session = Session.Demand();
      using (session.OpenTransaction(true)) {
        using (new ParameterContext().Activate()) {
          ctx = ParameterContext.Current;
          foreach (var tupleParameterBinding in tupleParameterBindings)
            tupleParameterBinding.Key.Value = tupleParameterBinding.Value;
        }
        ParameterScope scope = null;
        var batched = rs.Select(tuple => itemMaterializer.Invoke(tuple, new ItemMaterializationContext(context, session))).Batch(2)
          .ApplyBeforeAndAfter(() => scope = ctx.Activate(), () => scope.DisposeSafely());
        foreach (var batch in batched)
          foreach (var result in batch)
            yield return result;
      }
    }

    public static Func<Tuple, ItemMaterializationContext, TResult> CompileItemMaterializer<TResult>(Expression<Func<Tuple, ItemMaterializationContext, TResult>> itemMaterializerLambda)
    {
      return itemMaterializerLambda.CachingCompile();
    }

// ReSharper restore UnusedMember.Global


    // Initializer

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
      ThrowSequenceExceptionMethodInfo = typeof(MaterializationHelper)
        .GetMethod("ThrowSequenceException", BindingFlags.Public | BindingFlags.Static);
    }
  }
}