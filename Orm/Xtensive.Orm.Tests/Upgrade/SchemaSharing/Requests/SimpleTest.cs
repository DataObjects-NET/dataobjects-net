// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.29

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using model= Xtensive.Orm.Tests.Upgrade.SchemaSharing.Requests.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.Requests
{
  public class SimpleTest
  {
    [OneTimeSetUp]
    public void TestFixtureSetUp() => CheckRequirements();

    protected virtual void CheckRequirements()
    {
    }

    [Mute]
    [Test]
    public void Recreate() => RunTest(DomainUpgradeMode.Recreate);

    [Mute]
    [Test]
    public void Skip() => RunTest(DomainUpgradeMode.Skip);

     [Mute]
   [Test]
    public void Validate() => RunTest(DomainUpgradeMode.Validate);

    [Mute]
    [Test]
    public void Perform() => RunTest(DomainUpgradeMode.Perform);

    [Mute]
    [Test]
    public void PerformSafely() => RunTest(DomainUpgradeMode.PerformSafely);

    [Mute]
    [Test]
    public void LegacySkip() => RunTest(DomainUpgradeMode.LegacySkip);

    [Mute]
    [Test]
    public void LegacyValidate() => RunTest(DomainUpgradeMode.LegacyValidate);

    public void RunTest(DomainUpgradeMode upgradeMode)
    {
      BuildInitialDomain();
      BuildTestDomain(upgradeMode);
    }

    protected void BuildInitialDomain()
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

    protected void BuildTestDomain(DomainUpgradeMode upgradeMode)
    {
      var configuration = GetDomainConfiguration();
      configuration.Types.Register(typeof(model.CustomUpgradeHandler));
      configuration.UpgradeMode = upgradeMode;
      configuration.ShareStorageSchemaOverNodes = true;

      using (var domain = Domain.Build(configuration)) {
        var nodes = GetNodes(upgradeMode).Where(n => n.NodeId != WellKnown.DefaultNodeId);
        foreach (var nodeConfiguration in nodes) {
          _ = domain.StorageNodeManager.AddNode(nodeConfiguration);
        }
      }
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
      new List<NodeConfiguration> { new NodeConfiguration(WellKnown.DefaultNodeId) };
  }
}
