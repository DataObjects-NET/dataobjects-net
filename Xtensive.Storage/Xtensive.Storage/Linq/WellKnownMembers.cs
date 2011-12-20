// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal static class WellKnownMembers
  {
    public static class Query
    {
      public static readonly MethodInfo All;
      public static readonly MethodInfo FreeTextString;
      public static readonly MethodInfo FreeTextExpression;
      public static readonly MethodInfo SingleKey;
      public static readonly MethodInfo SingleArray;
      public static readonly MethodInfo SingleOrDefaultKey;
      public static readonly MethodInfo SingleOrDefaultArray;

      static Query()
      {
        All = typeof(Storage.Query).GetMethod("All", ArrayUtils<Type>.EmptyArray);
        FreeTextString = typeof (Storage.Query).GetMethods().Where(m => m.Name=="FreeText").Single(ft => ft.GetParameterTypes()[0]==typeof (string));
        FreeTextExpression = typeof (Storage.Query).GetMethods().Where(m => m.Name=="FreeText").Single(ft => ft.GetParameterTypes()[0]==typeof (Expression<Func<string>>));
        var singleMethods = typeof (Storage.Query).GetMethods().Where(m => m.Name=="Single" && m.IsGenericMethod);
        SingleKey = singleMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (Storage.Key));
        SingleArray = singleMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (object[]));
        var singleOrDefaultMethods = typeof (Storage.Query).GetMethods().Where(m => m.Name=="SingleOrDefault" && m.IsGenericMethod);
        SingleOrDefaultKey = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (Storage.Key));
        SingleOrDefaultArray = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (object[]));
      }
    }

    public static class QueryProvider
    {
      public static readonly MethodInfo Execute;

      static QueryProvider()
      {
        Execute = typeof(Linq.QueryProvider)
          .GetMethods()
          .Where(mi => mi.Name == "Execute" && mi.IsGenericMethod)
          .Single();
      }
    }

    // Tuple
    public static class Tuple
    {
      public static readonly MethodInfo GenericAccessor;
      public static readonly MethodInfo Create;
      public static readonly PropertyInfo Descriptor;

      static Tuple()
      {
        // Tuple
        GenericAccessor = typeof (Core.Tuples.Tuple)
          .GetMethods()
          .Where(mi => mi.Name==Core.Reflection.WellKnown.Tuple.GetValueOrDefault && mi.IsGenericMethod)
          .Single();
        Create = typeof (Core.Tuples.Tuple)
          .GetMethods()
          .Where(mi => mi.Name==Core.Reflection.WellKnown.Tuple.Create
            && !mi.IsGenericMethod
              && mi.GetParameters().Count()==1
                && mi.GetParameters()[0].ParameterType==typeof (TupleDescriptor))
          .Single();
        Descriptor = typeof (Core.Tuples.Tuple).GetProperty(Core.Reflection.WellKnown.Tuple.Descriptor);
      }
    }

    public static class Key
    {
      // Key
      public static readonly PropertyInfo Value;
      public static readonly MethodInfo Create;

      static Key()
      {
        // Key
        Value = typeof (Storage.Key).GetProperty("Value");
        Create = typeof (Storage.Key).GetMethod(
          "Create",
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
          null,
           new[] {typeof(Domain), typeof (TypeInfo), typeof(TypeReferenceAccuracy), typeof (Core.Tuples.Tuple)}, null);

      }
    }

    public static class Enumerable
    {
      // Enumerable
      public static readonly MethodInfo Select;
      public static readonly MethodInfo First;
      public static readonly Type OfTuple;
      public static readonly MethodInfo DefaultIfEmpty;
      public static readonly MethodInfo Contains;
      public static readonly MethodInfo Cast;

      static Enumerable()
      {
        // Enumerable
        Select = typeof (System.Linq.Enumerable).GetMethods().Where(m => m.Name=="Select").First();
        First = typeof (System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name==Core.Reflection.WellKnown.Queryable.First && m.GetParameters().Length==1);
        OfTuple = typeof (IEnumerable<>).MakeGenericType(typeof (Core.Tuples.Tuple));
        DefaultIfEmpty = typeof (System.Linq.Enumerable).GetMethods().Where(m => m.Name=="DefaultIfEmpty").First();
        Contains = GetMethod(typeof(System.Linq.Enumerable), "Contains", 1, 2);
        Cast = GetMethod(typeof (System.Linq.Enumerable), "Cast", 1, 1);
      }
    }

    public static class Collection
    {
      // Collection extensions
      public static readonly MethodInfo ExtensionContainsAll;
      public static readonly MethodInfo ExtensionContainsAny;
      public static readonly MethodInfo ExtensionContainsNone;

      static Collection()
      {
        ExtensionContainsAll = GetMethod(typeof (CollectionExtensions), "ContainsAll", 1, 2);
        ExtensionContainsAny = GetMethod(typeof (CollectionExtensions), "ContainsAny", 1, 2);
        ExtensionContainsNone = GetMethod(typeof (CollectionExtensions), "ContainsNone", 1, 2);
      }
    }

    public static class Queryable
    {
      // Queryable
      public static readonly MethodInfo AsQueryable;
      public static readonly MethodInfo DefaultIfEmpty;
      public static readonly MethodInfo Take;
      public static readonly MethodInfo Count;
      public static readonly MethodInfo CountWithPredicate;
      public static readonly MethodInfo LongCount;
      public static readonly MethodInfo Where;
      public static readonly MethodInfo Contains;
      public static readonly MethodInfo Cast;

      // Querable extensions
      public static readonly MethodInfo ExtensionLeftJoin;
      public static readonly MethodInfo ExtensionLock;
      public static readonly MethodInfo ExtensionTake;
      public static readonly MethodInfo ExtensionSkip;
      public static readonly MethodInfo ExtensionElementAt;
      public static readonly MethodInfo ExtensionElementAtOrDefault;

      static Queryable()
      {
        // Queryable
        AsQueryable = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.AsQueryable, 1, 1);
        DefaultIfEmpty = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.DefaultIfEmpty, 1, 1);
        Count = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.Count, 1, 1);
        CountWithPredicate = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.Count, 1, 2);
        Take = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.Take, 1, 2);
        Contains = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.Contains, 1, 2);
        LongCount = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.LongCount, 1, 1);
        Cast = GetQueryableMethod(Core.Reflection.WellKnown.Queryable.Cast, 1, 1);
        Where = typeof (System.Linq.Queryable).GetMethods().Where(methodInfo => {
          ParameterInfo[] parameterInfos = methodInfo.GetParameters();
          return methodInfo.Name==Core.Reflection.WellKnown.Queryable.Where
            && methodInfo.IsGenericMethod
              && parameterInfos.Length==2
                && parameterInfos[1].ParameterType.IsGenericType
                  && parameterInfos[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length==2;
        }).First();

        // Querable extensions
        ExtensionLeftJoin = GetQueryableExtensionsMethod("LeftJoin", 4, 5);
        ExtensionLock = GetQueryableExtensionsMethod("Lock", 1, 3);
        ExtensionTake = GetQueryableExtensionsMethod("Take", 1, 2);
        ExtensionSkip = GetQueryableExtensionsMethod("Skip", 1, 2);
        ExtensionElementAt = GetQueryableExtensionsMethod("ElementAt", 1, 2);
        ExtensionElementAtOrDefault = GetQueryableExtensionsMethod("ElementAtOrDefault", 1, 2);
      }
    }

    // IEntity
    public static readonly PropertyInfo IEntityKey;
    public static readonly PropertyInfo TypeId;

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
      return GetMethod(typeof (System.Linq.Queryable), name, numberOfGenericArgument, numberOfArguments);
    }

    private static MethodInfo GetQueryableExtensionsMethod(string name, int numberOfGenericArgument, int numberOfArguments)
    {
      return GetMethod(typeof (QueryableExtensions), name, numberOfGenericArgument, numberOfArguments);
    }

    static WellKnownMembers()
    {
      // IEntity
      IEntityKey = typeof (IEntity).GetProperty(WellKnown.KeyFieldName);
      TypeId = typeof(IEntity).GetProperty(WellKnown.TypeIdFieldName);

      // ApplyParameter
      ApplyParameterValue = typeof (ApplyParameter).GetProperty("Value");

      // Parameter<Tuple>
      ParameterOfTupleValue = typeof (Parameter<Core.Tuples.Tuple>).GetProperty("Value", typeof (Core.Tuples.Tuple));

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
      CreateEntitySet = typeof (Internals.Activator).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
        .Where(methodInfo => methodInfo.Name=="CreateEntitySet" && methodInfo.GetParameters().Length==2)
        .Single();
    }
  }
}