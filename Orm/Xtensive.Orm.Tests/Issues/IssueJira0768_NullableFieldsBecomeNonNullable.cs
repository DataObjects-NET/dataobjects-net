// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.07.17

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0768_NullableFieldsBecomeNonNullableModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0768_NullableFieldsBecomeNonNullableModel
{
  [HierarchyRoot]
  public class TestEntity1 : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public int Id { get; set; }

    [Field]
    public TestEntity2 TestEntity1LinkToTestEntity2 { get; set; }

    public TestEntity1(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity2 : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public string Name { get; set; }

    [Field(Nullable = false)]
    public TestEntity3 TestEntity2LinkToTestEntity3 { get; set; }

    public TestEntity2(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity3 : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public string Name { get; set; }

    public TestEntity3(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity4 : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public string Name { get; set; }

    [Field(Nullable = false)]
    public string Description { get; set; }

    public TestEntity4(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0768_NullableFieldsBecomeNonNullable : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity1));
      configuration.Types.Register(typeof (TestEntity2));
      configuration.Types.Register(typeof (TestEntity3));
      configuration.Types.Register(typeof (TestEntity4));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var testEntity3 = new TestEntity3(session) {Name = "test"};
        var testEntity2 = new TestEntity2(session) {Name = "test", TestEntity2LinkToTestEntity3 = testEntity3};
        new TestEntity1(session) { TestEntity1LinkToTestEntity2 = testEntity2 };
        new TestEntity1(session);

        transaction.Complete();
      }
    }

    [Test]
    public void SingleWhereBeforeSelect()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var item = session.Query.All<TestEntity3>().Single();

        var resultCount = session.Query.All<TestEntity1>()
          .Where(it => true || it.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Select(e => new {e.Id, Test = e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name})
          .Count();

        Assert.AreEqual(2, resultCount);
      }
    }

    [Test]
    public void SingleWhereAfterSelect()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var item = session.Query.All<TestEntity3>().Single();

        var resultCount = session.Query.All<TestEntity1>()
          .Select(e => new {e, e.Id, Test = e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name})
          .Where(it => true || it.e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Count();

        Assert.AreEqual(2, resultCount);
      }
    }

    [Test]
    public void DoubleWhereBeforeSelect()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var item = session.Query.All<TestEntity3>().Single();

        var resultCount = session.Query.All<TestEntity1>()
          .Where(it => true || it.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Where(it => true || it.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Select(e => new {e.Id, Test = e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name})
          .Count();

        Assert.AreEqual(2, resultCount);
      }
    }

    [Test]
    public void DoubleWhereAfterSelect()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var item = session.Query.All<TestEntity3>().Single();

        var resultCount = session.Query.All<TestEntity1>()
          .Select(e => new {e, e.Id, Test = e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name})
          .Where(it => true || it.e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Where(it => true || it.e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Count();

        Assert.AreEqual(2, resultCount);
      }
    }

    [Test]
    public void SingleWhereAndSubquery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var item = session.Query.All<TestEntity3>().Single();

        var resultCount = session.Query.All<TestEntity1>()
          .Where(it => true || it.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Select(e => new {
            e.Id,
            Test = session.Query.All<TestEntity4>()
                     .SingleOrDefault(it => it.Name==e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name).Description
                   ?? e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name
          })
          .Count();

        Assert.AreEqual(2, resultCount);
      }
    }

    [Test]
    public void DoubleWhereBeforeSubquery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var item = session.Query.All<TestEntity3>().Single();

        var resultCount = session.Query.All<TestEntity1>()
          .Where(it => true || it.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Where(it => true || it.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Select(e => new {
            e.Id,
            Test = session.Query.All<TestEntity4>()
                     .SingleOrDefault(it => it.Name==e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name).Description
                   ?? e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name
          })
          .Count();

        Assert.AreEqual(2, resultCount);
      }
    }

    [Test]
    public void DoubleWhereAfterSubquery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var item = session.Query.All<TestEntity3>().Single();

        var resultCount = session.Query.All<TestEntity1>()
          .Select(e => new {
            e,
            e.Id,
            Test = session.Query.All<TestEntity4>()
                     .SingleOrDefault(it => it.Name==e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name).Description
                   ?? e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3.Name
          })
          .Where(it => true || it.e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Where(it => true || it.e.TestEntity1LinkToTestEntity2.TestEntity2LinkToTestEntity3==item)
          .Count();

        Assert.AreEqual(2, resultCount);
      }
    }
  }
}
