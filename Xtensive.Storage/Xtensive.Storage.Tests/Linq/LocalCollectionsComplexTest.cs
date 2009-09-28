// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2009.09.28

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Linq.LocalCollectionsComplexTestModel;

namespace Xtensive.Storage.Tests.Linq.LocalCollectionsComplexTestModel
{
  public class EntityStructure : Structure
  {
    [Field]
    public EntityA A { get; set; }

    [Field]
    public string StructureName { get; set; }

    [Field]
    public DateTime StructureAge { get; set; }
  }

  public class ComplexStructure : Structure
  {
    [Field]
    public EntityA A { get; set; }

    [Field]
    public string StructureName { get; set; }

    [Field]
    public DateTime StructureAge { get; set; }

    [Field]
    public EntityStructure EntityStructure { get; set; }
  }

  [HierarchyRoot]
  public class EntityA : Entity
  {
    [Field]
    [Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime Age { get; set; }
  }

  [HierarchyRoot]
  public class EntityB : Entity
  {
    [Field]
    [Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime Age { get; set; }

    [Field]
    public ComplexStructure AdditionalInfo { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Linq
{
  public class LocalCollectionsComplexTest : AutoBuildTest
  {
    const int count = 100;
    
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (EntityA).Assembly, typeof (EntityA).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Session session = Session.Open(Domain)) {
        using (TransactionScope t = Transaction.Open()) {
          IEnumerable<EntityA> entitiesA = Enumerable
            .Range(0, count - 1)
            .Select(i => new EntityA {
              Age = new DateTime(i),
              Name = "NameA_" + i
            });

          IEnumerable<EntityB> entitiesB = Enumerable
            .Range(0, count - 1)
            .Select(i => new EntityB {
              Age = new DateTime(2000 + i, 2, 2),
              Name = "NameB_" + i,
              AdditionalInfo = new ComplexStructure {
                A = new EntityA {
                  Age = new DateTime(2000 + i, 3, 3),
                  Name = "NameA_1_" + i
                },
                StructureAge = new DateTime(2000 + i, 7, 7),
                StructureName = "StructureName_1_" + i,
                EntityStructure = new EntityStructure {
                  A = new EntityA {
                    Age = new DateTime(2000 + i, 1, 1),
                    Name = "NameA_2_" + i
                  },
                  StructureAge = new DateTime(2000 + i, 10, 10),
                  StructureName = "StructureName_2_" + i,
                }
              }
            })
            .ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void UnionEntityTest()
    {
      using (Session session = Session.Open(Domain)) {
        using (TransactionScope t = Transaction.Open()) {
          session.Persist();
          var localItems = Query<EntityB>.All.Take(count / 2).ToArray();
          var union = Query<EntityB>.All.Union(localItems);
          var expected = Query<EntityB>.All.AsEnumerable().Union(localItems);
          Assert.AreEqual(0, expected.Except(union).Count());
        }
      }
    }

    [Test]
    public void UnionStructureTest()
    {
      using (Session session = Session.Open(Domain)) {
        using (TransactionScope t = Transaction.Open()) {
          session.Persist();
          var localItems = Query<EntityB>.All.Select(b=>b.AdditionalInfo).Take(count / 2).ToArray();
          var union = Query<EntityB>.All.Select(b => b.AdditionalInfo).Union(localItems);
          var expected = Query<EntityB>.All.AsEnumerable().Select(b => b.AdditionalInfo).Union(localItems);
          Assert.AreEqual(0, expected.Except(union).Count());
        }
      }
    }

    [Test]
    public void UnionFieldTest()
    {
      using (Session session = Session.Open(Domain)) {
        using (TransactionScope t = Transaction.Open()) {
          session.Persist();
          var localItems = Query<EntityB>.All.Select(b=>b.Name).Take(count / 2).ToArray();
          var union = Query<EntityB>.All.Select(b => b.Name).Union(localItems);
          var expected = Query<EntityB>.All.AsEnumerable().Select(b => b.Name).Union(localItems);
          Assert.AreEqual(0, expected.Except(union).Count());
        }
      }
    }

    [Test]
    public void JoinEntityDirectTest()
    {
      using (Session session = Session.Open(Domain)) {
        using (TransactionScope t = Transaction.Open()) {
          session.Persist();
          var localItems = Query<EntityB>.All.Take(count / 2).ToArray();
          var join = Query<EntityB>.All.Join(localItems, b=>b, l=>l, (b,l) => new{b,l});
          var expected = Query<EntityB>.All.AsEnumerable().Join(localItems, b => b, l => l, (b, l) => new { b, l });
          Assert.AreEqual(0, expected.Except(join).Count());
        }
      }
    }


    [Test]
    public void JoinEntityIndirect2Test()
    {
      using (Session session = Session.Open(Domain))
      {
        using (TransactionScope t = Transaction.Open())
        {
          session.Persist();
          var localItems = Query<EntityB>.All.Take(count / 2).ToArray();
          var join = Query<EntityB>.All.Join(localItems, b => b.AdditionalInfo.A, l => l.AdditionalInfo.A, (b, l) => new { b, l });
          var expected = Query<EntityB>.All.AsEnumerable().Join(localItems, b => b.AdditionalInfo.A, l => l.AdditionalInfo.A, (b, l) => new { b, l });
          Assert.AreEqual(0, expected.Except(join).Count());
        }
      }
    }

    [Test]
    public void JoinEntityIndirect3Test()
    {
      using (Session session = Session.Open(Domain))
      {
        using (TransactionScope t = Transaction.Open())
        {
          session.Persist();
          var localItems = Query<EntityB>.All.Take(count / 2).ToArray();
          var join = Query<EntityA>.All.Join(localItems, a => a, l => l.AdditionalInfo.A, (a, l) => new { a, l });
          var expected = Query<EntityA>.All.AsEnumerable().Join(localItems, a => a, l => l.AdditionalInfo.A, (a, l) => new { a, l });
          Assert.AreEqual(0, expected.Except(join).Count());
        }
      }
    }

    [Test]
    public void JoinStructureDirectTest()
    {
      using (Session session = Session.Open(Domain)) {
        using (TransactionScope t = Transaction.Open()) {
          session.Persist();
          var localItems = Query<EntityB>.All.Select(b=>b.AdditionalInfo).Take(count / 2).ToArray();
          var join = Query<EntityB>.All.Select(b => b.AdditionalInfo).Join(localItems, b => b, l => l, (b, l) => new { b, l });
          var expected = Query<EntityB>.All.AsEnumerable().Select(b => b.AdditionalInfo).Join(localItems, b => b, l => l, (b, l) => new { b, l });
          Assert.AreEqual(0, expected.Except(join).Count());
        }
      }
    }

    [Test]
    public void JoinStructureIndirectTest()
    {
      using (Session session = Session.Open(Domain)) {
        using (TransactionScope t = Transaction.Open()) {
          session.Persist();
          var localItems = Query<EntityB>.All.Take(count / 2).ToArray();
          var join = Query<EntityB>.All.Join(localItems, b => b.AdditionalInfo, l => l.AdditionalInfo, (b, l) => new { b, l });
          var expected = Query<EntityB>.All.AsEnumerable().Join(localItems, b => b.AdditionalInfo, l => l.AdditionalInfo, (b, l) => new { b, l });
          Assert.AreEqual(0, expected.Except(join).Count());
        }
      }
    }

//    [Test]
//    public void UnionFieldTest()
//    {
//      using (Session session = Session.Open(Domain)) {
//        using (TransactionScope t = Transaction.Open()) {
//          session.Persist();
//          var localItems = Query<EntityB>.All.Select(b=>b.Name).Take(count / 2).ToArray();
//          var union = Query<EntityB>.All.Select(b => b.Name).Union(localItems);
//          var expected = Query<EntityB>.All.AsEnumerable().Select(b => b.Name).Union(localItems);
//          Assert.AreEqual(0, expected.Except(union).Count());
//        }
//      }
//    }
  }
}