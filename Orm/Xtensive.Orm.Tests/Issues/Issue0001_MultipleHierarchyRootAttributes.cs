// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.16

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0001_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0001_Model
{
  [HierarchyRoot]
  public class X : Entity
  {
    [Field, Key]
    public int ID { get; private set; }
  }

  [HierarchyRoot]
  public class Y : X
  {
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0001_MultipleHierarchyRootAttributes
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
      config.Types.Register(typeof (X));
      config.Types.Register(typeof (Y));
      return config;
    }
  }
}