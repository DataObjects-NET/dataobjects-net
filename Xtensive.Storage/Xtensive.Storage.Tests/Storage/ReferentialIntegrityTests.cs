// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Generators;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.ReferentialIntegrityModel;

namespace Xtensive.Storage.Tests.ReferentialIntegrityModel
{
  [HierarchyRoot(typeof (IncrementalGenerator), "Id")]
  public class A : Entity
  {
    [Field]
    private int Id { get; set; }

    [Field(OnDelete = ReferentialAction.SetNull, IsNullable = true)]
    public B B { get; set; }

    [Field(OnDelete = ReferentialAction.Restrict)]
    public C C { get; set; }
  }

  [HierarchyRoot(typeof (IncrementalGenerator), "Id")]
  public class B : Entity
  {
    [Field]
    private int Id { get; set; }

    [Field(OnDelete = ReferentialAction.Cascade, PairTo = "B")]
    public A A { get; set; }
  }

  [HierarchyRoot(typeof (IncrementalGenerator), "Id")]
  public class C : Entity
  {
    [Field]
    private int Id { get; set; }

    [Field(OnDelete = ReferentialAction.Cascade, PairTo = "C")]
    public A A { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class ReferentialIntegrityTests
  {
    [Test]
    public void MainTest()
    {
      DomainConfiguration config = new DomainConfiguration("memory://localhost/ReferentialIntegrityTests");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.ReferentialIntegrityModel");
      Domain domain = Domain.Build(config);
      domain.Model.Dump();
      using (domain.OpenSession()) {
        A a = new A();
        a.B = new B();
        a.B.A = a;
        a.C = new C();
        a.C.A = a;
        Session.Current.Persist();
        Assert.AreEqual(1, Session.Current.All<A>().Count<A>());
        Assert.AreEqual(1, Session.Current.All<B>().Count());
        Assert.AreEqual(1, Session.Current.All<C>().Count());

        a.B.Remove();
        Assert.AreEqual(null, a.B);
        AssertEx.Throws<ReferentialIntegrityException>(a.C.Remove);
        a.Remove();
        Assert.AreEqual(0, Session.Current.All<A>().Count());
        Assert.AreEqual(0, Session.Current.All<B>().Count());
        Assert.AreEqual(0, Session.Current.All<C>().Count());
      }
    }
  }
}