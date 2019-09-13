// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2019.02.14

using System;
using System.Data.SqlTypes;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0761_ReadingAverageByDecimalFieldModel;
using Xtensive.Sql.Drivers.SqlServer.Internals;

namespace Xtensive.Orm.Tests.Issues.IssueJira0761_ReadingAverageByDecimalFieldModel
{
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Precision = 29, Scale = 2)]
    public decimal Sum { get; set; }

    [Field(Precision = 29, Scale = 1)]
    public decimal Sum2 { get; set; }

    [Field(Precision = 28, Scale = 2)]
    public decimal Sum3 { get; set; }

    [Field(Precision = 27, Scale = 3)]
    public decimal Sum4 { get; set; }

    [Field(Precision = 26, Scale = 4)]
    public decimal Sum5 { get; set; }

    [Field(Precision = 25, Scale = 5)]
    public decimal Sum6 { get; set; }

    [Field(Precision = 24, Scale = 6)]
    public decimal Sum7 { get; set; }

    [Field(Precision = 23, Scale = 7)]
    public decimal Sum8 { get; set; }

    [Field(Precision = 22, Scale = 8)]
    public decimal Sum9 { get; set; }

    [Field(Precision = 21, Scale = 9)]
    public decimal Sum10 { get; set; }

    [Field (Precision = 10, Scale = 0)]
    public decimal Count { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0761_ReadingAverageByDecimalField : AutoBuildTest
  {
    private const int OrderCount = 100;

    private bool overrideDatabase = false;

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.SqlServerCe);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Order).Assembly, typeof (Order).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        for (int i = 0; i < OrderCount; i++) {
          new Order() {
            Sum = (i % 2==0) ? 100000000000000000000000000.11m : 100000000000000000000000000.12m,
            Sum2 = 100000000000000000000000000.3m,
            Sum3 = 10000000000000000000000000.33m,
            Sum4 = 100000000000000000000000.333m,
            Sum5 = 1000000000000000000000.3333m,
            Sum6 = 10000000000000000000.33333m,
            Sum7 = 100000000000000000.333333m,
            Sum8 = 1000000000000000.3333333m,
            Sum9 = 10000000000000.33333333m,
            Sum10 = 100000000000.333333333m,
            Count = OrderCount
          };
        }

        tx.Complete();
      }
    }

    [Test]
    public void AverageTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryResult = session.Query.All<Order>().Average(o => o.Sum);
        var localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum);
        Assert.That(queryResult, Is.EqualTo(localResult + 0.03m));

        queryResult = session.Query.All<Order>().Average(o => o.Sum2);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum2);
        Assert.That(queryResult, Is.EqualTo(localResult + 0.06m));

        queryResult = session.Query.All<Order>().Average(o => o.Sum3);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum3);
        Assert.That(queryResult, Is.EqualTo(localResult + 0.006m));

        queryResult = session.Query.All<Order>().Average(o => o.Sum4);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum4);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum5);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum5);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum6);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum6);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum7);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum7);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum8);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum8);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum9);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum9);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum10);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum10);
        Assert.That(queryResult, Is.EqualTo(localResult));
      }
    }

    [Test]
    public void SumTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryResult = session.Query.All<Order>().Sum(o => o.Sum);
        var localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum);
        Assert.That(queryResult, Is.EqualTo(localResult + 3));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum2);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum2);
        Assert.That(queryResult, Is.EqualTo(localResult + 6));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum3);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum3);
        Assert.That(queryResult, Is.EqualTo(localResult + 0.6m));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum4);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum4);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum5);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum5);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum6);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum6);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum7);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum7);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum8);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum8);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum9);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum9);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum10);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum10);
        Assert.That(queryResult, Is.EqualTo(localResult));
      }
    }
  }
}
