using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel.Interfaces.Alphabet;
using model = Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerator.Model;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerator
{
  public class SimpleTest : AutoBuildTest
  {
    private Dictionary<NodeConfiguration, int> skipStepsPerNode = new Dictionary<NodeConfiguration, int>();

    [TestFixtureSetUp]
    public void TestFixureSetUp()
    {
      CheckRequirements();
      FillInSkipParameters(skipStepsPerNode);
    }

    [Test]
    public void MainTest()
    {
      using (var referenceDomain = BuildDomain(BuildConfiguration().UseRecreate())) {

        foreach (var node in skipStepsPerNode.Keys.Where(n => n.NodeId!=WellKnown.DefaultNodeId))
          referenceDomain.StorageNodeManager.AddNode(node.UseRecreate());//node is in recreate;

        var sequenceAccessor = referenceDomain.Services.Get<IStorageSequenceAccessor>();

        var intSequence = referenceDomain.Model.Types[typeof (model.Part1.TestEntity1)].Hierarchy.Key.Sequence;
        var longSequence = referenceDomain.Model.Types[typeof (model.Part1.TestEntity2)].Hierarchy.Key.Sequence;

        foreach (var node in skipStepsPerNode) {
          using (var session = referenceDomain.OpenSession()) {
            session.SelectStorageNode(node.Key.NodeId);
            var skipCount = node.Value;
            for (int i = 0; i < skipCount; i++) {
              sequenceAccessor.NextBulk(intSequence, session);
              sequenceAccessor.NextBulk(longSequence, session);
            }
          }
        }
      }

      using (var testDomain = BuildDomain(BuildConfiguration().UsePerformSafely().MakeNodesShareSchema())) {
        foreach (var node in skipStepsPerNode.Keys.Where(n => n.NodeId!=WellKnown.DefaultNodeId))
          testDomain.StorageNodeManager.AddNode(node.UsePerformSafely());

        var sequenceAccessor = testDomain.Services.Get<IStorageSequenceAccessor>();

        var intSequence = testDomain.Model.Types[typeof (model.Part1.TestEntity1)].Hierarchy.Key.Sequence;
        var longSequence = testDomain.Model.Types[typeof (model.Part1.TestEntity2)].Hierarchy.Key.Sequence;

        foreach (var node in skipStepsPerNode) {
          using (var session = testDomain.OpenSession()) {
            session.SelectStorageNode(node.Key.NodeId);
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

      return configuration;
    }

    protected virtual void FillInSkipParameters(Dictionary<NodeConfiguration, int> dictionary)
    {
      dictionary.Add(new NodeConfiguration(WellKnown.DefaultNodeId), 3);
    }
  }
}
