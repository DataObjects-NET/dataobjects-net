// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.21

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects.Helpers;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class ImplementConstructorEpilogueAspectTest
  {
    public class HandlerModule
    {
      public static TestClass Class;

      public void HandlerMethod(Type type)
      {
        if (Class != null)
          Class.Seed = 20;
      }
    }

    public class TestClass
    {
      private int seed;

      public int Seed
      {
        get { return seed; }  
        set { seed = value; }
      }

      private void HandlerMethod(Type type)
      {
        seed = 10;
      }

      [ImplementConstructorEpilogueAspect(typeof(TestClass), "HandlerMethod")]
      public TestClass()
      {
      }

      [ImplementConstructorEpilogueAspect(typeof(HandlerModule), "HandlerMethod")]
      public TestClass(bool b)
      {
      }
    }

    [Test]
    public void Test()
    {
      TestClass testClass = new TestClass();
      Assert.AreEqual(10, testClass.Seed);
      HandlerModule.Class = testClass;
      TestClass testClass2 = new TestClass(true);
      Assert.AreEqual(20, testClass.Seed);
      Assert.AreEqual(0, testClass2.Seed);
    }
  }
}