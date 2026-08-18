// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.16

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0002_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0002_Model
{
  [HierarchyRoot]
  public class X : Entity
  {
    [Field(LazyLoad = true), Key]
    public int ID { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0002_PrimaryKeyWithLazyLoad
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
      config.Types.Register(typeof(X));
      return config;
    }
  }
}