// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal static class WellKnownMembers
  {
    // Tuple
    public static readonly MethodInfo TupleGenericAccessor;

    // Key
    public static readonly PropertyInfo KeyValue;
    public static readonly MethodInfo KeyResolve;
    public static readonly MethodInfo KeyResolveOfT;
    public static readonly MethodInfo KeyCreate;

    // Key extensions
    public static readonly MethodInfo KeyTryResolve;
    public static readonly MethodInfo KeyTryResolveOfT;

    // Enumerable
    public static readonly MethodInfo EnumerableSelect;
    public static readonly MethodInfo EnumerableFirst;
    public static readonly Type EnumerableOfTuple;

    // Queryable
    public static readonly MethodInfo QueryableDefaultIfEmpty;
    public static readonly MethodInfo QueryableTake;
    public static readonly MethodInfo QueryableCount;
    public static readonly MethodInfo QueryableCountWithPredicate;
    public static readonly MethodInfo QueryableLongCount;
    public static readonly MethodInfo QueryableWhere;
    public static readonly MethodInfo QueryableContains;

    // Querable extensions
    public static readonly MethodInfo QueryableExpandEntity;
    public static readonly MethodInfo QueryableExpandSubquery;
    public static readonly MethodInfo QueryableExcludeFields;
    public static readonly MethodInfo QueryableIncludeFields;


    // IEntity
    public static readonly PropertyInfo IEntityKey;

    // ApplyParameter
    public static readonly PropertyInfo ApplyParameterValue;

    // Parameter<Tuple>
    public static readonly PropertyInfo ParameterOfTupleValue;

    // Parameter
    public static readonly PropertyInfo ParameterValue;

    // SegmentTransform
    public static readonly MethodInfo SegmentTransformApply;

    // Record
    public static readonly MethodInfo RecordKey;

    // RecordSet
    public static readonly MethodInfo RecordSetParse;

    /// <exception cref="InvalidOperationException">Method not found.</exception>
    private static MethodInfo GetMethod(Type type, string name, int numberOfGenericArgument, int numberOfArguments)
    {
      var method = type.GetMethod(name,
        BindingFlags.Public | BindingFlags.Static,
        new string[numberOfGenericArgument],
        new object[numberOfArguments]);
      if (method==null)
        throw new InvalidOperationException(String.Format(Resources.Strings.ExMethodXNotFound, name));
      return method;
    }

    private static MethodInfo GetQueryableMethod(string name, int numberOfGenericArgument, int numberOfArguments)
    {
      return GetMethod(typeof (Queryable), name, numberOfGenericArgument, numberOfArguments);
    }

    private static MethodInfo GetQueryableExtensionsMethod(string name, int numberOfGenericArgument, int numberOfArguments)
    {
      return GetMethod(typeof (QueryableExtensions), name, numberOfGenericArgument, numberOfArguments);
    }

    static WellKnownMembers()
    {
      // Tuple
      TupleGenericAccessor = typeof (Tuple).GetMethods()
        .Where(mi => mi.Name==WellKnown.Tuple.GetValueOrDefault && mi.IsGenericMethod)
        .Single();

      // Key
      KeyValue = typeof (Key).GetProperty("Value");
      KeyResolve = typeof (Key).GetMethods()
        .Where(mi => mi.Name=="Resolve" && mi.IsGenericMethodDefinition==false && mi.GetParameters().Length==0)
        .Single();
      KeyResolveOfT = typeof (Key).GetMethod("Resolve", BindingFlags.Public | BindingFlags.Instance, new[] {"T"}, new object[0]);
      KeyCreate = typeof (Key).GetMethod("Create", new[] {typeof (TypeInfo), typeof (Tuple), typeof (bool)});

      // KeyExtensions
      KeyTryResolve = typeof (KeyExtensions).GetMethod("TryResolve", BindingFlags.Public | BindingFlags.Static, new string[0], new object[1]);
      KeyTryResolveOfT = typeof (KeyExtensions).GetMethod("TryResolve", BindingFlags.Public | BindingFlags.Static, new string[1], new object[1]);

      // Enumerable
      EnumerableSelect = typeof (Enumerable).GetMethods().Where(m => m.Name=="Select").First();
      EnumerableFirst = typeof (Enumerable)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(m => m.Name==WellKnown.Queryable.First && m.GetParameters().Length==1);
      EnumerableOfTuple = typeof (IEnumerable<>).MakeGenericType(typeof (Tuple));

      // Queryable
      QueryableDefaultIfEmpty = GetQueryableMethod(WellKnown.Queryable.DefaultIfEmpty, 1, 1);
      QueryableCount = GetQueryableMethod(WellKnown.Queryable.Count, 1, 1);
      QueryableCountWithPredicate = GetQueryableMethod(WellKnown.Queryable.Count, 1, 2);
      QueryableTake = GetQueryableMethod(WellKnown.Queryable.Take, 1, 2);
      QueryableContains = GetQueryableMethod(WellKnown.Queryable.Contains, 1, 2);
      QueryableLongCount = GetQueryableMethod(WellKnown.Queryable.LongCount, 1, 1);
      QueryableWhere = typeof (Queryable).GetMethods().Where(methodInfo => {
        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        return methodInfo.Name==WellKnown.Queryable.Where
          && methodInfo.IsGenericMethod
            && parameterInfos.Length==2
              && parameterInfos[1].ParameterType.IsGenericType
                && parameterInfos[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length==2;
      }).First();

      // Querable extensions
      QueryableExpandEntity = typeof (QueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(methodInfo => methodInfo.Name=="Expand"
          && methodInfo
            .GetParameterTypes()[1]
            .GetGenericArguments()[0]
            .GetGenericArguments()[1]==typeof (Entity))
        .First();
      QueryableExpandSubquery = typeof (QueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(methodInfo => methodInfo.Name=="Expand"
          && methodInfo
            .GetParameterTypes()[1]
            .GetGenericArguments()[0]
            .GetGenericArguments()[1]==typeof (IQueryable))
        .First();

      QueryableExcludeFields = GetQueryableExtensionsMethod("ExcludeFields", 2, 2);
      QueryableIncludeFields = GetQueryableExtensionsMethod("IncludeFields", 2, 2);

      // IEntity
      IEntityKey = typeof (IEntity).GetProperty(StorageWellKnown.Key);

      // ApplyParameter
      ApplyParameterValue = typeof (ApplyParameter).GetProperty("Value");

      // Parameter<Tuple>
      ParameterOfTupleValue = typeof (Parameter<Tuple>).GetProperty("Value", typeof (Tuple));

      // Parameter
      ParameterValue = typeof (Parameter).GetProperty("Value");

      // SegmentTransform
      SegmentTransformApply = typeof (SegmentTransform).GetMethod("Apply", new[] {typeof (TupleTransformType), typeof (Tuple)});

      // Record
      RecordKey = typeof (Record).GetProperty("Item", typeof (Key), new[] {typeof (int)}).GetGetMethod();

      // RecordSet
      RecordSetParse = typeof (RecordSetExtensions).GetMethod("Parse");
    }
  }
}