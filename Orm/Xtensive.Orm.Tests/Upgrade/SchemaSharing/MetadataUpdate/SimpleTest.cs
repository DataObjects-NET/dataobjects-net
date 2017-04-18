// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.04.03

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.MetadataUpdate.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.MetadataUpdate
{
  public class SimpleTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      CheckRequirements();
    }

    protected virtual void CheckRequirements()
    {
    }

    [Test]
    public void PerformTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.Perform);
    }

    [Test]
    public void PerformSafelyTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.PerformSafely);
    }

    [Test]
    public void RecreateTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.Recreate);
    }

    protected void BuildInitialDomain()
    {
      var emptyDomainConfiguration = DomainConfigurationFactory.Create();
      emptyDomainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      ApplyCustomSettingsToInitialConfiguration(emptyDomainConfiguration);

      using (var domain = Domain.Build(emptyDomainConfiguration)) {
        foreach (var nodeConfiguration in GetNodes(DomainUpgradeMode.Recreate).Where(n => n.NodeId!=WellKnown.DefaultNodeId))
          domain.StorageNodeManager.AddNode(nodeConfiguration);
      }
    }

    protected void BuildTestDomain(DomainUpgradeMode upgradeMode)
    {
      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.UpgradeMode = upgradeMode;
      ApplyCustomSettingsToTestConfiguration(domainConfiguration);
      domainConfiguration.ShareStorageSchemaOverNodes = true;

      using (var domain = Domain.Build(domainConfiguration)) {
        foreach (var nodeConfiguration in GetNodes(upgradeMode).Where(n => n.NodeId!=WellKnown.DefaultNodeId))
          domain.StorageNodeManager.AddNode(nodeConfiguration);
      }
    }

    protected virtual void ApplyCustomSettingsToInitialConfiguration(DomainConfiguration domainConfiguration)
    {
      domainConfiguration.Types.Register(typeof (model.Part1.TestEntity1));
      domainConfiguration.Types.Register(typeof (model.Part2.TestEntity2));
      domainConfiguration.Types.Register(typeof (model.Part3.TestEntity3));
      domainConfiguration.Types.Register(typeof (model.Part4.TestEntity4));
    }

    protected virtual void ApplyCustomSettingsToTestConfiguration(DomainConfiguration domainConfiguration)
    {
      domainConfiguration.Types.Register(typeof (model.CustomUpgradeHandler));

      domainConfiguration.Types.Register(typeof (model.Part1.TestEntity1));
      domainConfiguration.Types.Register(typeof (model.Part1.RecycledTestEntity1));
      domainConfiguration.Types.Register(typeof (model.Part1.NewTestEntity1));

      domainConfiguration.Types.Register(typeof (model.Part2.TestEntity2));
      domainConfiguration.Types.Register(typeof (model.Part2.RecycledTestEntity2));
      domainConfiguration.Types.Register(typeof (model.Part2.NewTestEntity2));
      
      domainConfiguration.Types.Register(typeof (model.Part3.TestEntity3));
      domainConfiguration.Types.Register(typeof (model.Part3.RecycledTestEntity3));
      domainConfiguration.Types.Register(typeof (model.Part3.NewTestEntity3));

      domainConfiguration.Types.Register(typeof (model.Part4.TestEntity4));
      domainConfiguration.Types.Register(typeof (model.Part4.RecycledTestEntity4));
      domainConfiguration.Types.Register(typeof (model.Part4.NewTestEntity4));
    }

    protected virtual IEnumerable<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      return new List<NodeConfiguration> {new NodeConfiguration(WellKnown.DefaultNodeId)};
    }
  }
}
