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
    public EntitySet<Slave> OneToMany { get; private set; }

    [Field(OnRemove = ReferentialAction.Clear)]
    public EntitySet<Slave> ZeroToMany { get; private set; }

    [Field(OnRemove = ReferentialAction.Clear)]
    public EntitySet<Slave> ManyToMany { get; private set; }
  }

  public class Slave : Root
  {
    [Field(PairTo = "OneToMany")]
    public Master ManyToOne { get; private set; }

    [Field(PairTo = "ManyToMany")]
    public EntitySet<Master> ManyToMany { get; private set; }
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
          Assert.AreEqual(1, Query<A>.All.Count());
          Assert.AreEqual(1, Query<B>.All.Count());
          Assert.AreEqual(1, Query<C>.All.Count());

          a.B.Remove();
          Assert.AreEqual(null, a.B);
          AssertEx.Throws<ReferentialIntegrityException>(a.C.Remove);
          a.Remove();
          Assert.AreEqual(0, Query<A>.All.Count());
          Assert.AreEqual(0, Query<B>.All.Count());
          Assert.AreEqual(0, Query<C>.All.Count());

          Master m = new Master();
          m.OneToMany.Add(new Slave());
          m.OneToMany.Add(new Slave());
          m.OneToMany.Add(new Slave());
          Assert.AreEqual(3, m.OneToMany.Count);
          Assert.AreEqual(3, m.FindReferencingObjects(m.Type.Fields["OneToMany"].Association.Reversed).Count());
          m.OneToMany.First().Remove();
          Assert.AreEqual(2, m.OneToMany.Count);

          m.ZeroToMany.Add(new Slave());
          m.ZeroToMany.Add(new Slave());
          m.ZeroToMany.Add(new Slave());
          Assert.AreEqual(3, m.ZeroToMany.Count);
          m.ZeroToMany.First().Remove();
          Assert.AreEqual(2, m.ZeroToMany.Count);

          m.ManyToMany.Add(new Slave());
          m.ManyToMany.Add(new Slave());
          m.ManyToMany.Add(new Slave());
          Assert.AreEqual(3, m.ManyToMany.Count);
          m.ManyToMany.First().Remove();
          Assert.AreEqual(2, m.ManyToMany.Count);

          t.Complete();
        }
      }
    }
  }
}