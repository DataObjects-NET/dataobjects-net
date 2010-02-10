// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.24

using System;
using System.Transactions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0359_CustomSessionConfigurationProblem_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0359_CustomSessionConfigurationProblem_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Class1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0359_UpgradeUsingAutoshortenTransaction : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      EnsureProviderIs(StorageProvider.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.AutoValidation = true;
      config.ForeignKeyMode = ForeignKeyMode.All;
      config.KeyGeneratorCacheSize = 32;

      config.UpgradeMode = DomainUpgradeMode.Recreate;

      config.Types.Register(typeof (Class1).Assembly, typeof (Class1).Namespace);

      var sessionConfig = new SessionConfiguration("Default") {
        DefaultIsolationLevel = IsolationLevel.Serializable, 
        Options = SessionOptions.AutoShortenTransactions
      };
      config.Sessions.Add(sessionConfig);

      return config;
    }
  }
}