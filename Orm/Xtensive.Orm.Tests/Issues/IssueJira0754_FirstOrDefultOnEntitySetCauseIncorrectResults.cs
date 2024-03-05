// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0754_FirstOrDefultOnEntitySetCauseIncorrectResultsModel;


namespace Xtensive.Orm.Tests.Issues.IssueJira0754_FirstOrDefultOnEntitySetCauseIncorrectResultsModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    public string String { get; set; }

    [Field]
    public ListEntity LinkOnList { get; set; }

    [Field]
    [Association(PairTo = "Owner")]
    public EntitySet<ListEntity> List { get; set; }

    [Field]
    public TestEntity2 Link { get; set; }

    public TestEntity2 VirtualList { get; set; }

    public TestEntity2 VirtualLink { get; set; }

    public TestEntity(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class ListEntity : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    public string String { get; set; }

    [Field(Nullable = false)]
    public TestEntity Owner { get; set; }

    [Field(Nullable = false)]
    public TestEntity2 Link { get; set; }

    public ListEntity(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity2 : Entity
  {
    public TestEntity2(Session session)
      : base(session)
    {
    }

    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field(Nullable = false)]
    public string String { get; set; }
  }
}


namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0754_FirstOrDefultOnEntitySetCauseIncorrectResults : AutoBuildTest
  {
    protected override void CheckRequirements() => Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;

      config.Types.Register(typeof(TestEntity));
      config.Types.Register(typeof(TestEntity2));
      config.Types.Register(typeof(ListEntity));

      Expression<Func<TestEntity, TestEntity2>> exp = e => e.List.FirstOrDefault().Link;
      Expression<Func<TestEntity, TestEntity2>> exp2 = e => e.LinkOnList.Link;
      config.LinqExtensions.Register(typeof(TestEntity).GetProperty("VirtualList"), exp);
      config.LinqExtensions.Register(typeof(TestEntity).GetProperty("VirtualLink"), exp2);

      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var item = new TestEntity(session) { String = "test" };
        var item2 = new TestEntity2(session) { String = "test" };
        var list = new ListEntity(session) { String = "test", Owner = item, Link = item2 };
        item.LinkOnList = list;

        _ = new TestEntity(session) { String = "test2" };

        tx.Complete();
      }
    }

    [Test]
    public void Case1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var count = session.Query.All<TestEntity>()
          .Select(e => new { e.VirtualList.String, e.VirtualList.Id })
          .Count();
        Assert.That(count, Is.EqualTo(2));

        var entities = session.Query.All<TestEntity>()
          .Select(e => new { e.VirtualList.String, e.VirtualList.Id })
          .ToArray().OrderBy(x => x.Id).ToArray();

        Assert.That(entities[0].String, Is.Null);
        Assert.That(entities[0].Id, Is.EqualTo(Guid.Empty));

        Assert.That(entities[1].String, Is.EqualTo("test"));
        Assert.That(entities[1].Id, Is.Not.EqualTo(Guid.Empty));
      }
    }

    [Test]
    public void Case2()
    {
      using (var s = Domain.OpenSession())
      using (s.Activate())
      using (var t = s.OpenTransaction()) {
        var count = s.Query.All<TestEntity>()
          .Select(e => new { e.String, e.Id })
          .Count();
        Assert.That(count, Is.EqualTo(2));

        var entities = s.Query.All<TestEntity>()
          .Select(e => new { e.String, e.Id })
          .ToArray().OrderBy(x => x.String).ToArray();

        Assert.That(entities[0].String, Is.EqualTo("test"));
        Assert.That(entities[0].Id, Is.Not.EqualTo(Guid.Empty));

        Assert.That(entities[1].String, Is.EqualTo("test2"));
        Assert.That(entities[1].Id, Is.Not.EqualTo(Guid.Empty));
      }
    }

    [Test]
    public void Case3()
    {
      using (var s = Domain.OpenSession())
      using (s.Activate())
      using (var t = s.OpenTransaction()) {
        var count = s.Query.All<TestEntity>()
          .Select(e => new { e.LinkOnList.String, e.LinkOnList.Id })
          .Count();
        Assert.That(count, Is.EqualTo(2));

        var entities = s.Query.All<TestEntity>()
          .Select(e => new { e.LinkOnList.String, e.LinkOnList.Id })
          .ToArray()
          .OrderBy(x => x.String)
          .ToArray();

        Assert.That(entities[0].String, Is.Null);
        Assert.That(entities[0].Id, Is.EqualTo(Guid.Empty));

        Assert.That(entities[1].String, Is.EqualTo("test"));
        Assert.That(entities[1].Id, Is.Not.EqualTo(Guid.Empty));
      }
    }
  }
}
