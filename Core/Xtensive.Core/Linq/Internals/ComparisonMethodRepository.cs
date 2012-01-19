// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Linq
{
  internal static class ComparisonMethodRepository
  {
    private static readonly Dictionary<MethodInfo, ComparisonMethodInfo> methods =
      new Dictionary<MethodInfo, ComparisonMethodInfo>();

    public static ComparisonMethodInfo Get(MethodInfo sample)
    {
      if (methods.ContainsKey(sample))
        return methods[sample];
      return methods.Select(pair => pair.Value).SingleOrDefault(info => info.Matches(sample));
    }

    private static void AddAllMethods(Type type, ComparisonKind comparisonKind)
    {
      AddMethods(type.GetMethods(), comparisonKind);
    }

    private static void AddMethods(IEnumerable<MethodInfo> newMethods,
      ComparisonKind comparisonKind)
    {
      foreach (var method in newMethods) {
        AddMethod(method, comparisonKind);
      }
    }

    private static void AddMethods(Type type, string methodName, BindingFlags bindingFlags,
      ComparisonKind comparisonKind)
    {
      foreach (var method in type.GetMethods(bindingFlags)) {
        if (method.Name == methodName)
          AddMethod(method, comparisonKind);
      }
    }

    private static void AddMethod(MethodInfo method, ComparisonKind comparisonKind)
    {
      methods.Add(method, new ComparisonMethodInfo(method, comparisonKind));
    }


    //Constructors

    static ComparisonMethodRepository()
    {
      AddAllMethods(typeof(IComparable), ComparisonKind.Default);
      AddAllMethods(typeof(IComparable<>), ComparisonKind.Default);
      const BindingFlags flagsForStatic = BindingFlags.Static | BindingFlags.Public;
      const BindingFlags flagsForInstanceAndStatic = BindingFlags.Public | BindingFlags.Static
        | BindingFlags.Instance;
      AddMethods(typeof(string), "Compare", flagsForStatic, ComparisonKind.Default);
      AddMethod(typeof(string).GetMethod("CompareOrdinal", new[] { typeof(string), typeof(string) }),
        ComparisonKind.Default);

      AddMethods(typeof(string), "StartsWith", flagsForInstanceAndStatic, ComparisonKind.LikeStartsWith);
      AddMethods(typeof(string), "EndsWith", flagsForInstanceAndStatic, ComparisonKind.LikeEndsWith);

      AddMethod(typeof(DateTime).GetMethod("Compare", flagsForStatic), ComparisonKind.Default);
      AddMethods(typeof(object), "Equals", flagsForInstanceAndStatic, ComparisonKind.Equality);
      AddMethods(typeof(string), "Equals", flagsForInstanceAndStatic, ComparisonKind.Equality);
      AddMethod(typeof(DateTime).GetMethod("Equals", flagsForStatic), ComparisonKind.Equality);
      AddMethod(typeof(IEquatable<>).GetMethod("Equals"), ComparisonKind.Equality);

      AddMethod(typeof(StringExtensions).GetMethod("GreaterThan"), ComparisonKind.ForcedGreaterThan);
      AddMethod(typeof(StringExtensions).GetMethod("GreaterThanOrEqual"),
        ComparisonKind.ForcedGreaterThanOrEqual);
      AddMethod(typeof(StringExtensions).GetMethod("LessThan"), ComparisonKind.ForcedLessThan);
      AddMethod(typeof(StringExtensions).GetMethod("LessThanOrEqual"),
        ComparisonKind.ForcedLessThanOrEqual);
    }
  }
}