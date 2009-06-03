// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.09

using System;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql;
using TypeInfo=Xtensive.Storage.Indexing.Model.TypeInfo;

namespace Xtensive.Storage.Providers.PgSql
{
  [Serializable]
  public class SchemaUpgradeHandler : Sql.SchemaUpgradeHandler
  {
    /// <inheritdoc/>
    protected override void BuildSequence(Schema schema, GeneratorInfo generator)
    {
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      var sequence = schema.CreateSequence(generator.MappingName);
      sequence.SequenceDescriptor.StartValue = generator.CacheSize;
      sequence.SequenceDescriptor.Increment = generator.CacheSize;
      sequence.DataType = domainHandler.ValueTypeMapper.BuildSqlValueType(generator.TupleDescriptor[0], 0);
    }

    /// <inheritdoc/>
    public override StorageInfo GetExtractedSchema()
    {
      // return new StorageInfo();
      var schema = ExtractStorageSchema();
      var sessionHandeler = (SessionHandler) BuildingContext.Demand().SystemSessionHandler;
      var converter = new PgSqlModelConverter(schema, sessionHandeler.ExecuteScalar, ConvertType);
      return converter.GetConversionResult();
    }

    private static TypeInfo ConvertType(SqlValueType valueType)
    {
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
  }
}