﻿// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.10

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Linq;

namespace Xtensive.Core.Tests.Linq
{
  partial class MemberCompilerProviderTest
  {
    private class NonGenericTarget
    {
      private string dummy;

      static public long StaticNonGenericMethod(int a, long b)
      {
        return a + b;
      }

      static public string StaticGenericMethod <T1> (T1 obj)
      {
        return obj.ToString();
      }

      public int InstanceNonGenericMethod(int k)
      {
        return k;
      }

      public T1 InstanceGenericMethod <T1> () where T1 : new()
      {
        return new T1();
      }

      public static int StaticProperty { get; set; }

      public string InstanceProperty { get; set; }

      public string this[int a, int b]{
        get { return dummy; }
        set { dummy = value; }
      }
    }

    private class GenericTarget<T>
    {
      private T dummy;

      static public string StaticNonGenericMethod(T obj, int n)
      {
        return obj.ToString() + n.ToString();
      }

      static public T1 StaticGenericMethod <T1> (T obj) where T1 : class
      {
        return obj as T1;
      }

      public short InstanceNonGenericMethod()
      {
        return 0;
      }

      public int InstanceGenericMethod<T1>(T1 obj, int n) where T1 : class
      {
        return obj.GetHashCode() + n;
      }

      public static int StaticProperty { get; set; }

      public string InstanceProperty { get; set; }

      public T this[string s]
      {
        get { return dummy; }
        set { dummy = value; }
      }
    }

    private class MethodCompiler
    {
      #region Compilers for NonGenericTarget methods

      [Compiler(typeof(NonGenericTarget), "StaticNonGenericMethod", TargetKind.Method | TargetKind.Static)]
      static public string C1([ParamType(typeof(int))]string a, [ParamType(typeof(long))]string b)
      {
        return "NonGenericTarget.StaticNonGenericMethod";
      }

      [Compiler(typeof(NonGenericTarget), "StaticGenericMethod", TargetKind.Method | TargetKind.Static, 1)]
      static public string C2(MethodInfo methodInfo, string s)
      {
        return "NonGenericTarget.StaticGenericMethod";
      }

      [Compiler(typeof(NonGenericTarget), "InstanceNonGenericMethod")]
      static public string C3(string this_, [ParamType(typeof(int))] string s)
      {
        return "NonGenericTarget.InstanceNonGenericMethod";
      }

      [Compiler(typeof(NonGenericTarget), "InstanceGenericMethod", 1)]
      static public string C4(MethodInfo methodInfo, string this_)
      {
        return "NonGenericTarget.InstanceGenericMethod";
      }

      #endregion

      #region Compilers for GenericTarget methods

      [Compiler(typeof(GenericTarget<>), "StaticNonGenericMethod", TargetKind.Method | TargetKind.Static)]
      static public string C5(MethodInfo methodInfo, string s1, [ParamType(typeof(int))] string s2)
      {
        return "GenericTarget`1.StaticNonGenericMethod";
      }

      [Compiler(typeof(GenericTarget<>), "StaticGenericMethod", TargetKind.Method | TargetKind.Static, 1)]
      static public string C6(MethodInfo methodInfo, string s)
      {
        return "GenericTarget`1.StaticGenericMethod";
      }

      [Compiler(typeof(GenericTarget<>), "InstanceNonGenericMethod")]
      static public string C7(MethodInfo methodInfo, string this_)
      {
        return "GenericTarget`1.InstanceNonGenericMethod";
      }

      [Compiler(typeof(GenericTarget<>), "InstanceGenericMethod", 1)]
      static public string C8(MethodInfo methodInfo, string this_, string s1, [ParamType(typeof(int))]string s2)
      {
        return "GenericTarget`1.InstanceGenericMethod";
      }

      #endregion
    }

    private class PropertyCompiler
    {
      #region Compilers for NonGenericTarget properties

      [Compiler(typeof(NonGenericTarget), "StaticProperty", TargetKind.Static | TargetKind.PropertyGet)]
      public static string C1()
      {
        return "NonGenericTarget.get_StaticProperty";
      }

      [Compiler(typeof(NonGenericTarget), "StaticProperty", TargetKind.Static | TargetKind.PropertySet)]
      public static string C2([ParamType(typeof(int))] string s)
      {
        return "NonGenericTarget.set_StaticProperty";
      }
      
      [Compiler(typeof(NonGenericTarget), "InstanceProperty", TargetKind.PropertyGet)]
      public static string C3(string this_)
      {
        return "NonGenericTarget.get_InstanceProperty";
      }

      [Compiler(typeof(NonGenericTarget), "InstanceProperty", TargetKind.PropertySet)]
      public static string C4(string this_, [ParamType(typeof(string))] string s)
      {
        return "NonGenericTarget.set_InstanceProperty";
      }

      [Compiler(typeof(NonGenericTarget), null, TargetKind.PropertyGet)]
      public static string C5(string this_,
        [ParamType(typeof(int))] string s1,
        [ParamType(typeof(int))] string s2)
      {
        return "NonGenericTarget.get_Item";
      }

      [Compiler(typeof(NonGenericTarget), null, TargetKind.PropertySet)]
      public static string C6(string this_,
        [ParamType(typeof(int))] string s1,
        [ParamType(typeof(int))] string s2,
        [ParamType(typeof(string))] string value)
      {
        return "NonGenericTarget.set_Item";
      }

      #endregion

      #region Compilers for GenericTarget properties

      [Compiler(typeof(GenericTarget<>), "StaticProperty", TargetKind.Static | TargetKind.PropertyGet)]
      static public string C7(MethodInfo methodInfo)
      {
        return "GenericTarget`1.get_StaticProperty";
      }

      [Compiler(typeof(GenericTarget<>), "StaticProperty", TargetKind.Static | TargetKind.PropertySet)]
      static public string C8(MethodInfo methodInfo, [ParamType(typeof(int))] string s)
      {
        return "GenericTarget`1.set_StaticProperty";
      }

      [Compiler(typeof(GenericTarget<>), "InstanceProperty", TargetKind.PropertyGet)]
      static public string C9(MethodInfo methodInfo, string this_)
      {
        return "GenericTarget`1.get_InstanceProperty";
      }

      [Compiler(typeof(GenericTarget<>), "InstanceProperty", TargetKind.PropertySet)]
      static public string C10(MethodInfo methodInfo, string this_, [ParamType(typeof(string))] string s)
      {
        return "GenericTarget`1.set_InstanceProperty";
      }

      [Compiler(typeof(GenericTarget<>), null, TargetKind.PropertyGet)]
      public static string C11(MethodInfo methodInfo, string this_,
        [ParamType(typeof(string))] string s)
      {
        return "GenericTarget`1.get_Item";
      }

      [Compiler(typeof(GenericTarget<>), null, TargetKind.PropertySet)]
      public static string C12(MethodInfo methodInfo, string this_,
        [ParamType(typeof(string))] string s, string value)
      {
        return "GenericTarget`1.set_Item";
      }

      #endregion
    }

    #region Classes for testing conflict handling 

    private class ConflictTarget
    {
      public int ConflictMethod()
      {
        return 0;
      }
    }

    private class ConflictCompiler1
    {
      [Compiler(typeof(ConflictTarget), "ConflictMethod")]
      static public string C([ParamType(typeof(ConflictTarget))] string s)
      {
        return "Compiler1";
      }
    }

    private class ConflictCompiler2
    {
      [Compiler(typeof(ConflictTarget), "ConflictMethod")]
      static public string C([ParamType(typeof(ConflictTarget))] string s)
      {
        return "Compiler2"; 
      }
    }

    #endregion

    #region Classes for testing fancy generic finding algorithm

    private class SuperGenericTarget<T> where T : class
    {
      public static string Method<T1>(T obj, string s, T1 obj1) where T1 : class
      {
        return obj.ToString() + s + obj1.ToString();
      }

      public static string Method<T1>(int n, string s, T1 obj1) where T1 : class
      {
        return n.ToString() + s + obj1.ToString();
      }

      public static string Method(int n, string s, double d)
      {
        return n.ToString() + s + d.ToString();
      }

      public static string Method<T1>(int n, T1 obj1, T obj) where T1 : class
      {
        return n.ToString() + obj1.ToString() + obj.ToString();
      }
    }

    private class SuperGenericCompiler
    {
      [Compiler(typeof(SuperGenericTarget<>), "Method", TargetKind.Static | TargetKind.Method, 1)]
      public static string Compiler(MethodInfo methodInfo,
        [ParamType(typeof(int))] string s1,
        [ParamType(typeof(string))] string s2,
        string s3)
      {
        return "OK";
      }
    }

    #endregion
  }
}