// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Upgrade;
using SqlModel = Xtensive.Sql.Dom.Database.Model;
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

    private SqlConnection Connection
    {
      get { return ((SessionHandler) Handlers.SessionHandler).Connection; }
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
          command.Transaction = SessionHandler.Transaction;
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
      var sqlValueType = DomainHandler.ValueTypeMapper.BuildSqlValueType(type, length);
      var convertedType = DomainHandler.Driver.ServerInfo.DataTypes[sqlValueType.DataType].Type;
      int? typeLength = null;
      if (sqlValueType.DataType != SqlDataType.Text
        && sqlValueType.DataType != SqlDataType.VarBinaryMax)
        typeLength = length;
      return new ModelTypeInfo(convertedType, typeLength);
    }

    private Schema GetStorageSchema()
    {
      var context = UpgradeContext.Demand();
      var schema = context.NativeExtractedSchema as Schema;
      if (schema == null) {
        var modelProvider = new SqlModelProvider(SessionHandler.Connection, SessionHandler.Transaction);
        var storageModel = SqlModel.Build(modelProvider);
        schema = storageModel.DefaultServer.DefaultCatalog.DefaultSchema;
        SaveSchemaInContext(schema);
      }
      return schema;
    }

    protected virtual ModelTypeInfo ConvertType(SqlValueType valueType)
    {
      var driver = SessionHandler.Connection.Driver;
      var dataType = driver.ServerInfo.DataTypes[valueType.DataType];
      var length = valueType.Size;
      var streamType = dataType as StreamDataTypeInfo;
      if (streamType!=null && valueType.Size == 0)
        length = streamType.Length.MaxValue;

      var type = dataType!=null ? dataType.Type : typeof (object);
      return new ModelTypeInfo(type, false, length);
    }
    
    private List<string> GenerateUpgradeScript(ActionSequence actions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var valueTypeMapper = DomainHandler.ValueTypeMapper;
      var enforceChangedColumns = UpgradeContext.Demand().Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToList();
      var translator = new SqlActionTranslator(
        actions,
        GetStorageSchema(),
        sourceSchema, targetSchema, DomainHandler.ProviderInfo, 
        Connection.Driver, valueTypeMapper, 
        Handlers.NameBuilder.TypeIdColumnName, enforceChangedColumns);

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