// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Sql.Dom;

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
      if (string.IsNullOrEmpty(upgradeScript))
        return;
      using (var command = new SqlCommand(Connection)) {
        Log.Info(upgradeScript);
        command.CommandText = upgradeScript;
        command.Prepare();
        command.Transaction = Transaction;
        command.ExecuteNonQuery();
      }
    }

    /// <inheritdoc/>
    public override StorageInfo GetExtractedSchema()
    {
      var schema = ExtractStorageSchema();
      var serverInfo = Connection.Driver.ServerInfo;
      var converter = new SqlModelConverter(schema, serverInfo);
      return converter.GetConversionResult();
    }

    /// <inheritdoc/>
    protected override bool IsSchemaBoundGenerator(GeneratorInfo generatorInfo)
    {
      // TODO: Replace this to KeyGeneratorFactory ?
      return generatorInfo.KeyGeneratorType==typeof (KeyGenerator)
        && (Type.GetTypeCode(generatorInfo.KeyGeneratorType)!=TypeCode.Object
          || generatorInfo.TupleDescriptor[0]!=typeof (Guid));
    }

    private string GenerateUpgradeScript(ActionSequence actions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var valueTypeMapper = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper;
      var translator = new SqlActionTranslator(
        actions,
        ExtractStorageSchema(),
        Connection.Driver,
        valueTypeMapper.BuildSqlValueType,
        sourceSchema, targetSchema);

      var commands = translator.UpgradeCommandText;
      var delimiter = Connection.Driver.Translator.BatchStatementDelimiter;
      var batch = string.Join(delimiter, commands.ToArray());
      return batch;
    }

  }
}