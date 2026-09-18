// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.03

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0331_ForgetHierarchyRoot_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0331_ForgetHierarchyRoot_Model
{
  [HierarchyRoot]
  public class Cell : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }

    [Field, Association(PairTo = "Cell", OnTargetRemove = OnRemoveAction.Clear)]
    public Creature Creature { get; set; }
  }

  public class Creature : Entity
  {
    [Key, Field]
    public int ID { get; private set; }

    [Field]
    public Cell Cell { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0331_ForgetHierarchyRoot
  {
    [Test]
    public void DomainBuildTest()
    {
      var configuration = BuildConfiguration();
      _ = Assert.Throws<DomainBuilderException>(() => Domain.Build(configuration).Dispose());
    }

    [Test]
    public void DomainBuildAsyncTest()
    {
      var configuration = BuildConfiguration();
      _ = Assert.ThrowsAsync<DomainBuilderException>(async () => (await Domain.BuildAsync(configuration)).Dispose());
    }

    private static DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (Cell));
      config.Types.Register(typeof (Creature));
      return config;
    }
  }
}