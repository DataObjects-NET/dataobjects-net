// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Building;
using Xtensive.Sql.Common;

namespace Xtensive.Storage.Providers.MsSql
{
  [Serializable]
  public class SchemaUpgradeHandler : Sql.SchemaUpgradeHandler
  {
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
          Log.Info(batch);
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
      var schema = ExtractStorageSchema();
      var sessionHandeler = (SessionHandler) BuildingContext.Demand().SystemSessionHandler;
      var converter = new MsSqlModelConverter(schema, sessionHandeler.ExecuteScalar, ConvertType);
      return converter.GetConversionResult();
    }

    private static TypeInfo ConvertType(SqlValueType valueType)
    {
      // var typeMapper = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper;
      var sessionHandeler = (SessionHandler) BuildingContext.Demand().SystemSessionHandler;
      var dataTypes = sessionHandeler.Connection.Driver.ServerInfo.DataTypes;
      var nativeType = sessionHandeler.Connection.Driver.Translator.Translate(valueType);

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

      return new TypeInfo(dataType.Type, false, length);
    }

    private List<string> GenerateUpgradeScript(ActionSequence actions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var valueTypeMapper = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper;
      var translator = new SqlActionTranslator(
        actions,
        ExtractStorageSchema(),
        Connection.Driver,
        valueTypeMapper.BuildSqlValueType,
        sourceSchema, targetSchema);

      var delimiter = Connection.Driver.Translator.BatchStatementDelimiter;
      var batch = new List<string>();
      batch.Add(string.Join(delimiter, translator.PreUpgradeCommands.ToArray()));
      batch.Add(string.Join(delimiter, translator.UpgradeCommands.ToArray()));
      batch.Add(string.Join(delimiter, translator.DataManipulateCommands.ToArray()));
      batch.Add(string.Join(delimiter, translator.PostUpgradeCommands.ToArray()));

      return batch;
    }

  }
}