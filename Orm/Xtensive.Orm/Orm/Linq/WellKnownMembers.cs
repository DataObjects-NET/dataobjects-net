// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.03.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Nodes;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Rse;
using Xtensive.Reflection;
using Xtensive.Tuples;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Linq
{
  internal static partial class WellKnownMembers
  {
    public static class Query
    {
      public static readonly MethodInfo All;
      public static readonly MethodInfo FreeTextString;
      public static readonly MethodInfo FreeTextStringTopNByRank;
      public static readonly MethodInfo FreeTextExpression;
      public static readonly MethodInfo FreeTextExpressionTopNByRank;
      public static readonly MethodInfo ContainsTableExpr;
      public static readonly MethodInfo ContainsTableExprWithColumns;
      public static readonly MethodInfo ContainsTableExprTopNByRank;
      public static readonly MethodInfo ContainsTableExprWithColumnsTopNByRank;
      public static readonly MethodInfo SingleKey;
      public static readonly MethodInfo SingleArray;
      public static readonly MethodInfo SingleOrDefaultKey;
      public static readonly MethodInfo SingleOrDefaultArray;

      static Query()
      {
#pragma warning disable 612,618
        All = typeof(Orm.Query).GetMethod(nameof(Orm.Query.All), Array.Empty<Type>());

        var freetextMethods = typeof(Orm.Query).GetMethods().Where(m => m.Name==nameof(Orm.Query.FreeText)).ToArray();
        FreeTextString = freetextMethods
          .Single(ft => ft.GetParameters().Length==1 && ft.GetParameterTypes()[0]==WellKnownTypes.String);
        FreeTextStringTopNByRank = freetextMethods
          .Single(ft => ft.GetParameters().Length==2 && ft.GetParameterTypes()[0]==WellKnownTypes.String && ft.GetParameterTypes()[1]==WellKnownTypes.Int32);
        FreeTextExpression = freetextMethods
          .Single(ft => ft.GetParameters().Length==1 && ft.GetParameterTypes()[0]==typeof(Expression<Func<string>>));
        FreeTextExpressionTopNByRank = freetextMethods
          .Single(ft => ft.GetParameters().Length==2 && ft.GetParameterTypes()[0]==typeof(Expression<Func<string>>) && ft.GetParameterTypes()[1]==WellKnownTypes.Int32);
        var containsTableMethods = typeof (Orm.Query).GetMethods()
          .Where(m => m.Name==nameof(Orm.Query.ContainsTable))
          .Select(m => (Method: m, ParameterTypes: m.GetParameterTypes())).ToArray();
        ContainsTableExpr = containsTableMethods
          .Single(g => g.ParameterTypes.Length==1 && g.ParameterTypes[0]==typeof (Expression<Func<ConditionEndpoint, IOperand>>)).Method;
        ContainsTableExprWithColumns = containsTableMethods
          .Single(g => g.ParameterTypes.Length==2 &&
                       g.ParameterTypes[0]==typeof (Expression<Func<ConditionEndpoint, IOperand>>) &&
                       g.ParameterTypes[1].IsArray).Method;
        ContainsTableExprTopNByRank = containsTableMethods
          .Single(g => g.ParameterTypes.Length==2 && g.ParameterTypes[0]==typeof (Expression<Func<ConditionEndpoint, IOperand>>) && g.ParameterTypes[1]==WellKnownTypes.Int32).Method;
        ContainsTableExprWithColumnsTopNByRank = containsTableMethods
          .Single(g => g.ParameterTypes.Length==3 &&
                       g.ParameterTypes[0]==typeof (Expression<Func<ConditionEndpoint, IOperand>>) &&
                       g.ParameterTypes[1].IsArray &&
                       g.ParameterTypes[2]==WellKnownTypes.Int32).Method;
        var singleMethods = typeof (Orm.Query).GetMethods().Where(m => m.Name==nameof(Orm.Query.Single) && m.IsGenericMethod);
        SingleKey = singleMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (Orm.Key));
        SingleArray = singleMethods.Single(ft => ft.GetParameterTypes()[0]==WellKnownTypes.ObjectArray);
        var singleOrDefaultMethods = typeof (Orm.Query).GetMethods().Where(m => m.Name==nameof(Orm.Query.SingleOrDefault) && m.IsGenericMethod);
        SingleOrDefaultKey = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0]==typeof (Orm.Key));
        SingleOrDefaultArray = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0]==WellKnownTypes.ObjectArray);

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
      public static readonly MethodInfo ContainsTableExpr;
      public static readonly MethodInfo ContainsTableExprWithColumns;
      public static readonly MethodInfo ContainsTableExprTopNByRank;
      public static readonly MethodInfo ContainsTableExprWithColumnsTopNByRank;
      public static readonly MethodInfo SingleKey;
      public static readonly MethodInfo SingleArray;
      public static readonly MethodInfo SingleOrDefaultKey;
      public static readonly MethodInfo SingleOrDefaultArray;
      public static readonly MethodInfo Items;

      static QueryEndpoint()
      {
#pragma warning disable 612,618
        All = typeof(Orm.QueryEndpoint).GetMethod(nameof(Orm.QueryEndpoint.All), Array.Empty<Type>());

        var freetextMethods = typeof(Orm.QueryEndpoint).GetMethods().Where(m => m.Name==nameof(Orm.QueryEndpoint.FreeText)).ToArray();
        FreeTextString = freetextMethods
          .Single(ft => ft.GetParameters().Length==1 && ft.GetParameterTypes()[0]==WellKnownTypes.String);
        FreeTextStringTopNByRank = freetextMethods
          .Single(ft => ft.GetParameters().Length==2 && ft.GetParameterTypes()[0]==WellKnownTypes.String && ft.GetParameterTypes()[1]==WellKnownTypes.Int32);
        FreeTextExpression = freetextMethods
          .Single(ft => ft.GetParameters().Length==1 && ft.GetParameterTypes()[0]==typeof(Expression<Func<string>>));
        FreeTextExpressionTopNByRank = freetextMethods
          .Single(ft => ft.GetParameters().Length==2 && ft.GetParameterTypes()[0]==typeof(Expression<Func<string>>) && ft.GetParameterTypes()[1]==WellKnownTypes.Int32);
        var containsTableMethods = typeof (Orm.QueryEndpoint).GetMethods()
          .Where(m => m.Name==nameof(Orm.QueryEndpoint.ContainsTable))
          .Select(m=> (Method: m, ParameterTypes: m.GetParameterTypes())).ToArray();
        ContainsTableExpr = containsTableMethods
          .Single(g => g.ParameterTypes.Length==1 && g.ParameterTypes[0]==typeof (Expression<Func<ConditionEndpoint, IOperand>>)).Method;
        ContainsTableExprWithColumns = containsTableMethods
          .Single(g => g.ParameterTypes.Length==2 &&
                       g.ParameterTypes[0]==typeof (Expression<Func<ConditionEndpoint, IOperand>>) &&
                       g.ParameterTypes[1].IsArray).Method;
        ContainsTableExprTopNByRank = containsTableMethods
          .Single(g => g.ParameterTypes.Length==2 && g.ParameterTypes[0]==typeof (Expression<Func<ConditionEndpoint, IOperand>>) && g.ParameterTypes[1]==WellKnownTypes.Int32).Method;
        ContainsTableExprWithColumnsTopNByRank = containsTableMethods
          .Single(g => g.ParameterTypes.Length==3 &&
                       g.ParameterTypes[0]==typeof (Expression<Func<ConditionEndpoint, IOperand>>) &&
                       g.ParameterTypes[1].IsArray &&
                       g.ParameterTypes[2]==WellKnownTypes.Int32).Method;
        var singleMethods = typeof(Orm.QueryEndpoint).GetMethods().Where(m => m.Name==nameof(Orm.QueryEndpoint.Single) && m.IsGenericMethod);
        SingleKey = singleMethods.Single(ft => ft.GetParameterTypes()[0]==typeof(Orm.Key));
        SingleArray = singleMethods.Single(ft => ft.GetParameterTypes()[0]==WellKnownTypes.ObjectArray);
        var singleOrDefaultMethods = typeof(Orm.QueryEndpoint).GetMethods().Where(m => m.Name==nameof(Orm.QueryEndpoint.SingleOrDefault) && m.IsGenericMethod);
        SingleOrDefaultKey = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0]==typeof(Orm.Key));
        SingleOrDefaultArray = singleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0]==WellKnownTypes.ObjectArray);
        Items = typeof (Orm.QueryEndpoint).GetMethod(nameof(Orm.QueryEndpoint.Items));
#pragma warning restore 612,618
      }
    }

    public static class QueryProvider
    {
      public static readonly MethodInfo ExecuteScalar;
      public static readonly MethodInfo ExecuteSequence;

      static QueryProvider()
      {
        var methodInfos = typeof(Linq.QueryProvider).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var methodInfo in methodInfos) {
          switch (methodInfo.Name) {
            case nameof(Linq.QueryProvider.ExecuteScalar):
              ExecuteScalar = methodInfo;
              break;
            case nameof(Linq.QueryProvider.ExecuteSequence):
              ExecuteSequence = methodInfo;
              break;
          }
        }
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
        Value = typeof (Orm.Key).GetProperty(nameof(Orm.Key.Value));
        Create = typeof (Orm.Key).GetMethod(
          nameof(Orm.Key.Create),
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
          null,
          new[] {typeof (Domain), WellKnownTypes.String, typeof (TypeInfo), typeof (TypeReferenceAccuracy), typeof (Tuples.Tuple)}, null);
      }
    }

    public static class Enumerable
    {
      // Enumerable
      public static readonly MethodInfo Select;
      public static readonly MethodInfo First;
      public static readonly MethodInfo FirstOrDefault;
      public static readonly MethodInfo Single;
      public static readonly MethodInfo SingleOrDefault;
      public static readonly Type OfTuple;
      public static readonly MethodInfo DefaultIfEmpty;
      public static readonly MethodInfo Contains;
      public static readonly MethodInfo Cast;

      static Enumerable()
      {
        // Enumerable
        Select = typeof (System.Linq.Enumerable).GetMethods().First(m => m.Name==nameof(System.Linq.Enumerable.Select));
        First = typeof (System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name==nameof(System.Linq.Enumerable.First) && m.GetParameters().Length==1);
        FirstOrDefault = typeof (System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name==nameof(System.Linq.Enumerable.FirstOrDefault) && m.GetParameters().Length==1);
        Single = typeof (System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name==nameof(System.Linq.Enumerable.Single) && m.GetParameters().Length==1);
        SingleOrDefault = typeof (System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name==nameof(System.Linq.Enumerable.SingleOrDefault) && m.GetParameters().Length==1);
        OfTuple = WellKnownInterfaces.EnumerableOfT.MakeGenericType(typeof (Xtensive.Tuples.Tuple));
        DefaultIfEmpty = typeof (System.Linq.Enumerable).GetMethods().First(m => m.Name==nameof(System.Linq.Enumerable.DefaultIfEmpty));
        Contains = GetMethod(typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.Contains), 1, 2);
        Cast = GetMethod(typeof (System.Linq.Enumerable), nameof(System.Linq.Enumerable.Cast), 1, 1);
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
        var collectionExType = typeof (CollectionExtensionsEx);
        ExtensionContainsAll = GetMethod(collectionExType, nameof(CollectionExtensionsEx.ContainsAll), 1, 2);
        ExtensionContainsAny = GetMethod(collectionExType, nameof(CollectionExtensionsEx.ContainsAny), 1, 2);
        ExtensionContainsNone = GetMethod(collectionExType, nameof(CollectionExtensionsEx.ContainsNone), 1, 2);
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
      IEntityKey = WellKnownOrmInterfaces.Entity.GetProperty(WellKnown.KeyFieldName);
      TypeId = WellKnownOrmInterfaces.Entity.GetProperty(WellKnown.TypeIdFieldName);

      // ApplyParameter
      ApplyParameterValue = WellKnownOrmTypes.ApplyParameter.GetProperty("Value");

      // Parameter<Tuple>
      ParameterOfTupleValue = WellKnownOrmTypes.ParameterOfTuple.GetProperty("Value", typeof (Tuples.Tuple));

      // Parameter
      ParameterValue = WellKnownOrmTypes.Parameter.GetProperty("Value");

      // Record
      RecordKey = typeof (Record).GetMethods()
        .Single(methodInfo => methodInfo.Name=="GetKey" && methodInfo.GetParameters().Length==1);

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