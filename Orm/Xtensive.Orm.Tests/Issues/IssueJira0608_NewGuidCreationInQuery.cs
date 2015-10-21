// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.10.19

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0608_NewGuidCreationInQueryModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0608_NewGuidCreationInQueryModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Guid? UniqueIdentifier { get; set; }

    [Field]
    public int Count { get; set; }
  }

  [HierarchyRoot]
  public class TestEntity2 : Entity
  {
    [Field,Key]
    public int Id { get; set; }

    [Field]
    public int Count { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0608_NewGuidCreationInQuery : AutoBuildTest
  {
    private const int ItemsInGroup = 4;
    private const int GroupCount = 10;

    [Test]
    public void GroupByTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var queryResult = session.Query.All<TestEntity>().GroupBy(el => el.Name).Select(grouping => new {grouping.Key, Id = Guid.NewGuid()}).ToList();
        Assert.That(queryResult.Count, Is.EqualTo(GroupCount));
        Assert.That(queryResult.Select(el=>el.Id).Distinct().Count(), Is.EqualTo(queryResult.Count));

        queryResult = session.Query.All<TestEntity>()
          .Select(el => new {el.Id, el.Name, el.UniqueIdentifier})
          .GroupBy(selected => selected.Name)
          .Select(grouping => new {grouping.Key, Id = Guid.NewGuid()}).ToList();

        Assert.That(queryResult.Count, Is.EqualTo(GroupCount));
        Assert.That(queryResult.Select(el => el.Id).Distinct().Count(), Is.EqualTo(queryResult.Count));

        queryResult = session.Query.All<TestEntity>()
          .Where(el => el.Count==GroupCount - 3)
          .GroupBy(el => el.Name)
          .Select(grouping => new {grouping.Key, Id = Guid.NewGuid()}).ToList();

        Assert.That(queryResult.Count, Is.EqualTo(ItemsInGroup));
        Assert.That(queryResult.Select(el => el.Id).Distinct().Count(), Is.EqualTo(queryResult.Count));

        var validNames = Enumerable.Range(3, GroupCount - 8).Select(el => string.Format("Name{0}", el)).ToArray();
        queryResult = session.Query.All<TestEntity>()
          .Where(el=>el.Name.In(validNames))
          .GroupBy(validEntity=> validEntity.Name)
          .Select(grouping => new {grouping.Key, Id = Guid.NewGuid()}).ToList();

        Assert.That(queryResult.Count, Is.EqualTo(ItemsInGroup * (GroupCount - 8)));
        Assert.That(queryResult.Select(el => el.Id).Distinct().Count(), Is.EqualTo(queryResult.Count));

        queryResult = session.Query.All<TestEntity>()
          .LeftJoin(Query.All<TestEntity2>(), el => el.Count, el => el.Count, (entity, entity2) => new {entity.Name, entity.UniqueIdentifier, entity2.Count})
          .GroupBy(el => el.Name).Select(grouping => new {grouping.Key, Id = Guid.NewGuid()}).ToList();

        Assert.That(queryResult.Count, Is.EqualTo(ItemsInGroup * GroupCount));
        Assert.That(queryResult.Select(el => el.Id).Distinct().Count(), Is.EqualTo(queryResult.Count));
      }
    }

    [Test]
    public void FiltersTest()
    {
      
    }

    [Test]
    public void SelectTest()
    {
      
    }

    [Test]
    public void OrderByTest()
    {
      
    }

    [Test]
    public void AggregateTest()
    {
      
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (var i = 0; i < ItemsInGroup; i++)
          foreach (var counterValue in Enumerable.Range(1,GroupCount))
            new TestEntity() { Name = "Name" + counterValue, Count = counterValue, UniqueIdentifier = Guid.NewGuid() };

        transaction.Complete();
      }
    }
  }
}
