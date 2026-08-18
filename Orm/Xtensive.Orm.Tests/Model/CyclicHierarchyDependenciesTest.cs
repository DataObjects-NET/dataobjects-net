// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.12

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.CyclicHierarchyDependenciesModel;

namespace Xtensive.Orm.Tests.CyclicHierarchyDependenciesModel
{
  [HierarchyRoot]
  public class H1 : Entity
  {
    [Field, Key(0)]
    public H2 First { get; private set; }

    [Field, Key(1)]
    public H3 Second { get; private set; }
  }

  [HierarchyRoot]
  public class H2 : Entity
  {
    [Field, Key(0)]
    public H1 First { get; private set; }

    [Field, Key(1)]
    public H3 Second { get; private set; }
  }

  [HierarchyRoot]
  public class H3 : Entity
  {
    [Field, Key(0)]
    public H1 First { get; private set; }

    [Field, Key(1)]
    public H2 Second { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture, Category("Model")]
  public class CyclicHierarchyDependenciesTest
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.RegisterCaching(typeof(H1).Assembly, typeof(H1).Namespace);
      var ex = Assert.Throws<DomainBuilderException>(() => Domain.Build(config).Dispose());
      Assert.That(ex.Message.StartsWith("At least one loop have been found"), Is.True);
    }
  }
}