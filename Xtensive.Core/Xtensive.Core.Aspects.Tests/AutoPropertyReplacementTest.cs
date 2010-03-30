// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.21

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class AutoPropertyReplacementTest
  {
    public class TestClassBase
    {
      public Dictionary<string, object> properties = new Dictionary<string, object>();
      
      public T GetProperty<T>(string name)
      {
        return (T) properties[name];
      }
    
      public void SetProperty<T>(string name, T value)
      {
        properties[name] = value;
      }
    }

    public class TestClass: TestClassBase
    {
      public int Property1
      {
        [AutoPropertyReplacementAspect(typeof(TestClassBase), "Property")]
        get; 
        [AutoPropertyReplacementAspect(typeof(TestClassBase), "Property")]
        set;
      }

      public int Property2
      {
        [AutoPropertyReplacementAspect(typeof(TestClassBase), "Property")]
        get; 
        [AutoPropertyReplacementAspect(typeof(TestClassBase), "Property")]
        set;
      }
    }

    [Test]
    public void CombinedTest()
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      TestClass c = new TestClass();

      int v1 = r.Next();
      int v2 = v1 + 1;

      c.Property1 = v1;
      Assert.AreEqual(v1, c.Property1);
      c.Property1 = v2;
      Assert.AreEqual(v2, c.Property1);

      c.Property2 = v1;
      Assert.AreEqual(v1, c.Property2);
      c.Property2 = v2;
      Assert.AreEqual(v2, c.Property2);
    }
  }
}