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
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Model.Conversion;
using Xtensive.Sql.Dom.Dml;
using TypeInfo=Xtensive.Storage.Indexing.Model.TypeInfo;
using TableInfo=Xtensive.Storage.Indexing.Model.TableInfo;

namespace Xtensive.Storage.Providers.MsSql
{
  [Serializable]
  public class SchemaUpgradeHandler : Sql.SchemaUpgradeHandler
  {
    private SqlConnection Connection
    {
      get{return ((SessionHandler) Handlers.SessionHandler).Connection;}
    }

    private DbTransaction Transaction
    {
      get { return ((SessionHandler) Handlers.SessionHandler).Transaction; }
    }

    /// <inheritdoc/>
    public override void UpgradeSchema(ActionSequence upgradeActions, StorageInfo targetSchema)
    {
      var upgradeScript = GenerateUpgradeScript(upgradeActions);
      if (upgradeScript.Count==0)
        return;
      using (var command = new SqlCommand(Connection)) {
        command.CommandText = string.Join(";",
          upgradeScript.ToArray());
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

    private List<string> GenerateUpgradeScript(ActionSequence actions)
    {
      var valueTypeMapper = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper;
      var translator = new SqlActionTranslator(
        actions,
        ExtractStorageSchema(),
        Connection.Driver,
        valueTypeMapper.BuildSqlValueType);
      
      return translator.UpgradeCommandText;
    }

  }
}