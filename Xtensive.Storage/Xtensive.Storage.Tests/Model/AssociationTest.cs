// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Model.Association;
using Xtensive.Integrity.Transactions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Xtensive.Storage.Tests.Model.Association
{
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public abstract class Root : Entity
  {
    [Field]
    public int Id { get; private set; }

    protected Root(Tuple tuple)
      : base(tuple)
    {
    }

    protected Root()
    {
      
    }
  }

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public abstract class Root2 : Entity
  {
    [Field]
    public Guid Id { get; private set; }

    protected Root2(Tuple tuple)
      : base(tuple)
    {
    }

    protected Root2()
    {

    }
  }

  public class A : Root
  {
    [Field]
    public B OneToZero { get; set; }

    [Field]
    public C OneToOneMaster { get; set; }

    [Field]
    public D OneToManyMaster { get; set; }

    [Field]
    public EntitySet<E> ManyToZero { get; private set; }

    [Field]
    public EntitySet<F> ManyToOneMaster { get; private set; }

    [Field]
    public EntitySet<G> ManyToManyMaster { get; private set; }

    [Field]
    public IntermediateStructure1 IndirectA { get; set; }
  }

  public class B : Root2
  {
    public B(int id)
      : base(Tuple.Create(id))
    {
      
    }

    public B()
    {
      
    }
  }

  public class C : Root2
  {
    [Field(PairTo = "OneToOneMaster")]
    public A OneToOnePaired { get; set; }

    [Field]
    public IntermediateStructure1 IndirectA { get; set; }
  }

  public class D : Root2
  {
    [Field(PairTo = "OneToManyMaster")]
    public EntitySet<A> ManyToOnePaired { get; private set; }
  }

  public class E : Root2
  {
  }

  public class F : Root2
  {
    [Field(PairTo = "ManyToOneMaster")]
    public A OneToManyPaired { get; set; }
  }

  public class G : Root2
  {
    [Field(PairTo = "ManyToManyMaster")]
    public EntitySet<A> ManytoManyPaired { get; private set; }
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
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(A).Namespace);
      return config;
    }

    [Test]
    public void ToRecordsetTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var a1 = new A();
          var a2 = new A();
          var rs = a1.Type.Indexes.PrimaryIndex.ToRecordSet();
          foreach (Tuple tuple in rs) {
            var rs2 = a1.Type.Indexes.PrimaryIndex.ToRecordSet();
            foreach (Tuple tuple2 in rs2) {
              Core.Log.Debug(tuple2.ToString());
            }
          }
        }
      }
    }

    [Test]
    public void EntitySetCreation()
    {
      // Domain.Model.Dump();
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var a = new A();
          Assert.IsNotNull(a.ManyToManyMaster);
          Assert.IsNotNull(a.ManyToOneMaster);
          Assert.IsNotNull(a.ManyToZero);
        }
      }
    }

    [Test]
    public void SimpleEntitySetEnumerator()
    {
      using (Domain.OpenSession()) {
        A a;
        F f1;
        F f2;
        F f3;
        F f4;
        using (var transaction = Transaction.Open()) {
          a = new A();
          f1 = new F();
          f2 = new F();
          f3 = new F();
          f4 = new F();
          transaction.Complete();
        }
        IEnumerator<F> enumerator; 
        using (var transaction = Transaction.Open()) {
          Assert.AreEqual(0, a.ManyToOneMaster.Count()); // Linq count through enumerate
          f1.OneToManyPaired = a;
          f2.OneToManyPaired = a;
          f3.OneToManyPaired = a;
          f4.OneToManyPaired = a;

          Assert.AreEqual(4, a.ManyToOneMaster.Count()); // Enumerate through internal cacee
          foreach (F f in a.ManyToOneMaster) {
            a.ManyToOneMaster.Contains(f); // Enumerate through internal cahce
          }
          enumerator = a.ManyToOneMaster.GetEnumerator();
          Assert.IsTrue(enumerator.MoveNext());
          Assert.IsTrue(enumerator.MoveNext());
          transaction.Complete();
        }
        // clear cache
        using (Transaction.Open()) {
          AssertEx.Throws<Exception>(()=>enumerator.MoveNext());  
          Assert.AreEqual(4, a.ManyToOneMaster.Count()); // Enumerate through recordset request
          foreach (F f in a.ManyToOneMaster) {
            // a.ManyToOneMaster.Contains(f.Key); // Enumerate through recordset request
            a.ManyToOneMaster.Contains(f); // Enumerate through recordset request
          }
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
        }
        using (Transaction.Open()) {
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
          Assert.IsNull(a1.OneToOneMaster);
          Assert.IsNull(a2.OneToOneMaster);
          Assert.IsNull(c.OneToOnePaired);
          c.OneToOnePaired = a1;
          Assert.IsNotNull(a1.OneToOneMaster);
          Assert.IsNull(a2.OneToOneMaster);
          Assert.IsNotNull(c.OneToOnePaired);
          Assert.AreEqual(a1.OneToOneMaster, c);
          Assert.AreEqual(c.OneToOnePaired, a1);
          //change owner
          c.OneToOnePaired = a2;
          Assert.IsNull(a1.OneToOneMaster);
          Assert.IsNotNull(a2.OneToOneMaster);
          Assert.IsNotNull(c.OneToOnePaired);
          Assert.AreEqual(a2.OneToOneMaster, c);
          Assert.AreEqual(c.OneToOnePaired, a2);
          // change back trough another class
          a1.OneToOneMaster = c;
          Assert.IsNotNull(a1.OneToOneMaster);
          Assert.IsNull(a2.OneToOneMaster);
          Assert.IsNotNull(c.OneToOnePaired);
          Assert.AreEqual(a1.OneToOneMaster, c);
          Assert.AreEqual(c.OneToOnePaired, a1);
        }
        using (Transaction.Open()) {
          Assert.IsNull(a1.OneToOneMaster);
          Assert.IsNull(a2.OneToOneMaster);
          Assert.IsNull(c.OneToOnePaired);
        }
      }
    }

    [Test]
    public void ManyToOneAssign()
    {
      using (Domain.OpenSession()) {
        A a;
        F f;
        using (var transaction = Transaction.Open()) {
          a = new A();
          f = new F();
          transaction.Complete();
        }
        using (Transaction.Open()) {
          Assert.IsNull(f.OneToManyPaired);
          Assert.IsNotNull(a.ManyToOneMaster);
          Assert.AreEqual(0, a.ManyToOneMaster.Count);
          Assert.IsFalse(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f));
          f.OneToManyPaired = a;
          Assert.IsNotNull(f.OneToManyPaired);
          Assert.AreEqual(1, a.ManyToOneMaster.Count);
          Assert.IsTrue(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f));
          f.OneToManyPaired = a;
          Assert.IsNotNull(f.OneToManyPaired);
          Assert.AreEqual(1, a.ManyToOneMaster.Count);
          Assert.IsTrue(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f));
        }
        // rollback
        using (Transaction.Open()) {
          Assert.IsNull(f.OneToManyPaired);
          Assert.IsNotNull(a.ManyToOneMaster);
          Assert.AreEqual(0, a.ManyToOneMaster.Count);
          Assert.IsFalse(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f));
        }
        //assign back and commit
        using (var transaction = Transaction.Open()) {
          f.OneToManyPaired = a;
          Assert.IsNotNull(f.OneToManyPaired);
          Assert.AreEqual(1, a.ManyToOneMaster.Count);
          Assert.IsTrue(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f));
          transaction.Complete();
        }
        // check commited state
        using (Transaction.Open()) {
          Assert.IsNotNull(f.OneToManyPaired);
          Assert.AreEqual(1, a.ManyToOneMaster.Count);
          Assert.IsTrue(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f));
        }
      }
    }

    [Test]
    public void ManyToOneAddToSet()
    {
      using (Domain.OpenSession()) {
        A a;
        F f;
        using (var transaction = Transaction.Open()) {
          a = new A();
          f = new F();
          transaction.Complete();
        }
        using (Transaction.Open()) {
          Assert.IsNull(f.OneToManyPaired);
          Assert.IsNotNull(a.ManyToOneMaster);
          Assert.AreEqual(0, a.ManyToOneMaster.Count);
          Assert.IsFalse(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f));
          a.ManyToOneMaster.Add(f);
          Assert.IsNotNull(f.OneToManyPaired);
          Assert.AreEqual(f.OneToManyPaired, a);
          Assert.AreEqual(1, a.ManyToOneMaster.Count);
          Assert.IsTrue(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f));
          a.ManyToOneMaster.Add(f);
          Assert.IsNotNull(f.OneToManyPaired);
          Assert.AreEqual(f.OneToManyPaired, a);
          Assert.AreEqual(1, a.ManyToOneMaster.Count);
          Assert.IsTrue(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f));
        }
        // rollback
        using (Transaction.Open()) {
          Assert.IsNull(f.OneToManyPaired);
          Assert.IsNotNull(a.ManyToOneMaster);
          Assert.AreEqual(0, a.ManyToOneMaster.Count);
          Assert.IsFalse(a.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f));
        }
      }
    }

    [Test]
    public void ManyToOneChangeOwnerByAssign()
    {
      using (Domain.OpenSession()) {
        A a;
        F f1;
        F f2;
        using (var transaction = Transaction.Open()) {
          a = new A();
          f1 = new F();
          f2 = new F();
          transaction.Complete();
        }
        using (Transaction.Open()) {
          Assert.IsNull(f1.OneToManyPaired);
          Assert.IsNull(f2.OneToManyPaired);
          Assert.IsNotNull(a.ManyToOneMaster);
          Assert.AreEqual(0, a.ManyToOneMaster.Count);
          Assert.IsFalse(a.ManyToOneMaster.Contains(f1.Key));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f1));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f2.Key));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f2));
          f1.OneToManyPaired = a;
          Assert.IsNotNull(f1.OneToManyPaired);
          Assert.AreEqual(1, a.ManyToOneMaster.Count);
          Assert.IsTrue(a.ManyToOneMaster.Contains(f1.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f1));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f2.Key));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f2));
          f2.OneToManyPaired = a;
          Assert.IsNotNull(f1.OneToManyPaired);
          Assert.IsNotNull(f2.OneToManyPaired);
          Assert.AreEqual(2, a.ManyToOneMaster.Count);
          Assert.IsTrue(a.ManyToOneMaster.Contains(f1));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f1.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f2.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f2));
          f1.OneToManyPaired = null;
          Assert.IsNull(f1.OneToManyPaired);
          Assert.IsNotNull(f2.OneToManyPaired);
          Assert.IsNotNull(a.ManyToOneMaster);
          Assert.AreEqual(1, a.ManyToOneMaster.Count);
          Assert.IsFalse(a.ManyToOneMaster.Contains(f1.Key));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f1));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f2.Key));
          Assert.IsTrue(a.ManyToOneMaster.Contains(f2));
          f1.OneToManyPaired = a;
        }
        // rollback
        using (Transaction.Open()) {
          Assert.IsNull(f1.OneToManyPaired);
          Assert.IsNull(f2.OneToManyPaired);
          Assert.IsNotNull(a.ManyToOneMaster);
          Assert.AreEqual(0, a.ManyToOneMaster.Count);
          Assert.IsFalse(a.ManyToOneMaster.Contains(f1.Key));
          Assert.IsFalse(a.ManyToOneMaster.Contains(f1));
        }
      }
    }

    [Test]
    public void ManyToOneChangeOwnerByEntitySet()
    {
      using (Domain.OpenSession()) {
        A a1;
        A a2;
        F f;
        using (var transaction = Transaction.Open()) {
          a1 = new A();
          a2 = new A();
          f = new F();
          transaction.Complete();
        }
        using (Transaction.Open()) {
          Assert.IsNull(f.OneToManyPaired);
          Assert.IsNotNull(a1.ManyToOneMaster);
          Assert.IsNotNull(a2.ManyToOneMaster);
          Assert.IsFalse(a1.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a1.ManyToOneMaster.Contains(f));
          Assert.IsFalse(a2.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a2.ManyToOneMaster.Contains(f));
          Assert.AreEqual(0, a1.ManyToOneMaster.Count);
          Assert.AreEqual(0, a2.ManyToOneMaster.Count);
          a1.ManyToOneMaster.Add(f);
          Assert.IsNotNull(f.OneToManyPaired);
          Assert.AreEqual(f.OneToManyPaired, a1);
          Assert.AreEqual(1, a1.ManyToOneMaster.Count);
          Assert.AreEqual(0, a2.ManyToOneMaster.Count);
          Assert.IsTrue(a1.ManyToOneMaster.Contains(f.Key));
          Assert.IsTrue(a1.ManyToOneMaster.Contains(f));
          Assert.IsFalse(a2.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a2.ManyToOneMaster.Contains(f));
          a1.ManyToOneMaster.Remove(f);
          Assert.IsNull(f.OneToManyPaired);
          Assert.IsNotNull(a1.ManyToOneMaster);
          Assert.IsNotNull(a2.ManyToOneMaster);
          Assert.IsFalse(a1.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a1.ManyToOneMaster.Contains(f));
          Assert.IsFalse(a2.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a2.ManyToOneMaster.Contains(f));
          Assert.AreEqual(0, a1.ManyToOneMaster.Count);
          Assert.AreEqual(0, a2.ManyToOneMaster.Count);
          a1.ManyToOneMaster.Add(f);
          // change owner
          a2.ManyToOneMaster.Add(f);
          Assert.IsNotNull(f.OneToManyPaired);
          Assert.AreEqual(f.OneToManyPaired, a2);
          Assert.AreEqual(0, a1.ManyToOneMaster.Count);
          Assert.AreEqual(1, a2.ManyToOneMaster.Count);
          Assert.IsFalse(a1.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a1.ManyToOneMaster.Contains(f));
          Assert.IsTrue(a2.ManyToOneMaster.Contains(f.Key));
          Assert.IsTrue(a2.ManyToOneMaster.Contains(f));
        }
        // rollback
        using (Transaction.Open()) {
          Assert.IsNull(f.OneToManyPaired);
          Assert.IsNotNull(a1.ManyToOneMaster);
          Assert.IsNotNull(a2.ManyToOneMaster);
          Assert.IsFalse(a1.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a1.ManyToOneMaster.Contains(f));
          Assert.IsFalse(a2.ManyToOneMaster.Contains(f.Key));
          Assert.IsFalse(a2.ManyToOneMaster.Contains(f));
          Assert.AreEqual(0, a1.ManyToOneMaster.Count);
          Assert.AreEqual(0, a2.ManyToOneMaster.Count);
        }
      }
    }

    [Test]
    public void OneToZero()
    {
      using (Domain.OpenSession()) {
        A a;
        B b;
        using (var transaction = Transaction.Open()) {
          a = new A();
          b = new B();
          transaction.Complete();
        }
        using (Transaction.Open()) {
          Assert.IsNull(a.OneToZero);
          a.OneToZero = b;
          Assert.AreEqual(a.OneToZero, b);
          a.OneToZero = null;
          Assert.IsNull(a.OneToZero);
          a.OneToZero = b;
        }
        using (var transaction = Transaction.Open()) {
          Assert.IsNull(a.OneToZero);
          a.OneToZero = b;
          transaction.Complete();
        }
        using (var transaction = Transaction.Open()) {
          Assert.AreEqual(a.OneToZero, b);
        }
      }
    }

    [Test]
    public void EntitySetMultipleAddRemove(){
      using (var session = Domain.OpenSession()) {
        using (var transaction = Transaction.Open()) {
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
        using (var transaction = Transaction.Open()) {
          a1 = new A();
          a2 = new A();
          g1 = new G();
          g2 = new G();
          transaction.Complete();
        }
        using (var t = Transaction.Open()) {
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
          session.Session.Persist();
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
        using (var transaction = Transaction.Open()) {
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
        using (var t = Transaction.Open()) {
          Assert.AreEqual(2, a1.ManyToManyMaster.Count);
          Assert.AreEqual(2, a2.ManyToManyMaster.Count);
          Assert.AreEqual(2, g1.ManytoManyPaired.Count);
          Assert.AreEqual(2, g2.ManytoManyPaired.Count);
          CheckEnumerator(a1.ManyToManyMaster, g1, g2);
          CheckEnumerator(a2.ManyToManyMaster, g1, g2);
          CheckEnumerator(g1.ManytoManyPaired, a1, a2);
          CheckEnumerator(g2.ManytoManyPaired, a1, a2);
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
        using (var transaction = Transaction.Open()) {
          a = new A();
          e1 = new E();
          e2 = new E();
          transaction.Complete();
        }
        using (var transaction = Transaction.Open()) {
          Assert.IsNotNull(a.ManyToZero);
          Assert.AreEqual(0, a.ManyToZero.Count);
          Assert.IsFalse(a.ManyToZero.Contains(e1));
          Assert.IsFalse(a.ManyToZero.Contains(e2));
          a.ManyToZero.Add(e1);
          a.ManyToZero.Add(e2);
          Assert.AreEqual(2, a.ManyToZero.Count);
          Assert.IsTrue(a.ManyToZero.Contains(e1));
          Assert.IsTrue(a.ManyToZero.Contains(e2));
          a.ManyToZero.Remove(e1);
          a.ManyToZero.Remove(e2);
          Assert.AreEqual(0, a.ManyToZero.Count);
          Assert.IsFalse(a.ManyToZero.Contains(e1));
          Assert.IsFalse(a.ManyToZero.Contains(e2));
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
        using (var transaction = Transaction.Open()) {
          a = new A();
          e1 = new E();
          e2 = new E();
          transaction.Complete();
        }
        using (var transaction = Transaction.Open()) {
          Assert.IsNotNull(a.ManyToZero);
          Assert.AreEqual(0, a.ManyToZero.Count);
          Assert.IsFalse(a.ManyToZero.Contains(e1));
          Assert.IsFalse(a.ManyToZero.Contains(e2));
          a.ManyToZero.Add(e1);
          a.ManyToZero.Add(e2);
          CheckEnumerator(a.ManyToZero, e1, e2);
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

    
  }
}