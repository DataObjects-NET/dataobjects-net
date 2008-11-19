// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.21

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class ImplementPrivateFieldAccessorTest
  {
    [PrivateFieldAccessorsAspect("field1", "field2")]
    public class TestClass
    {
      private int field1;
      private int field2;

      public int Field1
      {
        get { return field1; }
        set { field1 = value; }
      }

      public int Field2
      {
        get { return field2; }
        set { field2 = value; }
      }
    }

    [Test]
    public void CombinedTest()
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      TestClass c = new TestClass();

      var getField1 = DelegateHelper.CreateGetMemberDelegate<TestClass, int>("field1");
      var getField2 = DelegateHelper.CreateGetMemberDelegate<TestClass, int>("field2");

      int v1 = r.Next();
      int v2 = v1 + 1;

      c.Field1 = v1;
      Assert.AreEqual(v1, getField1(c));
      c.Field1 = v2;
      Assert.AreEqual(v2, getField1(c));

      c.Field2 = v1;
      Assert.AreEqual(v1, getField2(c));
      c.Field2 = v2;
      Assert.AreEqual(v2, getField2(c));
    }
  }
}