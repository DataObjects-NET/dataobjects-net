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
        using (var command = Connection.CreateCommand()) {
          command.CommandText = batch;
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
        ConvertType, DomainHandler.ProviderInfo, SessionHandler.ExecuteScalarStatement);
      return converter.GetConversionResult();
    }

    /// <inheritdoc/>
    protected override ModelTypeInfo CreateTypeInfo(Type type, int? length)
    {
      var sqlValueType = DomainHandler.ValueTypeMapper.BuildSqlValueType(type, length);
      var convertedType = ConvertSqlType(sqlValueType.Type);
      int? typeLength = null;
      if (sqlValueType.Type!=SqlType.VarCharMax && sqlValueType.Type!=SqlType.VarBinaryMax)
        typeLength = length;
      return new ModelTypeInfo(convertedType, typeLength);
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

    protected virtual ModelTypeInfo ConvertType(SqlValueType valueType)
    {
      return new ModelTypeInfo(ConvertSqlType(valueType.Type), false, valueType.Length);
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

    internal static Type ConvertSqlType(SqlType type)
    {
      switch (type) {
      case SqlType.Boolean:
        return typeof (bool);
      case SqlType.Int8:
        return typeof (sbyte);
      case SqlType.UInt8:
        return typeof (byte);
      case SqlType.Int16:
        return typeof (short);
      case SqlType.UInt16:
        return typeof (ushort);
      case SqlType.Int32:
        return typeof (int);
      case SqlType.UInt32:
        return typeof (uint);
      case SqlType.Int64:
        return typeof (long);
      case SqlType.UInt64:
        return typeof (ulong);
      case SqlType.Decimal:
        return typeof (decimal);
      case SqlType.Float:
        return typeof (float);
      case SqlType.Double:
        return typeof (double);
      case SqlType.DateTime:
        return typeof (DateTime);
      case SqlType.Interval:
        return typeof (TimeSpan);
      case SqlType.Char:
      case SqlType.VarChar:
      case SqlType.VarCharMax:
        return typeof (string);
      case SqlType.Binary:
      case SqlType.VarBinary:
      case SqlType.VarBinaryMax:
        return typeof (byte[]);
      case SqlType.Guid:
        return typeof (Guid);
      default:
        throw new ArgumentOutOfRangeException("type");
      }
    }
  }
}