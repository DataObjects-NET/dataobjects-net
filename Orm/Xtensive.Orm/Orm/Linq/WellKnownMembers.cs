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
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Reflection;
using Xtensive.Tuples;

namespace Xtensive.Orm.Linq
{
  internal static class WellKnownMembers
  {
    public static class Query
    {
      public static readonly MethodInfo All;
      public static readonly MethodInfo FreeTextString;
      public static readonly MethodInfo FreeTextStringTopNByRank;
      public static readonly MethodInfo FreeTextExpression;
      public static readonly MethodInfo FreeTextExpressionTopNByRank;
      public static readonly MethodInfo SingleKey;
      public static readonly MethodInfo SingleArray;
      public static readonly MethodInfo SingleOrDefaultKey;
      public static readonly MethodInfo SingleOrDefaultArray;

      static Query()
      {
#pragma warning disable 612,618
        All = typeof(Orm.Query).GetMethod("All", ArrayUtils<Type>.EmptyArray);
        FreeTextString = typeof (Orm.Query).GetMethods().Where(m => m.Name=="FreeText")
          .Single(ft => ft.GetParameters().Length==1 && ft.GetParameterTypes()[0]==typeof (string));
        FreeTextStringTopNByRank = typeof (Orm.Query).GetMethods()
          .Where(m => m.Name=="FreeText")
          .Single(ft => ft.GetParameters().Length==2 && ft.GetParameterTypes()[0]==typeof (string) && ft.GetParameterTypes()[1]==typeof(int));
        FreeTextExpression = typeof (Orm.Query).GetMethods().Where(m => m.Name=="FreeText")
          .Single(ft =>ft.GetParameters().Length==1 && ft.GetParameterTypes()[0]==typeof (Expression<Func<string>>));
        FreeTextExpressionTopNByRank = typeof (Orm.Query).GetMethods()
          .Where(m => m.Name=="FreeText")
          .Single(ft => ft.GetParameters().Length==2 && ft.GetParameterTypes()[0]==typeof (Expression<Func<string>>) && ft.GetParameterTypes()[1]==typeof (int));
        var singleMethods = typeof (Orm.Query).GetMethods().Where(m => m.Name=="Single" && m.IsGenericMethod);
        SingleKey = singleMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (Orm.Key));
        SingleArray = singleMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (object[]));
        var singleOrDefaultMethods = typeof (Orm.Query).GetMethods().Where(m => m.Name=="SingleOrDefault" && m.IsGenericMethod);
        SingleOrDefaultKey = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (Orm.Key));
        SingleOrDefaultArray = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (object[]));
#pragma warning restore 612,618
      }
    }

    public static class QueryEndpoint
    {
      public static readonly MethodInfo All;
      public static readonly MethodInfo FreeTextString;
      public static readonly MethodInfo FreeTextStringTopNByRank;
      public static readonly MethodInfo FreeTextExpression;
      public static readonly MethodInfo FreeTextExpressionTopNByRank;
      public static readonly MethodInfo SingleKey;
      public static readonly MethodInfo SingleArray;
      public static readonly MethodInfo SingleOrDefaultKey;
      public static readonly MethodInfo SingleOrDefaultArray;
      public static readonly MethodInfo Items;

      static QueryEndpoint()
      {
#pragma warning disable 612,618
        All = typeof(Orm.QueryEndpoint).GetMethod("All", ArrayUtils<Type>.EmptyArray);
        FreeTextString = typeof(Orm.QueryEndpoint).GetMethods()
          .Where(m => m.Name=="FreeText")
          .Single(ft => ft.GetParameters().Length==1 && ft.GetParameterTypes()[0]==typeof(string));
        FreeTextStringTopNByRank = typeof(Orm.QueryEndpoint).GetMethods()
          .Where(m => m.Name=="FreeText")
          .Single(ft => ft.GetParameters().Length==2 && ft.GetParameterTypes()[0]==typeof(string) && ft.GetParameterTypes()[1]==typeof(int));
        FreeTextExpression = typeof(Orm.QueryEndpoint).GetMethods()
          .Where(m => m.Name=="FreeText")
          .Single(ft => ft.GetParameters().Length==1 && ft.GetParameterTypes()[0]==typeof(Expression<Func<string>>));
        FreeTextExpressionTopNByRank = typeof(Orm.QueryEndpoint).GetMethods()
          .Where(m => m.Name=="FreeText")
          .Single(ft => ft.GetParameters().Length==2 && ft.GetParameterTypes()[0]==typeof(Expression<Func<string>>) && ft.GetParameterTypes()[1]==typeof(int));
        var singleMethods = typeof(Orm.QueryEndpoint).GetMethods().Where(m => m.Name == "Single" && m.IsGenericMethod);
        SingleKey = singleMethods.Single(ft => ft.GetParameterTypes()[0] == typeof(Orm.Key));
        SingleArray = singleMethods.Single(ft => ft.GetParameterTypes()[0] == typeof(object[]));
        var singleOrDefaultMethods = typeof(Orm.QueryEndpoint).GetMethods().Where(m => m.Name == "SingleOrDefault" && m.IsGenericMethod);
        SingleOrDefaultKey = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0] == typeof(Orm.Key));
        SingleOrDefaultArray = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0] == typeof(object[]));
        Items = typeof (Orm.QueryEndpoint).GetMethod("Items");
#pragma warning restore 612,618
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
        GenericAccessor = typeof (Xtensive.Tuples.Tuple)
          .GetMethods()
          .Where(mi => mi.Name==Xtensive.Reflection.WellKnown.Tuple.GetValueOrDefault && mi.IsGenericMethod)
          .Single();
        Create = typeof (Xtensive.Tuples.Tuple)
          .GetMethods()
          .Where(mi => mi.Name==Xtensive.Reflection.WellKnown.Tuple.Create
            && !mi.IsGenericMethod
              && mi.GetParameters().Count()==1
                && mi.GetParameters()[0].ParameterType==typeof (TupleDescriptor))
          .Single();
        Descriptor = typeof (Xtensive.Tuples.Tuple).GetProperty(Xtensive.Reflection.WellKnown.Tuple.Descriptor);
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
        Value = typeof (Orm.Key).GetProperty("Value");
        Create = typeof (Orm.Key).GetMethod(
          "Create",
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
          null,
          new[] {typeof (Domain), typeof (string), typeof (TypeInfo), typeof (TypeReferenceAccuracy), typeof (Tuples.Tuple)}, null);
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
        Select = typeof (System.Linq.Enumerable).GetMethods().First(m => m.Name=="Select");
        First = typeof (System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name==Xtensive.Reflection.WellKnown.Queryable.First && m.GetParameters().Length==1);
        OfTuple = typeof (IEnumerable<>).MakeGenericType(typeof (Xtensive.Tuples.Tuple));
        DefaultIfEmpty = typeof (System.Linq.Enumerable).GetMethods().First(m => m.Name=="DefaultIfEmpty");
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
      public static readonly MethodInfo Select;

      // Querable extensions
      public static readonly MethodInfo ExtensionCount;
      public static readonly MethodInfo ExtensionLeftJoin;
      public static readonly MethodInfo ExtensionLock;
      public static readonly MethodInfo ExtensionTake;
      public static readonly MethodInfo ExtensionSkip;
      public static readonly MethodInfo ExtensionElementAt;
      public static readonly MethodInfo ExtensionElementAtOrDefault;

      static Queryable()
      {
        // Queryable
        AsQueryable = GetQueryableMethod(Reflection.WellKnown.Queryable.AsQueryable, 1, 1);
        DefaultIfEmpty = GetQueryableMethod(Reflection.WellKnown.Queryable.DefaultIfEmpty, 1, 1);
        Count = GetQueryableMethod(Reflection.WellKnown.Queryable.Count, 1, 1);
        CountWithPredicate = GetQueryableMethod(Reflection.WellKnown.Queryable.Count, 1, 2);
        Take = GetQueryableMethod(Reflection.WellKnown.Queryable.Take, 1, 2);
        Contains = GetQueryableMethod(Reflection.WellKnown.Queryable.Contains, 1, 2);
        LongCount = GetQueryableMethod(Reflection.WellKnown.Queryable.LongCount, 1, 1);
        Cast = GetQueryableMethod(Reflection.WellKnown.Queryable.Cast, 1, 1);
        Where = typeof (System.Linq.Queryable)
          .GetMethods()
          .Where(m => {
            var parameters = m.GetParameters();
            return m.Name==Reflection.WellKnown.Queryable.Where
              && m.IsGenericMethod
              && parameters.Length==2
              && parameters[1].ParameterType.IsGenericType
              && parameters[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length==2;
          })
          .First();

        Select = typeof (System.Linq.Queryable)
          .GetMethods()
          .Where(m => {
            var parameters = m.GetParameters();
            return m.Name==Reflection.WellKnown.Queryable.Select
              && m.IsGenericMethod
              && parameters.Length==2
              && parameters[1].ParameterType.IsGenericType
              && parameters[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length==2;
          })
          .Single();


        // Querable extensions
        ExtensionCount = GetQueryableExtensionsMethod("Count", 0, 1);
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

    // Session
    public static readonly PropertyInfo SessionNodeId;

    private static MethodInfo GetMethod(Type type, string name, int numberOfGenericArgument, int numberOfArguments)
    {
      var method = type.GetMethod(name,
        BindingFlags.Public | BindingFlags.Static,
        new string[numberOfGenericArgument],
        new object[numberOfArguments]);
      if (method==null)
        throw new InvalidOperationException(String.Format(Strings.ExMethodXNotFound, name));
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
      ParameterOfTupleValue = typeof (Parameter<Tuples.Tuple>).GetProperty("Value", typeof (Tuples.Tuple));

      // Parameter
      ParameterValue = typeof (Parameter).GetProperty("Value");

      // Record
      RecordKey = typeof (Record).GetMethods()
        .Single(methodInfo => methodInfo.Name=="GetKey" && methodInfo.GetParameters().Length==1);

      // RecordSet
      RecordSetParse = typeof (RecordSetExtensions).GetMethod("Parse");

      // Structure
      CreateStructure = typeof (Internals.Activator)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Single(methodInfo =>
          methodInfo.Name=="CreateStructure"
          && methodInfo.GetParameters().Length==3
          && methodInfo.GetParameters()[0].ParameterType == typeof(Session));

      // EntitySet
      CreateEntitySet = typeof (Internals.Activator)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Single(methodInfo =>
          methodInfo.Name=="CreateEntitySet"
          && methodInfo.GetParameters().Length==2);

      // Session
      SessionNodeId = typeof (Session).GetProperty("StorageNodeId");
    }
  }
}