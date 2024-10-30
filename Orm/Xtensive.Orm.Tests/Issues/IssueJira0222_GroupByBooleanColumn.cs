// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.22

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0222_GroupByBooleanColumnModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0222_GroupByBooleanColumnModel
{
  [HierarchyRoot]
  public class Message : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public bool IsReceived { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0222_GroupByBooleanColumn : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Message).Assembly, typeof (Message).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Session.Current.OpenTransaction()) {
          new Message {IsReceived = true};
          new Message {IsReceived = true};
          new Message {IsReceived = false};

          var result = Query.All<Message>()
            .GroupBy(m => m.IsReceived)
            .OrderBy(g => g.Key)
            .Select(g => new {IsReceived = g.Key, Count = g.Count()})
            .ToList();

          var trueResult = result.Single(item => item.IsReceived).Count;
          var falseResult = result.Single(item => !item.IsReceived).Count;

          Assert.AreEqual(2, trueResult);
          Assert.AreEqual(1, falseResult);

          // Rollback
        }
      }
    }
  }
}