// Copyright (C) 2012-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
        _ = new Entity1();
        _ = new Entity2();
        tx.Complete();
      }
    }

    [Test]
    [IgnoreIfGithubActions("Set operations over rows that contain Key don't work, it is assumed that rowset belongs to single entity")]
    public void MainTest()
    {
      // The problem consists of two parts
      // 1 - we don't put type identifier to the query in such cases, basically we ignore it
      // 2 - we materialize keys without using type identifiers from query but get them from KeyExpression, but our expression has two key expressions
      //     and only first is used to materialize rows to keys.
      // Unless we use type identifier from row to determine key type, it is impossible to fix

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