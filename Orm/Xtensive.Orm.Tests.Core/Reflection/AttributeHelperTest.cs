// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Core.Reflection
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

    [Id("IBase")]
    interface IBase
    {
      [Id("IBase.Property")]
      int Property
      {
        [Id("IBase.get_Property")]
        get; 
        [Id("IBase.set_Property")]
        set;
      }

      [Id("IBase.IMethod()")]
      void IMethod();
    }

    [Id("Base")]
    class Base : IBase
    {
      [Id("Base.Property")]
      public virtual int Property { 
        [Id("Base.get_Property")]
        get; 
        [Id("Base.set_Property")]
        private set; }

      [Id("Base.IBase.Property")]
      int IBase.Property
      {
        [Id("Base.IBase.get_Property")]
        get { return Property; }
        [Id("Base.IBase.set_Property")]
        set { Property = value; }
      }

      [Id("Base.NewProperty")]
      int NewProperty { get; set; }

      [Id("Base.StrangeProperty")]
      public int StrangeProperty { get; internal set; }

      [Id("Base.Method(object)")]
      protected virtual void Method(object o)
      {
      }

      [Id("Base.IMethod()")]
      public virtual void IMethod()
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

      [Id("Derived.NewProperty")]
      int NewProperty { get; set; }

      [Id("Derived.Method(string)")]
      protected virtual void Method(string s)
      {
      }
    }

    [Id("Last")]
    class Last : Derived,
      IBase
    {
      [Id("Last.Property")]
      public override int Property {
        get {
          return base.Property;
        }
      }

      [Id("Last.IBase.Property")]
      int IBase.Property {
        [Id("Last.IBase.get_Property")]
        get { return Property; }
        [Id("Last.IBase.set_Property")]
        set { return; }
      }

      [Id("Last.Method(object)")]
      protected override void Method(object o)
      {
      }

      [Id("Last.IMethod()")]
      public override void IMethod()
      {
      }
    }

    [Test]
    public void DebugTest()
    {
      Type i = typeof (IBase);
      Type b = typeof (Base);
      Type d = typeof (Derived);
      Type l = typeof (Last);

      TestLog.Info("{0}", i.GetProperty("Property").GetImplementation(b));
    }

    [Test]
    public void CombinedTest()
    {
      Type i = typeof (IBase);
      Type b = typeof (Base);
      Type d = typeof (Derived);
      Type l = typeof (Last);

      ValidateIds(() => b.GetProperty("StrangeProperty", BindingFlags.Instance | BindingFlags.Public)
        .GetSetMethod(true)
        .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll),
        "Base.StrangeProperty");

      ValidateIds(() => d.GetProperty("NewProperty", BindingFlags.Instance | BindingFlags.NonPublic)
        .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll),
        "Derived.NewProperty");

      ValidateIds(() => b.GetProperty("NewProperty", BindingFlags.Instance | BindingFlags.NonPublic)
        .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll),
        "Base.NewProperty");

      ValidateId(() => b.GetAttribute<IdAttribute>(AttributeSearchOptions.InheritAll), 
        "Base");
      ValidateId(() => l.GetAttribute<IdAttribute>(AttributeSearchOptions.InheritNone), 
        "Last");

      ValidateIds(() => l.GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll), 
        "Last, Derived, Base");
      ValidateIds(() => d.GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll), 
        "Derived, Base");

      ValidateIds(() => l.GetProperty("Property").GetGetMethod()
        .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll),
        "Last.Property, Derived.get_Property, Base.get_Property");

      ValidateIds(() => l.GetMethod("Method", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll),
        "Last.Method(object), Base.Method(object)");

      ValidateIds(() => d.GetMethod("Method", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll),
        "Derived.Method(string)");

      ValidateIds(() => (i.GetProperty("Property").GetImplementation(l) as PropertyInfo).GetGetMethod(true)
        .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll),
        "Last.IBase.get_Property, IBase.get_Property");

      ValidateIds(() => i.GetMethod("IMethod").GetImplementation(d)
        .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll),
        "Base.IMethod(), IBase.IMethod()");

      ValidateIds(() => i.GetMethod("IMethod").GetImplementation(l)
        .GetAttributes<IdAttribute>(AttributeSearchOptions.InheritAll),
        "Last.IMethod(), Base.IMethod(), IBase.IMethod()");
    }

    private static void ValidateId(Expression<Func<IdAttribute>> getAttribute, string expected)
    {
      TestLog.Info("Trying: {0}", getAttribute.ToString(true));
      var attribute = getAttribute.Compile().Invoke();
      TestLog.Info("  Expect: {0}", expected);
      TestLog.Info("  Actual: {0}", attribute.Value);
      Assert.AreEqual(expected, attribute.Value);
    }

    private static void ValidateIds(Expression<Func<IEnumerable<IdAttribute>>> getAttributes, string expected)
    {
      TestLog.Info("Trying: {0}", getAttributes.ToString(true));
      var attributes = getAttributes.Compile().Invoke();
      var actual = attributes.Select(a => a.Value).ToCommaDelimitedString();
      TestLog.Info("  Expect: {0}", expected);
      TestLog.Info("  Actual: {0}", actual);
      Assert.AreEqual(expected, actual);
    }
  }
}