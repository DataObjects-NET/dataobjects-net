// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.01.31

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0427_IndexOutOfRangeOnContainsOperationModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0427_IndexOutOfRangeOnContainsOperationModel
  {
    [HierarchyRoot]
    public class Kind : Entity
    {
      [Key, Field(Length = 255)]
      public string Id { get; private set; }

      [Field]
      public string Name { get; private set; }
    }
  }

  public class IssueJira0427_IndexOutOfRangeOnContainsOperation : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Kind));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      // TODO: fix this to reproduce the bug

      var items = new string[0];
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Kind>()
          .Where(e => items.Contains(e.Name))
          .ToList();
        tx.Complete();
      }
    }
  }
}