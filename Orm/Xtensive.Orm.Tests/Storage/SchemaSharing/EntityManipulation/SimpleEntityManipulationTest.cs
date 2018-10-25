// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.04.05

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.UselessTypeInTheMiddleTestModel;
using model = Xtensive.Orm.Tests.Storage.SchemaSharing.EntityManipulation.Model;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.EntityManipulation
{
  [TestFixture]
  public class SimpleEntityManipulationTest
  {
    protected enum NodeConfigurationType
    {
      SingleSchemaNodes,
      MultischemaNodes,
      MultidatabaseNodes
    }

    public Dictionary<string, Dictionary<Type, Pair<string>>> map = new Dictionary<string, Dictionary<Type, Pair<string>>>();

    protected virtual NodeConfigurationType NodeConfiguration
    {
      get{return NodeConfigurationType.SingleSchemaNodes;}
    } 

    [OneTimeSetUp]
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

        map = new Dictionary<string, Dictionary<Type, Pair<string>>>();
        foreach (var storageNode in nodes) {
          var typesMap = new Dictionary<Type, Pair<string>>();
          using (var session = domain.OpenSession()) {
            session.SelectStorageNode(storageNode.NodeId);

            using (var transaction = session.OpenTransaction()) {
              var type = typeof(model.Part1.TestEntity1);
              var pair = GetDatabaseAndSchemaForType(session, type);
              new model.Part1.TestEntity1 {Text = session.StorageNodeId, DatabaseName = pair.First, SchemaName = pair.Second};
              typesMap.Add(type, pair);

              type = typeof(model.Part2.TestEntity2);
              pair = GetDatabaseAndSchemaForType(session, typeof(model.Part2.TestEntity2));
              new model.Part2.TestEntity2 {Text = session.StorageNodeId, DatabaseName = pair.First, SchemaName = pair.Second};
              typesMap.Add(type, pair);

              type = typeof(model.Part3.TestEntity3);
              pair = GetDatabaseAndSchemaForType(session, typeof(model.Part3.TestEntity3));
              new model.Part3.TestEntity3 {Text = session.StorageNodeId, DatabaseName = pair.First, SchemaName = pair.Second};
              typesMap.Add(type, pair);

              type = typeof(model.Part4.TestEntity4);
              pair = GetDatabaseAndSchemaForType(session, typeof(model.Part4.TestEntity4));
              new model.Part4.TestEntity4 {Text = session.StorageNodeId, DatabaseName = pair.First, SchemaName = pair.Second};
              typesMap.Add(type, pair);

              transaction.Complete();
            }
          }
          map.Add(storageNode.NodeId, typesMap);
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

    protected Pair<string> GetDatabaseAndSchemaForType(Session session, Type type)
    {
      var storageNode = session.StorageNode;
      var typeInfo = session.Domain.Model.Types[type];
      var table = storageNode.Mapping[typeInfo];
      var schema = table.Schema.DbName;
      var database = table.Schema.Catalog.DbName;
      return new Pair<string>(database, schema);
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
      MainSelectTest(session, initialCountOfEntities);
      if (NodeConfiguration==NodeConfigurationType.MultischemaNodes &&
          NodeConfiguration==NodeConfigurationType.MultidatabaseNodes)
        AdditionalSelectTest(session, initialCountOfEntities);
    }

    private void MainSelectTest(Session session, int initialCountOfEntities)
    {
      if (NodeConfiguration!=NodeConfigurationType.SingleSchemaNodes)
        return;
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

      Dictionary<Type, Pair<string>> typesMap;
      if (!map.TryGetValue(storageNodeId, out typesMap))
        throw new Exception(string.Format("Unknown node {0}. Probably you don't populate data", storageNodeId));

      var entity1 = a[0];
      Assert.That(entity1.Text, Is.EqualTo(storageNodeId));
      var databaseAndSchema = typesMap[entity1.GetType()];
      Assert.That(entity1.DatabaseName, Is.EqualTo(databaseAndSchema.First));
      Assert.That(entity1.SchemaName, Is.EqualTo(databaseAndSchema.Second));

      Entity entity = session.Query.All<model.Part1.TestEntity1>()
        .FirstOrDefault(e => e.Text==storageNodeId && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(entity, Is.Not.Null);

      var entity2 = b[0];
      Assert.That(b[0].Text, Is.EqualTo(storageNodeId));
      databaseAndSchema = typesMap[entity2.GetType()];
      Assert.That(entity2.DatabaseName, Is.EqualTo(databaseAndSchema.First));
      Assert.That(entity2.SchemaName, Is.EqualTo(databaseAndSchema.Second));

      entity = session.Query.All<model.Part2.TestEntity2>()
        .FirstOrDefault(e => e.Text==storageNodeId && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(entity, Is.Not.Null);

      var entity3 = c[0];
      Assert.That(c[0].Text, Is.EqualTo(storageNodeId));
      databaseAndSchema = typesMap[entity3.GetType()];
      Assert.That(entity3.DatabaseName, Is.EqualTo(databaseAndSchema.First));
      Assert.That(entity3.SchemaName, Is.EqualTo(databaseAndSchema.Second));

      entity = session.Query.All<model.Part3.TestEntity3>()
        .FirstOrDefault(e => e.Text==storageNodeId && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(entity, Is.Not.Null);

      var entity4 = d[0];
      Assert.That(d[0].Text, Is.EqualTo(storageNodeId));
      databaseAndSchema = typesMap[entity4.GetType()];
      Assert.That(entity4.DatabaseName, Is.EqualTo(databaseAndSchema.First));
      Assert.That(entity4.SchemaName, Is.EqualTo(databaseAndSchema.Second));

      entity = session.Query.All<model.Part4.TestEntity4>()
        .FirstOrDefault(e => e.Text==storageNodeId && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(entity, Is.Not.Null);
    }

    protected virtual void AdditionalSelectTest(Session session, int initialCountOfEntities)
    {
      var storageNodeId = session.StorageNodeId;
      Dictionary<Type, Pair<string>> typesMap;
      if (!map.TryGetValue(storageNodeId, out typesMap))
        throw new Exception(string.Format("Unknown node {0}. Probably you don't populate data", storageNodeId));

      if (NodeConfiguration!=NodeConfigurationType.MultischemaNodes)
        return;

      var type1BaseQuery = session.Query.All<model.Part1.TestEntity1>()
        .Select(e => new model.TestEntityDTO() {Id = e.Id, TypeId = e.TypeId, Text = e.Text, DatabaseName = e.DatabaseName, SchemaName = e.SchemaName});
      var type2BaseQuery = session.Query.All<model.Part2.TestEntity2>()
        .Select(e => new model.TestEntityDTO() {Id = e.Id, TypeId = e.TypeId, Text = e.Text, DatabaseName = e.DatabaseName, SchemaName = e.SchemaName});
      var type3BaseQuery = session.Query.All<model.Part3.TestEntity3>()
        .Select(e => new model.TestEntityDTO() {Id = e.Id, TypeId = e.TypeId, Text = e.Text, DatabaseName = e.DatabaseName, SchemaName = e.SchemaName});
      var type4BaseQuery = session.Query.All<model.Part4.TestEntity4>()
        .Select(e => new model.TestEntityDTO() {Id = e.Id, TypeId = e.TypeId, Text = e.Text, DatabaseName = e.DatabaseName, SchemaName = e.SchemaName});

      IEnumerable<model.TestEntityDTO> theGreatUnion = null;
      var result = type1BaseQuery.Union(type2BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2*initialCountOfEntities));
      theGreatUnion = result;

      result = type1BaseQuery.Union(type3BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));
      theGreatUnion = theGreatUnion.Union(result);

      result = type1BaseQuery.Union(type4BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));
      theGreatUnion = theGreatUnion.Union(result);

      result = type2BaseQuery.Union(type3BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));
      theGreatUnion = theGreatUnion.Union(result);

      result = type2BaseQuery.Union(type4BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));

      result = type3BaseQuery.Union(type4BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));
      theGreatUnion = theGreatUnion.Union(result);

      var model = session.Domain.Model;
      foreach (var testEntityDto in theGreatUnion) {
        var databaseAndSchema = typesMap[model.Types[testEntityDto.TypeId].UnderlyingType];
        var expectedDatabase = databaseAndSchema.First;
        var expectedSchema = databaseAndSchema.Second;

        Assert.That(testEntityDto.Text, Is.EqualTo(session.StorageNodeId));
        Assert.That(testEntityDto.DatabaseName, Is.EqualTo(expectedDatabase));
        Assert.That(testEntityDto.SchemaName, Is.EqualTo(expectedSchema));
      }

      var pair = typesMap[typeof(model.Part1.TestEntity1)];
      var filteredType1BaseQuery = type1BaseQuery.Where(e => e.Text==storageNodeId && e.DatabaseName==pair.First && e.SchemaName==pair.Second);
      pair = typesMap[typeof(model.Part2.TestEntity2)];
      var filteredType2BaseQuery = type1BaseQuery.Where(e => e.Text==storageNodeId && e.DatabaseName==pair.First && e.SchemaName==pair.Second);
      pair = typesMap[typeof(model.Part3.TestEntity3)];
      var filteredType3BaseQuery = type1BaseQuery.Where(e => e.Text==storageNodeId && e.DatabaseName==pair.First && e.SchemaName==pair.Second);
      pair = typesMap[typeof(model.Part4.TestEntity4)];
      var filteredType4BaseQuery = type1BaseQuery.Where(e => e.Text==storageNodeId && e.DatabaseName==pair.First && e.SchemaName==pair.Second);

      result = filteredType1BaseQuery.Union(filteredType2BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));

      result = filteredType1BaseQuery.Union(filteredType3BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));

      result = filteredType1BaseQuery.Union(filteredType4BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));

      result = filteredType2BaseQuery.Union(filteredType3BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));

      result = filteredType2BaseQuery.Union(filteredType4BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));

      result = filteredType3BaseQuery.Union(filteredType4BaseQuery).ToList();
      Assert.That(result.Count, Is.EqualTo(2 * initialCountOfEntities));
    }

    private Key[] Insert(Session session, int initialCountOfEntities)
    {
      var storageNodeId = session.StorageNodeId;
      Dictionary<Type, Pair<string>> typesMap;
      if (!map.TryGetValue(storageNodeId, out typesMap))
        throw new Exception(string.Format("Unknown node {0}. Probably you don't populate data", storageNodeId));

      var text = string.Format("{0}_new", storageNodeId);

      var a = new model.Part1.TestEntity1 {Text = text};
      var databaseAndSchema = typesMap[a.GetType()];
      a.DatabaseName = databaseAndSchema.First;
      a.SchemaName = databaseAndSchema.Second;

      session.SaveChanges();
      Assert.That(session.Query.All<model.Part1.TestEntity1>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      a = session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Text == text && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(a, Is.Not.Null);

      var b = new model.Part2.TestEntity2 {Text = text};
      databaseAndSchema = typesMap[b.GetType()];
      b.DatabaseName = databaseAndSchema.First;
      b.SchemaName = databaseAndSchema.Second;

      session.SaveChanges();
      Assert.That(session.Query.All<model.Part2.TestEntity2>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      b = session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Text==text && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(b, Is.Not.Null);

      var c = new model.Part3.TestEntity3 {Text = text};
      databaseAndSchema = typesMap[c.GetType()];
      c.DatabaseName = databaseAndSchema.First;
      c.SchemaName = databaseAndSchema.Second;

      session.SaveChanges();
      Assert.That(session.Query.All<model.Part3.TestEntity3>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      c = session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Text==text && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(c, Is.Not.Null);

      var d = new model.Part4.TestEntity4 {Text = text};
      databaseAndSchema = typesMap[d.GetType()];
      d.DatabaseName = databaseAndSchema.First;
      d.SchemaName = databaseAndSchema.Second;

      session.SaveChanges();
      Assert.That(session.Query.All<model.Part4.TestEntity4>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      d = session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Text==text && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(d, Is.Not.Null);

      return new Key[] { a.Key, b.Key, c.Key, d.Key };
    }

    private void Update(Session session, Key[] createdKeys, int initialCountOfEntities)
    {
      var storageNodeId = session.StorageNodeId;
      Dictionary<Type, Pair<string>> typesMap;
      if (!map.TryGetValue(storageNodeId, out typesMap))
        throw new Exception(string.Format("Unknown node {0}. Probably you don't populate data", storageNodeId));

      var updatedText = string.Format("{0}_new_updated", storageNodeId);
      var text = string.Format("{0}_new", storageNodeId);

      var databaseAndSchema = typesMap[typeof(model.Part1.TestEntity1)];
      var a = session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Key==createdKeys[0] && e.Text==text);
      Assert.That(a, Is.Not.Null);
      Assert.That(a.DatabaseName, Is.EqualTo(databaseAndSchema.First));
      Assert.That(a.SchemaName, Is.EqualTo(databaseAndSchema.Second));

      a.Text = updatedText;
      session.SaveChanges();

      Assert.That(session.Query.All<model.Part1.TestEntity1>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      Assert.That(session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Text==updatedText), Is.Not.Null);
      Assert.That(session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Text==text), Is.Null);
      Assert.That(
        session.Query.All<model.Part1.TestEntity1>()
          .FirstOrDefault(e => e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second),
        Is.Not.Null);

      databaseAndSchema = typesMap[typeof(model.Part2.TestEntity2)];
      var b = session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Key==createdKeys[1] && e.Text==text);
      Assert.That(b, Is.Not.Null);
      Assert.That(b.DatabaseName, Is.EqualTo(databaseAndSchema.First));
      Assert.That(b.SchemaName, Is.EqualTo(databaseAndSchema.Second));

      b.Text = updatedText;
      session.SaveChanges();

      Assert.That(session.Query.All<model.Part2.TestEntity2>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      Assert.That(session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Text==updatedText), Is.Not.Null);
      Assert.That(session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Text==text), Is.Null);
      Assert.That(
        session.Query.All<model.Part2.TestEntity2>()
          .FirstOrDefault(e => e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second),
        Is.Not.Null);

      databaseAndSchema = typesMap[typeof(model.Part3.TestEntity3)];
      var c = session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Key==createdKeys[2] && e.Text==text);
      Assert.That(c, Is.Not.Null);
      Assert.That(c.DatabaseName, Is.EqualTo(databaseAndSchema.First));
      Assert.That(c.SchemaName, Is.EqualTo(databaseAndSchema.Second));

      c.Text = updatedText;
      session.SaveChanges();

      Assert.That(session.Query.All<model.Part3.TestEntity3>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      Assert.That(session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Text==updatedText), Is.Not.Null);
      Assert.That(session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Text==text), Is.Null);
      Assert.That(
        session.Query.All<model.Part3.TestEntity3>()
          .FirstOrDefault(e => e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second),
        Is.Not.Null);

      databaseAndSchema = typesMap[typeof(model.Part4.TestEntity4)];
      var d = session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Key==createdKeys[3] && e.Text==text);
      Assert.That(d, Is.Not.Null);
      Assert.That(d.DatabaseName, Is.EqualTo(databaseAndSchema.First));
      Assert.That(d.SchemaName, Is.EqualTo(databaseAndSchema.Second));

      d.Text = updatedText;
      session.SaveChanges();

      Assert.That(session.Query.All<model.Part4.TestEntity4>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      Assert.That(session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Text==updatedText), Is.Not.Null);
      Assert.That(session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Text==text), Is.Null);
      Assert.That(session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second), Is.Not.Null);
    }

    private void Delete(Session session, Key[] createdKeys, int initialCountOfEntities)
    {
      var storageNodeId = session.StorageNodeId;

      Dictionary<Type, Pair<string>> typesMap;
      if (!map.TryGetValue(storageNodeId, out typesMap))
        throw new Exception(string.Format("Unknown node {0}. Probably you don't populate data", storageNodeId));

      var updatedText = string.Format("{0}_new_updated", storageNodeId);

      var databaseAndSchema = typesMap[typeof (model.Part1.TestEntity1)];
      var a = session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Key==createdKeys[0] && e.Text==updatedText);
      Assert.That(a, Is.Not.Null);
      a = session.Query.All<model.Part1.TestEntity1>()
        .FirstOrDefault(e => e.Key==createdKeys[0] && e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(a, Is.Not.Null);

      a.Remove();
      session.SaveChanges();


      databaseAndSchema = typesMap[typeof (model.Part2.TestEntity2)];
      var b = session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Key==createdKeys[1] && e.Text==updatedText);
      Assert.That(b, Is.Not.Null);
      b = session.Query.All<model.Part2.TestEntity2>()
        .FirstOrDefault(e => e.Key==createdKeys[1] && e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(b, Is.Not.Null);

      b.Remove();
      session.SaveChanges();

      databaseAndSchema = typesMap[typeof (model.Part3.TestEntity3)];
      var c = session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Key==createdKeys[2] && e.Text==updatedText);
      Assert.That(c, Is.Not.Null);
      c = session.Query.All<model.Part3.TestEntity3>()
        .FirstOrDefault(e => e.Key==createdKeys[2] && e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(c, Is.Not.Null);

      c.Remove();
      session.SaveChanges();

      databaseAndSchema = typesMap[typeof (model.Part4.TestEntity4)];
      var d = session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Key==createdKeys[3] && e.Text==updatedText);
      Assert.That(d, Is.Not.Null);
      d = session.Query.All<model.Part4.TestEntity4>()
        .FirstOrDefault(e => e.Key==createdKeys[3] && e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(d, Is.Not.Null);

      d.Remove();
      session.SaveChanges();

      a = session.Query.All<model.Part1.TestEntity1>().FirstOrDefault(e => e.Key==createdKeys[0] && e.Text==updatedText);
      Assert.That(a, Is.Null);
      a = session.Query.All<model.Part1.TestEntity1>()
        .FirstOrDefault(e => e.Key==createdKeys[0] && e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(a, Is.Null);

      b = session.Query.All<model.Part2.TestEntity2>().FirstOrDefault(e => e.Key==createdKeys[1] && e.Text==updatedText);
      Assert.That(b, Is.Null);
      b = session.Query.All<model.Part2.TestEntity2>()
        .FirstOrDefault(e => e.Key==createdKeys[1] && e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(b, Is.Null);

      c = session.Query.All<model.Part3.TestEntity3>().FirstOrDefault(e => e.Key==createdKeys[2] && e.Text==updatedText);
      Assert.That(c, Is.Null);
      c = session.Query.All<model.Part3.TestEntity3>()
        .FirstOrDefault(e => e.Key==createdKeys[2] && e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(c, Is.Null);

      d = session.Query.All<model.Part4.TestEntity4>().FirstOrDefault(e => e.Key==createdKeys[3] && e.Text==updatedText);
      Assert.That(d, Is.Null);
      d = session.Query.All<model.Part4.TestEntity4>()
        .FirstOrDefault(e => e.Key==createdKeys[3] && e.Text==updatedText && e.DatabaseName==databaseAndSchema.First && e.SchemaName==databaseAndSchema.Second);
      Assert.That(d, Is.Null);
    }
  }
}
