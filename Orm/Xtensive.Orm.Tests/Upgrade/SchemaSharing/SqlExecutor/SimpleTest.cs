// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.28

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.SqlExecutor.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.SqlExecutor
{
  public class SimpleTest
  {
    [OneTimeSetUp]
    public void TestFixtureSetup() => CheckRequirements();

    protected virtual void CheckRequirements() => Require.ProviderIsNot(StorageProvider.Firebird);

    [Mute]
    [Test]
    public void PerformSafelyTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.PerformSafely);
    }

    [Mute]
    [Test]
    public void PerformTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.Perform);
    }

    [Mute]
    [Test]
    public void SkipTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.Skip);
    }

    [Mute]
    [Test]
    public void ValidateTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.Validate);
    }

    [Mute]
    [Test]
    public void LegacySkipTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.LegacySkip);
    }

    [Mute]
    [Test]
    public void LegacyValidateTest()
    {
      BuildInitialDomain();
      BuildTestDomain(DomainUpgradeMode.LegacyValidate);
    }

    protected virtual DomainConfiguration GetDomainConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(model.Part1.TestEntity1));
      configuration.Types.Register(typeof(model.Part2.TestEntity2));
      configuration.Types.Register(typeof(model.Part3.TestEntity3));
      configuration.Types.Register(typeof(model.Part4.TestEntity4));
      return configuration;
    }

    protected virtual List<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode) =>
      new List<NodeConfiguration>() { new NodeConfiguration(WellKnown.DefaultNodeId) { UpgradeMode = upgradeMode } };

    private void BuildInitialDomain()
    {
      var configuration = GetDomainConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration)) {
        var nodes = GetNodes(DomainUpgradeMode.Recreate);
        foreach (var nodeConfiguration in nodes.Where(n => n.NodeId != WellKnown.DefaultNodeId)) {
          _ = domain.StorageNodeManager.AddNode(nodeConfiguration);
        }

        foreach (var nodeConfiguration in nodes) {
          var selectedNode = domain.StorageNodeManager.GetNode(nodeConfiguration.NodeId);
          using (var session = selectedNode.OpenSession())
          using (var transaction = session.OpenTransaction()) {

            var storageNodeIdText = string.IsNullOrEmpty(session.StorageNodeId)
              ? "<default>"
              : session.StorageNodeId;

            _ = new model.Part1.TestEntity1(session) { Text = storageNodeIdText };
            _ = new model.Part2.TestEntity2(session) { Text = storageNodeIdText };
            _ = new model.Part3.TestEntity3(session) { Text = storageNodeIdText };
            _ = new model.Part4.TestEntity4(session) { Text = storageNodeIdText };

            transaction.Complete();
          }
        }
      }
    }

    private void BuildTestDomain(DomainUpgradeMode upgradeMode)
    {
      var configuration = GetDomainConfiguration();
      configuration.Types.Register(typeof(model.CustomUpgradeHandler));
      if (upgradeMode == DomainUpgradeMode.Perform || upgradeMode == DomainUpgradeMode.PerformSafely) {
        configuration.Types.Register(typeof(model.Part1.NewTestEntity1));
        configuration.Types.Register(typeof(model.Part2.NewTestEntity2));
        configuration.Types.Register(typeof(model.Part3.NewTestEntity3));
        configuration.Types.Register(typeof(model.Part4.NewTestEntity4));
      }
      configuration.UpgradeMode = upgradeMode;
      configuration.ShareStorageSchemaOverNodes = true;
      using (var domain = Domain.Build(configuration)) {
        foreach (var nodeConfiguration in GetNodes(upgradeMode).Where(n => n.NodeId != WellKnown.DefaultNodeId)) {
          _ = domain.StorageNodeManager.AddNode(nodeConfiguration);
        }
      }
    }
  }
}
