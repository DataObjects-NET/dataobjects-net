// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
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
      get
      {
        return ((SessionHandler) Handlers.SessionHandler).Connection;
      }
    }
    
    /// <inheritdoc/>
    public override void UpgradeStorageSchema()
    {
      var upgradeScript = GenerateUpgradeScript();
      using (var command = new SqlCommand(Connection)) {
        command.Statement = upgradeScript;
        command.Prepare();
        command.Transaction = SessionHandler.Transaction;
        command.ExecuteNonQuery();
      }
    }

    private SqlBatch GenerateUpgradeScript()
    {
      var sourceSchema = ExtractStorageSchema();
      var taragetSchema = ExtractStorageSchema();
      
      var valueTypeMapper = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper;

      var storageModel = GetStorageModel(sourceSchema);
      var domainModel = GetDomainModel(storageModel.Name);

      var actions = Compare(storageModel, domainModel,
        new HintSet(storageModel, domainModel));
      
      var translator = new SqlActionTranslator(actions, domainModel, storageModel, 
        sourceSchema, taragetSchema, valueTypeMapper.BuildSqlValueType, false);
      return translator.Translate();
    }
    
    private StorageInfo GetDomainModel(string name)
    {
      var buildingContext = BuildingContext.Current;
      var domainModelConverter = new DomainModelConverter(
        buildingContext.NameBuilder.BuildForeignKeyName,
        buildingContext.NameBuilder.BuildForeignKeyName,
        IsGeneratorPersistent);
      return domainModelConverter.Convert(buildingContext.Model, name);
    }

    private StorageInfo GetStorageModel(Schema schema)
    {
      var serverInfo = Connection.Driver.ServerInfo;
      var converter = new SqlModelConverter(schema, serverInfo);
      return converter.GetConversionResult();
    }

    private static ActionSequence Compare(StorageInfo oldModel, StorageInfo newModel, HintSet hints)
    {
      var diff = BuildDifference(oldModel, newModel, hints);
      var actions = new ActionSequence() {
        new Upgrader().GetUpgradeSequence(diff, hints, 
        new Comparer())
      };
      return actions;
    }

    private static Difference BuildDifference(StorageInfo oldModel, StorageInfo newModel, HintSet hints)
    {
      var comparer = new Comparer();
      return comparer.Compare(oldModel, newModel, hints);
    }

    private static bool IsGeneratorPersistent(GeneratorInfo generatorInfo)
    {
      var isNotPersistent = (generatorInfo.KeyGeneratorType!=typeof (KeyGenerator)
        || (Type.GetTypeCode(generatorInfo.KeyGeneratorType)==TypeCode.Object
          && generatorInfo.TupleDescriptor[0]==typeof (Guid)));
      return !isNotPersistent;
    }
  }
}