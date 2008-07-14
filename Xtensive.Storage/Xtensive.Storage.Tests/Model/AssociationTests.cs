// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.KeyProviders;

namespace Xtensive.Storage.Tests.Model.Relations
{
  [HierarchyRoot(typeof (Int32Provider), "Id")]
  public abstract class Root : Entity
  {
    [Field]
    public int Id { get; private set; }
  }

  public class A : Root
  {
    [Field]
    public B OneToZero { get; set; }

    [Field]
    public C OneToOne { get; set; }

    [Field]
    public D OneToMany { get; set; }

    [Field]
    public EntitySet<E> ManyToZero { get; private set; }

    [Field]
    public EntitySet<F> ManyToOne { get; private set; }

    [Field]
    public EntitySet<G> ManyToMany { get; private set; }
  }

  public class B : Root
  {
  }

  public class C : Root
  {
    [Field(PairTo = "OneToOne")]
    public A A { get; set; }
  }

  public class D : Root
  {
    [Field(PairTo = "OneToMany")]
    public EntitySet<A> As { get; private set; }
  }

  public class E : Root
  {
  }

  public class F : Root
  {
    [Field(PairTo = "ManyToOne")]
    public A A { get; set; }
  }

  public class G : Root
  {
    [Field(PairTo = "ManyToMany")]
    public EntitySet<A> As { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class AssociationTests
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      DomainConfiguration config = new DomainConfiguration();
      config.ConnectionInfo = new UrlInfo("memory://localhost/relationtests");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.Relations");
      domain = Domain.Build(config);
      domain.Model.Dump();
    }

    [Test]
    public void MainTest()
    {
    }
  }
}