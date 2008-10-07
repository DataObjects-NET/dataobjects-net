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
    public int Id { get; private set; }

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
    public B(int id)
      : base(Tuple.Create(id))
    {
      
    }

    public B()
    {
      
    }
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

  public class E : Root2
  {
  }

  public class F : Root
  {
    [Field(PairTo = "ManyToOne")]
    public A A { get; set; }
  }

  public class G : Root2
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
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(A).Namespace);
      return config;
    }

    [Test]
    public void TestRemove()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          B b = new B(1);
          b.Remove();
          b = new B(1);
        }
      }
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
          Assert.IsNotNull(a.ManyToMany);
          Assert.IsNotNull(a.ManyToOne);
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
          Assert.AreEqual(0, a.ManyToOne.Count()); // Linq count through enumerate
          f1.A = a;
          f2.A = a;
          f3.A = a;
          f4.A = a;
          Assert.AreEqual(4, a.ManyToOne.Count()); // Enumerate through internal cacee
          foreach (F f in a.ManyToOne) {
            a.ManyToOne.Contains(f); // Enumerate through internal cahce
          }
          enumerator = a.ManyToOne.GetEnumerator();
          Assert.IsTrue(enumerator.MoveNext());
          Assert.IsTrue(enumerator.MoveNext());
          transaction.Complete();
        }
        // clear cache
        using (Transaction.Open()) {
          AssertEx.Throws<Exception>(()=>enumerator.MoveNext());  
          Assert.AreEqual(4, a.ManyToOne.Count()); // Enumerate through recordset request
          foreach (F f in a.ManyToOne) {
            // a.ManyToOne.Contains(f.Key); // Enumerate through recordset request
            a.ManyToOne.Contains(f); // Enumerate through recordset request
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
          Assert.IsNull(a.OneToOne);
          Assert.IsNull(c.A);
          c.A = a;
          Assert.IsNotNull(a.OneToOne);
          Assert.IsNotNull(c.A);
          Assert.AreEqual(a.OneToOne, c);
          Assert.AreEqual(c.A, a);
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
          Assert.IsNull(f.A);
          Assert.IsNotNull(a.ManyToOne);
          Assert.AreEqual(0, a.ManyToOne.Count);
          Assert.IsFalse(a.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a.ManyToOne.Contains(f));
          f.A = a;
          Assert.IsNotNull(f.A);
          Assert.AreEqual(1, a.ManyToOne.Count);
          Assert.IsTrue(a.ManyToOne.Contains(f.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f));
          f.A = a;
          Assert.IsNotNull(f.A);
          Assert.AreEqual(1, a.ManyToOne.Count);
          Assert.IsTrue(a.ManyToOne.Contains(f.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f));
        }
        // rollback
        using (Transaction.Open()) {
          Assert.IsNull(f.A);
          Assert.IsNotNull(a.ManyToOne);
          Assert.AreEqual(0, a.ManyToOne.Count);
          Assert.IsFalse(a.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a.ManyToOne.Contains(f));
        }
        //assign back and commit
        using (var transaction = Transaction.Open()) {
          f.A = a;
          Assert.IsNotNull(f.A);
          Assert.AreEqual(1, a.ManyToOne.Count);
          Assert.IsTrue(a.ManyToOne.Contains(f.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f));
          transaction.Complete();
        }
        // check commited state
        using (Transaction.Open()) {
          Assert.IsNotNull(f.A);
          Assert.AreEqual(1, a.ManyToOne.Count);
          Assert.IsTrue(a.ManyToOne.Contains(f.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f));
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
          Assert.IsNull(f.A);
          Assert.IsNotNull(a.ManyToOne);
          Assert.AreEqual(0, a.ManyToOne.Count);
          Assert.IsFalse(a.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a.ManyToOne.Contains(f));
          a.ManyToOne.Add(f);
          Assert.IsNotNull(f.A);
          Assert.AreEqual(f.A, a);
          Assert.AreEqual(1, a.ManyToOne.Count);
          Assert.IsTrue(a.ManyToOne.Contains(f.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f));
          a.ManyToOne.Add(f);
          Assert.IsNotNull(f.A);
          Assert.AreEqual(f.A, a);
          Assert.AreEqual(1, a.ManyToOne.Count);
          Assert.IsTrue(a.ManyToOne.Contains(f.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f));
        }
        // rollback
        using (Transaction.Open()) {
          Assert.IsNull(f.A);
          Assert.IsNotNull(a.ManyToOne);
          Assert.AreEqual(0, a.ManyToOne.Count);
          Assert.IsFalse(a.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a.ManyToOne.Contains(f));
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
          Assert.IsNull(f1.A);
          Assert.IsNull(f2.A);
          Assert.IsNotNull(a.ManyToOne);
          Assert.AreEqual(0, a.ManyToOne.Count);
          Assert.IsFalse(a.ManyToOne.Contains(f1.Key));
          Assert.IsFalse(a.ManyToOne.Contains(f1));
          Assert.IsFalse(a.ManyToOne.Contains(f2.Key));
          Assert.IsFalse(a.ManyToOne.Contains(f2));
          f1.A = a;
          Assert.IsNotNull(f1.A);
          Assert.AreEqual(1, a.ManyToOne.Count);
          Assert.IsTrue(a.ManyToOne.Contains(f1.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f1));
          Assert.IsFalse(a.ManyToOne.Contains(f2.Key));
          Assert.IsFalse(a.ManyToOne.Contains(f2));
          f2.A = a;
          Assert.IsNotNull(f1.A);
          Assert.IsNotNull(f2.A);
          Assert.AreEqual(2, a.ManyToOne.Count);
          Assert.IsTrue(a.ManyToOne.Contains(f1));
          Assert.IsTrue(a.ManyToOne.Contains(f1.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f2.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f2));
          f1.A = null;
          Assert.IsNull(f1.A);
          Assert.IsNotNull(f2.A);
          Assert.IsNotNull(a.ManyToOne);
          Assert.AreEqual(1, a.ManyToOne.Count);
          Assert.IsFalse(a.ManyToOne.Contains(f1.Key));
          Assert.IsFalse(a.ManyToOne.Contains(f1));
          Assert.IsTrue(a.ManyToOne.Contains(f2.Key));
          Assert.IsTrue(a.ManyToOne.Contains(f2));
          f1.A = a;
        }
        // rollback
        using (Transaction.Open()) {
          Assert.IsNull(f1.A);
          Assert.IsNull(f2.A);
          Assert.IsNotNull(a.ManyToOne);
          Assert.AreEqual(0, a.ManyToOne.Count);
          Assert.IsFalse(a.ManyToOne.Contains(f1.Key));
          Assert.IsFalse(a.ManyToOne.Contains(f1));
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
          Assert.IsNull(f.A);
          Assert.IsNotNull(a1.ManyToOne);
          Assert.IsNotNull(a2.ManyToOne);
          Assert.IsFalse(a1.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a1.ManyToOne.Contains(f));
          Assert.IsFalse(a2.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a2.ManyToOne.Contains(f));
          Assert.AreEqual(0, a1.ManyToOne.Count);
          Assert.AreEqual(0, a2.ManyToOne.Count);
          a1.ManyToOne.Add(f);
          Assert.IsNotNull(f.A);
          Assert.AreEqual(f.A, a1);
          Assert.AreEqual(1, a1.ManyToOne.Count);
          Assert.AreEqual(0, a2.ManyToOne.Count);
          Assert.IsTrue(a1.ManyToOne.Contains(f.Key));
          Assert.IsTrue(a1.ManyToOne.Contains(f));
          Assert.IsFalse(a2.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a2.ManyToOne.Contains(f));
          a1.ManyToOne.Remove(f);
          Assert.IsNull(f.A);
          Assert.IsNotNull(a1.ManyToOne);
          Assert.IsNotNull(a2.ManyToOne);
          Assert.IsFalse(a1.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a1.ManyToOne.Contains(f));
          Assert.IsFalse(a2.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a2.ManyToOne.Contains(f));
          Assert.AreEqual(0, a1.ManyToOne.Count);
          Assert.AreEqual(0, a2.ManyToOne.Count);
          a1.ManyToOne.Add(f);
          // change owner
          a2.ManyToOne.Add(f);
          Assert.IsNotNull(f.A);
          Assert.AreEqual(f.A, a2);
          Assert.AreEqual(0, a1.ManyToOne.Count);
          Assert.AreEqual(1, a2.ManyToOne.Count);
          Assert.IsFalse(a1.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a1.ManyToOne.Contains(f));
          Assert.IsTrue(a2.ManyToOne.Contains(f.Key));
          Assert.IsTrue(a2.ManyToOne.Contains(f));
        }
        // rollback
        using (Transaction.Open()) {
          Assert.IsNull(f.A);
          Assert.IsNotNull(a1.ManyToOne);
          Assert.IsNotNull(a2.ManyToOne);
          Assert.IsFalse(a1.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a1.ManyToOne.Contains(f));
          Assert.IsFalse(a2.ManyToOne.Contains(f.Key));
          Assert.IsFalse(a2.ManyToOne.Contains(f));
          Assert.AreEqual(0, a1.ManyToOne.Count);
          Assert.AreEqual(0, a2.ManyToOne.Count);
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
          g1.As.Add(a1);
          g1.As.Add(a1);
          Assert.AreEqual(1, g1.As.Count);
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
          Assert.IsNotNull(a1.ManyToMany);
          Assert.IsNotNull(g1.As);
          Assert.AreEqual(0, a1.ManyToMany.Count);
          Assert.AreEqual(0, g1.As.Count);
//          Assert.AreEqual(0, g2.As.Count);
          a1.ManyToMany.Add(g2);
          a1.ManyToMany.Add(g1);
//          session.Session.Persist();
          Assert.AreEqual(2, a1.ManyToMany.Count);
          Assert.AreEqual(1, g1.As.Count, "G1");
          Assert.AreEqual(1, g2.As.Count, "G2");
          a1.ManyToMany.Remove(g1);
          a1.ManyToMany.Remove(g2);
          Assert.IsNotNull(a1.ManyToMany);
          Assert.IsNotNull(g1.As);
          Assert.AreEqual(0, a1.ManyToMany.Count);
          Assert.AreEqual(0, g1.As.Count);
          Assert.AreEqual(0, g2.As.Count);
          session.Session.Persist();
          a1.ManyToMany.Add(g1);
          a2.ManyToMany.Add(g1);
          a1.ManyToMany.Add(g2);
          a1.ManyToMany.Add(g2);
          a1.ManyToMany.Add(g1);
          a1.ManyToMany.Add(g1);
          g1.As.Add(a1);
          g2.As.Add(a2);
          Assert.AreEqual(2, a1.ManyToMany.Count);
          Assert.AreEqual(2, a2.ManyToMany.Count);
          Assert.AreEqual(2, g1.As.Count);
          Assert.AreEqual(2, g2.As.Count);
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
          a1.ManyToMany.Add(g1);
          g2.As.Add(a1);
          g1.As.Add(a2);
          a2.ManyToMany.Add(g2);
          transaction.Complete();
        }
        using (var t = Transaction.Open()) {
          Assert.AreEqual(2, a1.ManyToMany.Count);
          Assert.AreEqual(2, a2.ManyToMany.Count);
          Assert.AreEqual(2, g1.As.Count);
          Assert.AreEqual(2, g2.As.Count);
          CheckEnumerator(a1.ManyToMany, g1, g2);
          CheckEnumerator(a2.ManyToMany, g1, g2);
          CheckEnumerator(g1.As, a1, a2);
          CheckEnumerator(g2.As, a1, a2);
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