// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.27

using System;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Sql.Dom;
using SqlModel = Xtensive.Sql.Dom.Database.Model;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Core;
using Xtensive.Sql.Common;
using ColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;
using TableInfo=Xtensive.Storage.Indexing.Model.TableInfo;
using SqlRefAction = Xtensive.Sql.Dom.ReferentialAction;
using ReferentialAction = Xtensive.Storage.Indexing.Model.ReferentialAction;

namespace Xtensive.Storage.Tests.Upgrade
{
  /// <summary>
  /// Manages SqlStorage schema.
  /// </summary>
  public sealed class SchemaManager
  {
    private readonly string url;
    private ServerInfo serverInfo;

    public void ClearSchema()
    {
      var schema = GetStorageSchema();
      var model = Convert(schema);
      var script = GenerateClearScript(schema, model);
      Execute(script);
    }

    public void CreateSchema(StorageInfo model)
    {
      var schema = GetStorageSchema();
      if (schema.Tables.Count > 0)
        ClearSchema();
      var script = GenerateCreateScript(GetStorageSchema(), model);
      Execute(script);
    }

    public StorageInfo GetStorageModel()
    {
      var schema = GetStorageSchema();
      return Convert(schema);
    }

    public Schema GetStorageSchema()
    {
      Schema schema = null;
      Execute(c =>
      {
        using (var transaction = c.BeginTransaction())
        {
          var modelProvider = new SqlModelProvider(c, transaction);
          schema = SqlModel.Build(modelProvider).DefaultServer
            .DefaultCatalog.DefaultSchema;
        }
      });
      return schema;
    }

    private static SqlBatch GenerateClearScript(Schema schema, StorageInfo model)
    {
      var batch = SqlFactory.Batch();
      foreach (var foreignKey in model.Tables.SelectMany(table => table.ForeignKeys))
        batch.Add(
          SqlFactory.Alter(
            GetTable(schema, foreignKey.Parent.Name),
            SqlFactory.DropConstraint(GetForeignKey(schema, foreignKey.Parent.Name, foreignKey.Name))));
      foreach (var table in model.Tables)
      {
        batch.Add(SqlFactory.Drop(GetTable(schema, table.Name)));
      }
      return batch;
    }

    private static SqlBatch GenerateCreateScript(Schema schema, StorageInfo model)
    {
      var batch = SqlFactory.Batch();
      foreach (var tableInfo in model.Tables)
        batch.Add(GenerateCreateTableScript(schema, tableInfo));
      foreach (var tableInfo in model.Tables)
        batch.Add(GenerateCreateForeignKeyScript(schema, tableInfo));
      return batch;
    }

    private static SqlBatch GenerateCreateTableScript(Schema schema, TableInfo tableInfo)
    {
      var batch = SqlFactory.Batch();
      var table = schema.CreateTable(tableInfo.Name);
      foreach (var columnInfo in tableInfo.Columns)
      {
        var column = table.CreateColumn(columnInfo.Name, ConvertType(columnInfo));
        column.IsNullable = columnInfo.Type.IsNullable;
      }
      CreatePrimaryKey(tableInfo, table);
      batch.Add(SqlFactory.Create(table));

      foreach (var indexInfo in tableInfo.SecondaryIndexes)
      {
        var index = CreateSecondaryIndex(table, indexInfo);
        batch.Add(SqlFactory.Create(index));
      }
      return batch;
    }

    private static SqlBatch GenerateCreateForeignKeyScript(Schema schema, TableInfo tableInfo)
    {
      var batch = SqlFactory.Batch();
      foreach (var foreignKeyInfo in tableInfo.ForeignKeys)
      {
        var referencingTable = schema.Tables[tableInfo.Name];
        var foreignKey = referencingTable.CreateForeignKey(foreignKeyInfo.Name);
        foreignKey.OnUpdate = ConvertReferentialAction(foreignKeyInfo.OnUpdateAction);
        foreignKey.OnDelete = ConvertReferentialAction(foreignKeyInfo.OnRemoveAction);
        var referncingColumns = foreignKeyInfo.ForeignKeyColumns
          .Select(cr => referencingTable.TableColumns[cr.Value.Name])
          .ToArray();
        foreignKey.Columns.AddRange(referncingColumns);
        var referencedTable = schema.Tables[foreignKeyInfo.PrimaryKey.Parent.Name];
        var referencedColumns = foreignKeyInfo.PrimaryKey.KeyColumns
          .Select(cr => referencedTable.TableColumns[cr.Value.Name])
          .ToArray();
        foreignKey.ReferencedTable = referencedTable;
        foreignKey.ReferencedColumns.AddRange(referencedColumns);
        referencingTable.TableConstraints.Remove(foreignKey);
        batch.Add(SqlFactory.Alter(referencingTable, SqlFactory.AddConstraint(foreignKey)));
      }
      return batch;
    }

    private static PrimaryKey CreatePrimaryKey(TableInfo tableInfo, Table table)
    {
      return
        table.CreatePrimaryKey(tableInfo.PrimaryIndex.Name,
          tableInfo.PrimaryIndex.KeyColumns
            .Select(cr => table.TableColumns[cr.Value.Name]).ToArray());
    }

    private static Index CreateSecondaryIndex(Table table, SecondaryIndexInfo indexInfo)
    {
      var oldIndex = table.Indexes.SingleOrDefault(i => i.Name == indexInfo.Name);
      if (oldIndex != null)
        table.Indexes.Remove(oldIndex);

      var index = table.CreateIndex(indexInfo.Name);
      index.IsUnique = indexInfo.IsUnique;
      foreach (var keyColumn in indexInfo.KeyColumns)
        index.CreateIndexColumn(
          table.TableColumns[keyColumn.Value.Name],
          keyColumn.Direction == Direction.Positive);
      index.NonkeyColumns.AddRange(
        indexInfo.IncludedColumns
          .Select(cr => table.TableColumns[cr.Value.Name]).ToArray());
      return index;
    }

    private static TableConstraint GetForeignKey(Schema schema, string tableName, string foreignKeyName)
    {
      return
        schema.Tables.Single(table => table.Name == tableName)
          .TableConstraints.Single(constraint => constraint.Name == foreignKeyName);
    }

    private static Table GetTable(Schema schema, string tableName)
    {
      return
        schema.Tables.Single(table => table.Name == tableName);
    }

    private static SqlValueType ConvertType(ColumnInfo columnInfo)
    {
      var dataType = GetDbType(columnInfo.Type.Type);

      return new SqlValueType(
        dataType,
        columnInfo.Type.Length);
    }

    private static SqlDataType GetDbType(Type type)
    {
      if (type.IsValueType && type.IsNullable())
        type = type.GetGenericArguments()[0];

      TypeCode typeCode = Type.GetTypeCode(type);
      switch (typeCode)
      {
      case TypeCode.Object:
        if (type == typeof(byte[]))
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

    private static SqlRefAction ConvertReferentialAction(ReferentialAction toConvert)
    {
      switch (toConvert)
      {
      case ReferentialAction.None:
        return SqlRefAction.NoAction;
      case ReferentialAction.Restrict:
        return SqlRefAction.Restrict;
      case ReferentialAction.Cascade:
        return SqlRefAction.Cascade;
      case ReferentialAction.Clear:
        return SqlRefAction.SetNull;
      default:
        return SqlRefAction.Restrict;
      }
    }

    private StorageInfo Convert(Schema schema)
    {
      if (serverInfo == null)
        Execute((c) => { serverInfo = c.Driver.ServerInfo; });

      var converter = new SqlModelConverter();
      return converter.Convert(schema, serverInfo);
    }

    private void Execute(SqlBatch batch)
    {
      using (var connection = new SqlConnectionProvider().CreateConnection(url))
      {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        using (var command = new SqlCommand(connection as SqlConnection))
        {
          command.Statement = batch;
          command.Prepare();
          command.Transaction = transaction;
          command.ExecuteNonQuery();
          transaction.Commit();
        }
      }
    }

    private void Execute(Action<SqlConnection> processor)
    {
      using (var connection = new SqlConnectionProvider().CreateConnection(url))
      {
        connection.Open();
        processor.Invoke(connection as SqlConnection);
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SchemaManager(string url)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url, "url");
      this.url = url;
    }
  }
}