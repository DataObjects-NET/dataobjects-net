// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.21

using System;
using NUnit.Framework;
using Xtensive.Aspects.Helpers;
using Xtensive.Reflection;

namespace Xtensive.Aspects.Tests
{
  [TestFixture]
  public class ImplementFactoryMethodTest
  {
    [ImplementConstructor(typeof(int))]
    [ImplementFactoryMethod(typeof(int))]
    public class BaseClass
    {
      protected BaseClass(int i, bool ignore)
      {}
    }

    public class ProtectedCtorClassA : BaseClass
    {
      public int I { get; set; }

      protected ProtectedCtorClassA(int i)
        : base(i, true)
      {
        I = i;
      }
    }

    public class ProtectedCtorGenericA<T> : ProtectedCtorClassA
    {
      public T Ignored { get; set; }

      protected ProtectedCtorGenericA(T i)
        : base(typeof(T)==typeof(int) ? (int) (object) i : 0)
      {}
    }

    public class ProtectedCtorClassB : ProtectedCtorGenericA<int>
    {
      protected ProtectedCtorClassB(int i)
        : base(i)
      {
      }
    }

    public class ProtectedCtorClassC : BaseClass
    {
      protected ProtectedCtorClassC(int i, bool ignore)
        : base(i, ignore)
      {
      }
    }

    [ImplementFactoryMethod(typeof(int), typeof(bool))]
    public class ProtectedCtorClassD
    {
      protected ProtectedCtorClassD(int i, bool ignore)
      {
      }
    }
    
    public class CtorClassC
    {
      public double Value { get; set; }
      public string Text  { get; set; }

      public CtorClassC(double value, string text)
        : this(ref value, ref text)
      {
      }

      public CtorClassC(ref double value, ref string text)
      {
        Value = value;
        Text  = text;
      }
    }

    private delegate CtorClassC CtorClassCDelegate(ref double value, ref string text);

    [Test]
    public void ConstructorDelegatesTest()
    {
      var createA = DelegateHelper.CreateConstructorDelegate<Func<int, ProtectedCtorClassA>>(typeof (ProtectedCtorClassA));
      Assert.IsNotNull(createA);
      var createB = DelegateHelper.CreateConstructorDelegate<Func<int, ProtectedCtorClassA>>(typeof (ProtectedCtorClassB));
      Assert.IsNotNull(createB);
      var createC3 = DelegateHelper.CreateConstructorDelegate<Func<double, string, object>>(typeof(CtorClassC));
      Assert.IsNotNull(createC3);
      var createC2 = DelegateHelper.CreateConstructorDelegate<Func<double, string, CtorClassC>>(typeof(CtorClassC));
      Assert.IsNotNull(createC2);
      var createC1 = DelegateHelper.CreateConstructorDelegate<CtorClassCDelegate>(typeof(CtorClassC));
      Assert.IsNotNull(createC1);

      int i = 1; 
      ProtectedCtorClassA a = createA(i);
      Assert.IsNotNull(a);
      Assert.AreSame(typeof(ProtectedCtorClassA), a.GetType());
      Assert.AreEqual(i, a.I);

      a = createB(i);
      Assert.IsNotNull(a);
      Assert.AreSame(typeof(ProtectedCtorClassB), a.GetType());
      Assert.AreEqual(i, a.I);

      double d = 2.2;
      string s = "Text";
      CtorClassC c = createC1(ref d, ref s);
      Assert.IsNotNull(c);
      Assert.AreSame(typeof(CtorClassC), c.GetType());
      Assert.AreEqual(d, c.Value);
      Assert.AreEqual(s, c.Text);

      c = createC2(d, s);
      Assert.IsNotNull(c);
      Assert.AreSame(typeof(CtorClassC), c.GetType());
      Assert.AreEqual(d, c.Value);
      Assert.AreEqual(s, c.Text);

      c = (CtorClassC) createC3(d, s);
      Assert.IsNotNull(c);
      Assert.AreSame(typeof(CtorClassC), c.GetType());
      Assert.AreEqual(d, c.Value);
      Assert.AreEqual(s, c.Text);
    }
  }
}