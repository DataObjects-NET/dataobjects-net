// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.ReferentialIntegrityModel;

namespace Xtensive.Storage.Tests.ReferentialIntegrityModel
{
  [HierarchyRoot(typeof (Generator), "Id")]
  public class Root : Entity
  {
    [Field]
    private int Id { get; set; }
  }

  public class A : Root
  {
    [Field(OnDelete = ReferentialAction.SetNull)]
    public B B { get; set; }

    [Field(OnDelete = ReferentialAction.Restrict)]
    public C C { get; set; }
  }

  public class B : Root
  {
    [Field]
    private int Id { get; set; }

    [Field(OnDelete = ReferentialAction.Cascade, PairTo = "B")]
    public A A { get; set; }
  }

  public class C : Root
  {
    [Field]
    private int Id { get; set; }

    [Field(OnDelete = ReferentialAction.Cascade, PairTo = "C")]
    public A A { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class ReferentialIntegrityTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.ReferentialIntegrityModel");
      return config;
    }

    [Test]
    public void MainTest()
    {
      Domain.Model.Dump();
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          A a = new A();
          a.B = new B();
          a.B.A = a;
          a.C = new C();
          a.C.A = a;
          Session.Current.Persist();
          Assert.AreEqual(1, Session.Current.All<A>().Count());
          Assert.AreEqual(1, Session.Current.All<B>().Count());
          Assert.AreEqual(1, Session.Current.All<C>().Count());

          a.B.Remove();
          Assert.AreEqual(null, a.B);
          AssertEx.Throws<ReferentialIntegrityException>(a.C.Remove);
          a.Remove();
          Assert.AreEqual(0, Session.Current.All<A>().Count());
          Assert.AreEqual(0, Session.Current.All<B>().Count());
          Assert.AreEqual(0, Session.Current.All<C>().Count());
          t.Complete();
        }
      }
    }
  }
}