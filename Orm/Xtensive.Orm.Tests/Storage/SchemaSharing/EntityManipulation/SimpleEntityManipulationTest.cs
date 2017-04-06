// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.04.05

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using model = Xtensive.Orm.Tests.Storage.SchemaSharing.EntityManipulation.Model;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.EntityManipulation
{
  [TestFixture]
  public class SimpleEntityManipulationTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      CheckRequirements();
    }

    [SetUp]
    public void SetUp()
    {
      PopulateInitialDataForNodes();
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

    protected void RunTest(DomainUpgradeMode upgradeMode)
    {
      var configuration = BuildConfiguration();
      ApplyCustomSettingsToTestConfiguration(configuration);
      configuration.UpgradeMode = upgradeMode;
      configuration.ShareStorageSchemaOverNodes = true;
      var initialEntitiesCount = (upgradeMode!=DomainUpgradeMode.Recreate) ? 1 : 0;

      using (var domain = Domain.Build(configuration)) {
        var nodes = GetNodes(upgradeMode);

        foreach (var storageNode in nodes.Where(n=>n.NodeId!=WellKnown.DefaultNodeId))
          domain.StorageNodeManager.AddNode(storageNode);

        foreach (var storageNode in nodes) {
          using (var session = domain.OpenSession()) {
            session.SelectStorageNode(storageNode.NodeId);
            using (var transaction = session.OpenTransaction()) {
              Select(session, initialEntitiesCount);
              var createdKeys = Insert(session, initialEntitiesCount);
              Update(session, createdKeys, initialEntitiesCount);
              Delete(session, createdKeys, initialEntitiesCount);
              Select(session, initialEntitiesCount);
            }
          }
        }
      }
    }

    protected void PopulateInitialDataForNodes()
    {
      var configuration = BuildConfiguration();
      ApplyCustomSettingsToInitialConfiguration(configuration);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.ShareStorageSchemaOverNodes = false;

      using (var domain = Domain.Build(configuration)) {
        var nodes = GetNodes(DomainUpgradeMode.Recreate);
        foreach (var storageNode in nodes.Where(n=>n.NodeId!=WellKnown.DefaultNodeId))
          domain.StorageNodeManager.AddNode(storageNode);

        foreach (var storageNode in nodes) {
          using (var session = domain.OpenSession()) {
            session.SelectStorageNode(storageNode.NodeId);
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

    protected DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (model.Part1.TestEntity1));
      configuration.Types.Register(typeof (model.Part2.TestEntity2));
      configuration.Types.Register(typeof (model.Part3.TestEntity3));
      configuration.Types.Register(typeof (model.Part4.TestEntity4));
      return configuration;
    }

    protected virtual void ApplyCustomSettingsToInitialConfiguration(DomainConfiguration configuration)
    {
    }

    protected virtual void ApplyCustomSettingsToTestConfiguration(DomainConfiguration configuration)
    {
    }

    protected virtual IEnumerable<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      return new[] {new NodeConfiguration(WellKnown.DefaultNodeId) {UpgradeMode = upgradeMode}};
    }

    private void Select(Session session, int initialCountOfEntities)
    {
      var storageNodeId = session.StorageNodeId;
      var a = session.Query.All<model.Part1.TestEntity1>().ToList();
      var b = session.Query.All<model.Part2.TestEntity2>().ToList();
      var c = session.Query.All<model.Part3.TestEntity3>().ToList();
      var d = session.Query.All<model.Part4.TestEntity4>().ToList();
      Assert.That(a.Count, Is.EqualTo(initialCountOfEntities));
      Assert.That(b.Count, Is.EqualTo(initialCountOfEntities));
      Assert.That(c.Count, Is.EqualTo(initialCountOfEntities));
      Assert.That(d.Count, Is.EqualTo(initialCountOfEntities));

      if (initialCountOfEntities==0)
        return;

      Assert.That(a[0].Text, Is.EqualTo(storageNodeId));
      Assert.That(b[0].Text, Is.EqualTo(storageNodeId));
      Assert.That(c[0].Text, Is.EqualTo(storageNodeId));
      Assert.That(d[0].Text, Is.EqualTo(storageNodeId));

      Assert.That(session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Text==storageNodeId), Is.Not.Null);
      Assert.That(session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Text==storageNodeId), Is.Not.Null);
      Assert.That(session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Text==storageNodeId), Is.Not.Null);
      Assert.That(session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Text==storageNodeId), Is.Not.Null);
    }

    private Key[] Insert(Session session, int initialCountOfEntities)
    {
      var storageNodeId = session.StorageNodeId;
      var text = string.Format("{0}_new", storageNodeId);
      var a = new model.Part1.TestEntity1 {Text = text};
      var b = new model.Part2.TestEntity2 {Text = text};
      var c = new model.Part3.TestEntity3 {Text = text};
      var d = new model.Part4.TestEntity4 {Text = text};

      session.SaveChanges();

      Assert.That(session.Query.All<model.Part1.TestEntity1>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      a = session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Text==text);
      Assert.That(a, Is.Not.Null);

      Assert.That(session.Query.All<model.Part2.TestEntity2>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      b = session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Text==text);
      Assert.That(b, Is.Not.Null);

      Assert.That(session.Query.All<model.Part3.TestEntity3>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      c = session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Text==text);
      Assert.That(c, Is.Not.Null);

      Assert.That(session.Query.All<model.Part4.TestEntity4>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      d = session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Text==text);
      Assert.That(d, Is.Not.Null);

      return new Key[] { a.Key, b.Key, c.Key, d.Key };
    }

    private void Update(Session session, Key[] createdKeys, int initialCountOfEntities)
    {
      var storageNodeId = session.StorageNodeId;
      var updatedText = string.Format("{0}_new_updated", storageNodeId);
      var text = string.Format("{0}_new", storageNodeId);

      var a = session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Key==createdKeys[0] && e.Text==text);
      Assert.That(a, Is.Not.Null);
      a.Text = updatedText;

      var b = session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Key==createdKeys[1] && e.Text==text);
      Assert.That(b, Is.Not.Null);
      b.Text = updatedText;

      var c = session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Key==createdKeys[2] && e.Text==text);
      Assert.That(c, Is.Not.Null);
      c.Text = updatedText;

      var d = session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Key==createdKeys[3] && e.Text==text);
      Assert.That(d, Is.Not.Null);
      d.Text = updatedText;

      session.SaveChanges();

      Assert.That(session.Query.All<model.Part1.TestEntity1>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      Assert.That(session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Text==updatedText), Is.Not.Null);
      Assert.That(session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Text==text), Is.Null);

      Assert.That(session.Query.All<model.Part2.TestEntity2>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      Assert.That(session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Text==updatedText), Is.Not.Null);
      Assert.That(session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Text==text), Is.Null);

      Assert.That(session.Query.All<model.Part3.TestEntity3>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      Assert.That(session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Text==updatedText), Is.Not.Null);
      Assert.That(session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Text==text), Is.Null);

      Assert.That(session.Query.All<model.Part4.TestEntity4>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      Assert.That(session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Text==updatedText), Is.Not.Null);
      Assert.That(session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Text==text), Is.Null);
    }

    private void Delete(Session session, Key[] createdKeys, int initialCountOfEntities)
    {
      var storageNodeId = session.StorageNodeId;
      var updatedText = string.Format("{0}_new_updated", storageNodeId);

      var a = session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Key==createdKeys[0] && e.Text==updatedText);
      Assert.That(a, Is.Not.Null);
      var b = session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Key==createdKeys[1] && e.Text==updatedText);
      Assert.That(b, Is.Not.Null);
      var c = session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Key==createdKeys[2] && e.Text==updatedText);
      Assert.That(c, Is.Not.Null);
      var d = session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Key==createdKeys[3] && e.Text==updatedText);
      Assert.That(d, Is.Not.Null);

      a.Remove();
      b.Remove();
      c.Remove();
      d.Remove();

      session.SaveChanges();

      a = session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Key==createdKeys[0] && e.Text==updatedText);
      Assert.That(a, Is.Null);
      b = session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Key==createdKeys[1] && e.Text==updatedText);
      Assert.That(b, Is.Null);
      c = session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Key==createdKeys[2] && e.Text==updatedText);
      Assert.That(c, Is.Null);
      d = session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Key==createdKeys[3] && e.Text==updatedText);
      Assert.That(d, Is.Null);
    }
  }
}
