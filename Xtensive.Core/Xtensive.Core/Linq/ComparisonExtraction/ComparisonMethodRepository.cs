// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xtensive.Core.Linq.ComparisonExtraction
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

    private static void AddAllMethods(Type type)
    {
      AddMethods(type.GetMethods());
    }

    private static void AddMethods(IEnumerable<MethodInfo> newMethods)
    {
      foreach (var method in newMethods) {
        AddMethod(method);
      }
    }

    private static void AddMethods(Type type, string methodName, BindingFlags bindingFlags)
    {
      foreach (var method in type.GetMethods(bindingFlags)) {
        if (method.Name == methodName)
          AddMethod(method);
      }
    }

    private static void AddMethod(MethodInfo method)
    {
      methods.Add(method, new ComparisonMethodInfo(method));
    }

    //Constructors

    static ComparisonMethodRepository()
    {
      AddAllMethods(typeof(IComparable));
      AddAllMethods(typeof(IComparable<>));
      var flagsForStatic = BindingFlags.Static | BindingFlags.Public;
      AddMethods(typeof(string), "Compare", flagsForStatic);
      AddMethods(typeof(string), "CompareOrdinal", flagsForStatic);
      AddMethods(ComparisonMethodInfo.GetMethodsCorrespondingToLike());
      AddMethod(typeof (DateTime).GetMethod("Compare", flagsForStatic));
    }
  }
}