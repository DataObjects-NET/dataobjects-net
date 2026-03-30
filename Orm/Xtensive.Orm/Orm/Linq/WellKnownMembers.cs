// Copyright (C) 2009-2021 Xtensive LLC.
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
#pragma warning disable 612,618
    public static class Query
    {
      private static readonly MethodInfo[] FreetextMethods = typeof(Orm.Query).GetMethods().Where(m => m.Name == nameof(Orm.Query.FreeText)).ToArray();      
      private static readonly (MethodInfo Method, Type[] ParameterTypes)[] containsTableMethods = typeof(Orm.Query).GetMethods()
          .Where(m => m.Name == nameof(Orm.Query.ContainsTable))
          .Select(m => (Method: m, ParameterTypes: m.GetParameterTypes())).ToArray();

      private static readonly MethodInfo[] SingleMethods = typeof(Orm.Query).GetMethods().Where(m => m.Name == nameof(Orm.Query.Single) && m.IsGenericMethod).ToArray();
      private static readonly MethodInfo[] SingleOrDefaultMethods = typeof(Orm.Query).GetMethods().Where(m => m.Name == nameof(Orm.Query.SingleOrDefault) && m.IsGenericMethod).ToArray();

      public static readonly MethodInfo All = typeof(Orm.Query).GetMethod(nameof(Orm.Query.All), Array.Empty<Type>());

      public static readonly MethodInfo FreeTextString = FreetextMethods
          .Single(ft => ft.GetParameters().Length == 1 && ft.GetParameterTypes()[0] == WellKnownTypes.String);

      public static readonly MethodInfo FreeTextStringTopNByRank = FreetextMethods
          .Single(ft => ft.GetParameters().Length == 2 && ft.GetParameterTypes()[0] == WellKnownTypes.String && ft.GetParameterTypes()[1] == WellKnownTypes.Int32);

      public static readonly MethodInfo FreeTextExpression = FreetextMethods
          .Single(ft => ft.GetParameters().Length == 1 && ft.GetParameterTypes()[0] == typeof(Expression<Func<string>>));

      public static readonly MethodInfo FreeTextExpressionTopNByRank = FreetextMethods
          .Single(ft => ft.GetParameters().Length == 2 && ft.GetParameterTypes()[0] == typeof(Expression<Func<string>>) && ft.GetParameterTypes()[1] == WellKnownTypes.Int32);

      public static readonly MethodInfo ContainsTableExpr = containsTableMethods
          .Single(g => g.ParameterTypes.Length == 1 && g.ParameterTypes[0] == typeof(Expression<Func<ConditionEndpoint, IOperand>>)).Method;

      public static readonly MethodInfo ContainsTableExprWithColumns = containsTableMethods
          .Single(g => g.ParameterTypes.Length == 2 &&
                       g.ParameterTypes[0] == typeof(Expression<Func<ConditionEndpoint, IOperand>>) &&
                       g.ParameterTypes[1].IsArray).Method;

      public static readonly MethodInfo ContainsTableExprTopNByRank = containsTableMethods
          .Single(g => g.ParameterTypes.Length == 2 && g.ParameterTypes[0] == typeof(Expression<Func<ConditionEndpoint, IOperand>>) && g.ParameterTypes[1] == WellKnownTypes.Int32).Method;

      public static readonly MethodInfo ContainsTableExprWithColumnsTopNByRank = containsTableMethods
          .Single(g => g.ParameterTypes.Length == 3 &&
                       g.ParameterTypes[0] == typeof(Expression<Func<ConditionEndpoint, IOperand>>) &&
                       g.ParameterTypes[1].IsArray &&
                       g.ParameterTypes[2] == WellKnownTypes.Int32).Method;

      public static readonly MethodInfo SingleKey = SingleMethods.Single(ft => ft.GetParameterTypes()[0] == typeof(Orm.Key));
      public static readonly MethodInfo SingleArray = SingleMethods.Single(ft => ft.GetParameterTypes()[0] == WellKnownTypes.ObjectArray);
      public static readonly MethodInfo SingleOrDefaultKey = SingleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0] == typeof(Orm.Key));
      public static readonly MethodInfo SingleOrDefaultArray = SingleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0] == WellKnownTypes.ObjectArray);
    }

    public static class QueryEndpoint
    {
      private static readonly MethodInfo[] FreetextMethods = typeof(Orm.QueryEndpoint).GetMethods().Where(m => m.Name == nameof(Orm.QueryEndpoint.FreeText)).ToArray();
      private static readonly (MethodInfo Method, Type[] ParameterTypes)[] containsTableMethods = typeof(Orm.QueryEndpoint).GetMethods()
          .Where(m => m.Name == nameof(Orm.QueryEndpoint.ContainsTable))
          .Select(m => (Method: m, ParameterTypes: m.GetParameterTypes())).ToArray();

      private static readonly MethodInfo[] SingleMethods = typeof(Orm.QueryEndpoint).GetMethods().Where(m => m.Name == nameof(Orm.QueryEndpoint.Single) && m.IsGenericMethod).ToArray();
      private static readonly MethodInfo[] SingleOrDefaultMethods = typeof(Orm.QueryEndpoint).GetMethods().Where(m => m.Name == nameof(Orm.QueryEndpoint.SingleOrDefault) && m.IsGenericMethod).ToArray();

      public static readonly MethodInfo All = typeof(Orm.QueryEndpoint).GetMethod(nameof(Orm.QueryEndpoint.All), Array.Empty<Type>());
      
      public static readonly MethodInfo FreeTextString = FreetextMethods
          .Single(ft => ft.GetParameters().Length == 1 && ft.GetParameterTypes()[0] == WellKnownTypes.String);
      
      public static readonly MethodInfo FreeTextStringTopNByRank = FreetextMethods
          .Single(ft => ft.GetParameters().Length == 2 && ft.GetParameterTypes()[0] == WellKnownTypes.String && ft.GetParameterTypes()[1] == WellKnownTypes.Int32);
      
      public static readonly MethodInfo FreeTextExpression = FreetextMethods
          .Single(ft => ft.GetParameters().Length == 1 && ft.GetParameterTypes()[0] == typeof(Expression<Func<string>>));
      
      public static readonly MethodInfo FreeTextExpressionTopNByRank = FreetextMethods
          .Single(ft => ft.GetParameters().Length == 2 && ft.GetParameterTypes()[0] == typeof(Expression<Func<string>>) && ft.GetParameterTypes()[1] == WellKnownTypes.Int32);
      
      public static readonly MethodInfo ContainsTableExpr = containsTableMethods
          .Single(g => g.ParameterTypes.Length == 1 && g.ParameterTypes[0] == typeof(Expression<Func<ConditionEndpoint, IOperand>>)).Method;

      public static readonly MethodInfo ContainsTableExprWithColumns = containsTableMethods
          .Single(g => g.ParameterTypes.Length == 2 &&
                       g.ParameterTypes[0] == typeof(Expression<Func<ConditionEndpoint, IOperand>>) &&
                       g.ParameterTypes[1].IsArray).Method;

      public static readonly MethodInfo ContainsTableExprTopNByRank = containsTableMethods
          .Single(g => g.ParameterTypes.Length == 2 && g.ParameterTypes[0] == typeof(Expression<Func<ConditionEndpoint, IOperand>>) && g.ParameterTypes[1] == WellKnownTypes.Int32).Method;

      public static readonly MethodInfo ContainsTableExprWithColumnsTopNByRank = containsTableMethods
          .Single(g => g.ParameterTypes.Length == 3 &&
                       g.ParameterTypes[0] == typeof(Expression<Func<ConditionEndpoint, IOperand>>) &&
                       g.ParameterTypes[1].IsArray &&
                       g.ParameterTypes[2] == WellKnownTypes.Int32).Method;

      public static readonly MethodInfo SingleKey = SingleMethods.Single(ft => ft.GetParameterTypes()[0] == typeof(Orm.Key));
      public static readonly MethodInfo SingleArray = SingleMethods.Single(ft => ft.GetParameterTypes()[0] == WellKnownTypes.ObjectArray);
      public static readonly MethodInfo SingleOrDefaultKey = SingleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0] == typeof(Orm.Key));
      public static readonly MethodInfo SingleOrDefaultArray = SingleOrDefaultMethods.Single(ft => ft.GetParameterTypes()[0] == WellKnownTypes.ObjectArray);
      public static readonly MethodInfo Items = typeof(Orm.QueryEndpoint).GetMethod(nameof(Orm.QueryEndpoint.Items));
    }
#pragma warning restore 612,618

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
      public static readonly MethodInfo GenericAccessor = typeof(Xtensive.Tuples.Tuple)
          .GetMethods()
          .Where(mi => mi.Name == Xtensive.Reflection.WellKnown.Tuple.GetValueOrDefault && mi.IsGenericMethod)
          .Single();

      public static readonly MethodInfo Create = typeof(Xtensive.Tuples.Tuple)
          .GetMethods()
          .Where(mi => mi.Name == Xtensive.Reflection.WellKnown.Tuple.Create
            && !mi.IsGenericMethod
              && mi.GetParameters().Count() == 1
                && mi.GetParameters()[0].ParameterType == typeof(TupleDescriptor))
          .Single();

      public static readonly PropertyInfo Descriptor = typeof(Xtensive.Tuples.Tuple).GetProperty(Xtensive.Reflection.WellKnown.Tuple.Descriptor);
    }

    public static class Key
    {
      // Key
      public static readonly PropertyInfo Value = typeof(Orm.Key).GetProperty(nameof(Orm.Key.Value));
      public static readonly MethodInfo Create = typeof(Orm.Key).GetMethod(
          nameof(Orm.Key.Create),
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
          null,
          new[] { typeof(Domain), WellKnownTypes.String, typeof(TypeInfo), typeof(TypeReferenceAccuracy), typeof(Tuples.Tuple) }, null);
    }

    public static class Enumerable
    {
      // Enumerable
      public static readonly MethodInfo Select = typeof(System.Linq.Enumerable).GetMethods().First(m => m.Name == nameof(System.Linq.Enumerable.Select));

      public static readonly MethodInfo First = typeof(System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name == nameof(System.Linq.Enumerable.First) && m.GetParameters().Length == 1);

      public static readonly MethodInfo FirstOrDefault = typeof(System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name == nameof(System.Linq.Enumerable.FirstOrDefault) && m.GetParameters().Length == 1);

      public static readonly MethodInfo Single = typeof(System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name == nameof(System.Linq.Enumerable.Single) && m.GetParameters().Length == 1);

      public static readonly MethodInfo SingleOrDefault = typeof(System.Linq.Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name == nameof(System.Linq.Enumerable.SingleOrDefault) && m.GetParameters().Length == 1);

      public static readonly Type OfTuple = WellKnownInterfaces.EnumerableOfT.CachedMakeGenericType(typeof(Xtensive.Tuples.Tuple));
      public static readonly MethodInfo DefaultIfEmpty = typeof(System.Linq.Enumerable).GetMethods().First(m => m.Name == nameof(System.Linq.Enumerable.DefaultIfEmpty));
      public static readonly MethodInfo Contains = GetMethod(typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.Contains), 1, 2);
      public static readonly MethodInfo Cast = GetMethod(typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.Cast), 1, 1);
    }

    // IEntity
    public static readonly PropertyInfo IEntityKey = WellKnownOrmInterfaces.Entity.GetProperty(WellKnown.KeyFieldName);
    public static readonly PropertyInfo TypeId = WellKnownOrmInterfaces.Entity.GetProperty(WellKnown.TypeIdFieldName);

    // ApplyParameter
    public static readonly PropertyInfo ApplyParameterValue = WellKnownOrmTypes.ApplyParameter.GetProperty("Value");

    // Parameter<Tuple>
    public static readonly PropertyInfo ParameterOfTupleValue = WellKnownOrmTypes.ParameterOfTuple.GetProperty("Value", typeof(Tuples.Tuple));

    // Parameter
    public static readonly PropertyInfo ParameterValue = WellKnownOrmTypes.Parameter.GetProperty("Value");

    // Record
    public static readonly MethodInfo RecordKey = typeof(Record).GetMethods()
        .Single(methodInfo => methodInfo.Name == "GetKey" && methodInfo.GetParameters().Length == 1);

    // Structure
    public static readonly MethodInfo CreateStructure = typeof(Internals.Activator)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Single(methodInfo =>
          methodInfo.Name == "CreateStructure"
          && methodInfo.GetParameters().Length == 3
          && methodInfo.GetParameters()[0].ParameterType == typeof(Session));

    // EntitySet
    public static readonly MethodInfo CreateEntitySet = typeof(Internals.Activator)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Single(methodInfo =>
          methodInfo.Name == "CreateEntitySet"
          && methodInfo.GetParameters().Length == 2);

    // Session
    public static readonly PropertyInfo SessionNodeId = typeof(Session).GetProperty("StorageNodeId");

    private static MethodInfo GetMethod(Type type, string name, int numberOfGenericArgument, int numberOfArguments)
    {
      var method = type.GetMethodEx(name,
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
  }
}