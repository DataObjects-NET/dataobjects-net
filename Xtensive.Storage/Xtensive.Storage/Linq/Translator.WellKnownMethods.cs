// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.24

using System.Linq;
using System.Reflection;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq
{
  internal partial class Translator
  {
    public static class WellKnownMethods
    {
      // Tuple
      public static readonly MethodInfo TupleGenericAccessor;
      public static readonly MethodInfo TupleNonGenericAccessor;

      // Key
      public static readonly PropertyInfo KeyValue;
      public static readonly MethodInfo KeyResolve;
      public static readonly MethodInfo KeyResolveOfT;
      public static readonly MethodInfo KeyCreate;

      // KeyExtensions
      public static readonly MethodInfo KeyTryResolve;
      public static readonly MethodInfo KeyTryResolveOfT;

      // Enumerable
      public static readonly MethodInfo EnumerableSelect;

      // Queryable
      public static readonly MethodInfo QueryableDefaultIfEmpty;
      public static readonly MethodInfo QueryableTake;
      public static readonly MethodInfo QueryableCount;
      public static readonly MethodInfo QueryableCountWithPredicate;
      public static readonly MethodInfo QueryableLongCount;
      public static readonly MethodInfo QueryableWhere;
      public static readonly MethodInfo QueryableContains;

      // IEntity
      public static readonly PropertyInfo IEntityKey;

      // Parameter<Tuple>
      public static readonly PropertyInfo ParameterOfTupleValue;

      // SegmentTransform
      public static readonly MethodInfo SegmentTransformApply;

      // Record
      public static readonly MethodInfo RecordKey;

      // RecordSet
      public static readonly MethodInfo RecordSetParse;

      private static MethodInfo GetQueryableMethod(string name, int numberOfGenericArgument, int numberOfArguments)
      {
        return typeof(Queryable).GetMethod(name,
          BindingFlags.Public | BindingFlags.Static,
          new string[numberOfGenericArgument],
          new object[numberOfArguments]);
      }

      static WellKnownMethods()
      {
        // Tuple
        foreach (var method in typeof(Tuple).GetMethods().Where(mi => mi.Name == WellKnown.Tuple.GetValueOrDefault))
          if (method.IsGenericMethod)
            TupleGenericAccessor = method;
          else
            TupleNonGenericAccessor = method;

        // Key
        KeyValue = typeof(Key).GetProperty("Value");
        KeyResolve = typeof(Key).GetMethods()
          .Where(mi => mi.Name == "Resolve" && mi.IsGenericMethodDefinition == false && mi.GetParameters().Length == 0)
          .Single();
        KeyResolveOfT = typeof (Key).GetMethod("Resolve", BindingFlags.Public | BindingFlags.Instance, new[] {"T"}, new object[0]);
        KeyCreate = typeof(Key).GetMethod("Create", new[] { typeof(TypeInfo), typeof(Tuple), typeof(bool) });

        // KeyExtensions
        KeyTryResolve = typeof(KeyExtensions).GetMethod("TryResolve", BindingFlags.Public | BindingFlags.Static, new string[0], new object[1]);
        KeyTryResolveOfT = typeof (KeyExtensions).GetMethod("TryResolve", BindingFlags.Public | BindingFlags.Static, new string[1], new object[1]);

        // Enumerable
        EnumerableSelect = typeof(Enumerable).GetMethods().Where(m => m.Name == "Select").First();

        // Queryable
        QueryableDefaultIfEmpty = GetQueryableMethod(WellKnown.Queryable.DefaultIfEmpty, 1, 1);
        QueryableCount = GetQueryableMethod(WellKnown.Queryable.Count, 1, 1);
        QueryableCountWithPredicate = GetQueryableMethod(WellKnown.Queryable.Count, 1, 2);
        QueryableTake = GetQueryableMethod(WellKnown.Queryable.Take, 1, 2);
        QueryableContains = GetQueryableMethod(WellKnown.Queryable.Contains, 1, 2);
        QueryableLongCount = GetQueryableMethod(WellKnown.Queryable.LongCount, 1, 1);

        QueryableWhere = typeof(Queryable).GetMethods().Where(methodInfo =>
        {
          ParameterInfo[] parameterInfos = methodInfo.GetParameters();
          return methodInfo.Name == WellKnown.Queryable.Where
            && methodInfo.IsGenericMethod
              && parameterInfos.Length == 2
                && parameterInfos[1].ParameterType.IsGenericType
                  && parameterInfos[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2;
        }).First();

        // IEntity
        IEntityKey = typeof(IEntity).GetProperty("Key");

        // Parameter<Tuple>
        ParameterOfTupleValue = typeof(Parameter<Core.Tuples.Tuple>).GetProperty("Value", typeof(Tuple));

        // SegmentTransform
        SegmentTransformApply = typeof(SegmentTransform).GetMethod("Apply", new[] { typeof(TupleTransformType), typeof(Tuple) });

        // Record
        RecordKey = typeof(Record).GetProperty("Item", typeof(Key), new[] { typeof(int) }).GetGetMethod();

        // RecordSet
        RecordSetParse = typeof(RecordSetExtensions).GetMethod("Parse");
      }
    }
  }
}
