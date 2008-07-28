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
      public int I { get; set; }
      public string Text { get; set; }

      public CtorClassC(int i, ref string text)
      {
        I = i;
        Text = text;
      }
    }

    private delegate CtorClassC CtorClassCDelegate(int i, ref string text);

    [Test]
    public void ConstructorDelegatesTest()
    {
      var createA = DelegateHelper.CreateProtectedConstructorDelegate<Func<int, ProtectedCtorClassA>>(typeof (ProtectedCtorClassA));
      Assert.IsNotNull(createA);
      var createB = DelegateHelper.CreateProtectedConstructorDelegate<Func<int, ProtectedCtorClassA>>(typeof (ProtectedCtorClassB));
      Assert.IsNotNull(createB);

      var createC = (CtorClassCDelegate)DelegateHelper.CreateConstructorDelegate(typeof(CtorClassC), typeof(CtorClassCDelegate));
      Assert.IsNotNull(createC);

      int i = 1; 
      ProtectedCtorClassA a = createA(i);
      Assert.IsNotNull(a);
      Assert.AreSame(typeof(ProtectedCtorClassA), a.GetType());
      Assert.AreEqual(i, a.I);

      ProtectedCtorClassA b = createB(i);
      Assert.IsNotNull(b);
      Assert.AreSame(typeof(ProtectedCtorClassB), b.GetType());
      Assert.AreEqual(i, b.I);

      string text = "Text";
      CtorClassC c = createC(i, ref text);
      Assert.IsNotNull(c);
      Assert.AreSame(typeof(CtorClassC), c.GetType());
      Assert.AreEqual(i, c.I);
      Assert.AreEqual(text, c.Text);
    }
  }
}