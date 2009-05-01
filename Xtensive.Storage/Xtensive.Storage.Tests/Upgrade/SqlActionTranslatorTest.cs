// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Core;
using ColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;
using TableInfo = Xtensive.Storage.Indexing.Model.TableInfo;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture]
  public class SqlActionTranslatorTest
  {
    private static string Url = "mssql2005://localhost/DO40-Tests";

    private static StorageInfo BuildOldModel()
    {
      var storage = new StorageInfo();
      var t1 = new TableInfo(storage, "table1");
      var t1Id = new ColumnInfo(t1, "Id", new TypeInfo(typeof(int)));
      var t1C1 = new ColumnInfo(t1, "col1", new TypeInfo(typeof(int?), true));
      var t1C2 = new ColumnInfo(t1, "col2", new TypeInfo(typeof(string), true, 256));
      var t1pk = new PrimaryIndexInfo(t1, "PK_table1");
      new KeyColumnRef(t1pk, t1Id, Direction.Positive);
      t1pk.PopulateValueColumns();
      var t1fk = new SecondaryIndexInfo(t1, "FK_Col1");
      new KeyColumnRef(t1fk, t1C1, Direction.Negative);
      new IncludedColumnRef(t1fk, t1C2);
      t1fk.PopulatePrimaryKeyColumns();
      

      var t3 = new TableInfo(storage, "table3");
      var t3Id = new ColumnInfo(t3, "Id", new TypeInfo(typeof(int)));
      var t3C1 = new ColumnInfo(t3, "col1", new TypeInfo(typeof(int?), true));
      var t3pk = new PrimaryIndexInfo(t3, "PK_table3");
      new KeyColumnRef(t3pk, t3Id, Direction.Positive);
      t3pk.PopulateValueColumns();
      

      var foreignKey = new ForeignKeyInfo(t1, "FK");
      foreignKey.ForeignKeyColumns.Set(t1fk);
      foreignKey.PrimaryKey = t3pk;

      new SequenceInfo(storage, "IntSequence")
        {
          StartValue = 0,
          Increment = 1,
          Type = new TypeInfo(typeof(int))
        };

      return storage;
    }

    private static StorageInfo BuildNewModel()
    {
      var storage = new StorageInfo();
      var t1 = new TableInfo(storage, "table1");
      var t1Id = new ColumnInfo(t1, "Id", new TypeInfo(typeof(int)));
      var t1C1 = new ColumnInfo(t1, "col1", new TypeInfo(typeof(int?), true));
      var t1C3 = new ColumnInfo(t1, "col3", new TypeInfo(typeof(Guid), false));
      var t1pk = new PrimaryIndexInfo(t1, "PK_table1");
      new KeyColumnRef(t1pk, t1Id, Direction.Positive);
      t1pk.PopulateValueColumns();
      var t1fk = new SecondaryIndexInfo(t1, "FK_Col1");
      new KeyColumnRef(t1fk, t1C1, Direction.Negative);
      new IncludedColumnRef(t1fk, t1C3);
      t1fk.PopulatePrimaryKeyColumns();

      var t3 = new TableInfo(storage, "table3");
      var t3Id = new ColumnInfo(t3, "Id", new TypeInfo(typeof(int)));
      var t3C1 = new ColumnInfo(t3, "col1", new TypeInfo(typeof(int?), true));
      var t3pk = new PrimaryIndexInfo(t3, "PK_table3");
      new KeyColumnRef(t3pk, t3Id, Direction.Positive);
      t3pk.PopulateValueColumns();
      

      var foreignKey = new ForeignKeyInfo(t1, "FK");
      foreignKey.ForeignKeyColumns.Set(t1fk);
      foreignKey.PrimaryKey = t3pk;

      new SequenceInfo(storage, "IntSequence")
        {
          StartValue = 1,
          Increment = 2,
          Current = 5,
          Type = new TypeInfo(typeof(int))
        };

      return storage;
    }

    [Test]
    public void UpdateSchemaTest()
    {
      ClearSchema();
      var oldModel = BuildOldModel();
      Create(oldModel);
      var newModel = BuildNewModel();
      var actions = Compare(oldModel, newModel, null);
      Tests.Log.Info(actions.ToString());
      UpgradeCurrentSchema(newModel, actions);
      var diff = BuildDifference(newModel, ExtractModel(), null);
      Assert.IsNull(diff);
    }

    # region SchemaManager tests

    [Test]
    public void CreateNewSchemaTest()
    {
      var model = BuildNewModel();
      Create(model);
      var storageModel = ExtractModel();
      var diff = BuildDifference(model, storageModel, null);
      Assert.IsNull(diff);
    }

    [Test]
    public void CreateOldSchemaTest()
    {
      var model = BuildOldModel();
      Create(model);
      var storageModel = ExtractModel();
      var diff = BuildDifference(model, storageModel, null);
      Assert.IsNull(diff);
    }

    # endregion

    # region Helper methods

    private static ActionSequence Compare(StorageInfo oldModel, StorageInfo newModel, HintSet hints)
    {
      if (hints==null) 
        hints = new HintSet(oldModel, newModel);
      var diff = BuildDifference(oldModel, newModel, hints);
      var actions = new ActionSequence() {
        new Upgrader().GetUpgradeSequence(diff, hints, 
          new Comparer())
      };
      return actions;
    }

    private static Difference BuildDifference(StorageInfo oldModel, StorageInfo newModel, HintSet hints)
    {
      if (hints==null)
        hints = new HintSet(oldModel, newModel);
      var comparer = new Comparer();
      return comparer.Compare(oldModel, newModel, hints);
    }

    private static void Create(StorageInfo model)
    {
      var manager = new SchemaManager(Url, false);
      manager.CreateSchema(model);
    }

    private static void ClearSchema()
    {
      var manager = new SchemaManager(Url, false);
      manager.ClearSchema();
    }

    private static void UpgradeCurrentSchema(StorageInfo newModel, ActionSequence actions)
    {
      var manager = new SchemaManager(Url, false);
      var schema = manager.GetStorageSchema();
      var newSchema = manager.GetStorageSchema();
      var oldModel = manager.GetStorageModel();

      
      var provider = new SqlConnectionProvider();
      using (var connection = provider.CreateConnection(Url) as SqlConnection) {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        using (var command = new SqlCommand(connection)) {
          var translator = new SqlActionTranslator(actions, schema, 
            connection.Driver, null);
          command.CommandText = string.Join(";", 
            translator.UpgradeCommandText.ToArray());
          command.Prepare();
          command.Transaction = transaction;
          command.ExecuteNonQuery();
          transaction.Commit();
        }
      }
    }
    
    private static StorageInfo ExtractModel()
    {
      var manager = new SchemaManager(Url, false);
      return manager.GetStorageModel();
    }

    # endregion
  }
}