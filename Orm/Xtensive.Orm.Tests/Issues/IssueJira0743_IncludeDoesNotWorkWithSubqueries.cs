// Copyright (C) 2018-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2018.10.18

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0743_IncludeDoesNotWorkWithSubqueriesModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0743_IncludeDoesNotWorkWithSubqueries : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();

      configuration.Types.Register(typeof(TestEntity).Assembly, typeof(TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      Expression<Func<TestEntity, TestEntity2>> exp = e => e.List.FirstOrDefault().Link;
      Expression<Func<TestEntity, TestEntity2>> exp2 = e => e.LinkOnList.Link;

      configuration.LinqExtensions.Register(typeof(TestEntity).GetProperty("VirtualList"), exp);
      configuration.LinqExtensions.Register(typeof(TestEntity).GetProperty("VirtualLink"), exp2);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var item = new TestEntity(session) { String = "test1" };
        var item2 = new TestEntity2(session) { String = "test2", Value3 = MyEnum.Foo };
        item.Link = item2;
        var list = new ListEntity(session) { String = "test3", Owner = item, Link = item2 };
        item.LinkOnList = list;
        var item3 = new TestEntity(session) { String = "test4" };

        transaction.Complete();
      }
    }

    [Test]
    public void Case01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 1 };
        var result = session.Query.All<TestEntity>()
          .Where(e => ids.Contains(e.List.FirstOrDefault().Link.Id) && e.List.FirstOrDefault().Link.Id != 0)
          .ToArray();
        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 1 };
        var result = session.Query.All<TestEntity>()
          .Where(e => e.List.FirstOrDefault().Link.Id != 0 && ids.Contains(e.List.FirstOrDefault().Link.Id))
          .ToArray();
        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 1 };
        var result = session.Query.All<TestEntity>()
          .Where(e => ids.Contains(e.List.FirstOrDefault().Link.Id))
          .ToArray();
        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Where(e => e.Link != null && e.Link.Id != 632)
          .OrderBy(e => e.Link)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case05Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Where(e => e.VirtualLink != null && e.VirtualLink.Id != 6456)
          .OrderBy(e => e.VirtualLink)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case06Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Where(e => e.VirtualList != null && e.VirtualList.Id != 2457567L)
          .OrderBy(e => e.VirtualList)
          .ToArray();
        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case07Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 325453 };
        var result = session.Query.All<TestEntity>()
          .Where(e => e.Link != null && !ids.Contains(e.Link.Id))
          .OrderBy(e => e.Link)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case08Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 325453 };
        var result = session.Query.All<TestEntity>()
          .Where(e => e.VirtualLink != null && !ids.Contains(e.VirtualLink.Id))
          .OrderBy(e => e.VirtualLink)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case09Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 325453 };
        var result = session.Query.All<TestEntity>()
          .Where(e => e.VirtualList != null && !ids.Contains(e.VirtualList.Id))
          .OrderBy(e => e.VirtualList)
          .ToArray();
        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case10Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Where(e => e.Link.Id != 4573567L)
          .OrderBy(e => e.Link)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case11Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Where(e => e.VirtualLink.Id != 45275466L)
          .OrderBy(e => e.VirtualLink)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case12Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Where(e => e.VirtualList.Id != 247456L)
          .OrderBy(e => e.VirtualList)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case13Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 325453 };
        var result = session.Query.All<TestEntity>()
          .Where(e => !ids.Contains(e.Link.Id))
          .OrderBy(e => e.Link)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case14Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 325453 };
        var result = session.Query.All<TestEntity>()
          .Where(e => !ids.Contains(e.VirtualLink.Id))
          .OrderBy(e => e.VirtualLink)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case15Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 325453 };
        var result = session.Query.All<TestEntity>()
          .Where(e => !ids.Contains(e.VirtualList.Id))
          .OrderBy(e => e.VirtualList)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case16Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 325453 };
        var result = session.Query.All<TestEntity>()
          .Where(e => !ids.Contains(e.VirtualLink.Id))
          .OrderBy(e => e.VirtualList)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case17Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new[] { "test1", "test2" };
        var result = session.Query.All<TestEntity>()
          .Where(e => ids.Contains(e.List.Where(y => y.Id != Guid.NewGuid()).First().Owner.List.FirstOrDefault().Link.String))
          .OrderBy(e => e.VirtualList)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case18Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new[] { "test1", "test2" };
        var result = session.Query.All<TestEntity>()
          .Where(e => e.List.Where(y => y.Id != Guid.NewGuid()).First().Owner.List.FirstOrDefault().Link.String.In(ids))
          .OrderBy(e => e.VirtualList)
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case19Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var ids = new long[] { 1 };
        var result = session.Query.All<TestEntity>()
          .Where(e => e.List.FirstOrDefault().Link.Id.In(ids))
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case20Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Where(e => e.List.FirstOrDefault().Link.Id.In(1L, 34654565637L, 45756723L))
          .ToArray();

        Assert.That(result.Single().Link.Id, Is.EqualTo(1));
      }
    }

    [Test]
    public void Case21Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var values = new[] { MyEnum.Bar, MyEnum.Foo };
        var result = session.Query.All<TestEntity>()
          .Select(e => values.Contains(e.List.FirstOrDefault().Link.Value3.Value)).ToArray();

        var values2 = new MyEnum?[] { MyEnum.Bar, MyEnum.Foo };
        var expected = session.Query.All<TestEntity>().AsEnumerable()
          .Select(e => values2.Contains(e.List.FirstOrDefault()?.Link.Value3.Value)).ToArray();

        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(expected.Length, Is.EqualTo(2));
        Assert.That(result.Except(expected).Count(), Is.EqualTo(0));
      }
    }

    [Test]
    public void Case22Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var values = new[] { MyEnum.Bar, MyEnum.Foo };
        var result = session.Query.All<TestEntity>()
          .Select(e => e.List.FirstOrDefault().Link.Value3.Value.In(values)).ToArray();

        var expected = session.Query.All<TestEntity>().AsEnumerable()
          .Select(e => {
            var firsItem = e.List.FirstOrDefault();
            if (firsItem == null)
              return false;
            return firsItem.Link.Value3.Value.In(values);
          })
          .ToArray();

        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(expected.Length, Is.EqualTo(2));
        Assert.That(result.Except(expected).Count(), Is.EqualTo(0));
      }
    }

    [Test]
    public void Case23Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var values = new long[] { 56, 89, 0 };
        var result = session.Query.All<TestEntity>()
          .Select(e => values.Contains(e.Link.Value2.GetValueOrDefault())).ToArray();

        Assert.That(result, Is.All.True);
      }
    }

    [Test]
    public void Case24Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var values = new long[] { 56, 89, 0 };
        var result = session.Query.All<TestEntity>()
          .Select(e => e.List.FirstOrDefault().Link.Value2.GetValueOrDefault().In(values)).ToArray();

        Assert.That(result, Is.All.True);
      }
    }

    [Test]
    public void Case25Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var values = new[] { "st1", "st2" };
        var result = session.Query.All<TestEntity>()
          .Select(e => values.Contains(e.List.FirstOrDefault().Link.String.Substring(2, 3))).ToArray();

        var expected = session.Query.All<TestEntity>().AsEnumerable()
         .Select(e => {
           var firsItem = e.List.FirstOrDefault();
           if (firsItem == null)
             return false;
           return values.Contains(firsItem.Link.String.Substring(2, 3));
         })
         .ToArray();

        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(expected.Length, Is.EqualTo(2));
        Assert.That(result.Except(expected).Count(), Is.EqualTo(0));
      }
    }

    [Test]
    public void Case26Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var values = new long[] { 56, 89, 1 };
        var result = session.Query.All<TestEntity>()
          .Select(e => new { Contains = values.Contains(e.Link.Value2.GetValueOrDefault() + e.Link.Id), String = e.String }).ToArray();

        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.First(i => i.String == "test1").Contains, Is.True);
        Assert.That(result.First(i => i.String == "test4").Contains, Is.False);
      }
    }

    [Test]
    public void Case27Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var values = new long[] { 56, 89, 1 };
        var result = session.Query.All<TestEntity>()
          .Select(e => values.Contains(e.List.Select(l => l.Link.Value2 + e.Link.Value2).First().Value)).ToArray();

        Assert.That(result, Is.All.False);
      }
    }

    [Test]
    public void Case28Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Select(
            e => new {
              IsIn = e.List.Select(x => x.String).First().In("test1", "test2", "test3", "test4"),
              String = e.String
            })
          .ToArray();

        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.First(i => i.String == "test1").IsIn, Is.True);
        Assert.That(result.First(i => i.String == "test4").IsIn, Is.False);
      }
    }

    [Test]
    public void Case29Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<PowerDialerCampaign>()
          .Where(
            x => (x.Active
              ? x.Archived
                ? ListCategory.Archived
                  : x.Status.In(CampaignStatus.Draft, CampaignStatus.Stopped)
                    ? ListCategory.InactiveList
                    : ListCategory.ActiveList
                : ListCategory.Deleted)
              .In(ListCategory.ActiveList,
                 ListCategory.InactiveList,
                 ListCategory.Archived,
                 ListCategory.Deleted))
         .ToList();
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0743_IncludeDoesNotWorkWithSubqueriesModel
{
  [HierarchyRoot]
  public class PowerDialerCampaign : Entity 
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public bool Active { get; set; }

    [Field]
    public bool Archived { get; set; }

    [Field]
    public CampaignStatus Status { get; set; }

    [Field]
    public ListCategory ListCategory { get; set; }
  }

  public enum CampaignStatus
  {
    Draft,
    Stopped
  }

  public enum ListCategory
  {
    ActiveList,
    InactiveList,
    Archived,
    Deleted
  }

  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
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

    public TestEntity(Session session, string value)
      : this(session)
    {
      String = value;
    }
  }

  [HierarchyRoot]
  public class ListEntity : Entity
  {
    [Field, Key]
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
    [Field, Key]
    public long Id { get; private set; }

    [Field(Nullable = false)]
    public string String { get; set; }

    [Field]
    public MyEnum? Value { get; set; }

    [Field]
    public long? Value2 { get; set; }

    [Field]
    public MyEnum? Value3 { get; set; }

    public TestEntity2(Session session)
      : base(session)
    {
    }
  }

  public enum MyEnum
  {
    Foo = 0,
    Bar = 1,
    Qux = 2,
  }
}
