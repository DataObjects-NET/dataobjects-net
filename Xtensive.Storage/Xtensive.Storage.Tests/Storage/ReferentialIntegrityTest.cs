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
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class Root : Entity
  {
    [Field]
    private int Id { get; set; }
  }

  public class A : Root
  {
    [Field(OnRemove = ReferentialAction.Clear)]
    public B B { get; set; }

    [Field(OnRemove = ReferentialAction.Restrict)]
    public C C { get; set; }
  }

  public class B : Root
  {
    [Field(OnRemove = ReferentialAction.Cascade, PairTo = "B")]
    public A A { get; set; }
  }

  public class C : Root
  {
    [Field(OnRemove = ReferentialAction.Cascade, PairTo = "C")]
    public A A { get; set; }
  }

  public class Master : Root
  {
    [Field(OnRemove = ReferentialAction.Clear)]
    public EntitySet<Slave> Slaves { get; private set; }
  }

  public class Slave : Root
  {
    [Field(PairTo = "Slaves")]
    public Master Master { get; private set; }
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
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          A a = new A();
          a.B = new B();
          a.C = new C();
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

          Master m = new Master();
          m.Slaves.Add(new Slave());
          Assert.AreEqual(1, m.Slaves.Count);
          m.Slaves.First().Remove();
          Assert.AreEqual(0, m.Slaves.Count);
          t.Complete();
        }
      }
    }
  }
}