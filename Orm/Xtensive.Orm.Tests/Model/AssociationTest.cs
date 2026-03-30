// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.06.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Tests;
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
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), typeof(A).Namespace);
      return config;
    }

    [Test]
    public void ToRecordsetTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var a1 = new A();
          var a2 = new A();
          var rs = a1.TypeInfo.Indexes.PrimaryIndex.GetQuery();

          var parameterContext = new ParameterContext();
          foreach (Tuple tuple in rs.GetRecordSetReader(Session.Current, parameterContext).ToEnumerable()) {
            var rs2 = a1.TypeInfo.Indexes.PrimaryIndex.GetQuery();
            foreach (Tuple tuple2 in rs2.GetRecordSetReader(Session.Current, parameterContext).ToEnumerable()) {
              TestLog.Debug(tuple2.ToString());
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
          Assert.That(a.ManyToManyMaster, Is.Not.Null);
          Assert.That(a.OneToManyMaster, Is.Not.Null);
          Assert.That(a.ZeroToMany, Is.Not.Null);
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
          Assert.That(a.OneToManyMaster.Count(), Is.EqualTo(0)); // Linq count through enumerate
          f1.ManyToOnePaired = a;
          f2.ManyToOnePaired = a;
          f3.ManyToOnePaired = a;
          f4.ManyToOnePaired = a;

          Assert.That(a.OneToManyMaster.Count(), Is.EqualTo(4)); // Enumerate through internal cacee
          foreach (F f in a.OneToManyMaster) {
            _ = a.OneToManyMaster.Contains(f); // Enumerate through internal cahce
          }
          enumerator = a.OneToManyMaster.GetEnumerator();
          Assert.That(enumerator.MoveNext(), Is.True);
          Assert.That(enumerator.MoveNext(), Is.True);
          transaction.Complete();
        }
        // Cache is invalidated here
        using (session.OpenTransaction()) {
          Assert.That(a.OneToManyMaster.Count(), Is.EqualTo(4)); // Enumerate through recordset request
          foreach (F f in a.OneToManyMaster) {
            // a.OneToManyMaster.Contains(f.Key); // Enumerate through recordset request
            _ = a.OneToManyMaster.Contains(f); // Enumerate through recordset request
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
          Assert.That(a.OneToOneMaster, Is.Null);
          Assert.That(c.OneToOnePaired, Is.Null);
          c.OneToOnePaired = a;
          Assert.That(a.OneToOneMaster, Is.Not.Null);
          Assert.That(c.OneToOnePaired, Is.Not.Null);
          Assert.That(c, Is.EqualTo(a.OneToOneMaster));
          Assert.That(a, Is.EqualTo(c.OneToOnePaired));
          c.OneToOnePaired = a;
          Assert.That(a.OneToOneMaster, Is.Not.Null);
          Assert.That(c.OneToOnePaired, Is.Not.Null);
          Assert.That(c, Is.EqualTo(a.OneToOneMaster));
          Assert.That(a, Is.EqualTo(c.OneToOnePaired));
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.That(a.OneToOneMaster, Is.Null);
          Assert.That(c.OneToOnePaired, Is.Null);
          a.OneToOneMaster = c;
          Assert.That(a.OneToOneMaster, Is.Not.Null);
          Assert.That(c.OneToOnePaired, Is.Not.Null);
          Assert.That(c, Is.EqualTo(a.OneToOneMaster));
          Assert.That(a, Is.EqualTo(c.OneToOnePaired));
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
          Assert.That(a1.OneToOneMaster, Is.Null);
          Assert.That(a2.OneToOneMaster, Is.Null);
          Assert.That(c.OneToOnePaired, Is.Null);
          c.OneToOnePaired = a1;
          Assert.That(a1.OneToOneMaster, Is.Not.Null);
          Assert.That(a2.OneToOneMaster, Is.Null);
          Assert.That(c.OneToOnePaired, Is.Not.Null);
          Assert.That(c, Is.EqualTo(a1.OneToOneMaster));
          Assert.That(a1, Is.EqualTo(c.OneToOnePaired));
          // Change owner
          c.OneToOnePaired = a2;
          Assert.That(a1.OneToOneMaster, Is.Null);
          Assert.That(a2.OneToOneMaster, Is.Not.Null);
          Assert.That(c.OneToOnePaired, Is.Not.Null);
          Assert.That(c, Is.EqualTo(a2.OneToOneMaster));
          Assert.That(a2, Is.EqualTo(c.OneToOnePaired));
          // Change back trough another class
          a1.OneToOneMaster = c;
          Assert.That(a1.OneToOneMaster, Is.Not.Null);
          Assert.That(a2.OneToOneMaster, Is.Null);
          Assert.That(c.OneToOnePaired, Is.Not.Null);
          Assert.That(c, Is.EqualTo(a1.OneToOneMaster));
          Assert.That(a1, Is.EqualTo(c.OneToOnePaired));
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.That(a1.OneToOneMaster, Is.Null);
          Assert.That(a2.OneToOneMaster, Is.Null);
          Assert.That(c.OneToOnePaired, Is.Null);
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
          Assert.That(f.ManyToOnePaired, Is.Null);
          Assert.That(a.OneToManyMaster, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f), Is.False);
          f.ManyToOnePaired = a;
          Assert.That(f.ManyToOnePaired, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(1));
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f), Is.True);
          f.ManyToOnePaired = a;
          Assert.That(f.ManyToOnePaired, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(1));
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f), Is.True);
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.That(f.ManyToOnePaired, Is.Null);
          Assert.That(a.OneToManyMaster, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f), Is.False);
          // Rollback
        }
        
        // Assign back and commit
        using (var transaction = session.OpenTransaction()) {
          f.ManyToOnePaired = a;
          Assert.That(f.ManyToOnePaired, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f), Is.True);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(1));
          transaction.Complete();
        }

        // Check commited state
        using (session.OpenTransaction()) {
          Assert.That(f.ManyToOnePaired, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(1));
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f), Is.True);
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
          Assert.That(f.ManyToOnePaired, Is.Null);
          Assert.That(a.OneToManyMaster, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f), Is.False);
          _ = a.OneToManyMaster.Add(f);
          Assert.That(f.ManyToOnePaired, Is.Not.Null);
          Assert.That(a, Is.EqualTo(f.ManyToOnePaired));
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(1));
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f), Is.True);
          _ = a.OneToManyMaster.Add(f);
          Assert.That(f.ManyToOnePaired, Is.Not.Null);
          Assert.That(a, Is.EqualTo(f.ManyToOnePaired));
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(1));
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f), Is.True);
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.That(f.ManyToOnePaired, Is.Null);
          Assert.That(a.OneToManyMaster, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f), Is.False);
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
          Assert.That(f1.ManyToOnePaired, Is.Null);
          Assert.That(f2.ManyToOnePaired, Is.Null);
          Assert.That(a.OneToManyMaster, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a.OneToManyMaster.Contains(f1.Key), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f1), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f2.Key), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f2), Is.False);
          f1.ManyToOnePaired = a;
          Assert.That(f1.ManyToOnePaired, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(1));
          Assert.That(a.OneToManyMaster.Contains(f1.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f1), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f2.Key), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f2), Is.False);
          f2.ManyToOnePaired = a;
          Assert.That(f1.ManyToOnePaired, Is.Not.Null);
          Assert.That(f2.ManyToOnePaired, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(2));
          Assert.That(a.OneToManyMaster.Contains(f1), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f1.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f2.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f2), Is.True);
          f1.ManyToOnePaired = null;
          Assert.That(f1.ManyToOnePaired, Is.Null);
          Assert.That(f2.ManyToOnePaired, Is.Not.Null);
          Assert.That(a.OneToManyMaster, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(1));
          Assert.That(a.OneToManyMaster.Contains(f1.Key), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f1), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f2.Key), Is.True);
          Assert.That(a.OneToManyMaster.Contains(f2), Is.True);
          f1.ManyToOnePaired = a;
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.That(f1.ManyToOnePaired, Is.Null);
          Assert.That(f2.ManyToOnePaired, Is.Null);
          Assert.That(a.OneToManyMaster, Is.Not.Null);
          Assert.That(a.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a.OneToManyMaster.Contains(f1.Key), Is.False);
          Assert.That(a.OneToManyMaster.Contains(f1), Is.False);
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
          Assert.That(f.ManyToOnePaired, Is.Null);
          Assert.That(a1.OneToManyMaster, Is.Not.Null);
          Assert.That(a2.OneToManyMaster, Is.Not.Null);
          Assert.That(a1.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a1.OneToManyMaster.Contains(f), Is.False);
          Assert.That(a2.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a2.OneToManyMaster.Contains(f), Is.False);
          Assert.That(a1.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a2.OneToManyMaster.Count, Is.EqualTo(0));
          _ = a1.OneToManyMaster.Add(f);
          Assert.That(f.ManyToOnePaired, Is.Not.Null);
          Assert.That(a1, Is.EqualTo(f.ManyToOnePaired));
          Assert.That(a1.OneToManyMaster.Count, Is.EqualTo(1));
          Assert.That(a2.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a1.OneToManyMaster.Contains(f.Key), Is.True);
          Assert.That(a1.OneToManyMaster.Contains(f), Is.True);
          Assert.That(a2.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a2.OneToManyMaster.Contains(f), Is.False);
          _ = a1.OneToManyMaster.Remove(f);
          Assert.That(f.ManyToOnePaired, Is.Null);
          Assert.That(a1.OneToManyMaster, Is.Not.Null);
          Assert.That(a2.OneToManyMaster, Is.Not.Null);
          Assert.That(a1.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a1.OneToManyMaster.Contains(f), Is.False);
          Assert.That(a2.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a2.OneToManyMaster.Contains(f), Is.False);
          Assert.That(a1.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a2.OneToManyMaster.Count, Is.EqualTo(0));
          _ = a1.OneToManyMaster.Add(f);
          // Change owner
          _ = a2.OneToManyMaster.Add(f);
          Assert.That(f.ManyToOnePaired, Is.Not.Null);
          Assert.That(a2, Is.EqualTo(f.ManyToOnePaired));
          Assert.That(a1.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a2.OneToManyMaster.Count, Is.EqualTo(1));
          Assert.That(a1.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a1.OneToManyMaster.Contains(f), Is.False);
          Assert.That(a2.OneToManyMaster.Contains(f.Key), Is.True);
          Assert.That(a2.OneToManyMaster.Contains(f), Is.True);
          // Rollback
        }

        using (session.OpenTransaction()) {
          Assert.That(f.ManyToOnePaired, Is.Null);
          Assert.That(a1.OneToManyMaster, Is.Not.Null);
          Assert.That(a2.OneToManyMaster, Is.Not.Null);
          Assert.That(a1.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a1.OneToManyMaster.Contains(f), Is.False);
          Assert.That(a2.OneToManyMaster.Contains(f.Key), Is.False);
          Assert.That(a2.OneToManyMaster.Contains(f), Is.False);
          Assert.That(a1.OneToManyMaster.Count, Is.EqualTo(0));
          Assert.That(a2.OneToManyMaster.Count, Is.EqualTo(0));
          // Rollback
        }
      }
    }

    [Test]
    public void OneToZero()
    {
      using (var session = Domain.OpenSession()) {
        A a;
        B b;
        using (var transaction = session.OpenTransaction()) {
          a = new A();
          b = new B();
          transaction.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.That(a.ZeroToOne, Is.Null);
          a.ZeroToOne = b;
          Assert.That(b, Is.EqualTo(a.ZeroToOne));
          a.ZeroToOne = null;
          Assert.That(a.ZeroToOne, Is.Null);
          a.ZeroToOne = b;
          // Rollback
        }

        using (var transactionScope = session.OpenTransaction()) {
          Assert.That(a.ZeroToOne, Is.Null);
          a.ZeroToOne = b;
          transactionScope.Complete();
        }

        using (session.OpenTransaction()) {
          Assert.That(b, Is.EqualTo(a.ZeroToOne));
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
          _ = g1.ManytoManyPaired.Add(a1);
          _ = g1.ManytoManyPaired.Add(a1);
          Assert.That(g1.ManytoManyPaired.Count, Is.EqualTo(1));
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
          Assert.That(a1.ManyToManyMaster, Is.Not.Null);
          Assert.That(g1.ManytoManyPaired, Is.Not.Null);
          Assert.That(a1.ManyToManyMaster.Count, Is.EqualTo(0));
          Assert.That(g1.ManytoManyPaired.Count, Is.EqualTo(0));
          //          Assert.AreEqual(0, g2.ManyToOnePaired.Count);
          _ = a1.ManyToManyMaster.Add(g2);
          _ = a1.ManyToManyMaster.Add(g1);
          //          session.Session.Persist();
          Assert.That(a1.ManyToManyMaster.Count, Is.EqualTo(2));
          Assert.That(g1.ManytoManyPaired.Count, Is.EqualTo(1), "G1");
          Assert.That(g2.ManytoManyPaired.Count, Is.EqualTo(1), "G2");
          _ = a1.ManyToManyMaster.Remove(g1);
          _ = a1.ManyToManyMaster.Remove(g2);
          Assert.That(a1.ManyToManyMaster, Is.Not.Null);
          Assert.That(g1.ManytoManyPaired, Is.Not.Null);
          Assert.That(a1.ManyToManyMaster.Count, Is.EqualTo(0));
          Assert.That(g1.ManytoManyPaired.Count, Is.EqualTo(0));
          Assert.That(g2.ManytoManyPaired.Count, Is.EqualTo(0));
          session.SaveChanges();
          _ = a1.ManyToManyMaster.Add(g1);
          _ = a2.ManyToManyMaster.Add(g1);
          _ = a1.ManyToManyMaster.Add(g2);
          _ = a1.ManyToManyMaster.Add(g2);
          _ = a1.ManyToManyMaster.Add(g1);
          _ = a1.ManyToManyMaster.Add(g1);
          _ = g1.ManytoManyPaired.Add(a1);
          _ = g2.ManytoManyPaired.Add(a2);
          Assert.That(a1.ManyToManyMaster.Count, Is.EqualTo(2));
          Assert.That(a2.ManyToManyMaster.Count, Is.EqualTo(2));
          Assert.That(g1.ManytoManyPaired.Count, Is.EqualTo(2));
          Assert.That(g2.ManytoManyPaired.Count, Is.EqualTo(2));
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
          _ = a1.ManyToManyMaster.Add(g1);
          _ = g2.ManytoManyPaired.Add(a1);
          _ = g1.ManytoManyPaired.Add(a2);
          _ = a2.ManyToManyMaster.Add(g2);
          transaction.Complete();
        }

        using (var t = session.OpenTransaction()) {
          Assert.That(a1.ManyToManyMaster.Count, Is.EqualTo(2));
          Assert.That(a2.ManyToManyMaster.Count, Is.EqualTo(2));
          Assert.That(g1.ManytoManyPaired.Count, Is.EqualTo(2));
          Assert.That(g2.ManytoManyPaired.Count, Is.EqualTo(2));
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
          Assert.That(a.ZeroToMany, Is.Not.Null);
          Assert.That(a.ZeroToMany.Count, Is.EqualTo(0));
          Assert.That(a.ZeroToMany.Contains(e1), Is.False);
          Assert.That(a.ZeroToMany.Contains(e2), Is.False);
          _ = a.ZeroToMany.Add(e1);
          _ = a.ZeroToMany.Add(e2);
          Assert.That(a.ZeroToMany.Count, Is.EqualTo(2));
          Assert.That(a.ZeroToMany.Contains(e1), Is.True);
          Assert.That(a.ZeroToMany.Contains(e2), Is.True);
          _ = a.ZeroToMany.Remove(e1);
          _ = a.ZeroToMany.Remove(e2);
          Assert.That(a.ZeroToMany.Count, Is.EqualTo(0));
          Assert.That(a.ZeroToMany.Contains(e1), Is.False);
          Assert.That(a.ZeroToMany.Contains(e2), Is.False);
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
          Assert.That(a.ZeroToMany, Is.Not.Null);
          Assert.That(a.ZeroToMany.Count, Is.EqualTo(0));
          Assert.That(a.ZeroToMany.Contains(e1), Is.False);
          Assert.That(a.ZeroToMany.Contains(e2), Is.False);
          _ = a.ZeroToMany.Add(e1);
          _ = a.ZeroToMany.Add(e2);
          CheckEnumerator(a.ZeroToMany, e1, e2);
          // Rollback
        }
      }
    }

    private void CheckEnumerator<T>(EntitySet<T> entitySet, params T[] items) where T : Entity
    {
      T[] currentItems = entitySet.ToArray();
      Assert.That(items.Length, Is.EqualTo(currentItems.Length));
      foreach (T item in items) {
        Assert.That(currentItems.Contains(item), Is.True);
      }
    }

    [Test]
    public void ParentChildrenTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var h = new H();
          var children = h.Children;
          _ = children.Add(new H());
          var first = children.First();
          Assert.That(first.Parent, Is.SameAs(h));
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