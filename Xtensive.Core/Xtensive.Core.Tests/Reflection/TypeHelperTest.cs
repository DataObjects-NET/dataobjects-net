// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.17

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Testing;

namespace Xtensive.Tests.Reflection
{
  [TestFixture]
  public class TypeHelperTest
  {
    string[] associateSuffix = new string[]{"Associate"};
    string[] testHandlerSuffix = new string[]{"TestHandler"};
    private Pair<Assembly, string>[] locations = new Pair<Assembly, string>[] {};
    private Type foundFor;
    object o;

    [Test]
    public void DirectAssociateTest()
    {
      // Checking class A associates
      o = TypeHelper.CreateAssociate<AAssociate>(typeof (A), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<AIAssociate>(typeof (AI), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<AITAssociate<int>>(typeof (AIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<AIT2Associate<int, int>>(typeof (AIT2<int, int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
    }

    [Test]
    public void InheritedAssociateTest1()
    {
      // Checking class B associates
      o = TypeHelper.CreateAssociate<AAssociate>(typeof (BA), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<InterfaceAssociate>(typeof (BI), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(typeof (BIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<T2InterfaceAssociate<int, int>>(typeof (BIT2<int, int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
    }

    [Test]
    public void InheritedAssociateTest2()
    {
      // Checking class C associates
      o = TypeHelper.CreateAssociate<AAssociate>(typeof (CB), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<InterfaceAssociate>(typeof (CBI), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(typeof (CBIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<T2InterfaceAssociate<int, int>>(typeof (CBIT2<int, int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
    }

    [Test]
    public void InheritedAssociateTest3()
    {
      // Checking class D associates
      o = TypeHelper.CreateAssociate<AAssociate>(typeof (DA), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<AIAssociate>(typeof (DAI), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<AITAssociate<int>>(typeof (DAIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<AIT2Associate<int, int>>(typeof (DAIT2<int, int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
    }

    [Test]
    public void OverridenAssociateTest()
    {
      // Checking class E associates
      o = TypeHelper.CreateAssociate<EAAssociate>(typeof (EA), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<EAIAssociate>(typeof (EAI), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<EAITAssociate<int>>(typeof (EAIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<EAIT2Associate<int, int>>(typeof (EAIT2<int, int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
    }

    [Test]
    public void MultipleMatchAssociateTest()
    {
      // Checking multiple match associates
      try {
        TypeHelper.CreateAssociate<IAssociate>(
          typeof (FIIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
        Assert.Fail("{0} expected.", typeof (InvalidOperationException).Name);
      }
      catch (InvalidOperationException) {
      }
      try {
        TypeHelper.CreateAssociate<IAssociate>(
          typeof (FIIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
        Assert.Fail("{0} expected.", typeof (InvalidOperationException).Name);
      }
      catch (InvalidOperationException) {
      }

      o = TypeHelper.CreateAssociate<InterfaceAssociate>(
        typeof (FIIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(
        typeof (FIIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);

      try {
        TypeHelper.CreateAssociate<IAssociate>(
          typeof (GFIIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
        Assert.Fail("{0} expected.", typeof (InvalidOperationException).Name);
      }
      catch (InvalidOperationException) {
      }
      try {
        TypeHelper.CreateAssociate<IAssociate>(
          typeof (GFIIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
        Assert.Fail("{0} expected.", typeof (InvalidOperationException).Name);
      }
      catch (InvalidOperationException) {
      }

      o = TypeHelper.CreateAssociate<InterfaceAssociate>(
        typeof (GFIIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(
        typeof (GFIIT<int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);

      try {
        TypeHelper.CreateAssociate<IAssociate>(
          typeof (GIT2FIIT<int, int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
        Assert.Fail("{0} expected.", typeof (InvalidOperationException).Name);
      }
      catch (InvalidOperationException) {
      }
      try {
        TypeHelper.CreateAssociate<IAssociate>(
          typeof (GIT2FIIT<int, int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
        Assert.Fail("{0} expected.", typeof (InvalidOperationException).Name);
      }
      catch (InvalidOperationException) {
      }

      o = TypeHelper.CreateAssociate<InterfaceAssociate>(
        typeof (GIT2FIIT<int, int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(
        typeof (GIT2FIIT<int, int>), out foundFor, associateSuffix, ArrayUtils<object>.EmptyArray);
      Assert.IsNotNull(o);

      try {
        TypeHelper.CreateAssociate<ITestHandler>(typeof(SomeCloneableEnumerable), out foundFor, testHandlerSuffix, null, locations);
        Assert.Fail("{0} expected.", typeof (InvalidOperationException).Name);
      }
      catch (InvalidOperationException) {
      }
      o = TypeHelper.CreateAssociate<CloneableInterfaceTestHandler>(typeof(SomeCloneableEnumerable), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);

      try {
        TypeHelper.CreateAssociate<ITestHandler>(typeof(string), out foundFor, testHandlerSuffix, null, locations);
        Assert.Fail("{0} expected.", typeof (InvalidOperationException).Name);
      }
      catch (InvalidOperationException) {
      }
    }

    [Test]
    public void SystemTypeAssociateTest()
    {
      // System type associates tests
      o = TypeHelper.CreateAssociate<ITestHandler<int>>(typeof(int), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);

      o = TypeHelper.CreateAssociate<ITestHandler<long>>(typeof(long), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      
      o = TypeHelper.CreateAssociate<ITestHandler>(typeof(BitVector32), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(ObjectTestHandler), o.GetType());

      o = TypeHelper.CreateAssociate<ITestHandler<IEnumerable<char>>>(typeof(string), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(EnumerableInterfaceTestHandler<char>), o.GetType());
    }

    [Test]
    public void ArrayAssociateTest()
    {
      // Array associates tests
      o = TypeHelper.CreateAssociate<ITestHandler<int[]>>(typeof(int[]), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(ArrayTestHandler<int>), o.GetType());
      
      o = TypeHelper.CreateAssociate<ITestHandler<int[,]>>(typeof(int[,]), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(Array2DTestHandler<int>), o.GetType());
    }

    [Test]
    public void EnumerableAssociateTest()
    {
      // Enumerable associates tests
      o = TypeHelper.CreateAssociate<ITestHandler<IEnumerable<int>>>(typeof(int[]), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(EnumerableInterfaceTestHandler<int>), o.GetType());

      o = TypeHelper.CreateAssociate<ITestHandler<IEnumerable>>(typeof(int[]), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(EnumerableInterfaceTestHandler), o.GetType());

      o = TypeHelper.CreateAssociate<ITestHandler>(typeof(List<int>), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(EnumerableInterfaceTestHandler<int>), o.GetType());

      o = TypeHelper.CreateAssociate<ITestHandler>(typeof(EnumerableEnumerable<IEnumerable<int>,int>), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(EnumerableInterfaceTestHandler<int>), o.GetType());

      o = TypeHelper.CreateAssociate<ITestHandler>(typeof(SomeEnumerable), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(EnumerableInterfaceTestHandler), o.GetType());
    }

    [Test]
    public void GenericAssociateTest()
    {
      // Generic associates tests
      o = TypeHelper.CreateAssociate<ITestHandler<double>>(typeof(double), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(ObjectTestHandler<double>), o.GetType());

      o = TypeHelper.CreateAssociate<ITestHandler<object>>(typeof(object), out foundFor, testHandlerSuffix, null, locations);
      Assert.IsNotNull(o);
      Assert.AreEqual(typeof(ObjectTestHandler), o.GetType());
    }

    [Test]
    public void CreateDummyTypeTest()
    {
      Type t1 = TypeHelper.CreateDummyType("Test1", typeof(object));
      Assert.IsNotNull(t1);
      AssertEx.IsPatternMatch(t1.Name, "Test1*");
      Log.Info("t1 name: {0}", t1.GetFullName());
      
      Type t2 = TypeHelper.CreateDummyType("Test2", typeof(object));
      Assert.IsNotNull(t2);
      AssertEx.IsPatternMatch(t2.Name, "Test2*");
      Log.Info("t2 name: {0}", t2.GetFullName());

      Assert.AreNotEqual(t1, t2);

      Type listType = typeof (List<>);
      IList list = (IList)TypeHelper.Activate(listType.Assembly, listType.FullName, new Type[] {t1}, null);
      Log.Info("List<t1> name: {0}", list.GetType().GetFullName());
    }

    #region Nested types
    public class TestClass
    {
      public string value;

      public TestClass()
      {
        value = "Default Constructor";
      }

      public TestClass(string value)
      {
        this.value = value;
      }

      public TestClass(int intValue, string stringValue)
      {
        value = intValue.ToString() + stringValue;
      }

      private static TestClass ttttt(int intValue, string stringValue)
      {
        return new TestClass(intValue, stringValue);
        
      }
    }
    #endregion

    [Test]
    public void CreateDummyTypeConstructorTest()
    {
      Type type = TypeHelper.CreateDummyType("Test2", typeof(TestClass), true);
      var defaultConstructorInstance = (TestClass)type.Activate(null);
      Assert.AreEqual(defaultConstructorInstance.value, "Default Constructor");
      var customConstructorInstance1 = (TestClass)type.Activate(null, "Custom constructor");
      Assert.AreEqual(customConstructorInstance1.value, "Custom constructor");
      var customConstructorInstance2 = (TestClass)type.Activate(null, 11, "test");
      Assert.AreEqual(customConstructorInstance2.value, "11test");
      var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
      Assert.AreEqual(3, methods.Length);
      var ctor1 = (TestClass)methods[0].Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic, null, new object[0], null);
      var ctor2 = (TestClass)methods[1].Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { "Custom constructor" }, null);
      var ctor3 =(TestClass) methods[2].Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { 11, "test" }, null);
      Assert.AreEqual(ctor1.value, defaultConstructorInstance.value);
      Assert.AreEqual(ctor2.value, customConstructorInstance1.value);
      Assert.AreEqual(ctor3.value, customConstructorInstance2.value);
    }

    [Test]
    public void GetNameAndAddSuffixTest()
    {
      Type t = typeof (A<,>);
      string fName = t.GetFullName();
      Assert.AreEqual(t.Namespace + ".A<,>", fName);
      string sName = t.GetShortName();
      Assert.AreEqual("A<,>", sName);
      string wsName = TypeHelper.AddSuffix(fName, "#");
      Assert.AreEqual(t.Namespace + ".A#<,>", wsName);
      fName = t.FullName;
      wsName = TypeHelper.AddSuffix(fName, "#");
      Assert.AreEqual(t.Namespace + ".A#`2", wsName);

      t = typeof (A<int, string>);
      fName = t.GetFullName();
      Assert.AreEqual(t.Namespace + ".A<System.Int32,System.String>", fName);
      sName = t.GetShortName();
      Assert.AreEqual("A<Int32,String>", sName);
      wsName = TypeHelper.AddSuffix(fName, "#");
      Assert.AreEqual(t.Namespace + ".A#<System.Int32,System.String>", wsName);
      fName = t.FullName;
      wsName = TypeHelper.AddSuffix(fName, "#");
      AssertEx.IsPatternMatch(wsName, t.Namespace + ".A#`2[[*]]");

      t = typeof (A<int, string>.B<bool>);
      fName = t.GetFullName();
      Assert.AreEqual(t.Namespace + ".A<System.Int32,System.String>+B<System.Boolean>", fName);
      sName = t.GetShortName();
      Assert.AreEqual("A<Int32,String>+B<Boolean>", sName);

      t = t.GetGenericTypeDefinition().GetGenericArguments()[2];
      fName = t.GetFullName();
      Assert.AreEqual("T3", fName);
      sName = t.GetShortName();
      Assert.AreEqual("T3", sName);
    }

    [Test]
    public void ActivateTest()
    {
      o = TypeHelper.Activate(GetType().Assembly, typeof (A<int, string>).FullName, null, 1, "2");
      Assert.IsNotNull(o);
      A<int, string> a = o as A<int, string>;
      Assert.IsNotNull(a);
      Assert.AreEqual(1, a.F1);
      Assert.AreEqual("2", a.F2);

      o = TypeHelper.Activate(GetType().Assembly, typeof (A<,>).FullName, 
        new Type[] {typeof(int), typeof(string)}, 1, "2");
      Assert.IsNotNull(o);
      a = o as A<int, string>;
      Assert.IsNotNull(a);
      Assert.AreEqual(1, a.F1);
      Assert.AreEqual("2", a.F2);
    }
  }
}