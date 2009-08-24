// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.24

using NUnit.Framework;
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
  public class Issue0359_CustomSessionConfigurationProblem : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      EnsureProtocolIs(StorageProtocol.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfiguration.Load("CustomSessionConfigurationProblem", "mssql2005");
      config.Types.Register(typeof (Class1).Assembly, typeof (Class1).Namespace);
      var domain = Domain.Build(config);
      return config;
    }
  }
}