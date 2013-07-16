// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.04.30

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0443_FirstOrDefaultInSubqueryUsesWrongDefaultModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0443_FirstOrDefaultInSubqueryUsesWrongDefaultModel
  {
    [HierarchyRoot]
    public class MyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(PairTo = "Owner")]
      public EntitySet<MyItem> Items { get; set; }
    }

    [HierarchyRoot]
    public class MyItem : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public MyEnum MyEnum { get; set; }

      [Field]
      public MyEntity Owner { get; set; }
    }

    public enum MyEnum
    {
      None,
      Value1
    }
  }

  public class IssueJira0443_FirstOrDefaultInSubqueryUsesWrongDefault : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = new MyEntity();
        tx.Complete();
      }
    }

    [Test]
    public void FirstOrDefaultTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryResult = session.Query.All<MyItem>().Select(e => e.MyEnum).FirstOrDefault();
        Assert.That(queryResult, Is.EqualTo(MyEnum.None));

        var subqueryResult = session.Query.All<MyEntity>().Select(e => e.Items.Select(i => i.MyEnum).FirstOrDefault()).Single();
        Assert.That(subqueryResult, Is.EqualTo(MyEnum.None));

        var subqueryCountResult = session.Query.All<MyEntity>().Count(e => e.Items.Select(i => i.MyEnum).FirstOrDefault()==MyEnum.None);
        Assert.That(subqueryCountResult, Is.EqualTo(1));

        tx.Complete();
      }
    }

    [Test]
    public void SingleOrDefaultTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryResult = session.Query.All<MyItem>().Select(e => e.MyEnum).SingleOrDefault();
        Assert.That(queryResult, Is.EqualTo(MyEnum.None));

        var subqueryResult = session.Query.All<MyEntity>().Select(e => e.Items.Select(i => i.MyEnum).SingleOrDefault()).Single();
        Assert.That(subqueryResult, Is.EqualTo(MyEnum.None));

        var subqueryCountResult = session.Query.All<MyEntity>().Count(e => e.Items.Select(i => i.MyEnum).SingleOrDefault()==MyEnum.None);
        Assert.That(subqueryCountResult, Is.EqualTo(1));

        tx.Complete();
      }
    }
  }
}