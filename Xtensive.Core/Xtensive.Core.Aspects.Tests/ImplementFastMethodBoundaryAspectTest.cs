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
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    [Serializable]
    internal class LogMethodAspect : ImplementFastMethodBoundaryAspect
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

      public override bool OnError(object instance, Exception e)
      {
        Log.Error(e);
        return false;
      }

      public override void RuntimeInitialize(MethodBase method)
      {
        Method = method;
        Log.Info("RuntimeInitialize for {0}.", method.GetShortName(true));
      }
    }

    class BaseClass<T>
    {
      public virtual string AspectedMethod(T value)
      {
        throw new NotImplementedException();
      }

      public BaseClass()
      {
        Log.Info("Base ctor is called.");
      }
    }

    class TestClass : BaseClass<int>
    {
      private static LogMethodAspect testAspect = new LogMethodAspect();

      [LogMethodAspect]
      public override string AspectedMethod(int value)
      {
        return value.ToString();
      }

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
        return value.ToString();
      }

      public string NotAspectedMethodGeneric<T>(T value)
      {
        string returnValue = null;
        object onEntryResult = testAspect.OnEntry(this);
        try {
          string v = value.ToString();
          returnValue = v;
        }
        catch (Exception e) {
          if (testAspect.OnError(this, e)) {
            throw;
          }
        }
        finally {
          testAspect.OnExit(this, onEntryResult);
        }
        testAspect.OnSuccess(this, onEntryResult);
        return returnValue;
      }

      public string NotAspectedMethod(int value)
      {
        try {
          return value.ToString();
        }
        catch(Exception e) {
          throw;
        }
        finally {
          
        }
      }

      [LogMethodAspect]
      public TestClass(int value)
      {
        Log.Info(string.Format("Passed value: {0}", value));
      }
    }
  
      
    [Test]
    public void Test()
    {
      TestClass testClass = new TestClass(512);
      var result = testClass.AspectedMethod(123321);
      testClass.Method(20);
      testClass.Method("20");
      testClass.NotAspectedMethodGeneric(20);
      testClass.MethodGeneric(20);
      testClass.MethodGeneric("20", true);
      Assert.AreEqual("123321", result);
    }

    [Test]
    public void GenericTest()
    {
      var method = ReflectionHelper.GetMethod(typeof(TestClass), "MethodGeneric", "System.String MethodGeneric[T](T, Boolean)");
      var i = 10;
    }
  }
}