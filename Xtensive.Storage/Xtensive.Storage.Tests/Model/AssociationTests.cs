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

namespace Xtensive.Storage.Tests.Model.Relations
{
  [HierarchyRoot(typeof (DefaultGenerator), "Id")]
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

    [Field]
    public IntermediateStructure1 IndirectA { get; set; }
  }

  public class B : Root
  {
  }

  public class C : Root
  {
    [Field(PairTo = "OneToOne")]
    public A A { get; set; }

    [Field]
    public IntermediateStructure1 IndirectA { get; set; }
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

  public class IntermediateStructure1 : Structure
  {
    [Field]
    public IntermediateStructure2 IntermediateStructure2 { get; set; }
  }

  public class IntermediateStructure2 : Structure
  {
    [Field]
    public A A { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  public class AssociationTests : AutoBuildTestFixture
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.Relations");
      return config;
    }

    [Test]
    public void MainTest()
    {
    }
  }
}