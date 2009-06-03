// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Core;
using ColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;
using SequenceInfo=Xtensive.Storage.Indexing.Model.SequenceInfo;
using TableInfo = Xtensive.Storage.Indexing.Model.TableInfo;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture]
  public abstract class SqlActionTranslatorTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Url = GetConnectionUrl();
    }

    protected abstract string GetConnectionUrl();

    protected abstract TypeInfo ConvertType(SqlValueType valueType);

    protected string Url { get; private set; }

    protected abstract SqlModelConverter CreateSqlModelConverter(Schema storageSchema,
      Func<ISqlCompileUnit, object> commandExecutor, Func<SqlValueType, TypeInfo> valueTypeConverter);

    private static StorageInfo BuildOldModel()
    {
      var storage = new StorageInfo();
      var t1 = new TableInfo(storage, "table1");
      var t1Id = new ColumnInfo(t1, "Id", new TypeInfo(typeof (int)));
      var t1C1 = new ColumnInfo(t1, "col1", new TypeInfo(typeof (int?), true));
      var t1C2 = new ColumnInfo(t1, "col2", new TypeInfo(typeof (string), true, 256));
      var t1C4 = new ColumnInfo(t1, "col4", new TypeInfo(typeof (string), true, 256));
      var t1pk = new PrimaryIndexInfo(t1, "PK_table1");
      new KeyColumnRef(t1pk, t1Id, Direction.Positive);
      t1pk.PopulateValueColumns();
      var t1fk = new SecondaryIndexInfo(t1, "FK_Col1");
      new KeyColumnRef(t1fk, t1C1, Direction.Negative);
      new IncludedColumnRef(t1fk, t1C2);
      t1fk.PopulatePrimaryKeyColumns();
      

      var t3 = new TableInfo(storage, "table2");
      var t3Id = new ColumnInfo(t3, "Id", new TypeInfo(typeof (int)));
      var t3C1 = new ColumnInfo(t3, "col1", new TypeInfo(typeof (int?), true));
      var t3pk = new PrimaryIndexInfo(t3, "PK_table3");
      new KeyColumnRef(t3pk, t3Id, Direction.Positive);
      t3pk.PopulateValueColumns();
      

      var foreignKey = new ForeignKeyInfo(t1, "FK");
      foreignKey.ForeignKeyColumns.Set(t1fk);
      foreignKey.PrimaryKey = t3pk;

      new SequenceInfo(storage, "IntSequence") {
        StartValue = 0,
        Increment = 1,
        Type = new TypeInfo(typeof (int))
      };

      return storage;
    }

    private static StorageInfo BuildNewModel()
    {
      var storage = new StorageInfo();
      var t1 = new TableInfo(storage, "table1");
      var t1Id = new ColumnInfo(t1, "Id", new TypeInfo(typeof (int)));
      var t1C1 = new ColumnInfo(t1, "col1", new TypeInfo(typeof (int?), true));
      var t1C5 = new ColumnInfo(t1, "col5", new TypeInfo(typeof (string), true, 256));
      var t1C3 = new ColumnInfo(t1, "col3", new TypeInfo(typeof (string), false, 256));
      var t1pk = new PrimaryIndexInfo(t1, "PK_table1");
      new KeyColumnRef(t1pk, t1Id, Direction.Positive);
      t1pk.PopulateValueColumns();
      var t1fk = new SecondaryIndexInfo(t1, "FK_Col1");
      new KeyColumnRef(t1fk, t1C1, Direction.Negative);
      new IncludedColumnRef(t1fk, t1C3);
      t1fk.PopulatePrimaryKeyColumns();

      var t3 = new TableInfo(storage, "table3");
      var t3Id = new ColumnInfo(t3, "Id", new TypeInfo(typeof (int)));
      var t3C1 = new ColumnInfo(t3, "col2", new TypeInfo(typeof (int?), true));
      var t3pk = new PrimaryIndexInfo(t3, "PK_table3");
      new KeyColumnRef(t3pk, t3Id, Direction.Positive);
      t3pk.PopulateValueColumns();
      
      var foreignKey = new ForeignKeyInfo(t1, "FK");
      foreignKey.ForeignKeyColumns.Set(t1fk);
      foreignKey.PrimaryKey = t3pk;

      new SequenceInfo(storage, "IntSequence") {
        StartValue = 1,
        Increment = 2,
        Current = 5,
        Type = new TypeInfo(typeof (int))
      };

      return storage;
    }

    private static StorageInfo BuildOldModel2()
    {
      var storage = new StorageInfo();
      var t1 = new TableInfo(storage, "table1");
      var t1Id = new ColumnInfo(t1, "Id", new TypeInfo(typeof (int)));
      var t1C1 = new ColumnInfo(t1, "col1", new TypeInfo(typeof (int?), true));
      var t1pk = new PrimaryIndexInfo(t1, "PK_table1");
      new KeyColumnRef(t1pk, t1Id, Direction.Positive);
      t1pk.PopulateValueColumns();
      
      var t3 = new TableInfo(storage, "table2");
      var t3Id = new ColumnInfo(t3, "Id", new TypeInfo(typeof (int)));
      var t3C1 = new ColumnInfo(t3, "col2", new TypeInfo(typeof (int?), true));
      var t3pk = new PrimaryIndexInfo(t3, "PK_table3");
      new KeyColumnRef(t3pk, t3Id, Direction.Positive);
      t3pk.PopulateValueColumns();

      return storage;
    }

    private static StorageInfo BuildNewModel2()
    {
      var storage = new StorageInfo();
      var t1 = new TableInfo(storage, "table1");
      var t1Id = new ColumnInfo(t1, "Id", new TypeInfo(typeof (int)));
      var t1C1 = new ColumnInfo(t1, "col1", new TypeInfo(typeof (int?), true));
      var t1C2 = new ColumnInfo(t1, "col2", new TypeInfo(typeof (int?), true));
      var t1pk = new PrimaryIndexInfo(t1, "PK_table1");
      new KeyColumnRef(t1pk, t1Id, Direction.Positive);
      t1pk.PopulateValueColumns();
      
      var t2 = new TableInfo(storage, "table2");
      var t2Id = new ColumnInfo(t2, "Id", new TypeInfo(typeof (int)));
      var t2pk = new PrimaryIndexInfo(t2, "PK_table3");
      new KeyColumnRef(t2pk, t2Id, Direction.Positive);
      t2pk.PopulateValueColumns();

      return storage;
    }

    [Test]
    public void UpdateSchemaTest()
    {
      ClearSchema();
      var oldModel = BuildOldModel();
      Create(oldModel);
      var newModel = BuildNewModel();

      var hints = new HintSet(oldModel, newModel);
      hints.Add(new RenameHint("Tables/table1/Columns/col4", "Tables/table1/Columns/col5"));
      hints.Add(new RenameHint("Tables/table2", "Tables/table3"));
      hints.Add(new RenameHint("Tables/table2/Columns/col1", "Tables/table3/Columns/col2"));

      //var diff = BuildDifference(newModel, ExtractModel(), hints);
      //Tests.Log.Info(diff.ToString());
      var actions = Compare(oldModel, newModel, hints);
      Tests.Log.Info(actions.ToString());
      UpgradeCurrentSchema(oldModel, newModel, actions);

      var postUpgradeDifference = BuildDifference(newModel, ExtractModel(), null);
      Assert.IsNull(postUpgradeDifference);
    }

    [Test]
    public void MoveColumnTest()
    {
      ClearSchema();
      var oldModel = BuildOldModel2();
      Create(oldModel);
      var newModel = BuildNewModel2();

      var hints = new HintSet(oldModel, newModel);
      // {
      //  new CopyDataHint("Tables/table2/Columns/col2", "Tables/table1/Columns/col2",
      //    new[] {new IdentityParameter("Tables/table2/Columns/Id", "Tables/table1/Columns/Id")})
      // };

      var actions = Compare(oldModel, newModel, hints);
      Tests.Log.Info(actions.ToString());
      
      UpgradeCurrentSchema(oldModel, newModel, actions);
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
        new Modelling.Comparison.Upgrader().GetUpgradeSequence(diff, hints, 
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

    private void Create(StorageInfo model)
    {
      ClearSchema();
      var emptyModel = new StorageInfo();
      var actions = Compare(emptyModel, model, null);
      UpgradeCurrentSchema(emptyModel, model, actions);
    }

    private void ClearSchema()
    {
      var emptySchema = new StorageInfo();
      var currentModel = ExtractModel();
      var actions = Compare(currentModel, emptySchema, new HintSet(currentModel, emptySchema));
      UpgradeCurrentSchema(currentModel, emptySchema, actions);
    }

    private void UpgradeCurrentSchema(StorageInfo oldModel, StorageInfo newModel,
      ActionSequence actions)
    {
      var schema = GetSchema();

      var provider = new SqlConnectionProvider();
      using (var connection = provider.CreateConnection(Url) as SqlConnection) {
        connection.Open();
        using (var transaction = connection.BeginTransaction()) {
          var translator = new SqlActionTranslator(actions, schema,
            connection.Driver, BuildSqlValueType, oldModel, newModel);
          var delimiter = connection.Driver.Translator.BatchStatementDelimiter;
          var batch = new List<string>();
          batch.Add(string.Join(delimiter, translator.PreUpgradeCommands.ToArray()));
          batch.Add(string.Join(delimiter, translator.UpgradeCommands.ToArray()));
          batch.Add(string.Join(delimiter, translator.DataManipulateCommands.ToArray()));
          batch.Add(string.Join(delimiter, translator.PostUpgradeCommands.ToArray()));
          foreach (var commandText in batch) {
            if (!string.IsNullOrEmpty(commandText))
              using (var command = new SqlCommand(connection)) {
                Log.Info(commandText);
                command.CommandText = commandText;
                command.Prepare();
                command.Transaction = transaction;
                command.ExecuteNonQuery();
              }
          }
          transaction.Commit();
        }
      }
    }

    private StorageInfo ExtractModel()
    {
      var schema = GetSchema();
      return CreateSqlModelConverter(schema, GetGeneratorValue, ConvertType).GetConversionResult();
    }

    private Schema GetSchema()
    {
      Schema schema;
      using (var connection = new SqlConnectionProvider().CreateConnection(Url)) {
        connection.Open();
        using (var t = connection.BeginTransaction()) {
          var modelProvider = new SqlModelProvider(connection as SqlConnection, t);
          schema = Sql.Dom.Database.Model.Build(modelProvider).DefaultServer
            .DefaultCatalog.DefaultSchema;
        }
      }
      return schema;
    }

    private static object GetGeneratorValue(ISqlCompileUnit cmd)
    {
      return (long)0;
    }

    private static SqlValueType BuildSqlValueType(Type type, int length)
    {
      var dataType = GetDbType(type);
      return new SqlValueType(dataType, length);
    }

    private static SqlDataType GetDbType(Type type)
    {
      if (type.IsValueType && type.IsNullable())
        type = type.GetGenericArguments()[0];
      
      TypeCode typeCode = Type.GetTypeCode(type);
      switch (typeCode) {
      case TypeCode.Object:
        if (type==typeof (byte[]))
          return SqlDataType.Binary;
        if (type == typeof(Guid))
          return SqlDataType.Guid;
        throw new ArgumentOutOfRangeException();
      case TypeCode.Boolean:
        return SqlDataType.Boolean;
      case TypeCode.Char:
        return SqlDataType.Char;
      case TypeCode.SByte:
        return SqlDataType.SByte;
      case TypeCode.Byte:
        return SqlDataType.Byte;
      case TypeCode.Int16:
        return SqlDataType.Int16;
      case TypeCode.UInt16:
        return SqlDataType.UInt16;
      case TypeCode.Int32:
        return SqlDataType.Int32;
      case TypeCode.UInt32:
        return SqlDataType.UInt32;
      case TypeCode.Int64:
        return SqlDataType.Int64;
      case TypeCode.UInt64:
        return SqlDataType.UInt64;
      case TypeCode.Single:
        return SqlDataType.Float;
      case TypeCode.Double:
        return SqlDataType.Double;
      case TypeCode.Decimal:
        return SqlDataType.Decimal;
      case TypeCode.DateTime:
        return SqlDataType.DateTime;
      case TypeCode.String:
        return SqlDataType.VarChar;
      default:
        throw new ArgumentOutOfRangeException();
      }
    }
    #endregion
  }
}