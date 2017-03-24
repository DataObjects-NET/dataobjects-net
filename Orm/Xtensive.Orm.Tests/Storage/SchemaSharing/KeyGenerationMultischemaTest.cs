// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.06

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerationTestModel;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerationTestModel
{
  namespace Part1
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity1(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }

    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity2(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }
  }

  namespace Part2
  {
    [HierarchyRoot]
    public class TestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity3(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }

    [HierarchyRoot]
    public class TestEntity4 : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity4(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }
  }

  namespace Part3
  {
    [HierarchyRoot]
    public class TestEntity5 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity5(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }

    [HierarchyRoot]
    public class TestEntity6 : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity6(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }
  }

  namespace Part4
  {
    [HierarchyRoot]
    public class TestEntity7 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity7(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }

    [HierarchyRoot]
    public class TestEntity8 : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity8(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.SchemaSharing
{
  public class KeyGenerationMultischemaTest : AutoBuildTest
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
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
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

        var intSequence = referenceDomain.Model.Types[typeof(model.Part1.TestEntity1)].Hierarchy.Key.Sequence;
        var longSequence = referenceDomain.Model.Types[typeof(model.Part1.TestEntity2)].Hierarchy.Key.Sequence;

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

        var intSequence = testDomain.Model.Types[typeof(model.Part1.TestEntity1)].Hierarchy.Key.Sequence;
        var longSequence = testDomain.Model.Types[typeof(model.Part1.TestEntity2)].Hierarchy.Key.Sequence;

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
      configuration.Types.Register(typeof (model.Part1.TestEntity1).Assembly, typeof (model.Part1.TestEntity1).Namespace);
      configuration.Types.Register(typeof (model.Part2.TestEntity3).Assembly, typeof (model.Part2.TestEntity3).Namespace);
      configuration.Types.Register(typeof (model.Part3.TestEntity5).Assembly, typeof (model.Part3.TestEntity5).Namespace);
      configuration.Types.Register(typeof (model.Part4.TestEntity7).Assembly, typeof (model.Part4.TestEntity7).Namespace);

      configuration.MappingRules.Map(typeof (model.Part1.TestEntity1).Assembly, typeof (model.Part1.TestEntity1).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof (model.Part2.TestEntity3).Assembly, typeof (model.Part2.TestEntity3).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof (model.Part3.TestEntity5).Assembly, typeof (model.Part3.TestEntity5).Namespace).ToSchema("Model2");
      configuration.MappingRules.Map(typeof (model.Part4.TestEntity7).Assembly, typeof (model.Part4.TestEntity7).Namespace).ToSchema("Model2");

      configuration.DefaultSchema = "Model1";
      return configuration;
    }

    private NodeConfiguration BuildAdditionalNodeConfiguration()
    {
      var config = new NodeConfiguration(AdditionalNodeId);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.SchemaMapping.Add("Model1", "Model3");
      config.SchemaMapping.Add("Model2", "Model4");
      return config;
    }
  }
}
