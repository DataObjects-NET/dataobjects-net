// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.01

using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  internal interface IComposite
  {
    string First { get; set; }
    string Second { get; set; }
  }

  internal class A : IComposite
  {
    string IComposite.First { get; set; }
    public string Second { get; set; }

    public A()
    {}

    public A(string first, string second)
    {
      ((IComposite)this).First = first;
      Second = second;
    }
  }

  internal class B : A
  {
//    public string First { get; set; }
  }

  internal class C : IComposite
  {
    public string First { get; set; }
    public string Second { get; set; }
  }

  internal class D : C, IComposite
  {
    string IComposite.First { get; set; }
  }

  internal class E : D
  {
    
  }

  [TestFixture]
  public class InterfaceTest
  {
    [Test]
    public void Test()
    {
      var a = new A("First", "Second");
      var b = new B();
      b.Second = "B.Second";
      var i = (IComposite) b;
      i.First = "B.First";
      Assert.AreEqual("B.First", i.First);
      var e = new E();
      e.First = "First";
      e.Second = "Second";

      var c = (C) e;
      Assert.AreEqual("First", c.First);

      var ii = (IComposite) e;
      Assert.IsNull(ii.First);

      var guid1 = Guid.NewGuid();
      var guid2 = Guid.NewGuid();
      guid1.Equals(guid2);
    }
  }
}