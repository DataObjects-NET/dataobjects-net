// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.24

using System.Transactions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0359_CustomSessionConfigurationProblem_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0359_CustomSessionConfigurationProblem_Model
{
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
      EnsureProtocolIs(StorageProtocol.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var dc = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      dc.AutoValidation = true;
      dc.ForeignKeyMode = ForeignKeyMode.All;
      dc.KeyGeneratorCacheSize = 32;
      dc.SessionPoolSize = 5;

      dc.UpgradeMode = DomainUpgradeMode.Recreate;

      dc.Types.Register(typeof (Class1).Assembly, typeof (Class1).Namespace);

      var sc = new SessionConfiguration("Default");
      sc.DefaultIsolationLevel = IsolationLevel.Serializable;
      sc.Options = SessionOptions.AutoShortenTransactions;

      dc.Sessions.Add(sc);

      return dc;
    }
  }
}