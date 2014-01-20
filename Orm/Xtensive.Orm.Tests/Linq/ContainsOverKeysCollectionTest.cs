// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Tests.Linq.ContainsOverKeysCollectionTestModel;

namespace Xtensive.Orm.Tests.Linq.ContainsOverKeysCollectionTestModel
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Field { get; set; }
  }

  [HierarchyRoot]
  public class MyEntity2 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public MyEntity Entity { get; set; }
  }
  
  [HierarchyRoot]
  public class MyEntity3 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public MyEntity2 Entity { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Linq.ContainsOverKeysCollectionTest
{
  [TestFixture]
  public class ContainsOverKeysCollectionTest : AutoBuildTest
  {
    private IEnumerable<Key> keysList;
    private IEnumerable<int> idsList;
    private IEnumerable<MyEntity> myEntities; 
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new MyEntity3 {
          Entity = new MyEntity2() {
            Entity = new MyEntity {Field = "ABC"}
          }
        };

        new MyEntity3 {
          Entity = new MyEntity2 {
            Entity = new MyEntity {Field = "DEF"}
          }
        };

        new MyEntity3 {
          Entity = new MyEntity2() {
            Entity = new MyEntity {Field = "GHI"}
          }
        };
        
        keysList = session.Query.All<MyEntity>().Select(e => e.Key).ToList();
        transaction.Complete();
      }
    }

    [Test]
    public void DirectKeyContaisTest()
    {
      Assert.DoesNotThrow(
        () => {
          using (var session = Domain.OpenSession())
          using (var transaction = session.OpenTransaction()) {
            var query = session.Query.All<MyEntity>().Where(e => keysList.Contains(e.Key)).ToList();
          }
        });
    }

    [Test]
    public void OneLevelContainsText()
    {
      Assert.DoesNotThrow(
        () => {
          using (var session = Domain.OpenSession())
          using (var transaction = session.OpenTransaction()) {
            var query = session.Query.All<MyEntity2>().Where(e => keysList.Contains(e.Entity.Key)).ToList();
          }
        });

    }

    [Test]
    public void TwoLevelContainsText()
    {
      Assert.DoesNotThrow(
        () => {
          using (var session = Domain.OpenSession())
          using (var transaction = session.OpenTransaction()) {
            var query = session.Query.All<MyEntity3>().Where(e => keysList.Contains(e.Entity.Entity.Key)).ToList();
          }
        });
    }

    [Test]
    public void SubQueryTest()
    {
      Assert.Throws<QueryTranslationException>(
        () => {
          using (var session = Domain.OpenSession())
          using (var transaction = session.OpenTransaction()) {
            var query = session.Query.All<MyEntity3>()
              .Where(e => keysList.Contains(session.Query.All<MyEntity>().First(el => el.Key==e.Entity.Entity.Key).Key))
              .ToList();
          }
        });

    }

    [Test]
    public void AnonimusTypeKeyContainsTest()
    {
      Assert.Throws<QueryTranslationException>(
        () => {
          using (var session = Domain.OpenSession())
          using (var transaction = session.OpenTransaction()) {
            var query = session.Query.All<MyEntity3>().Where(e => keysList.Contains(
              new {
                Key = e.Key, 
                EntityKey = e.Entity.Key, 
                DoubleEntityKey = e.Entity.Entity.Key
              }
              .DoubleEntityKey)).ToList();
          }
        });
    }
  }
}
