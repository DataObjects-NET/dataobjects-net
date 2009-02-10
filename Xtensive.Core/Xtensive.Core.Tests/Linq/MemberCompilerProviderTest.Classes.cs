// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.10

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Linq;

namespace Xtensive.Core.Tests.Linq
{
  partial class MemberCompilerProviderTest
  {
    private class NonGenericTarget
    {
      public int InstanceMethod()
      {
        return GetHashCode();
      }

      static public long NonGenericMethod(int a, long b)
      {
        return a + b;
      }

      static public string GenericMethod<T1>(T1 obj)
      {
        return obj.ToString();
      }
    }

    private class GenericTarget<T>
    {
      static public string NonGenericMethod(T obj, string s, int n)
      {
        return obj + s + n;
      }

      static public T1 GenericMethod<T1>(T obj) where T1 : class
      {
        return obj as T1;
      }
    }

    private class MainCompiler
    {
      [Compiler(typeof(NonGenericTarget))]
      static public string InstanceMethod([ParamType(typeof(NonGenericTarget))] string this_)
      {
        return "InstanceCompiler";
      }

      [Compiler(
        typeof(NonGenericTarget),
        TargetMethod = "NonGenericMethod",
        TargetKind = TargetKind.Static | TargetKind.Method)]
      static public string CompilerNN(
        [ParamType(typeof(int))] string s1,
        [ParamType(typeof(long))] string s2)
      {
        return "CompilerNN";
      }

      [Compiler(
        typeof(NonGenericTarget),
        TargetMethod = "GenericMethod",
        TargetKind = TargetKind.Static | TargetKind.Method,
        GenericParamsCount = 1)]
      static public string CompilerNG(MethodInfo methodInfo, string s)
      {
        return "CompilerNG";
      }

      [Compiler(
        typeof(GenericTarget<>),
        TargetMethod = "NonGenericMethod",
        TargetKind = TargetKind.Static | TargetKind.Method)]
      static public string CompilerGN(
        MethodInfo methodInfo,
        string s1,
        [ParamType(typeof(string))] string s2,
        [ParamType(typeof(int))]string s3)
      {
        return "CompilerGN";
      }

      [Compiler(
        typeof(GenericTarget<>),
        TargetMethod = "GenericMethod",
        TargetKind = TargetKind.Static | TargetKind.Method,
        GenericParamsCount = 1)]
      static public string CompilerGG(MethodInfo methodInfo, string s)
      {
        return "CompilerGG";
      }
    }

    private class AnotherCompiler
    {

    }
  }
}