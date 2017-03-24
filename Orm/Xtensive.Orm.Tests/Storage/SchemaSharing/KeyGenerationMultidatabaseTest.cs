// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.06

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing
{
  public class KeyGenerationMultidatabaseTest : AutoBuildTest
  {
    private const string MainNodeId = WellKnown.DefaultNodeId;
    private const string AdditionalNodeId = "Additional";

    private Dictionary<string, int> skipStepsPerNode = new Dictionary<string, int>();

    public override void TestFixtureSetUp()
    {
      CheckRequirements();
      PopulateData();
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override void PopulateData()
    {
      skipStepsPerNode.Add(MainNodeId, 3);
      skipStepsPerNode.Add(AdditionalNodeId, 7);
    }

    [Test]
    public void MainTest()
    {
      using (var referenceDomain = BuildDomain(BuildConfiguration().UseRecreate())) {
        referenceDomain.StorageNodeManager.AddNode(BuildAdditionalNodeConfiguration().UseRecreate());
        IStorageSequenceAccessor sequenceAccessor = referenceDomain.Services.Get<IStorageSequenceAccessor>();
        var intSequence = referenceDomain.Model.Types[typeof (KeyGenerationTestModel.Part1.TestEntity1)].Hierarchy.Key.Sequence;
        var longSequence = referenceDomain.Model.Types[typeof (KeyGenerationTestModel.Part1.TestEntity2)].Hierarchy.Key.Sequence;

        foreach (var node in skipStepsPerNode) {
          using (var session = referenceDomain.OpenSession()) {
            session.SelectStorageNode(node.Key);
            var skipCount = node.Value;
            for (int i = 0; i < skipCount; i++) {
              sequenceAccessor.NextBulk(intSequence, session);
              sequenceAccessor.NextBulk(longSequence, session);
            }
          }
        }
      }

      using (var testDomain = BuildDomain(BuildConfiguration().UsePerformSafely().MakeNodesShareSchema())) {
        testDomain.StorageNodeManager.AddNode(BuildAdditionalNodeConfiguration().UsePerformSafely());
        IStorageSequenceAccessor sequenceAccessor = testDomain.Services.Get<IStorageSequenceAccessor>();

        var intSequence = testDomain.Model.Types[typeof (KeyGenerationTestModel.Part1.TestEntity1)].Hierarchy.Key.Sequence;
        var longSequence = testDomain.Model.Types[typeof (KeyGenerationTestModel.Part1.TestEntity2)].Hierarchy.Key.Sequence;

        foreach (var node in skipStepsPerNode) {
          using (var session = testDomain.OpenSession()) {
            session.SelectStorageNode(node.Key);
            var expectedIntOffset = node.Value * 128 + 1;
            var expectedLongOffset = node.Value * 128 + 1;

            var actualIntOffest = sequenceAccessor.NextBulk(intSequence, session).Offset;
            var actualLongOffset = sequenceAccessor.NextBulk(longSequence, session).Offset;

            Assert.That(actualIntOffest, Is.EqualTo(expectedIntOffset));
            Assert.That(actualLongOffset, Is.EqualTo(expectedLongOffset));
          }
        }
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (KeyGenerationTestModel.Part1.TestEntity1).Assembly, typeof (KeyGenerationTestModel.Part1.TestEntity1).Namespace);
      configuration.Types.Register(typeof (KeyGenerationTestModel.Part2.TestEntity3).Assembly, typeof (KeyGenerationTestModel.Part2.TestEntity3).Namespace);
      configuration.Types.Register(typeof (KeyGenerationTestModel.Part3.TestEntity5).Assembly, typeof (KeyGenerationTestModel.Part3.TestEntity5).Namespace);
      configuration.Types.Register(typeof (KeyGenerationTestModel.Part4.TestEntity7).Assembly, typeof (KeyGenerationTestModel.Part4.TestEntity7).Namespace);

      configuration.MappingRules.Map(typeof (KeyGenerationTestModel.Part1.TestEntity1).Assembly, typeof (KeyGenerationTestModel.Part1.TestEntity1).Namespace).To("DO-Tests-1", "Model1");
      configuration.MappingRules.Map(typeof (KeyGenerationTestModel.Part2.TestEntity3).Assembly, typeof (KeyGenerationTestModel.Part2.TestEntity3).Namespace).To("DO-Tests-1", "Model2");
      configuration.MappingRules.Map(typeof (KeyGenerationTestModel.Part3.TestEntity5).Assembly, typeof (KeyGenerationTestModel.Part3.TestEntity5).Namespace).To("DO-Tests-2", "Model1");
      configuration.MappingRules.Map(typeof (KeyGenerationTestModel.Part4.TestEntity7).Assembly, typeof (KeyGenerationTestModel.Part4.TestEntity7).Namespace).To("DO-Tests-2", "Model2");

      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.DefaultSchema = "Model1";
      return configuration;
    }

    private NodeConfiguration BuildAdditionalNodeConfiguration()
    {
      var config = new NodeConfiguration(AdditionalNodeId);
      config.UpgradeMode = DomainUpgradeMode.Recreate;

      config.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-3");
      config.DatabaseMapping.Add("DO-Tests-2", "DO-Tests-4");

      config.SchemaMapping.Add("Model1", "Model3");
      config.SchemaMapping.Add("Model2", "Model4");
      return config;
    }
  }
}