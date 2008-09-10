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
using Xtensive.Storage.Tests.Model.Association;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage.Tests.Model.Association
{
  [HierarchyRoot(typeof (Generator), "Id")]
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
  public class AssociationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.Association");
      return config;
    }

    [Test]
    public void EntitySetCreation()
    {
      // Domain.Model.Dump();
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var a = new A();
          Assert.IsNotNull(a.ManyToMany);
          Assert.IsNotNull(a.ManyToOne);
          Assert.IsNotNull(a.ManyToZero);
        }
      }
    }

    [Test]
    public void OneToOneAssign()
    {
      // Domain.Model.Dump();
      using (Domain.OpenSession()) {
        C c;
        A a;
        using (var t = Transaction.Open()) {
          c = new C();
          a = new A();
          t.Complete();        
        }
        using (Transaction.Open()) {
          Assert.IsNull(a.OneToOne);
          Assert.IsNull(c.A);
          c.A = a;
          Assert.IsNotNull(a.OneToOne);
          Assert.IsNotNull(c.A);
          Assert.AreEqual(a.OneToOne, c);
          Assert.AreEqual(c.A, a);
        }
        using (Transaction.Open()) {
          Assert.IsNull(a.OneToOne);
          Assert.IsNull(c.A);
          a.OneToOne = c;
          Assert.IsNotNull(a.OneToOne);
          Assert.IsNotNull(c.A);
          Assert.AreEqual(a.OneToOne, c);
          Assert.AreEqual(c.A, a);
        }
      }
    }

    [Test]
    public void OneToOneChangeOwner()
    {
      // Domain.Model.Dump();
      using (Domain.OpenSession()) {        
        C c;
        A a1;
        A a2;
        using (var t = Transaction.Open()) {
          c = new C();
          a1 = new A();
          a2 = new A();
          t.Complete();
        }
        using (Transaction.Open()) {
          Assert.IsNull(a1.OneToOne);
          Assert.IsNull(a2.OneToOne);
          Assert.IsNull(c.A);
          c.A = a1;
          Assert.IsNotNull(a1.OneToOne);
          Assert.IsNull(a2.OneToOne);
          Assert.IsNotNull(c.A);
          Assert.AreEqual(a1.OneToOne, c);
          Assert.AreEqual(c.A, a1);
          //change owner
          c.A = a2;
          Assert.IsNull(a1.OneToOne);
          Assert.IsNotNull(a2.OneToOne);
          Assert.IsNotNull(c.A);
          Assert.AreEqual(a2.OneToOne, c);
          Assert.AreEqual(c.A, a2);
          // change back trough another class
          a1.OneToOne = c;
          Assert.IsNotNull(a1.OneToOne);
          Assert.IsNull(a2.OneToOne);
          Assert.IsNotNull(c.A);
          Assert.AreEqual(a1.OneToOne, c);
          Assert.AreEqual(c.A, a1);
        }
        using (Transaction.Open()) {
          Assert.IsNull(a1.OneToOne);
          Assert.IsNull(a2.OneToOne);
          Assert.IsNull(c.A);
        }
      }
    }

    [Test]
    public void OneToManyPairedAssign()
    {
      // Domain.Model.Dump();
      using (Domain.OpenSession()) {        
        using (Transaction.Open()) {
          var d = new D();
          var a = new A();
          Assert.IsNull(a.OneToMany);
          Assert.IsNotNull(d.As);
        }
      }
    }
  }
}