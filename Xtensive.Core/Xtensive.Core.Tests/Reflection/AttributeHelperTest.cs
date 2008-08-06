// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.06

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Reflection;
using Xtensive.Core.Testing;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Tests.Reflection
{
  [TestFixture]
  public class AttributeHelperTest
  {
    [AttributeUsage(AttributeTargets.All)]
    public class IdAttribute: Attribute
    {
      public string Value { get; set; }

      public IdAttribute(string value)
      {
        Value = value;
      }
    }

    [Id("Base")]
    class Base
    {
      [Id("Base.Property")]
      public virtual int Property { 
        [Id("Base.get_Property")]
        get; 
        [Id("Base.set_Property")]
        private set; }

      [Id("Base.Method(object)")]
      protected virtual void Method(object o)
      {
      }
    }

    [Id("Derived")]
    class Derived : Base
    {
      [Id("Derived.Property")]
      public override int Property {
        [Id("Derived.get_Property")]
        get {
          return base.Property;
        }
      }

      [Id("Derived.Method(string)")]
      protected virtual void Method(string s)
      {
      }
    }

    [Id("Last")]
    class Last : Derived
    {
      [Id("Last.Property")]
      public override int Property {
        get {
          return base.Property;
        }
      }

      [Id("Last.Method(object)")]
      protected override void Method(object o)
      {
      }
    }

    [Test]
    public void CombinedTest()
    {
      Type b = typeof (Base);
      Type d = typeof (Derived);
      Type l = typeof (Last);
      Assert.AreEqual("Base", b.GetAttribute<IdAttribute>(AttributeSearchOptions.InheritAll).Value);
      Assert.AreEqual("Last", l.GetAttribute<IdAttribute>(AttributeSearchOptions.InheritNone).Value);
      AssertEx.AreEqual(new[] {"Last", "Derived", "Base"}, 
        l.GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll).Select(a => a.Value));
      AssertEx.AreEqual(new[] {"Derived", "Base"}, 
        d.GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll).Select(a => a.Value));

      // TODO: Finish it

      Log.Info("{0}", 
        l.GetProperty("Property").GetGetMethod()
          .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll).Select(a => a.Value).ToCommaDelimitedString());

      Log.Info("{0}", 
        l.GetMethod("Method", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll).Select(a => a.Value).ToCommaDelimitedString());

      Log.Info("{0}", 
        d.GetMethod("Method", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll).Select(a => a.Value).ToCommaDelimitedString());
    }
  }
}