// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.06.25

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0446_TypeAsOnSubqueryOperandModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0446_TypeAsOnSubqueryOperandModel
  {
    [HierarchyRoot]
    public class Owner : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field, Association(PairTo = "Owner")]
      public EntitySet<Item> Items { get; set; }
    }

    [HierarchyRoot]
    public class Item : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public Owner Owner { get; set; }
    }

    public class Item2 : Item
    {
      [Field]
      public string Info { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0446_TypeAsOnSubqueryOperand : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Owner).Assembly, typeof (Owner).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var q = session.Query.All<Owner>().Select(o => (o.Items.First() as Item2).Info).ToList();
      }
    }
  }
}