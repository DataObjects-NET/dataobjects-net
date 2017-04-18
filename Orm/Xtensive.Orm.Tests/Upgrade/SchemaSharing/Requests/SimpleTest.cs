// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      CheckRequirements();
    }

    protected virtual void CheckRequirements()
    {
    }

    [Test]
    public void Recreate()
    {
      RunTest(DomainUpgradeMode.Recreate);
    }

    [Test]
    public void Skip()
    {
      RunTest(DomainUpgradeMode.Skip);
    }

    [Test]
    public void Validate()
    {
      RunTest(DomainUpgradeMode.Validate);
    }

    [Test]
    public void Perform()
    {
      RunTest(DomainUpgradeMode.Perform);
    }

    [Test]
    public void PerformSafely()
    {
      RunTest(DomainUpgradeMode.PerformSafely);
    }

    [Test]
    public void LegacySkip()
    {
      RunTest(DomainUpgradeMode.LegacySkip);
    }

    [Test]
    public void LegacyValidate()
    {
      RunTest(DomainUpgradeMode.LegacyValidate);
    }

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

    protected void BuildTestDomain(DomainUpgradeMode upgradeMode)
    {
      var configuration = GetDomainConfiguration();
      configuration.Types.Register(typeof (model.CustomUpgradeHandler));
      configuration.UpgradeMode = upgradeMode;
      configuration.ShareStorageSchemaOverNodes = true;

      using (var domain = Domain.Build(configuration)) {
        var nodes = GetNodes(upgradeMode);

        foreach (var nodeConfiguration in nodes.Where(n=>n.NodeId!=WellKnown.DefaultNodeId))
          domain.StorageNodeManager.AddNode(nodeConfiguration);
      }
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
      return new List<NodeConfiguration> {new NodeConfiguration(WellKnown.DefaultNodeId)};
    }
  }
}
