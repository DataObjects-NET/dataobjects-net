// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.12

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.SelfContainedStructureModel;

namespace Xtensive.Orm.Tests.Model.SelfContainedStructureModel
{
  public class SelfContained : Structure
  {
    [Field]
    public SelfContained Value { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class SelfContainedStructureTest
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
      config.Types.Register(typeof(SelfContained));
      return config;
    }
  }
}