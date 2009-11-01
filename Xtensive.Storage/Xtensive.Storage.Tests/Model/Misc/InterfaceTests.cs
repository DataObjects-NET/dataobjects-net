// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.23

using NUnit.Framework;
using Xtensive.Storage.Tests.Model.Misc.NET;

namespace Xtensive.Storage.Tests.Model.Misc
{
  namespace NET
  {
    internal interface IHasName
    {
      string Name { get; set; }
    }

    internal interface ICreature : IHasName
    {
    }

    internal interface IHasName2
    {
      string Name { get; set; }
    }

    internal class A : IHasName, IHasName2
    {
      string IHasName.Name { get; set; }
      public virtual string Name { get; set; }
    }

    internal class B : A, IHasName
    {
      public override string Name { get; set; }
    }

    internal class C : ICreature
    {
      string IHasName.Name { get; set; }
    }
  }

  [TestFixture]
  public class InterfaceTests
  {
    [Test]
    public void NETTest()
    {
      B b = new B();
      b.Name = "Name";

      Assert.AreEqual("Name", ((IHasName) b).Name);

      A a = b;
      Assert.AreEqual("Name", ((IHasName) a).Name);
      Assert.AreEqual("Name", ((IHasName2) a).Name);
    }
  }
}