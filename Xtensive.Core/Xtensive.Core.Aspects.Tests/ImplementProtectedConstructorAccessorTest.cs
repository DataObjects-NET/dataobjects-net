// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.21

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class ImplementProtectedConstructorAccessorTest
  {
    [ImplementProtectedConstructorAccessorAspect(new [] { typeof(int) }, typeof(ProtectedCtorClassA))]
    public class ProtectedCtorClassB : ProtectedCtorClassA
    {
      protected ProtectedCtorClassB(int i)
        : base(i)
      {
      }
    }

    [ImplementProtectedConstructorAccessorAspect(new [] { typeof(int) }, typeof(ProtectedCtorClassA))]
    public class ProtectedCtorClassA
    {
      public int I { get; set; }

      protected ProtectedCtorClassA(int i)
      {
        I = i;
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
      var createC1 = DelegateHelper.CreateConstructorDelegate<CtorClassCDelegate>(typeof(CtorClassC));
      Assert.IsNotNull(createC1);
      var createC2 = DelegateHelper.CreateConstructorDelegate<Func<double, string, CtorClassC>>(typeof(CtorClassC));
      Assert.IsNotNull(createC2);

      int i = 1; 
      ProtectedCtorClassA a = createA(i);
      Assert.IsNotNull(a);
      Assert.AreSame(typeof(ProtectedCtorClassA), a.GetType());
      Assert.AreEqual(i, a.I);

      ProtectedCtorClassA b = createB(i);
      Assert.IsNotNull(b);
      Assert.AreSame(typeof(ProtectedCtorClassB), b.GetType());
      Assert.AreEqual(i, b.I);

      double d = 2.2;
      string s = "Text";
      CtorClassC c1 = createC1(ref d, ref s);
      Assert.IsNotNull(c1);
      Assert.AreSame(typeof(CtorClassC), c1.GetType());
      Assert.AreEqual(d, c1.Value);
      Assert.AreEqual(s, c1.Text);

      CtorClassC c2 = createC2(d, s);
      Assert.IsNotNull(c2);
      Assert.AreSame(typeof(CtorClassC), c2.GetType());
      Assert.AreEqual(d, c2.Value);
      Assert.AreEqual(s, c2.Text);
    }
  }
}