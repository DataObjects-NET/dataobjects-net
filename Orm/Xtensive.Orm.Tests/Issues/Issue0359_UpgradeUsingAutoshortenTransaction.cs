// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.24

using System;
using System.Transactions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0359_CustomSessionConfigurationProblem_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0359_CustomSessionConfigurationProblem_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Class1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0359_UpgradeUsingAutoshortenTransaction : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.ForeignKeyMode = ForeignKeyMode.All;
      config.KeyGeneratorCacheSize = 32;

      config.UpgradeMode = DomainUpgradeMode.Recreate;

      config.Types.Register(typeof (Class1).Assembly, typeof (Class1).Namespace);
      config.Sessions.Default.DefaultIsolationLevel = IsolationLevel.Serializable;

      return config;
    }
  }
}