// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.06.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.ExecuteDelayedForIOrderedQueryableQueryModel;

namespace Xtensive.Orm.Tests.Linq.ExecuteDelayedForIOrderedQueryableQueryModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public int OrderByThisField { get; set; }
  }

}

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  public class ExecuteDelayedForIOrderedQueryableQuery : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      var randomer = new Random();
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var delaedQuery = session.Query.ExecuteDelayed(
          query => from zoo in query.All<TestEntity>()
            orderby zoo.OrderByThisField
            select zoo);
        Assert.IsInstanceOf<IEnumerable<TestEntity>>(delaedQuery);
        session.ExecuteDelayedQueries(true);
        int previousValue = -1;
        foreach (var testEntity in delaedQuery) {
          Assert.GreaterOrEqual(testEntity.OrderByThisField, previousValue);
          previousValue = testEntity.OrderByThisField;
        }
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      var randomer = new Random();
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction())
      {
        for (int i = 0; i < 10; i++)
        {
          new TestEntity { OrderByThisField = randomer.Next(0, 1000) };
        }
        transaction.Complete();
      }
    }
  }
}
