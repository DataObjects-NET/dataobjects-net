// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.24

using System;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0359_CustomSessionConfigurationProblem_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0359_CustomSessionConfigurationProblem_Model
{
  [HierarchyRoot]
  public class Class1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0359_UpgradeUsingAutoshortenTransaction
  {
    [Test]
    public void DomainBuildTest()
    {
      var configuration = BuildConfiguration();
      Assert.DoesNotThrow(() => Domain.Build(configuration).Dispose());
    }

    [Test]
    public void DomainBuildAsyncTest()
    {
      var configuration = BuildConfiguration();
      Assert.DoesNotThrowAsync(async () => (await Domain.BuildAsync(configuration)).Dispose());
    }

    private static DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.ForeignKeyMode = ForeignKeyMode.All;
      config.KeyGeneratorCacheSize = 32;

      config.UpgradeMode = DomainUpgradeMode.Recreate;

      config.Types.Register(typeof(Class1));
      config.Sessions.Default.DefaultIsolationLevel = IsolationLevel.Serializable;

      return config;
    }
  }
}