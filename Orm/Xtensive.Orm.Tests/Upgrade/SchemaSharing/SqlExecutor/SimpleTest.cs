// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.28

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.SqlExecutor.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.SqlExecutor
{
  public class SimpleTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      CheckRequirements();
    }

    [Test]
    public void PerformSafelyTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.PerformSafely);
    }

    [Test]
    public void PerformTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.Perform);
    }

    [Test]
    public void SkipTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.Skip);
    }

    [Test]
    public void ValidateTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.Validate);
    }

    [Test]
    public void LegacySkipTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.LegacySkip);
    }

    [Test]
    public void LegacyValidateTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.LegacyValidate);
    }

    protected virtual void CheckRequirements()
    {
    }

    protected virtual DomainConfiguration GetDomainConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (model.Part1.TestEntity1));
      configuration.Types.Register(typeof (model.Part2.TestEntity2));
      configuration.Types.Register(typeof (model.Part3.TestEntity3));
      configuration.Types.Register(typeof (model.Part4.TestEntity4));
      return configuration;
    }

    protected virtual List<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      return new List<NodeConfiguration>() {new NodeConfiguration(WellKnown.DefaultNodeId) {UpgradeMode = upgradeMode}};
    }

    private void BuildInitialDomain()
    {
      var configuration = GetDomainConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var nodes = GetNodes(DomainUpgradeMode.Recreate);
        foreach (var nodeConfiguration in nodes.Where(n => n.NodeId!=WellKnown.DefaultNodeId))
          domain.StorageNodeManager.AddNode(nodeConfiguration);

        foreach (var nodeConfiguration in nodes) {
          using (var session = domain.OpenSession()) {
            session.SelectStorageNode(nodeConfiguration.NodeId);
            using (session.Activate())
            using (var transaction = session.OpenTransaction()) {
              new model.Part1.TestEntity1 {Text = session.StorageNodeId};
              new model.Part2.TestEntity2 {Text = session.StorageNodeId};
              new model.Part3.TestEntity3 {Text = session.StorageNodeId};
              new model.Part4.TestEntity4 {Text = session.StorageNodeId};

              transaction.Complete();
            }
          }
        }
      }
    }

    private void BuildTestDomain(DomainUpgradeMode upgradeMode)
    {
      var configuration = GetDomainConfiguration();
      configuration.Types.Register(typeof (model.CustomUpgradeHandler));
      if (upgradeMode==DomainUpgradeMode.Perform || upgradeMode==DomainUpgradeMode.PerformSafely) {
        configuration.Types.Register(typeof (model.Part1.NewTestEntity1));
        configuration.Types.Register(typeof (model.Part2.NewTestEntity2));
        configuration.Types.Register(typeof (model.Part3.NewTestEntity3));
        configuration.Types.Register(typeof (model.Part4.NewTestEntity4));
      }
      configuration.UpgradeMode = upgradeMode;
      configuration.ShareStorageSchemaOverNodes = true;

      using (var domain = Domain.Build(configuration)) {
        foreach (var nodeConfiguration in GetNodes(upgradeMode).Where(n => n.NodeId!=WellKnown.DefaultNodeId))
          domain.StorageNodeManager.AddNode(nodeConfiguration);
      }
    }
  }
}
