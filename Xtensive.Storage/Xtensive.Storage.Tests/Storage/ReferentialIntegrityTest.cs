// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ReferentialIntegrityModel;

namespace Xtensive.Storage.Tests.ReferentialIntegrityModel
{
  [HierarchyRoot]
  public class Root : Entity
  {
    [Field, Key]
    private int Id { get; set; }
  }

  public class A : Root
  {
    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public B B { get; set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Deny)]
    public C C { get; set; }

    [Field]
    public string Name { get; set; }
  }

  public class B : Root
  {
    [Field, Association(OnTargetRemove = OnRemoveAction.Cascade, PairTo = "B")]
    public A A { get; set; }
  }

  public class C : Root
  {
    [Field, Association(OnTargetRemove = OnRemoveAction.Cascade, PairTo = "C")]
    public A A { get; set; }
  }

  public class Master : Root
  {
    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Slave> OneToMany { get; private set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Slave> ZeroToMany { get; private set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Slave> ManyToMany { get; private set; }
  }

  public class Slave : Root
  {
    [Field, Association(PairTo = "OneToMany")]
    public Master ManyToOne { get; private set; }

    [Field, Association(PairTo = "ManyToMany")]
    public EntitySet<Master> ManyToMany { get; private set; }
  }

  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
    public Package Package1 { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
    public Package Package2 { get; set; }
  }

  [HierarchyRoot]
  public class Package : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Deny)]
    public EntitySet<PackageItem> Items { get; private set; }
  }

  [HierarchyRoot]
  public class PackageItem : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class ReferentialIntegrityTest : AutoBuildTest
  {
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
          // Session.Current.Persist();
          Assert.AreEqual(0, Query<A>.All.Count());
          Assert.AreEqual(0, Query<B>.All.Count());
          Assert.AreEqual(0, Query<C>.All.Count());

          Master m = new Master();
          m.OneToMany.Add(new Slave());
          m.OneToMany.Add(new Slave());
          m.OneToMany.Add(new Slave());
          Assert.AreEqual(3, m.OneToMany.Count);
          Assert.AreEqual(3, ReferentialHelper.FindReferencingEntities(m, m.Type.Fields["OneToMany"].Association.Reversed).Count());
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

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.ReferentialIntegrityModel");
      return config;
    }

    [Test]
    public void RemoveWithException()
    {
      using (Domain.OpenSession()) {
        A a;
        C c;
        using (var t = Transaction.Open()) {
          a = new A();
          c = new C();
          a.C = c;
          Log.Debug(a.Key.ToString());
          Log.Debug(c.Key.ToString());
          a.Remove();
          Session.Current.Persist();
        }
      }
    }

    [Test]
    public void DeletedEntityAddToReference()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          A a = new A();
          B b = new B();
          b.Remove();
          AssertEx.ThrowsInvalidOperationException(() => a.B = b);
        }
      }
    }

    [Test]
    public void DeletedEntityChangeFields()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          A a = new A();
          a.Remove();
          AssertEx.ThrowsInvalidOperationException(() => a.Name = "newName");
        }
      }
    }

    [Test]
    public void DeepCascadeRemoveTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var c = new Container();
          c.Package1 = new Package();
          c.Package2 = new Package();
          var item = new PackageItem();
          c.Package1.Items.Add(item);
          c.Package2.Items.Add(item);

          c.Remove();
        }
      }
    }
  }
}