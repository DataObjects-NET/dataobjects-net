// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.10

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0042_IndexAttributeIsOmitted_Model;

namespace Xtensive.Orm.Tests.Issues.IssueJira0042_IndexAttributeIsOmitted_Model
{
  [Index("Name", Unique = true)]
  public abstract class Root : Entity
  {
    [Field(Length = 100)]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class Child : Root
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class GrandChild : Child
  {
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0042_IndexAttributeIsOmitted : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Child).Assembly, typeof (Child).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      var model = Domain.Model;
      var type = model.Types[typeof(Child)];
      Assert.IsTrue(type.Indexes.Any(i => i.IsSecondary && i.IsUnique));
    }
  }
}