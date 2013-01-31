// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.01.31

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0424_DateTimeOperationInvalidSqlModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0424_DateTimeOperationInvalidSqlModel
  {
    [HierarchyRoot]
    public class EntityWithDate : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public DateTime Date { get; set; }

      [Field]
      public string Type { get; set; }

      [Field]
      public string Description { get; set; }
    }
  }

  public class IssueJira0424_DateTimeOperationInvalidSql : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithDate));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new EntityWithDate {Date = new DateTime(2013, 1, 30, 12, 0, 0)};
        new EntityWithDate {Date = new DateTime(2013, 1, 31, 5, 0, 0)};
        new EntityWithDate();
        var offset = TimeSpan.FromHours(6);
        var q =
          from t in session.Query.All<EntityWithDate>()
          group t by new {
            Date = t.Date.TimeOfDay > offset
              ? new DateTime(t.Date.Year, t.Date.Month, t.Date.Day)
              : new DateTime(t.Date.Year, t.Date.Month, t.Date.Day) + TimeSpan.FromDays(1),
            t.Type,
            Description = t.Type.StartsWith("XYZ") ? "" : t.Description
          };
        var result = q.ToList();
        var expectedDate = new DateTime(2013, 1, 31);
        Assert.That(result.Count, Is.EqualTo(1));
        foreach (var item in result[0])
          Assert.That(item.Date, Is.EqualTo(expectedDate));
        tx.Complete();
      }
    }
  }
}