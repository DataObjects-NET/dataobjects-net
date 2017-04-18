// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.03

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.TemporaryTableManager.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.TemporaryTableManager
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
    public void MainTest()
    {
      using (var referenceDomain = BuildReferenceDomain())
      using (var testDomain = BuildTestDomain()) {
        RunTest(referenceDomain, testDomain);
      }
    }

    protected virtual Domain BuildReferenceDomain()
    {
      return Domain.Build(GetDomainConfiguration());
    }

    protected virtual Domain BuildTestDomain()
    {
      var configuration = GetDomainConfiguration();
      configuration.ShareStorageSchemaOverNodes = true;
      return Domain.Build(configuration);
    }

    protected virtual DomainConfiguration GetDomainConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (model.Part1.TestEntity1).Assembly, typeof (model.Part1.TestEntity1).Namespace);
      configuration.Types.Register(typeof (model.Part2.TestEntity2).Assembly, typeof (model.Part2.TestEntity2).Namespace);
      configuration.Types.Register(typeof (model.Part3.TestEntity3).Assembly, typeof (model.Part3.TestEntity3).Namespace);
      configuration.Types.Register(typeof (model.Part4.TestEntity4).Assembly, typeof (model.Part4.TestEntity4).Namespace);

      return configuration;
    }

    protected virtual IEnumerable<string> GetNodeIdentifiers()
    {
      yield return WellKnown.DefaultNodeId;
    }

    private void RunTest(Domain referenceDomain, Domain testDomain)
    {
      foreach (var nodeIdentifier in GetNodeIdentifiers()) {
        var expectedMapping = referenceDomain.StorageNodeManager.GetNode(nodeIdentifier).Mapping;
        var testMapping = testDomain.StorageNodeManager.GetNode(nodeIdentifier).Mapping;
        Assert.That(testMapping.TemporaryTableDatabase, Is.EqualTo(expectedMapping.TemporaryTableDatabase));
        Assert.That(testMapping.TemporaryTableSchema, Is.EqualTo(expectedMapping.TemporaryTableSchema));
      }
    }
  }
}
