// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.11

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.KeysUnionTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace KeysUnionTestModel
  {
    [HierarchyRoot]
    public class Entity1 : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; private set; }

      public Entity1()
      {
        Name = TypeInfo.Name;
      }
    }

    [HierarchyRoot]
    public class Entity2 : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; private set; }

      public Entity2()
      {
        Name = TypeInfo.Name;
      }
    }
  }

  public class KeysUnionTest : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Entity1).Assembly, typeof (Entity1).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new Entity1();
        new Entity2();
        tx.Complete();
      }
    }

    [Mute]
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var q1 = session.Query.All<Entity1>().Select(e => new {e.Key, e.Name});
        var q2 = session.Query.All<Entity2>().Select(e => new {e.Key, e.Name});
        var result = q1.Union(q2).ToArray();
        foreach (var r in result) {
          Assert.That(r.Key.TypeInfo.Name, Is.EqualTo(r.Name));
        }
      }
    }
  }
}