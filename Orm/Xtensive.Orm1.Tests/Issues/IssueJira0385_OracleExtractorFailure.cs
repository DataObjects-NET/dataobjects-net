// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.18

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0385Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0385Model
  {
    [HierarchyRoot]
    [Index("DateFrom:DESC", "DateTo:DESC")]
    public class J0385Entity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public double? Duration { get; set; }

      [Field]
      public DateTime DateFrom { get; set; }

      [Field]
      public DateTime? DateTo { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0385_OracleExtractorFailure : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (J0385Entity));
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
    }

    public void BuildWithMode(DomainUpgradeMode mode)
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = mode;
      var domain = BuildDomain(configuration);
      domain.Dispose();
    }

    [Test]
    public void MainTest()
    {
      Require.ProviderIs(StorageProvider.Oracle);
      BuildWithMode(DomainUpgradeMode.Recreate);
      BuildWithMode(DomainUpgradeMode.Validate);
    }
  }
} 