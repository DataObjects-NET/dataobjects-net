// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.21

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Aspects.Helpers;
using Xtensive.Reflection;
using Xtensive.Testing;

namespace Xtensive.Aspects.Tests
{
  [TestFixture]
  public class AutoPropertyReplacementTest
  {
    public class TestClassBase
    {
      public Dictionary<string, object> properties = new Dictionary<string, object>();

      public T GetProperty<T>(int id)
      {
        throw new NotImplementedException();
      }

      public void SetProperty<T>(int id, T value)
      {
        throw new NotImplementedException();
      }

      public object GetProperty(string name)
      {
        throw new NotImplementedException();
      }

      public void SetProperty(string name, object value)
      {
        throw new NotImplementedException();
      }
      
      public T GetProperty<T>(string name)
      {
        return (T) properties[name];
      }
    
      public void SetProperty<T>(string name, T value)
      {
        properties[name] = value;
      }
    }

    [ReplaceAutoProperty("Property")]
    public class TestClass : TestClassBase
    {
      private int manualProperty;
      [SampleAspect]
      public int Property1 { get; set; }
      public virtual int Property2 { get; set; }
      public virtual int Property3 { get; set; }
      internal virtual int Property4 { get; set; }
      private int Property5 { get; set; }

      public int ManualProperty
      {
        get { return manualProperty; }
        set { manualProperty = value; }
      }
    }

    public class DerivedClass : TestClass
    {
      public new int Property1 { get; set; }
      public new int Property2 { get; set; }
      public override int Property3 { get; set; }
      internal new virtual int Property4 { get;set; }
      public int Property5 { get; set; }
      [NotSupported]
      public int Property8 { get; set; }
    }

    public class GenericClass<T> : TestClass
    {
      public T GenericProperty { get; set; }

      public int Property { get; set; }
    }

    [ReplaceAutoProperty("Property")]
    public interface IPropertyMarker
    {
      int InterfaceProperty { get; set; }
    }

    public class Implementation : TestClassBase, 
      IPropertyMarker
    {
      public int Property1 { get; set; }
      public int Property2 { get; set; }

      public int InterfaceProperty { get; set;}
    }

    public class ExplicitImplementation : Implementation,
      IPropertyMarker
    {
      public new int Property1 { get; set; }
      int IPropertyMarker.InterfaceProperty { get; set; }
    }

    [Test]
    public void CombinedTest()
    {
      var r = new Random();
      var c = new TestClass();
      var d = new DerivedClass();
      var i = new Implementation();
      var e = new ExplicitImplementation();
      var g = new GenericClass<int>();

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

      d.Property2 = v1;
      Assert.AreEqual(v1, d.Property2);
      d.Property2 = v2;
      Assert.AreEqual(v2, d.Property2);

      d.Property3 = v1;
      Assert.AreEqual(v1, d.Property3);
      d.Property3 = v2;
      Assert.AreEqual(v2, d.Property3);

      i.Property1 = v1;
      Assert.AreEqual(v1, i.Property1);
      i.Property2 = v2;
      Assert.AreEqual(v2, i.Property2);

      e.Property1 = v1;
      Assert.AreEqual(v1, e.Property1);
      e.Property2 = v2;
      Assert.AreEqual(v2, e.Property2);
      e.InterfaceProperty = v2;
      Assert.AreEqual(v2, e.InterfaceProperty);

      var marker = e as IPropertyMarker;
      marker.InterfaceProperty = v1;
      Assert.AreEqual(v1, marker.InterfaceProperty);
      Assert.AreEqual(v2, e.InterfaceProperty);

      g.GenericProperty = v1;
      Assert.AreEqual(v1, g.GenericProperty);
      g.GenericProperty = v2;
      Assert.AreEqual(v2, g.GenericProperty);
    }
  }
}