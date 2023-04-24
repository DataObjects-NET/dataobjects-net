// Copyright (C) 2017-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerator.Model;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerator
{
  public class SimpleTest
  {
    [OneTimeSetUp]
    public void TestFixureSetUp() => CheckRequirements();

    protected virtual void CheckRequirements()
    {
    }

    [Mute]
    [Test]
    public void MainTest()
    {
      using (var referenceDomain = BuildDomain(BuildConfiguration().UseRecreate())) {
        var skipParametersPerNode = GetSkipParameters(DomainUpgradeMode.Recreate);
        foreach (var node in skipParametersPerNode.Keys.Where(n => n.NodeId != WellKnown.DefaultNodeId)) {
          _ = referenceDomain.StorageNodeManager.AddNode(node.UseRecreate());//node is in recreate;
        }

        var sequenceAccessor = referenceDomain.Services.Get<IStorageSequenceAccessor>();

        var intSequence = referenceDomain.Model.Types[typeof(model.Part1.TestEntity1)].Hierarchy.Key.Sequence;
        var longSequence = referenceDomain.Model.Types[typeof(model.Part1.TestEntity2)].Hierarchy.Key.Sequence;

        foreach (var node in skipParametersPerNode) {
          var selectedNode = referenceDomain.StorageNodeManager.GetNode(node.Key.NodeId);
          using (var session = selectedNode.OpenSession()) {
            var skipCount = node.Value;
            for (int i = 0; i < skipCount; i++) {
              _ = sequenceAccessor.NextBulk(intSequence, session);
              _ = sequenceAccessor.NextBulk(longSequence, session);
            }
          }
        }
      }

      using (var testDomain = BuildDomain(BuildConfiguration().UsePerformSafely().MakeNodesShareSchema())) {
        var skipParametersPerNode = GetSkipParameters(DomainUpgradeMode.PerformSafely);
        foreach (var node in skipParametersPerNode.Keys.Where(n => n.NodeId != WellKnown.DefaultNodeId)) {
          _ = testDomain.StorageNodeManager.AddNode(node.UsePerformSafely());
        }

        var sequenceAccessor = testDomain.Services.Get<IStorageSequenceAccessor>();

        var intSequence = testDomain.Model.Types[typeof(model.Part1.TestEntity1)].Hierarchy.Key.Sequence;
        var longSequence = testDomain.Model.Types[typeof(model.Part1.TestEntity2)].Hierarchy.Key.Sequence;

        foreach (var node in skipParametersPerNode) {
          var selectedNode = testDomain.StorageNodeManager.GetNode(node.Key.NodeId);
          using (var session = selectedNode.OpenSession()) {
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

    protected Domain BuildDomain(DomainConfiguration configuration)
    {
      return Domain.Build(configuration);
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace);
      configuration.Types.Register(typeof(model.Part2.TestEntity3).Assembly, typeof(model.Part2.TestEntity3).Namespace);
      configuration.Types.Register(typeof(model.Part3.TestEntity5).Assembly, typeof(model.Part3.TestEntity5).Namespace);
      configuration.Types.Register(typeof(model.Part4.TestEntity7).Assembly, typeof(model.Part4.TestEntity7).Namespace);

      return configuration;
    }

    protected virtual Dictionary<NodeConfiguration, int> GetSkipParameters(DomainUpgradeMode upgradeMode)
    {
      var dictionary = new Dictionary<NodeConfiguration, int>();
      dictionary.Add(new NodeConfiguration(WellKnown.DefaultNodeId){UpgradeMode = upgradeMode}, 3);
      return dictionary;
    }
  }
}
