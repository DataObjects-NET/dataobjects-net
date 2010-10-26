// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0641_GroupBySumError_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0641_GroupBySumError_Model
  {
    [Serializable]
    [HierarchyRoot]
    public class MyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Text { get; set; }

      [Field]
      public EntitySet<Line> Lines { get; private set; }
    }

    [HierarchyRoot]
    public class Line : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public decimal? Rate { get; set; }

      [Field]
      public decimal? Amount { get; set; }
    }
  }

  [Serializable]
  public class Issue0641_GroupBySumError : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (Line).Assembly, typeof (Line).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      {
        using (TransactionScope transactionScope = session.OpenTransaction())
        {
          // Creating new persistent object
          var helloWorld = new MyEntity
                             {
                               Text = "Hello World!",
                               Lines =
                                 {
                                   new Line {Rate = 5.5M, Amount = 2M},
                                   new Line {Rate = 5.5M, Amount = 2M},
                                   new Line {Rate = 5.5M, Amount = 2M},
                                   new Line {Rate = 5.5M, Amount = 2M},
                                   new Line {Rate = 5.5M, Amount = 2M},
                                   new Line {Rate = 5.5M, Amount = 2M}
                                 }
                             };
          // Committing transaction
          transactionScope.Complete();
        }
      }

      // Reading all persisted objects from another Session
      using (var session = Domain.OpenSession())
      {
        using (var transactionScope = session.OpenTransaction()) {
          var query = from l in session.Query.All<MyEntity>().First().Lines
                      group l by l.Rate.GetValueOrDefault() into g
                      select new {Rate = g.Key, BaseAmount = g.Sum(l => l.Amount.GetValueOrDefault())};
          var actual = query.ToList().Single();
          var expected = (from l in session.Query.All<MyEntity>().First().Lines.ToList()
                         group l by l.Rate.GetValueOrDefault()
                         into g
                         select new {Rate = g.Key, BaseAmount = g.Sum(l => l.Amount.GetValueOrDefault())})
                         .Single();
          Assert.AreEqual(expected.Rate, actual.Rate);
          Assert.AreEqual(expected.BaseAmount, actual.BaseAmount);
        }
      }
    }
  }
}