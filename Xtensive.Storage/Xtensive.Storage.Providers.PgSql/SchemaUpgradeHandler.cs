// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.09

using System;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Indexing.Model;

namespace Xtensive.Storage.Providers.PgSql
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  public class SchemaUpgradeHandler : Sql.SchemaUpgradeHandler
  {
    /// <inheritdoc/>
    protected override TypeInfo ConvertType(SqlValueType valueType)
    {
      var driver = SessionHandler.Connection.Driver;
      var dataType = driver.ServerInfo.DataTypes[valueType.DataType];
      int? length = null;
      var streamType = dataType as StreamDataTypeInfo;
      if (streamType!=null 
        && streamType.SqlType!=SqlDataType.Text
        && streamType.SqlType!=SqlDataType.VarBinaryMax)
        length = valueType.Size;

      var type = dataType!=null ? dataType.Type : typeof (object);
      return new TypeInfo(type, false, length);
    }

    /// <inheritdoc/>
    protected override TypeInfo CreateTypeInfo(Type type, int? length)
    {
      var sqlValueType = DomainHandler.ValueTypeMapper.BuildSqlValueType(type, length);
      var convertedType = DomainHandler.Driver.ServerInfo.DataTypes[sqlValueType.DataType].Type;
      int? typeLength = null;
      if (sqlValueType.DataType != SqlDataType.Text
        && sqlValueType.DataType != SqlDataType.VarBinaryMax)
        typeLength = length;
      return new TypeInfo(convertedType, typeLength);
    }
  }
}