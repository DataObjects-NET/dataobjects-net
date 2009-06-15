// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Upgrade;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;
using IndexInfo = Xtensive.Storage.Model.IndexInfo;
using SqlModel = Xtensive.Sql.Dom.Database.Model;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using TypeInfo = Xtensive.Storage.Model.TypeInfo;
using ModelTypeInfo = Xtensive.Storage.Indexing.Model.TypeInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  public class SchemaUpgradeHandler : Providers.SchemaUpgradeHandler
  {
    /// <summary>
    /// Gets the domain handler.
    /// </summary>
    protected DomainHandler DomainHandler
    {
      get { return (DomainHandler) Handlers.DomainHandler; }
    }

    /// <summary>
    /// Gets the session handler.
    /// </summary>
    protected SessionHandler SessionHandler
    {
      get { return (SessionHandler) BuildingContext.Current.SystemSessionHandler; }
    }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    protected NameBuilder NameBuilder
    {
      get { return BuildingContext.Current.NameBuilder; }
    }

    private SqlConnection Connection
    {
      get { return ((SessionHandler) Handlers.SessionHandler).Connection; }
    }

    private DbTransaction Transaction
    {
      get { return ((SessionHandler) Handlers.SessionHandler).Transaction; }
    }

    /// <inheritdoc/>
    public override void UpgradeSchema(ActionSequence upgradeActions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var upgradeScript = GenerateUpgradeScript(upgradeActions, sourceSchema, targetSchema);
      foreach (var batch in upgradeScript) {
        if (string.IsNullOrEmpty(batch))
          continue;
        using (var command = new SqlCommand(Connection)) {
          command.CommandText = batch;
          command.Prepare();
          command.Transaction = Transaction;
          command.ExecuteNonQuery();
        }
      }
    }

    /// <inheritdoc/>
    public override StorageInfo GetExtractedSchema()
    {
      var schema = GetStorageSchema();
      var converter = new SqlModelConverter(schema,
        ConvertType, DomainHandler.ProviderInfo, SessionHandler.ExecuteScalar);
      return converter.GetConversionResult();
    }

    /// <inheritdoc/>
    protected override ModelTypeInfo CreateTypeInfo(Type type, int? length)
    {
      var valueTypeMapper = DomainHandler.ValueTypeMapper;
      var sqlValueType = length.HasValue
        ? valueTypeMapper.BuildSqlValueType(type, length.Value)
        : valueTypeMapper.BuildSqlValueType(type, 0);
      var convertedType = DomainHandler.Driver.ServerInfo.DataTypes[sqlValueType.DataType].Type;
      return new ModelTypeInfo(convertedType, length);
    }

    private Schema GetStorageSchema()
    {
      var context = UpgradeContext.Demand();
      var schema = context.LegacyExtractedSchema as Schema;
      if (schema == null) {
        var modelProvider = new SqlModelProvider(SessionHandler.Connection, SessionHandler.Transaction);
        var storageModel = SqlModel.Build(modelProvider);
        schema = storageModel.DefaultServer.DefaultCatalog.DefaultSchema;
        SaveSchemaInContext(schema);
      }
      return schema;
    }

    private ModelTypeInfo ConvertType(SqlValueType valueType)
    {
      var driver = SessionHandler.Connection.Driver;
      var dataTypes = driver.ServerInfo.DataTypes;
      var nativeType = driver.Translator.Translate(valueType);

      var dataType = dataTypes[nativeType] ?? dataTypes[valueType.DataType];

      int? length = 0;
      var streamType = dataType as StreamDataTypeInfo;
      if (streamType!=null
        && (streamType.SqlType==SqlDataType.VarBinaryMax
        || streamType.SqlType==SqlDataType.VarCharMax
        || streamType.SqlType==SqlDataType.AnsiVarCharMax))
        length = null;
      else
        length = valueType.Size;

      var type = dataType!=null ? dataType.Type : typeof (object);
      return new ModelTypeInfo(type, false, length);
    }
    
    private List<string> GenerateUpgradeScript(ActionSequence actions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var valueTypeMapper = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper;
      var translator = new SqlActionTranslator(
        actions,
        GetStorageSchema(),
        sourceSchema, targetSchema, DomainHandler.ProviderInfo, 
        Connection.Driver, valueTypeMapper, Handlers.NameBuilder.TypeIdColumnName);

      var delimiter = Connection.Driver.Translator.BatchStatementDelimiter;
      var batch = new List<string>();
      batch.Add(string.Join(delimiter, translator.PreUpgradeCommands.ToArray()));
      batch.Add(string.Join(delimiter, translator.UpgradeCommands.ToArray()));
      batch.Add(string.Join(delimiter, translator.DataManipulateCommands.ToArray()));
      batch.Add(string.Join(delimiter, translator.PostUpgradeCommands.ToArray()));

      WriteToLog(delimiter, translator);

      return batch;
    }

    private void WriteToLog(string delimiter, SqlActionTranslator translator)
    {
      var logDelimiter = delimiter + Environment.NewLine;
      var logBatch = new List<string>();
      translator.PreUpgradeCommands.Apply(logBatch.Add);
      translator.UpgradeCommands.Apply(logBatch.Add);
      translator.DataManipulateCommands.Apply(logBatch.Add);
      translator.PostUpgradeCommands.Apply(logBatch.Add);
      if (logBatch.Count > 0)
        Log.Info("Upgrade DDL: {0}", 
          Environment.NewLine + string.Join(logDelimiter, logBatch.ToArray()));
    }
  }
}