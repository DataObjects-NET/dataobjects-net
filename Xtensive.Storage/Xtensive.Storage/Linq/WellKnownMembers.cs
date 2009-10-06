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
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal static class WellKnownMembers
  {
    // Tuple
    public static readonly MethodInfo TupleGenericAccessor;
    public static readonly MethodInfo TupleCreate;
    public static readonly PropertyInfo TupleDescriptor;

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
    public static readonly MethodInfo EnumerableDefaultIfEmpty;

    // Queryable
    public static readonly MethodInfo QueryableAsQueryable;
    public static readonly MethodInfo QueryableDefaultIfEmpty;
    public static readonly MethodInfo QueryableTake;
    public static readonly MethodInfo QueryableCount;
    public static readonly MethodInfo QueryableCountWithPredicate;
    public static readonly MethodInfo QueryableLongCount;
    public static readonly MethodInfo QueryableWhere;
    public static readonly MethodInfo QueryableContains;

    // Querable extensions
    public static readonly MethodInfo QueryableJoinLeft;
    public static readonly MethodInfo QueryableLock;


    // IEntity
    public static readonly PropertyInfo IEntityKey;

    // ApplyParameter
    public static readonly PropertyInfo ApplyParameterValue;

    // Parameter<Tuple>
    public static readonly PropertyInfo ParameterOfTupleValue;

    // Parameter
    public static readonly PropertyInfo ParameterValue;

    // Record
    public static readonly MethodInfo RecordKey;

    // RecordSet
    public static readonly MethodInfo RecordSetParse;

    // Structure
    public static readonly MethodInfo CreateStructure;

    // EntitySet
    public static readonly MethodInfo CreateEntitySet;

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
      TupleGenericAccessor = typeof (Tuple)
        .GetMethods()
        .Where(mi => mi.Name==Core.Reflection.WellKnown.Tuple.GetValueOrDefault && mi.IsGenericMethod)
        .Single();
      TupleCreate = typeof (Tuple)
        .GetMethods()
        .Where(mi => mi.Name == Core.Reflection.WellKnown.Tuple.Create
          && !mi.IsGenericMethod
            && mi.GetParameters().Count()==1
              && mi.GetParameters()[0].ParameterType==typeof (TupleDescriptor))
        .Single();
      TupleDescriptor = typeof(Tuple).GetProperty(Core.Reflection.WellKnown.Tuple.Descriptor);

      // Key
      KeyValue = typeof (Key).GetProperty("Value");
//      KeyResolve = typeof (Query).GetMethods()
//        .Where(mi => mi.Name=="SingleOrDefault" && mi.IsGenericMethodDefinition==false && mi.GetParameters().Length==1)
//        .Single();
//      KeyResolveOfT = typeof (Query<>).GetMethod("SingleOrDefault", BindingFlags.Public | BindingFlags.Instance, new[] {"T"}, new object[0]);
      KeyCreate = typeof (Key).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static, null,
        new[] {typeof (TypeInfo), typeof (Tuple), typeof (bool)}, null);

      // KeyExtensions
//      KeyTryResolve = typeof (KeyExtensions).GetMethod("TryResolve", BindingFlags.Public | BindingFlags.Static, new string[0], new object[1]);
//      KeyTryResolveOfT = typeof (KeyExtensions).GetMethod("TryResolve", BindingFlags.Public | BindingFlags.Static, new string[1], new object[1]);

      // Enumerable
      EnumerableSelect = typeof (Enumerable).GetMethods().Where(m => m.Name=="Select").First();
      EnumerableFirst = typeof (Enumerable)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(m => m.Name==Core.Reflection.WellKnown.Queryable.First && m.GetParameters().Length==1);
      EnumerableOfTuple = typeof (IEnumerable<>).MakeGenericType(typeof (Tuple));
      EnumerableDefaultIfEmpty = typeof (Enumerable).GetMethods().Where(m => m.Name=="DefaultIfEmpty").First();
      

      // Queryable
      QueryableAsQueryable = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.AsQueryable, 1, 1);
      QueryableDefaultIfEmpty = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.DefaultIfEmpty, 1, 1);
      QueryableCount = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.Count, 1, 1);
      QueryableCountWithPredicate = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.Count, 1, 2);
      QueryableTake = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.Take, 1, 2);
      QueryableContains = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.Contains, 1, 2);
      QueryableLongCount = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.LongCount, 1, 1);
      QueryableWhere = typeof (Queryable).GetMethods().Where(methodInfo => {
        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        return methodInfo.Name==Core.Reflection.WellKnown.Queryable.Where
          && methodInfo.IsGenericMethod
            && parameterInfos.Length==2
              && parameterInfos[1].ParameterType.IsGenericType
                && parameterInfos[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length==2;
      }).First();

      // Querable extensions
      QueryableJoinLeft = GetQueryableExtensionsMethod("JoinLeft", 4, 5);
      QueryableLock = GetQueryableExtensionsMethod("Lock", 1, 3);

      // IEntity
      IEntityKey = typeof (IEntity).GetProperty(WellKnown.KeyFieldName);


      // ApplyParameter
      ApplyParameterValue = typeof (ApplyParameter).GetProperty("Value");

      // Parameter<Tuple>
      ParameterOfTupleValue = typeof (Parameter<Tuple>).GetProperty("Value", typeof (Tuple));

      // Parameter
      ParameterValue = typeof (Parameter).GetProperty("Value");

      // Record
      RecordKey = typeof (Record).GetMethods().Where(methodInfo => methodInfo.Name=="GetKey" && methodInfo.GetParameters().Length==1)
        .Single();

      // RecordSet
      RecordSetParse = typeof (RecordSetExtensions).GetMethod("Parse");

      // Structure
      CreateStructure = typeof (Internals.Activator).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
        .Where(methodInfo => methodInfo.Name=="CreateStructure" && methodInfo.GetParameters().Length==2)
        .Single();

      // EntitySet
      CreateEntitySet = typeof(Internals.Activator).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
        .Where(methodInfo => methodInfo.Name == "CreateEntitySet" && methodInfo.GetParameters().Length == 2)
        .Single();
    }
  }
}