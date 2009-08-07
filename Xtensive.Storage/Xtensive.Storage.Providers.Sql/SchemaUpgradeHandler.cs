// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Upgrade;
using ModelTypeInfo = Xtensive.Storage.Indexing.Model.TypeInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  public class SchemaUpgradeHandler : Providers.SchemaUpgradeHandler
  {
    private DomainHandler DomainHandler { get { return (DomainHandler) Handlers.DomainHandler; } }
    private SessionHandler SessionHandler { get { return (SessionHandler) BuildingContext.Current.SystemSessionHandler; } }
    private SqlConnection Connection { get { return ((SessionHandler) Handlers.SessionHandler).Connection; } }
    private SqlDriver Driver { get { return DomainHandler.Driver; } }

    /// <inheritdoc/>
    public override void UpgradeSchema(ActionSequence upgradeActions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var valueTypeMapper = DomainHandler.ValueTypeMapper;
      var enforceChangedColumns = UpgradeContext.Demand().Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToList();
      var translator = new SqlActionTranslator(
        upgradeActions,
        GetStorageSchema(),
        sourceSchema, targetSchema, DomainHandler.ProviderInfo, 
        Connection.Driver, valueTypeMapper, 
        Handlers.NameBuilder.TypeIdColumnName, 
        enforceChangedColumns,
        SessionHandler.ExecuteScalarStatement);

      WriteToLog(translator);

      Execute(translator.PreUpgradeCommands);
      Execute(translator.UpgradeCommands);
      Execute(translator.DataManipulateCommands);
      Execute(translator.PostUpgradeCommands);
    }

    /// <inheritdoc/>
    public override StorageInfo GetExtractedSchema()
    {
      var schema = GetStorageSchema();
      var converter = new SqlModelConverter(schema, DomainHandler.ProviderInfo);
      return converter.GetConversionResult();
    }

    /// <inheritdoc/>
    protected override ModelTypeInfo CreateTypeInfo(Type type, int? length)
    {
      var sqlValueType = DomainHandler.ValueTypeMapper.BuildSqlValueType(type, length);
      var convertedType = sqlValueType.Type.ToClrType();
      var typeLength = sqlValueType.Length;
      return new ModelTypeInfo(convertedType, typeLength);
    }

    private void Execute(IEnumerable<string> batch)
    {
      if (DomainHandler.ProviderInfo.SupportsBatches) {
        var commandText = Driver.Translator.BuildBatch(batch.ToArray());
        if (!string.IsNullOrEmpty(commandText))
          using (var command = Connection.CreateCommand()) {
            command.CommandText = commandText;
            command.Transaction = SessionHandler.Transaction;
            command.ExecuteNonQuery();
          }
      }
      else {
        foreach (var commandText in batch)
          if (!string.IsNullOrEmpty(commandText))
            using (var command = Connection.CreateCommand()) {
              command.CommandText = commandText;
              command.Transaction = SessionHandler.Transaction;
              command.ExecuteNonQuery();
            }
      }
    }

    private Schema GetStorageSchema()
    {
      var context = UpgradeContext.Demand();
      var schema = context.NativeExtractedSchema as Schema;
      if (schema == null) {
        schema = DomainHandler.Driver
          .ExtractDefaultSchema(SessionHandler.Connection, SessionHandler.Transaction)
          .DefaultSchema;
        SaveSchemaInContext(schema);
      }
      return schema;
    }

    private void WriteToLog(SqlActionTranslator translator)
    {
      var logDelimiter = DomainHandler.Driver.Translator.BatchItemDelimiter + Environment.NewLine;
      var logBatch = new List<string>();
      logBatch.Add(Driver.Translator.BatchBegin);
      translator.PreUpgradeCommands.Apply(logBatch.Add);
      translator.UpgradeCommands.Apply(logBatch.Add);
      translator.DataManipulateCommands.Apply(logBatch.Add);
      translator.PostUpgradeCommands.Apply(logBatch.Add);
      logBatch.Add(Driver.Translator.BatchEnd);
      if (logBatch.Count > 2)
        Log.Info("Upgrade DDL: {0}", 
          Environment.NewLine + string.Join(logDelimiter, logBatch.ToArray()));
    }
  }
}