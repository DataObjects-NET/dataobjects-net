// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.26

using System;
using System.Reflection;
using NUnit.Framework;
using PostSharp.Reflection;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class ImplementFastMethodBoundaryAspectTest
  {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true, Inherited = false)]
    [Serializable]
    internal class LogMethodAspect : ReprocessMethodBoundaryAspect
    {
      public MethodBase Method { get; private set; }

      public override object OnEntry(object instance)
      {
        Log.Info("OnEntry called on {0}.", Method.GetShortName(true));
        return "OnEntry";
      }

      public override void OnExit(object instance, object onEntryResult)
      {
        Log.Info("OnExit called.");
        Log.Info(string.Format("OnEntry result: {0}", onEntryResult));
      }

      public override void OnSuccess(object instance, object onEntryResult)
      {
        Log.Info("OnSuccess called.");
      }

      public override ErrorFlowBehavior OnError(object instance, Exception e)
      {
        Log.Error(e);
        return ErrorFlowBehavior.Reprocess;
      }

      public override void RuntimeInitialize(MethodBase method)
      {
        Method = method;
        Log.Info("RuntimeInitialize for {0}.", method.GetShortName(true));
      }
    }

    class BaseClass<T>
    {
      public virtual string Method(T value)
      {
        throw new NotImplementedException();
      }

      public BaseClass()
      {
        Log.Info("Base ctor is called.");
      }
    }

    struct TestStruct
    {
      [LogMethodAspect]
      public string Method(int value)
      {
        return value.ToString();
      }

      [LogMethodAspect]
      public string Method(string value)
      {
        return value;
      }

      [LogMethodAspect]
      public string MethodGeneric<T>(T value)
      {
        return value.ToString();
      }

      [LogMethodAspect]
      public string MethodGeneric<T>(T value, bool isTrue)
      {
        if (isTrue)
          return value+".IsTrue";
        return value.ToString();
      }
    }

    static class TestClassStatic
    {
      [LogMethodAspect]
      public static string Method(int value)
      {
        return value.ToString();
      }

      [LogMethodAspect]
      public static string Method(string value)
      {
        return value;
      }

      [LogMethodAspect]
      public static string MethodGeneric<T>(T value)
      {
        return value.ToString();
      }

      [LogMethodAspect]
      public static string MethodGeneric<T>(T value, bool isTrue)
      {
        if (isTrue)
          return value + ".IsTrue";
        return value.ToString();
      }

    }

    class TestClass : BaseClass<int>
    {
      private static LogMethodAspect testAspect = new LogMethodAspect();
      private int iterationCount = 0;

      [LogMethodAspect]
      public override string Method(int value)
      {
        if (iterationCount == 0) {
          iterationCount++;
          throw new InvalidOperationException();
        }
        return value.ToString();
      }

      [LogMethodAspect]
      public string Method(string value)
      {
        return value;
      }

      [LogMethodAspect]
      public string MethodGeneric<T>(T value)
      {
        return value.ToString();
      }

      [LogMethodAspect]
      public string MethodGeneric<T>(T value, bool isTrue)
      {
        if (isTrue)
          return value + ".IsTrue";
        return value.ToString();
      }

      [LogMethodAspect]
      [LogMethodAspect]
      public TestClass(int value)
      {
        Log.Info(string.Format("Passed value: {0}", value));
      }
    }
  
      
    [Test]
    public void Test()
    {
      var testClass = new TestClass(512);
      Assert.AreEqual("20", testClass.Method(20));
      Assert.AreEqual("20", testClass.Method("20"));
      Assert.AreEqual("20", testClass.MethodGeneric(20));
      Assert.AreEqual("20", testClass.MethodGeneric("20", false));
      Assert.AreEqual("20.IsTrue", testClass.MethodGeneric("20", true));

      var testStruct = new TestStruct();
      Assert.AreEqual("20", testStruct.Method(20));
      Assert.AreEqual("20", testStruct.Method("20"));
      Assert.AreEqual("20", testStruct.MethodGeneric(20));
      Assert.AreEqual("20", testStruct.MethodGeneric("20", false));
      Assert.AreEqual("20.IsTrue", testStruct.MethodGeneric("20", true));

      Assert.AreEqual("20", TestClassStatic.Method(20));
      Assert.AreEqual("20", TestClassStatic.Method("20"));
      Assert.AreEqual("20", TestClassStatic.MethodGeneric(20));
      Assert.AreEqual("20", TestClassStatic.MethodGeneric("20", false));
      Assert.AreEqual("20.IsTrue", TestClassStatic.MethodGeneric("20", true));
    }

    [Test]
    public void GenericTest()
    {
      var method = ReflectionHelper.GetMethod(typeof(TestClass), "MethodGeneric", "System.String MethodGeneric[T](T, Boolean)");
      var type = typeof (LogMethodAspect);
      var i = 10;

    }

  }
}