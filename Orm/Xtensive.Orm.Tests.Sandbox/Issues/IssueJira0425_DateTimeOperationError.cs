// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.01.31

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0425_DateTimeOperationErrorModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0425_DateTimeOperationErrorModel
  {
    [HierarchyRoot]
    public class EntityWithDate : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public DateTime Date { get; set; }
    }
  }

  public class IssueJira0425_DateTimeOperationError : AutoBuildTest
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
        TimeSpan offset = TimeSpan.FromHours(5);
        var q =
          from t in session.Query.All<EntityWithDate>()
          select new {
            Date = t.Date + offset
          }
          into ts
          group ts by new {
            Date = new DateTime(ts.Date.Year, ts.Date.Month, ts.Date.Day)
          };
        var res = q.FirstOrDefault();
      }
    }
  }
}