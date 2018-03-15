// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    }

    [HierarchyRoot]
    public class NewTestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
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
    }

    [HierarchyRoot]
    public class NewTestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
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
    }

    [HierarchyRoot]
    public class NewTestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
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
    }

    [HierarchyRoot]
    public class NewTestEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }
  }

  public class CustomUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnSchemaReady()
    {
      base.OnSchemaReady();

      //there is no selected storage node yet
      var session = Session.Current;
      var sqlExecutor = session.Services.Get<ISqlExecutor>();

      if (UpgradeContext.NodeConfiguration.UpgradeMode.IsMultistage()) {
        if (UpgradeContext.Stage==UpgradeStage.Upgrading)
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        else
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
      }
      else {
        if (UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.LegacySkip ||
            UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.Skip) {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
        }
        else
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
      }

    }

    public override void OnBeforeExecuteActions(UpgradeActionSequence actions)
    {
      base.OnBeforeExecuteActions(actions);

      //there is no selected storage node yet
      var session = Session.Current;
      var sqlExecutor = session.Services.Get<ISqlExecutor>();

      if (UpgradeContext.NodeConfiguration.UpgradeMode.IsMultistage()) {
        if (UpgradeContext.Stage==UpgradeStage.Upgrading)
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        else
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
      }
      else {
        if (UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.LegacySkip ||
            UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.Skip) {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
        }
        else
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
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
        if (UpgradeContext.Stage==UpgradeStage.Upgrading)
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveNormalCatalogName, ResolveNormalSchemaName);
        else
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
      }
      else {
        if (UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.LegacySkip ||
            UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.Skip) {
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
        }
        else
          ValidateSchemaBasedQueriesWork(sqlExecutor, ResolveSharedCatalogName, ResolveSharedSchemaName);
      }

      ValidateNodeBasedQueriesWork(sqlExecutor, session);
    }

    private void ValidateNodeBasedQueriesWork(ISqlExecutor sqlExecutor, Session session)
    {
      var typeinfo = session.Domain.Model.Types[typeof (Part1.TestEntity1)];
      var testEntity1 = session.StorageNode.Mapping[typeinfo];
      var tableRef = SqlDml.TableRef(testEntity1);
      var select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      var text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));

      typeinfo = session.Domain.Model.Types[typeof (Part2.TestEntity2)];
      var testEntity2 = session.StorageNode.Mapping[typeinfo];
      tableRef = SqlDml.TableRef(testEntity2);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));

      typeinfo = session.Domain.Model.Types[typeof (Part3.TestEntity3)];
      var testEntity3 = session.StorageNode.Mapping[typeinfo];
      tableRef = SqlDml.TableRef(testEntity3);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));

      typeinfo = session.Domain.Model.Types[typeof (Part4.TestEntity4)];
      var testEntity4 = session.StorageNode.Mapping[typeinfo];
      tableRef = SqlDml.TableRef(testEntity4);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));
    }

    private void ValidateSchemaBasedQueriesWork(ISqlExecutor sqlExecutor, Func<string, string> catalogNameResolver, Func<string, string> schemaNameResolver)
    {
      var storageSchema = GetStorageSchema();
      var databaseMap = GetDatabaseMap();
      var schemaMap = GetSchemaMap();

      var type = typeof (Part1.TestEntity1);
      var catalogName = catalogNameResolver(databaseMap[type]);
      var schemaName = schemaNameResolver(schemaMap[type]);
      var testEntity1 = storageSchema.Catalogs[catalogName].Schemas[schemaName].Tables["TestEntity1"];
      var tableRef = SqlDml.TableRef(testEntity1);
      var select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      var text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));

      type = typeof (Part2.TestEntity2);
      catalogName = catalogNameResolver(databaseMap[type]);
      schemaName = schemaNameResolver(schemaMap[type]);
      var testEntity2 = storageSchema.Catalogs[catalogName].Schemas[schemaName].Tables["TestEntity2"];
      tableRef = SqlDml.TableRef(testEntity2);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));

      type = typeof (Part3.TestEntity3);
      catalogName = catalogNameResolver(databaseMap[type]);
      schemaName = schemaNameResolver(schemaMap[type]);
      var testEntity3 = storageSchema.Catalogs[catalogName].Schemas[schemaName].Tables["TestEntity3"];
      tableRef = SqlDml.TableRef(testEntity3);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));

      type = typeof (Part4.TestEntity4);
      catalogName = catalogNameResolver(databaseMap[type]);
      schemaName = schemaNameResolver(schemaMap[type]);
      var testEntity4 = storageSchema.Catalogs[catalogName].Schemas[schemaName].Tables["TestEntity4"];
      tableRef = SqlDml.TableRef(testEntity4);
      select = SqlDml.Select(tableRef);
      select.Columns.Add(tableRef["Text"]);

      text = sqlExecutor.ExecuteScalar(select);
      Assert.That(text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));
    }

    private string ResolveSharedCatalogName(string baseName)
    {
      return baseName;
    }

    private string ResolveNormalCatalogName(string baseName)
    {
      return UpgradeContext.NodeConfiguration.DatabaseMapping.Apply(baseName);
    }

    private string ResolveSharedSchemaName(string baseName)
    {
      return baseName;
    }

    private string ResolveNormalSchemaName(string baseName)
    {
      return UpgradeContext.NodeConfiguration.SchemaMapping.Apply(baseName);
    }

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
        dictionary.Add(typeof (Part1.TestEntity1), "DO-Tests-1");
        dictionary.Add(typeof (Part2.TestEntity2), "DO-Tests-1");
        dictionary.Add(typeof (Part3.TestEntity3), "DO-Tests-2");
        dictionary.Add(typeof (Part4.TestEntity4), "DO-Tests-2");
      }
      else {
        dictionary.Add(typeof (Part1.TestEntity1), "DO-Tests");
        dictionary.Add(typeof (Part2.TestEntity2), "DO-Tests");
        dictionary.Add(typeof (Part3.TestEntity3), "DO-Tests");
        dictionary.Add(typeof (Part4.TestEntity4), "DO-Tests");
      }
      return dictionary;
    }

    private IDictionary<Type, string> GetSchemaMap()
    {
      var configuration = UpgradeContext.Configuration;
      var dictionary = new Dictionary<Type, string>();
      if (configuration.IsMultidatabase) {
        dictionary.Add(typeof (Part1.TestEntity1), "Model1");
        dictionary.Add(typeof (Part2.TestEntity2), "Model1");
        dictionary.Add(typeof (Part3.TestEntity3), "Model3");
        dictionary.Add(typeof (Part4.TestEntity4), "Model3");
      }
      else if (configuration.IsMultischema) {
        dictionary.Add(typeof (Part1.TestEntity1), "Model1");
        dictionary.Add(typeof (Part2.TestEntity2), "Model1");
        dictionary.Add(typeof (Part3.TestEntity3), "Model3");
        dictionary.Add(typeof (Part4.TestEntity4), "Model3");
      }
      else {
        dictionary.Add(typeof (Part1.TestEntity1), "dbo");
        dictionary.Add(typeof (Part2.TestEntity2), "dbo");
        dictionary.Add(typeof (Part3.TestEntity3), "dbo");
        dictionary.Add(typeof (Part4.TestEntity4), "dbo");
      }
      return dictionary;
    }
  }
}
