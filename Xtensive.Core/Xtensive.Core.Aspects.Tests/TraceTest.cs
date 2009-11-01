// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using System;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class TraceTest
  {
    public const int BaseCount = 100000;
    public TraceSample Sample = new TraceSample();
    
    [TestFixtureSetUp]
    public void Setup()
    {
      Log.Info("Test setup completed.");
    }
    
    [Test]
    public void StaticMethodTest()
    {
      TraceSample.StaticVoid();
    }

    [Test]
    public void InstanceMethodTest1()
    {
      Assert.IsFalse(Sample.WriteMessage("Message"));
      Assert.IsTrue (Sample.WriteMessage("AnotherOne"));
    }

    [Test]
    public void InstanceMethodTest2()
    {
      Sample.SimpleMethod("A", "B");
    }
    
    [Test]
    public void InstanceMethodTest3()
    {
      Sample.ComplexMethod();
      Sample.ComplexMethod(null);
      Sample.ComplexMethod("1", "2");
      Sample.ComplexMethod(1, new object[] {"2", 3}, "4");
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ExceptionTest1()
    {
      Sample.WriteMessage("");
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ExceptionTest2()
    {
      Sample.WriteMessage(null);
    }

    [Test]
    [ExpectedException(typeof(ApplicationException))]
    public void ExceptionTest3()
    {
      Sample.ComplexMethod(new ApplicationException("Test error"));
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      int count = BaseCount;
      Sample.DoNothing();
      using (new Measurement("Performance test", count)) {
        for (int i = 0; i<count; i++)
          Sample.DoNothing();
      }
    }
  }
}