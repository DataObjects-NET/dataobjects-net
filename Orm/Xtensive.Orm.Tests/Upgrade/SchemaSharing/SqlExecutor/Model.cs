// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.28

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.SqlExecutor.Model
{
  namespace Part1
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      public TestEntity1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class NewTestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      public NewTestEntity1(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Part2
  {
    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      public TestEntity2(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class NewTestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      public NewTestEntity2(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Part3
  {
    [HierarchyRoot]
    public class TestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      public TestEntity3(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class NewTestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      public NewTestEntity3(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Part4
  {
    [HierarchyRoot]
    public class TestEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      public TestEntity4(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class NewTestEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      public NewTestEntity4(Session session)
        : base(session)
      {
      }
    }
  }

  public class CustomUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    public override void OnSchemaReady()
    {
      base.OnSchemaReady();

      //there is no selected storage node yet
      var session = Session.Current;
      var sqlExecutor = session.Services.Get<ISqlExecutor>();

      if (UpgradeContext.NodeConfiguration.UpgradeMode.IsMultistage()) {
        if (UpgradeContext.Stage == UpgradeStage.Upgrading) {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        }
        else {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        }
      }
      else {
        if (UpgradeContext.NodeConfiguration.UpgradeMode == DomainUpgradeMode.LegacySkip ||
            UpgradeContext.NodeConfiguration.UpgradeMode == DomainUpgradeMode.Skip) {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
        }
        else {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        }
      }

    }

    public override void OnBeforeExecuteActions(UpgradeActionSequence actions)
    {
      base.OnBeforeExecuteActions(actions);

      //there is no selected storage node yet
      var session = Session.Current;
      var sqlExecutor = session.Services.Get<ISqlExecutor>();

      if (UpgradeContext.NodeConfiguration.UpgradeMode.IsMultistage()) {
        if (UpgradeContext.Stage == UpgradeStage.Upgrading) {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        }
        else {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        }
      }
      else {
        if (UpgradeContext.NodeConfiguration.UpgradeMode == DomainUpgradeMode.LegacySkip ||
            UpgradeContext.NodeConfiguration.UpgradeMode == DomainUpgradeMode.Skip) {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
        }
        else {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        }
      }
    }

    public override void OnUpgrade()
    {
      base.OnUpgrade();

      // we have a storageNode so we have an access to tables;
      var session = Session.Current;
      var sqlExecutor = session.Services.Get<ISqlExecutor>();

      ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
      ValidateNodeBasedQueriesWork(sqlExecutor, session);
    }

    public override void OnStage()
    {
      base.OnStage();

      //we have a storage node so we have an access to tables
      var session = Session.Current;
      var sqlExecutor = session.Services.Get<ISqlExecutor>();

      if (UpgradeContext.NodeConfiguration.UpgradeMode.IsMultistage()) {
        if (UpgradeContext.Stage == UpgradeStage.Upgrading) {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        }
        else {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
        }
      }
      else {
        if (UpgradeContext.NodeConfiguration.UpgradeMode == DomainUpgradeMode.LegacySkip ||
            UpgradeContext.NodeConfiguration.UpgradeMode == DomainUpgradeMode.Skip) {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
        }
        else {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
        }
      }

      ValidateNodeBasedQueriesWork(sqlExecutor, session);
    }

    private void ValidateNodeBasedQueriesWork(ISqlExecutor sqlExecutor, Session session)
    {
      var storageNodeText = TryGetStorageNodeText(UpgradeContext.NodeConfiguration.NodeId);

      var typeinfo = session.Domain.Model.Types[typeof(Part1.TestEntity1)];
      var testEntity1 = session.StorageNode.Mapping[typeinfo];
      var tableRef = SqlDml.TableRef(testEntity1);
      var select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      var text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(storageNodeText));

      typeinfo = session.Domain.Model.Types[typeof(Part2.TestEntity2)];
      var testEntity2 = session.StorageNode.Mapping[typeinfo];
      tableRef = SqlDml.TableRef(testEntity2);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(storageNodeText));

      typeinfo = session.Domain.Model.Types[typeof(Part3.TestEntity3)];
      var testEntity3 = session.StorageNode.Mapping[typeinfo];
      tableRef = SqlDml.TableRef(testEntity3);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(storageNodeText));

      typeinfo = session.Domain.Model.Types[typeof(Part4.TestEntity4)];
      var testEntity4 = session.StorageNode.Mapping[typeinfo];
      tableRef = SqlDml.TableRef(testEntity4);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(storageNodeText));
    }

    private void ValidateSchemaBasedQueriesWork(
      ISqlExecutor sqlExecutor,
      Func<string, string> catalogNameResolver,
      Func<string, string> schemaNameResolver)
    {
      var storageSchema = GetStorageSchema();
      var databaseMap = GetDatabaseMap();
      var schemaMap = GetSchemaMap();

      var storageNodeText = TryGetStorageNodeText(UpgradeContext.NodeConfiguration.NodeId);

      var type = typeof(Part1.TestEntity1);
      var catalogName = catalogNameResolver(databaseMap[type]);
      var schemaName = schemaNameResolver(schemaMap[type]);
      var testEntity1 = storageSchema.Catalogs[catalogName].Schemas[schemaName].Tables["TestEntity1"];
      var tableRef = SqlDml.TableRef(testEntity1);
      var select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      var text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(storageNodeText));

      type = typeof(Part2.TestEntity2);
      catalogName = catalogNameResolver(databaseMap[type]);
      schemaName = schemaNameResolver(schemaMap[type]);
      var testEntity2 = storageSchema.Catalogs[catalogName].Schemas[schemaName].Tables["TestEntity2"];
      tableRef = SqlDml.TableRef(testEntity2);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(storageNodeText));

      type = typeof(Part3.TestEntity3);
      catalogName = catalogNameResolver(databaseMap[type]);
      schemaName = schemaNameResolver(schemaMap[type]);
      var testEntity3 = storageSchema.Catalogs[catalogName].Schemas[schemaName].Tables["TestEntity3"];
      tableRef = SqlDml.TableRef(testEntity3);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(storageNodeText));

      type = typeof(Part4.TestEntity4);
      catalogName = catalogNameResolver(databaseMap[type]);
      schemaName = schemaNameResolver(schemaMap[type]);
      var testEntity4 = storageSchema.Catalogs[catalogName].Schemas[schemaName].Tables["TestEntity4"];
      tableRef = SqlDml.TableRef(testEntity4);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(storageNodeText));
    }

    private string ResolveSharedCatalogName(string baseName) => baseName;

    private string ResolveNormalCatalogName(string baseName) =>
      UpgradeContext.NodeConfiguration.DatabaseMapping.Apply(baseName);

    private string ResolveSharedSchemaName(string baseName) => baseName;

    private string ResolveNormalSchemaName(string baseName) =>
      UpgradeContext.NodeConfiguration.SchemaMapping.Apply(baseName);

    private SchemaExtractionResult GetStorageSchema()
    {
      //hypothetically we can get such information from UpgradeContext using reflection
      return UpgradeContext.ExtractedSqlModelCache;
    }

    private IDictionary<Type, string> GetDatabaseMap()
    {
      var configuration = UpgradeContext.Configuration;
      var dictionary = new Dictionary<Type, string>();
      if (configuration.IsMultidatabase) {
        dictionary.Add(typeof(Part1.TestEntity1), WellKnownDatabases.MultiDatabase.AdditionalDb1);
        dictionary.Add(typeof(Part2.TestEntity2), WellKnownDatabases.MultiDatabase.AdditionalDb1);
        dictionary.Add(typeof(Part3.TestEntity3), WellKnownDatabases.MultiDatabase.AdditionalDb2);
        dictionary.Add(typeof(Part4.TestEntity4), WellKnownDatabases.MultiDatabase.AdditionalDb2);
      }
      else {
        dictionary.Add(typeof(Part1.TestEntity1), UpgradeContext.DefaultSchemaInfo.Database);
        dictionary.Add(typeof(Part2.TestEntity2), UpgradeContext.DefaultSchemaInfo.Database);
        dictionary.Add(typeof(Part3.TestEntity3), UpgradeContext.DefaultSchemaInfo.Database);
        dictionary.Add(typeof(Part4.TestEntity4), UpgradeContext.DefaultSchemaInfo.Database);
      }
      return dictionary;
    }

    private IDictionary<Type, string> GetSchemaMap()
    {
      var configuration = UpgradeContext.Configuration;
      var dictionary = new Dictionary<Type, string>();
      if (configuration.IsMultidatabase || configuration.IsMultischema) {
        dictionary.Add(typeof(Part1.TestEntity1), WellKnownSchemas.Schema1);
        dictionary.Add(typeof(Part2.TestEntity2), WellKnownSchemas.Schema1);
        dictionary.Add(typeof(Part3.TestEntity3), WellKnownSchemas.Schema3);
        dictionary.Add(typeof(Part4.TestEntity4), WellKnownSchemas.Schema3);
      }
      else {
        dictionary.Add(typeof(Part1.TestEntity1), UpgradeContext.DefaultSchemaInfo.Schema);
        dictionary.Add(typeof(Part2.TestEntity2), UpgradeContext.DefaultSchemaInfo.Schema);
        dictionary.Add(typeof(Part3.TestEntity3), UpgradeContext.DefaultSchemaInfo.Schema);
        dictionary.Add(typeof(Part4.TestEntity4), UpgradeContext.DefaultSchemaInfo.Schema);
      }
      return dictionary;
    }

    private string TryGetStorageNodeText(string nodeId) => string.IsNullOrEmpty(nodeId) ? "<default>" : nodeId;
  }
}
