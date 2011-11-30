// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.Association;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Xtensive.Orm.Tests.Model.Association
{
  [Serializable]
  [HierarchyRoot]
  public abstract class Root : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public abstract class Root2 : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  [Serializable]
  public class A : Root
  {
    [Field]
    public B ZeroToOne { get; set; }

    [Field]
    public C OneToOneMaster { get; set; }

    [Field]
    public D ManyToOneMaster { get; set; }

    [Field]
    public EntitySet<E> ZeroToMany { get; private set; }

    [Field]
    public EntitySet<F> OneToManyMaster { get; private set; }

    [Field]
    public EntitySet<G> ManyToManyMaster { get; private set; }

    [Field]
    public IntermediateStructure1 IndirectA { get; set; }
  }

  [Serializable]
  public class B : Root2
  {
  }

  [Serializable]
  public class C : Root2
  {
    [Field, Association(PairTo = "OneToOneMaster")]
    public A OneToOnePaired { get; set; }

    [Field]
    public IntermediateStructure1 IndirectA { get; set; }
  }

  [Serializable]
  public class D : Root2
  {
    [Field, Association(PairTo = "ManyToOneMaster")]
    public EntitySet<A> OneToManyPaired { get; private set; }
  }

  [Serializable]
  public class E : Root2
  {
  }

  [Serializable]
  public class F : Root2
  {
    [Field, Association(PairTo = "OneToManyMaster")]
    public A ManyToOnePaired { get; set; }
  }

  [Serializable]
  public class G : Root2
  {
    [Field, Association(PairTo = "ManyToManyMaster")]
    public EntitySet<A> ManytoManyPaired { get; private set; }
  }

  [Serializable]
  public class H : Root2
  {
    [Field]
    public H Parent { get; set; }

    [Field, Association(PairTo = "Parent")]
    public EntitySet<H> Children { get; private set; }
  }

  public class I : A
  {
    
  }

  [Serializable]
  public class IntermediateStructure1 : Structure
  {
    [Field]
    public IntermediateStructure2 IntermediateStructure2 { get; set; }
  }

  [Serializable]
  public class IntermediateStructure2 : Structure
  {
    [Field]
    public A A { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class AssociationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(A).Namespace);
      return config;
    }

    [Test]
    public void ToRecordsetTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var a1 = new A();
          var a2 = new A();
          var rs = a1.TypeInfo.Indexes.PrimaryIndex.ToRecordQuery();

          foreach (Tuple tuple in rs.ToRecordSet(Session.Current)) {
            var rs2 = a1.TypeInfo.Indexes.PrimaryIndex.ToRecordQuery();
            foreach (Tuple tuple2 in rs2.ToRecordSet(Session.Current)) {
              Log.Debug(tuple2.ToString());
            }
          }
          // Rollback
        }
      }
    }

    [Test]
    public void EntitySetCreation()
    {
      // Domain.Model.Dump();
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var a = new A();
          Assert.IsNotNull(a.ManyToManyMaster);
          Assert.IsNotNull(a.OneToManyMaster);
          Assert.IsNotNull(a.ZeroToMany);
          // Rollback
        }
      }
    }

    [Test]
    public void SimpleEntitySetEnumerator()
    {
      using (var session = Domain.OpenSession()) {
        A a;
        F f1;
        F f2;
        F f3;
        F f4;
        using (var transaction = session.OpenTransaction()) {
          a = new A();
          f1 = new F();
          f2 = new F();
          f3 = new F();
          f4 = new F();
          transaction.Complete();
        }
        IEnumerator<F> enumerator; 
        using (var transaction = session.OpenTransaction()) {
          Assert.AreEqual(0, a.OneToManyMaster.Count()); // Linq count through enumerate
          f1.ManyToOnePaired = a;
          f2.ManyToOnePaired = a;
          f3.ManyToOnePaired = a;
          f4.ManyToOnePaired = a;

          Assert.AreEqual(4, a.OneToManyMaster.Count()); // Enumerate through internal cacee
          foreach (F f in a.OneToManyMaster) {
            a.OneToManyMaster.Contains(f); // Enumerate through internal cahce
          }
          enumerator = a.OneToManyMaster.GetEnumerator();
          Assert.IsTrue(enumerator.MoveNext());
          Assert.IsTrue(enumerator.MoveNext());
          transaction.Complete();
        }
        // Cache is invalidated here
        using (session.OpenTransaction()) {
          Assert.AreEqual(4, a.OneToManyMaster.Count()); // Enumerate through recordset request
          foreach (F f in a.OneToManyMaster) {
            // a.OneToManyMaster.Contains(f.Key); // Enumerate through recordset request
            a.OneToManyMaster.Contains(f); // Enumerate through recordset request
          }
          // Rollback
        }
      }
    }

    [Test]
    public void OneToOneAssign()
    {
      // Domain.Model.Dump();
      using (var session = Domain.OpenSession()) {
        C c;
        A a;

        using (var t = session.OpenTransaction()) {
          c = new C();
          a = new A();
          t.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(a.OneToOneMaster);
          Assert.IsNull(c.OneToOnePaired);
          c.OneToOnePaired = a;
          Assert.IsNotNull(a.OneToOneMaster);
          Assert.IsNotNull(c.OneToOnePaired);
          Assert.AreEqual(a.OneToOneMaster, c);
          Assert.AreEqual(c.OneToOnePaired, a);
          c.OneToOnePaired = a;
          Assert.IsNotNull(a.OneToOneMaster);
          Assert.IsNotNull(c.OneToOnePaired);
          Assert.AreEqual(a.OneToOneMaster, c);
          Assert.AreEqual(c.OneToOnePaired, a);
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(a.OneToOneMaster);
          Assert.IsNull(c.OneToOnePaired);
          a.OneToOneMaster = c;
          Assert.IsNotNull(a.OneToOneMaster);
          Assert.IsNotNull(c.OneToOnePaired);
          Assert.AreEqual(a.OneToOneMaster, c);
          Assert.AreEqual(c.OneToOnePaired, a);
        }
      }
    }

    [Test]
    public void OneToOneChangeOwner()
    {
      // Domain.Model.Dump();
      using (var session = Domain.OpenSession()) {        
        C c;
        A a1;
        A a2;

        using (var t = session.OpenTransaction()) {
          c = new C();
          a1 = new A();
          a2 = new A();
          t.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(a1.OneToOneMaster);
          Assert.IsNull(a2.OneToOneMaster);
          Assert.IsNull(c.OneToOnePaired);
          c.OneToOnePaired = a1;
          Assert.IsNotNull(a1.OneToOneMaster);
          Assert.IsNull(a2.OneToOneMaster);
          Assert.IsNotNull(c.OneToOnePaired);
          Assert.AreEqual(a1.OneToOneMaster, c);
          Assert.AreEqual(c.OneToOnePaired, a1);
          // Change owner
          c.OneToOnePaired = a2;
          Assert.IsNull(a1.OneToOneMaster);
          Assert.IsNotNull(a2.OneToOneMaster);
          Assert.IsNotNull(c.OneToOnePaired);
          Assert.AreEqual(a2.OneToOneMaster, c);
          Assert.AreEqual(c.OneToOnePaired, a2);
          // Change back trough another class
          a1.OneToOneMaster = c;
          Assert.IsNotNull(a1.OneToOneMaster);
          Assert.IsNull(a2.OneToOneMaster);
          Assert.IsNotNull(c.OneToOnePaired);
          Assert.AreEqual(a1.OneToOneMaster, c);
          Assert.AreEqual(c.OneToOnePaired, a1);
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(a1.OneToOneMaster);
          Assert.IsNull(a2.OneToOneMaster);
          Assert.IsNull(c.OneToOnePaired);
          // Rollback
        }
      }
    }

    [Test]
    public void ManyToOneAssign()
    {
      using (var session = Domain.OpenSession()) {
        A a;
        F f;

        using (var transaction = session.OpenTransaction()) {
          a = new A();
          f = new F();
          transaction.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(f.ManyToOnePaired);
          Assert.IsNotNull(a.OneToManyMaster);
          Assert.AreEqual(0, a.OneToManyMaster.Count);
          Assert.IsFalse(a.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a.OneToManyMaster.Contains(f));
          f.ManyToOnePaired = a;
          Assert.IsNotNull(f.ManyToOnePaired);
          Assert.AreEqual(1, a.OneToManyMaster.Count);
          Assert.IsTrue(a.OneToManyMaster.Contains(f.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f));
          f.ManyToOnePaired = a;
          Assert.IsNotNull(f.ManyToOnePaired);
          Assert.AreEqual(1, a.OneToManyMaster.Count);
          Assert.IsTrue(a.OneToManyMaster.Contains(f.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f));
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(f.ManyToOnePaired);
          Assert.IsNotNull(a.OneToManyMaster);
          Assert.AreEqual(0, a.OneToManyMaster.Count);
          Assert.IsFalse(a.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a.OneToManyMaster.Contains(f));
          // Rollback
        }
        
        // Assign back and commit
        using (var transaction = session.OpenTransaction()) {
          f.ManyToOnePaired = a;
          Assert.IsNotNull(f.ManyToOnePaired);
          Assert.IsTrue(a.OneToManyMaster.Contains(f.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f));
          Assert.AreEqual(1, a.OneToManyMaster.Count);
          transaction.Complete();
        }

        // Check commited state
        using (session.OpenTransaction()) {
          Assert.IsNotNull(f.ManyToOnePaired);
          Assert.AreEqual(1, a.OneToManyMaster.Count);
          Assert.IsTrue(a.OneToManyMaster.Contains(f.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f));
          // Rollback
        }
      }
    }

    [Test]
    public void ManyToOneAddToSet()
    {
      using (var session = Domain.OpenSession()) {
        A a;
        F f;

        using (var transaction = session.OpenTransaction()) {
          a = new A();
          f = new F();
          transaction.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(f.ManyToOnePaired);
          Assert.IsNotNull(a.OneToManyMaster);
          Assert.AreEqual(0, a.OneToManyMaster.Count);
          Assert.IsFalse(a.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a.OneToManyMaster.Contains(f));
          a.OneToManyMaster.Add(f);
          Assert.IsNotNull(f.ManyToOnePaired);
          Assert.AreEqual(f.ManyToOnePaired, a);
          Assert.AreEqual(1, a.OneToManyMaster.Count);
          Assert.IsTrue(a.OneToManyMaster.Contains(f.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f));
          a.OneToManyMaster.Add(f);
          Assert.IsNotNull(f.ManyToOnePaired);
          Assert.AreEqual(f.ManyToOnePaired, a);
          Assert.AreEqual(1, a.OneToManyMaster.Count);
          Assert.IsTrue(a.OneToManyMaster.Contains(f.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f));
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(f.ManyToOnePaired);
          Assert.IsNotNull(a.OneToManyMaster);
          Assert.AreEqual(0, a.OneToManyMaster.Count);
          Assert.IsFalse(a.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a.OneToManyMaster.Contains(f));
          // Rollback
        }
      }
    }

    [Test]
    public void ManyToOneChangeOwnerByAssign()
    {
      using (var session = Domain.OpenSession()) {
        A a;
        F f1;
        F f2;

        using (var transaction = session.OpenTransaction()) {
          a = new A();
          f1 = new F();
          f2 = new F();
          transaction.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(f1.ManyToOnePaired);
          Assert.IsNull(f2.ManyToOnePaired);
          Assert.IsNotNull(a.OneToManyMaster);
          Assert.AreEqual(0, a.OneToManyMaster.Count);
          Assert.IsFalse(a.OneToManyMaster.Contains(f1.Key));
          Assert.IsFalse(a.OneToManyMaster.Contains(f1));
          Assert.IsFalse(a.OneToManyMaster.Contains(f2.Key));
          Assert.IsFalse(a.OneToManyMaster.Contains(f2));
          f1.ManyToOnePaired = a;
          Assert.IsNotNull(f1.ManyToOnePaired);
          Assert.AreEqual(1, a.OneToManyMaster.Count);
          Assert.IsTrue(a.OneToManyMaster.Contains(f1.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f1));
          Assert.IsFalse(a.OneToManyMaster.Contains(f2.Key));
          Assert.IsFalse(a.OneToManyMaster.Contains(f2));
          f2.ManyToOnePaired = a;
          Assert.IsNotNull(f1.ManyToOnePaired);
          Assert.IsNotNull(f2.ManyToOnePaired);
          Assert.AreEqual(2, a.OneToManyMaster.Count);
          Assert.IsTrue(a.OneToManyMaster.Contains(f1));
          Assert.IsTrue(a.OneToManyMaster.Contains(f1.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f2.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f2));
          f1.ManyToOnePaired = null;
          Assert.IsNull(f1.ManyToOnePaired);
          Assert.IsNotNull(f2.ManyToOnePaired);
          Assert.IsNotNull(a.OneToManyMaster);
          Assert.AreEqual(1, a.OneToManyMaster.Count);
          Assert.IsFalse(a.OneToManyMaster.Contains(f1.Key));
          Assert.IsFalse(a.OneToManyMaster.Contains(f1));
          Assert.IsTrue(a.OneToManyMaster.Contains(f2.Key));
          Assert.IsTrue(a.OneToManyMaster.Contains(f2));
          f1.ManyToOnePaired = a;
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(f1.ManyToOnePaired);
          Assert.IsNull(f2.ManyToOnePaired);
          Assert.IsNotNull(a.OneToManyMaster);
          Assert.AreEqual(0, a.OneToManyMaster.Count);
          Assert.IsFalse(a.OneToManyMaster.Contains(f1.Key));
          Assert.IsFalse(a.OneToManyMaster.Contains(f1));
          // Rollback
        }
      }
    }

    [Test]
    public void ManyToOneChangeOwnerByEntitySet()
    {
      using (var session = Domain.OpenSession()) {
        A a1;
        A a2;
        F f;

        using (var transaction = session.OpenTransaction()) {
          a1 = new A();
          a2 = new A();
          f = new F();
          transaction.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(f.ManyToOnePaired);
          Assert.IsNotNull(a1.OneToManyMaster);
          Assert.IsNotNull(a2.OneToManyMaster);
          Assert.IsFalse(a1.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a1.OneToManyMaster.Contains(f));
          Assert.IsFalse(a2.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a2.OneToManyMaster.Contains(f));
          Assert.AreEqual(0, a1.OneToManyMaster.Count);
          Assert.AreEqual(0, a2.OneToManyMaster.Count);
          a1.OneToManyMaster.Add(f);
          Assert.IsNotNull(f.ManyToOnePaired);
          Assert.AreEqual(f.ManyToOnePaired, a1);
          Assert.AreEqual(1, a1.OneToManyMaster.Count);
          Assert.AreEqual(0, a2.OneToManyMaster.Count);
          Assert.IsTrue(a1.OneToManyMaster.Contains(f.Key));
          Assert.IsTrue(a1.OneToManyMaster.Contains(f));
          Assert.IsFalse(a2.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a2.OneToManyMaster.Contains(f));
          a1.OneToManyMaster.Remove(f);
          Assert.IsNull(f.ManyToOnePaired);
          Assert.IsNotNull(a1.OneToManyMaster);
          Assert.IsNotNull(a2.OneToManyMaster);
          Assert.IsFalse(a1.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a1.OneToManyMaster.Contains(f));
          Assert.IsFalse(a2.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a2.OneToManyMaster.Contains(f));
          Assert.AreEqual(0, a1.OneToManyMaster.Count);
          Assert.AreEqual(0, a2.OneToManyMaster.Count);
          a1.OneToManyMaster.Add(f);
          // Change owner
          a2.OneToManyMaster.Add(f);
          Assert.IsNotNull(f.ManyToOnePaired);
          Assert.AreEqual(f.ManyToOnePaired, a2);
          Assert.AreEqual(0, a1.OneToManyMaster.Count);
          Assert.AreEqual(1, a2.OneToManyMaster.Count);
          Assert.IsFalse(a1.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a1.OneToManyMaster.Contains(f));
          Assert.IsTrue(a2.OneToManyMaster.Contains(f.Key));
          Assert.IsTrue(a2.OneToManyMaster.Contains(f));
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(f.ManyToOnePaired);
          Assert.IsNotNull(a1.OneToManyMaster);
          Assert.IsNotNull(a2.OneToManyMaster);
          Assert.IsFalse(a1.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a1.OneToManyMaster.Contains(f));
          Assert.IsFalse(a2.OneToManyMaster.Contains(f.Key));
          Assert.IsFalse(a2.OneToManyMaster.Contains(f));
          Assert.AreEqual(0, a1.OneToManyMaster.Count);
          Assert.AreEqual(0, a2.OneToManyMaster.Count);
          // Rollback
        }
      }
    }

    [Test]
    public void OneToZero()
    {
      Require.ProviderIs(StorageProvider.Sql);

      using (var session = Domain.OpenSession()) {
        A a;
        B b;
        using (var transaction = session.OpenTransaction()) {
          a = new A();
          b = new B();
          transaction.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.IsNull(a.ZeroToOne);
          a.ZeroToOne = b;
          Assert.AreEqual(a.ZeroToOne, b);
          a.ZeroToOne = null;
          Assert.IsNull(a.ZeroToOne);
          a.ZeroToOne = b;
          // Rollback
        }

        using (var transactionScope = session.OpenTransaction()) {
          Assert.IsNull(a.ZeroToOne);
          a.ZeroToOne = b;
          transactionScope.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.AreEqual(a.ZeroToOne, b);
          // Rollback
        }
      }
    }

    [Test]
    public void EntitySetMultipleAddRemove(){
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          A a1 = new A();
          A a2 = new A();
          G g1 = new G();
          g1.ManytoManyPaired.Add(a1);
          g1.ManytoManyPaired.Add(a1);
          Assert.AreEqual(1, g1.ManytoManyPaired.Count);
        }
      }
    }

    [Test]
    public void ManyToMany()
    {
      using (var session = Domain.OpenSession()) {
        A a1;
        A a2;
        G g1;
        G g2;

        using (var transaction = session.OpenTransaction()) {
          a1 = new A();
          a2 = new A();
          g1 = new G();
          g2 = new G();
          transaction.Complete();
        }

        using (var t = session.OpenTransaction()) {
          Assert.IsNotNull(a1.ManyToManyMaster);
          Assert.IsNotNull(g1.ManytoManyPaired);
          Assert.AreEqual(0, a1.ManyToManyMaster.Count);
          Assert.AreEqual(0, g1.ManytoManyPaired.Count);
//          Assert.AreEqual(0, g2.ManyToOnePaired.Count);
          a1.ManyToManyMaster.Add(g2);
          a1.ManyToManyMaster.Add(g1);
//          session.Session.Persist();
          Assert.AreEqual(2, a1.ManyToManyMaster.Count);
          Assert.AreEqual(1, g1.ManytoManyPaired.Count, "G1");
          Assert.AreEqual(1, g2.ManytoManyPaired.Count, "G2");
          a1.ManyToManyMaster.Remove(g1);
          a1.ManyToManyMaster.Remove(g2);
          Assert.IsNotNull(a1.ManyToManyMaster);
          Assert.IsNotNull(g1.ManytoManyPaired);
          Assert.AreEqual(0, a1.ManyToManyMaster.Count);
          Assert.AreEqual(0, g1.ManytoManyPaired.Count);
          Assert.AreEqual(0, g2.ManytoManyPaired.Count);
          session.SaveChanges();
          a1.ManyToManyMaster.Add(g1);
          a2.ManyToManyMaster.Add(g1);
          a1.ManyToManyMaster.Add(g2);
          a1.ManyToManyMaster.Add(g2);
          a1.ManyToManyMaster.Add(g1);
          a1.ManyToManyMaster.Add(g1);
          g1.ManytoManyPaired.Add(a1);
          g2.ManytoManyPaired.Add(a2);
          Assert.AreEqual(2, a1.ManyToManyMaster.Count);
          Assert.AreEqual(2, a2.ManyToManyMaster.Count);
          Assert.AreEqual(2, g1.ManytoManyPaired.Count);
          Assert.AreEqual(2, g2.ManytoManyPaired.Count);
          // Rollback
        }
      }
    }

    [Test]
    public void ManyToManyEnumerator()
    {
      using (var session = Domain.OpenSession()) {
        A a1;
        A a2;
        G g1;
        G g2;

        using (var transaction = session.OpenTransaction()) {
          a1 = new A();
          a2 = new A();
          g1 = new G();
          g2 = new G();
          a1.ManyToManyMaster.Add(g1);
          g2.ManytoManyPaired.Add(a1);
          g1.ManytoManyPaired.Add(a2);
          a2.ManyToManyMaster.Add(g2);
          transaction.Complete();
        }

        using (var t = session.OpenTransaction()) {
          Assert.AreEqual(2, a1.ManyToManyMaster.Count);
          Assert.AreEqual(2, a2.ManyToManyMaster.Count);
          Assert.AreEqual(2, g1.ManytoManyPaired.Count);
          Assert.AreEqual(2, g2.ManytoManyPaired.Count);
          CheckEnumerator(a1.ManyToManyMaster, g1, g2);
          CheckEnumerator(a2.ManyToManyMaster, g1, g2);
          CheckEnumerator(g1.ManytoManyPaired, a1, a2);
          CheckEnumerator(g2.ManytoManyPaired, a1, a2);
          // Rollback
        }
      }
    }

    [Test]
    public void ManyToZero()
    {
      A a;
      E e1;
      E e2;
      using (var session = Domain.OpenSession()) {

        using (var transaction = session.OpenTransaction()) {
          a = new A();
          e1 = new E();
          e2 = new E();
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          Assert.IsNotNull(a.ZeroToMany);
          Assert.AreEqual(0, a.ZeroToMany.Count);
          Assert.IsFalse(a.ZeroToMany.Contains(e1));
          Assert.IsFalse(a.ZeroToMany.Contains(e2));
          a.ZeroToMany.Add(e1);
          a.ZeroToMany.Add(e2);
          Assert.AreEqual(2, a.ZeroToMany.Count);
          Assert.IsTrue(a.ZeroToMany.Contains(e1));
          Assert.IsTrue(a.ZeroToMany.Contains(e2));
          a.ZeroToMany.Remove(e1);
          a.ZeroToMany.Remove(e2);
          Assert.AreEqual(0, a.ZeroToMany.Count);
          Assert.IsFalse(a.ZeroToMany.Contains(e1));
          Assert.IsFalse(a.ZeroToMany.Contains(e2));
          // Rollback
        }
      }
    }

    [Test]
    public void ManyToZeroEnumerator()
    {
      A a;
      E e1;
      E e2;
      using (var session = Domain.OpenSession()) {

        using (var transaction = session.OpenTransaction()) {
          a = new A();
          e1 = new E();
          e2 = new E();
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          Assert.IsNotNull(a.ZeroToMany);
          Assert.AreEqual(0, a.ZeroToMany.Count);
          Assert.IsFalse(a.ZeroToMany.Contains(e1));
          Assert.IsFalse(a.ZeroToMany.Contains(e2));
          a.ZeroToMany.Add(e1);
          a.ZeroToMany.Add(e2);
          CheckEnumerator(a.ZeroToMany, e1, e2);
          // Rollback
        }
      }
    }

    private void CheckEnumerator<T>(EntitySet<T> entitySet, params T[] items) where T : Entity
    {
      T[] currentItems = entitySet.ToArray();
      Assert.AreEqual(currentItems.Length, items.Length);
      foreach (T item in items) {
        Assert.IsTrue(currentItems.Contains(item));
      }
    }

    [Test]
    public void ParentChildrenTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var h = new H();
          var children = h.Children;
          children.Add(new H());
          var first = children.First();
          Assert.AreSame(h, first.Parent);
          // Rollback
        }
      }
    }

    [Test]
    public void IntermediateStructureTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          
          A first = new A();
          A second = new A();
          first.IndirectA.IntermediateStructure2.A = second;

          AssertEx.Throws<ReferentialIntegrityException>(second.Remove);

          tx.Complete();
        }
      }
    }
  }
}