// Copyright (C) 2007-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
using Xtensive.Orm.Tests;
using System.Linq;

namespace Xtensive.Orm.Tests.Core.Reflection
{
  [TestFixture]
  public class TypeHelperTest
  {
    private readonly string[] associateSuffix = new string[]{"Associate"};
    private readonly string[] testHandlerSuffix = new string[]{"TestHandler"};
    private readonly Pair<Assembly, string>[] locations = new Pair<Assembly, string>[] {};
    private Type foundFor;
    private object o;

    [Test]
    public void DirectAssociateTest()
    {
      // Checking class A associates
      o = TypeHelper.CreateAssociate<AAssociate>(typeof (A), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<AIAssociate>(typeof (AI), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<AITAssociate<int>>(typeof (AIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<AIT2Associate<int, int>>(typeof (AIT2<int, int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
    }

    [Test]
    public void InheritedAssociateTest1()
    {
      // Checking class B associates
      o = TypeHelper.CreateAssociate<AAssociate>(typeof (BA), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<InterfaceAssociate>(typeof (BI), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(typeof (BIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<T2InterfaceAssociate<int, int>>(typeof (BIT2<int, int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
    }

    [Test]
    public void InheritedAssociateTest2()
    {
      // Checking class C associates
      o = TypeHelper.CreateAssociate<AAssociate>(typeof (CB), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<InterfaceAssociate>(typeof (CBI), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(typeof (CBIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<T2InterfaceAssociate<int, int>>(typeof (CBIT2<int, int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
    }

    [Test]
    public void InheritedAssociateTest3()
    {
      // Checking class D associates
      o = TypeHelper.CreateAssociate<AAssociate>(typeof (DA), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<AIAssociate>(typeof (DAI), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<AITAssociate<int>>(typeof (DAIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<AIT2Associate<int, int>>(typeof (DAIT2<int, int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
    }

    [Test]
    public void OverridenAssociateTest()
    {
      // Checking class E associates
      o = TypeHelper.CreateAssociate<EAAssociate>(typeof (EA), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<EAIAssociate>(typeof (EAI), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<EAITAssociate<int>>(typeof (EAIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<EAIT2Associate<int, int>>(typeof (EAIT2<int, int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
    }

    [Test]
    public void MultipleMatchAssociateTest()
    {
      // Checking multiple match associates
      try {
        _ = TypeHelper.CreateAssociate<IAssociate>(
          typeof (FIIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
        Assert.Fail($"{typeof(InvalidOperationException).Name} expected.");
      }
      catch (InvalidOperationException) {
      }
      try {
        _ = TypeHelper.CreateAssociate<IAssociate>(
          typeof (FIIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
        Assert.Fail($"{typeof(InvalidOperationException).Name} expected.");
      }
      catch (InvalidOperationException) {
      }

      o = TypeHelper.CreateAssociate<InterfaceAssociate>(
        typeof (FIIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(
        typeof (FIIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);

      try {
        _ = TypeHelper.CreateAssociate<IAssociate>(
          typeof (GFIIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
        Assert.Fail($"{typeof(InvalidOperationException).Name} expected.");
      }
      catch (InvalidOperationException) {
      }
      try {
        _ = TypeHelper.CreateAssociate<IAssociate>(
          typeof (GFIIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
        Assert.Fail($"{typeof(InvalidOperationException).Name} expected.");
      }
      catch (InvalidOperationException) {
      }

      o = TypeHelper.CreateAssociate<InterfaceAssociate>(
        typeof (GFIIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(
        typeof (GFIIT<int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);

      try {
        _ = TypeHelper.CreateAssociate<IAssociate>(
          typeof (GIT2FIIT<int, int>), out foundFor, associateSuffix,  Array.Empty<object>());
        Assert.Fail($"{typeof(InvalidOperationException).Name} expected.");
      }
      catch (InvalidOperationException) {
      }
      try {
        _ = TypeHelper.CreateAssociate<IAssociate>(
          typeof (GIT2FIIT<int, int>), out foundFor, associateSuffix,  Array.Empty<object>());
        Assert.Fail($"{typeof(InvalidOperationException).Name} expected.");
      }
      catch (InvalidOperationException) {
      }

      o = TypeHelper.CreateAssociate<InterfaceAssociate>(
        typeof (GIT2FIIT<int, int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);
      o = TypeHelper.CreateAssociate<TInterfaceAssociate<int>>(
        typeof (GIT2FIIT<int, int>), out foundFor, associateSuffix,  Array.Empty<object>());
      Assert.That(o, Is.Not.Null);

      try {
        _ = TypeHelper.CreateAssociate<ITestHandler>(typeof(SomeCloneableEnumerable), out foundFor, testHandlerSuffix, null, locations);
        Assert.Fail($"{typeof(InvalidOperationException).Name} expected.");
      }
      catch (InvalidOperationException) {
      }
      o = TypeHelper.CreateAssociate<CloneableInterfaceTestHandler>(typeof(SomeCloneableEnumerable), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);

      try {
        _ = TypeHelper.CreateAssociate<ITestHandler>(typeof(string), out foundFor, testHandlerSuffix, null, locations);
        Assert.Fail($"{typeof(InvalidOperationException).Name} expected.");
      }
      catch (InvalidOperationException) {
      }
    }

    [Test]
    public void SystemTypeAssociateTest()
    {
      // System type associates tests
      o = TypeHelper.CreateAssociate<ITestHandler<int>>(typeof(int), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);

      o = TypeHelper.CreateAssociate<ITestHandler<long>>(typeof(long), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      
      o = TypeHelper.CreateAssociate<ITestHandler>(typeof(BitVector32), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(ObjectTestHandler)));

      o = TypeHelper.CreateAssociate<ITestHandler<IEnumerable<char>>>(typeof(string), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(EnumerableInterfaceTestHandler<char>)));
    }

    [Test]
    public void ArrayAssociateTest()
    {
      // Array associates tests
      o = TypeHelper.CreateAssociate<ITestHandler<int[]>>(typeof(int[]), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(ArrayTestHandler<int>)));
      
      o = TypeHelper.CreateAssociate<ITestHandler<int[,]>>(typeof(int[,]), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(Array2DTestHandler<int>)));
    }

    [Test]
    public void EnumerableAssociateTest()
    {
      // Enumerable associates tests
      o = TypeHelper.CreateAssociate<ITestHandler<IEnumerable<int>>>(typeof(int[]), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(EnumerableInterfaceTestHandler<int>)));

      o = TypeHelper.CreateAssociate<ITestHandler<IEnumerable>>(typeof(int[]), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(EnumerableInterfaceTestHandler)));

      o = TypeHelper.CreateAssociate<ITestHandler>(typeof(List<int>), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(EnumerableInterfaceTestHandler<int>)));

      o = TypeHelper.CreateAssociate<ITestHandler>(typeof(SomeEnumerable), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(EnumerableInterfaceTestHandler)));
    }

    [Test]
    public void GenericAssociateTest()
    {
      // Generic associates tests
      o = TypeHelper.CreateAssociate<ITestHandler<double>>(typeof(double), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(ObjectTestHandler<double>)));

      o = TypeHelper.CreateAssociate<ITestHandler<object>>(typeof(object), out foundFor, testHandlerSuffix, null, locations);
      Assert.That(o, Is.Not.Null);
      Assert.That(o.GetType(), Is.EqualTo(typeof(ObjectTestHandler)));
    }

    [Test]
    public void CreateDummyTypeTest()
    {
      Type t1 = TypeHelper.CreateDummyType("Test1", typeof(object), false);
      Assert.That(t1, Is.Not.Null);
      AssertEx.IsPatternMatch(t1.Name, "Test1*");
      TestLog.Info($"t1 name: {t1.GetFullName()}");
      
      Type t2 = TypeHelper.CreateDummyType("Test2", typeof(object), false);
      Assert.That(t2, Is.Not.Null);
      AssertEx.IsPatternMatch(t2.Name, "Test2*");
      TestLog.Info($"t2 name: {t2.GetFullName()}");

      Assert.That(t2, Is.Not.EqualTo(t1));

      Type listType = typeof (List<>);
      IList list = (IList)TypeHelper.Activate(listType.Assembly, listType.FullName, new Type[] {t1}, null);
      TestLog.Info($"List<t1> name: {list.GetType().GetFullName()}");
    }

    #region Nested types
    public class TestClass
    {
      public readonly string Value;

      public TestClass()
      {
        Value = "Default Constructor";
      }

      public TestClass(string value)
      {
        this.Value = value;
      }

      public TestClass(int intValue, string stringValue)
      {
        Value = intValue.ToString() + stringValue;
      }

#pragma warning disable IDE1006, IDE0051 // Naming Styles
      private static TestClass ttttt(int intValue, string stringValue)
#pragma warning restore IDE1006 // Naming Styles
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
      Assert.That("Default Constructor", Is.EqualTo(defaultConstructorInstance.Value));
      var customConstructorInstance1 = (TestClass)type.Activate(null, "Custom constructor");
      Assert.That("Custom constructor", Is.EqualTo(customConstructorInstance1.Value));
      var customConstructorInstance2 = (TestClass)type.Activate(null, 11, "test");
      Assert.That("11test", Is.EqualTo(customConstructorInstance2.Value));
      var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
      Assert.That(methods.Length, Is.EqualTo(3));
      var ctor1 = (TestClass)methods[0].Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic, null, new object[0], null);
      var ctor2 = (TestClass)methods[1].Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { "Custom constructor" }, null);
      var ctor3 =(TestClass) methods[2].Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { 11, "test" }, null);
      Assert.That(defaultConstructorInstance.Value, Is.EqualTo(ctor1.Value));
      Assert.That(customConstructorInstance1.Value, Is.EqualTo(ctor2.Value));
      Assert.That(customConstructorInstance2.Value, Is.EqualTo(ctor3.Value));
    }

    [Test]
    public void GetNameAndAddSuffixTest()
    {
      Type t = typeof (A<,>);
      string fName = t.GetFullName();
      Assert.That(fName, Is.EqualTo(t.Namespace + ".A<,>"));
      string sName = t.GetShortName();
      Assert.That(sName, Is.EqualTo("A<,>"));
      string wsName = TypeHelper.AddSuffix(fName, "#");
      Assert.That(wsName, Is.EqualTo(t.Namespace + ".A#<,>"));
      fName = t.FullName;
      wsName = TypeHelper.AddSuffix(fName, "#");
      Assert.That(wsName, Is.EqualTo(t.Namespace + ".A#`2"));

      t = typeof (A<int, string>);
      fName = t.GetFullName();
      Assert.That(fName, Is.EqualTo(t.Namespace + ".A<System.Int32,System.String>"));
      sName = t.GetShortName();
      Assert.That(sName, Is.EqualTo("A<Int32,String>"));
      wsName = TypeHelper.AddSuffix(fName, "#");
      Assert.That(wsName, Is.EqualTo(t.Namespace + ".A#<System.Int32,System.String>"));
      fName = t.FullName;
      wsName = TypeHelper.AddSuffix(fName, "#");
      AssertEx.IsPatternMatch(wsName, t.Namespace + ".A#`2[[*]]");

      t = typeof (A<int, string>.B<bool>);
      fName = t.GetFullName();
      Assert.That(fName, Is.EqualTo(t.Namespace + ".A<System.Int32,System.String>+B<System.Boolean>"));
      sName = t.GetShortName();
      Assert.That(sName, Is.EqualTo("A<Int32,String>+B<Boolean>"));

      t = t.GetGenericTypeDefinition().GetGenericArguments()[2];
      fName = t.GetFullName();
      Assert.That(fName, Is.EqualTo("T3"));
      sName = t.GetShortName();
      Assert.That(sName, Is.EqualTo("T3"));
    }

    [Test]
    public void ActivateTest()
    {
      o = TypeHelper.Activate(GetType().Assembly, typeof (A<int, string>).FullName, null, 1, "2");
      Assert.That(o, Is.Not.Null);
      A<int, string> a = o as A<int, string>;
      Assert.That(a, Is.Not.Null);
      Assert.That(a.F1, Is.EqualTo(1));
      Assert.That(a.F2, Is.EqualTo("2"));

      o = TypeHelper.Activate(GetType().Assembly, typeof (A<,>).FullName, 
        new Type[] {typeof(int), typeof(string)}, 1, "2");
      Assert.That(o, Is.Not.Null);
      a = o as A<int, string>;
      Assert.That(a, Is.Not.Null);
      Assert.That(a.F1, Is.EqualTo(1));
      Assert.That(a.F2, Is.EqualTo("2"));
    }

    [Test]
    public void IsNumericTest()
    {
      var numericTypes = new[] {
        typeof (byte),
        typeof (byte?),
        typeof (sbyte),
        typeof (sbyte?),
        typeof (short),
        typeof (short?),
        typeof (ushort),
        typeof (ushort?),
        typeof (int),
        typeof (int?),
        typeof (uint),
        typeof (uint?),
        typeof (long),
        typeof (long?),
        typeof (ulong),
        typeof (ulong?),
        typeof (float),
        typeof (float?),
        typeof (double),
        typeof (double?),
        typeof (decimal),
        typeof (decimal?)
      };

      var nonNumericTypes = new[] {
        typeof (string),
        typeof (char),
        typeof (char?),
        typeof (bool),
        typeof (bool?),
        typeof (DateTime),
        typeof (DateTime?),
        typeof (TimeSpan),
        typeof (TimeSpan?),
        typeof (Guid),
        typeof (Guid?),
        typeof (TypeCode),
        typeof (TypeCode?),
        typeof (byte[]),
        typeof (Key),
        this.GetType(),
      };

      foreach (var numericType in numericTypes) {
        Assert.That(numericType.IsNumericType(), Is.True);
      }

      foreach (var nonNumericType in nonNumericTypes) {
        Assert.That(nonNumericType.IsNumericType(), Is.False);
      }
    }

    [Test]
    public void IsNullableTest()
    {
      var nullableTypes = new[] {
        typeof (Nullable<>),
        typeof (byte?),
        typeof (sbyte?),
        typeof (short?),
        typeof (ushort?),
        typeof (int?),
        typeof (uint?),
        typeof (long?),
        typeof (ulong?),
        typeof (float?),
        typeof (double?),
        typeof (decimal?),
        typeof (Guid?)
      };

      var nonNullableTypes = new[] {
        typeof (string),
        typeof (char),
        typeof (bool),
        typeof (DateTime),
        typeof (TimeSpan),
        typeof (Guid),
        typeof (TypeCode),
        typeof (byte[]),
        typeof (Key),
        this.GetType()
      };

      foreach (var type in nullableTypes) {
        Assert.That(type.IsNullable(), Is.True);
      }

      foreach (var type in nonNullableTypes) {
        Assert.That(type.IsNullable(), Is.False);
      }
    }

    [Test]
    public void GenericIsNullableTest()
    {
      Assert.That(TypeHelper.IsNullable<Guid?>(), Is.True);
      Assert.That(TypeHelper.IsNullable<int?>(), Is.True);

      Assert.That(TypeHelper.IsNullable<int>(), Is.False);
      Assert.That(TypeHelper.IsNullable<string>(), Is.False);
    }

    [Test]
    public void IsValueTupleTest()
    {
      var tupleTypes = new[] {
        typeof (ValueTuple<int>),
        typeof (ValueTuple<int, int>),
        typeof (ValueTuple<int, int, int>),
        typeof (ValueTuple<int, int, int, int>),
        typeof (ValueTuple<int, int, int, int, int>),
        typeof (ValueTuple<int, int, int, int, int, int>),
        typeof (ValueTuple<int, int, int, int, int, int, int>),
        typeof (ValueTuple<int, int, int, int, int, int, int, int>)
      };

      var otherTypes = new[] {
        typeof (string),
        typeof (char),
        typeof (bool),
        typeof (DateTime),
        typeof (TimeSpan),
        typeof (Guid),
        typeof (TypeCode),
        typeof (byte[]),
        typeof (Key),
        this.GetType()
      };

      var startingToken = tupleTypes[0].MetadataToken;

      Assert.That(
        tupleTypes.Select(t => t.MetadataToken - startingToken).SequenceEqual(Enumerable.Range(0, tupleTypes.Length)),
        Is.True);

      foreach (var type in tupleTypes) {
        Assert.That(type.IsValueTuple(), Is.True);
      }

      foreach (var type in otherTypes) {
        Assert.That(type.IsValueTuple(), Is.False);
      }
    }
  }
}