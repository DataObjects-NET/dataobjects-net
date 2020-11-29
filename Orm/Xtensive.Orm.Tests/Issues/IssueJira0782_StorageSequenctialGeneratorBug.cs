// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.12.16

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals.KeyGenerators;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Tuples;
using Xtensive.Orm.Tests.Issues.IssueJira0782_StorageSequenctialGeneratorBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0782_StorageSequenctialGeneratorBugModel
{
  [HierarchyRoot(IncludeTypeId = true)]
  public class Entity1 : Entity
  {
    [Field, Key]
    public int Id { get; set; }
  }

  [HierarchyRoot]
  public class Entity2 : Entity
  {
    [Field, Key]
    public int Id { get; set; }
  }

  [HierarchyRoot]
  public class Entity3 : Entity
  {
    [Field, Key]
    public long Id { get; set; }
  }

  [HierarchyRoot(IncludeTypeId = true)]
  public class Entity4 : Entity
  {
    [Field, Key]
    public long Id { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0782_StorageSequenctialGeneratorBug : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Entity1).Assembly, typeof(Entity1).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public void Test01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new Entity1();
        _ = new Entity2();
      }
    }

    [Test]
    public void Test02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new Entity3();
        _ = new Entity4();
      }
    }
  }
}