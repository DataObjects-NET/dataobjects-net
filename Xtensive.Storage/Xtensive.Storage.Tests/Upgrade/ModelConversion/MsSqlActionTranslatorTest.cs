// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.27

using System;
using NUnit.Framework;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Providers.MsSql;
using Xtensive.Storage.Providers.Sql;

namespace Xtensive.Storage.Tests.Upgrade
{
  [Explicit("Requires MSSQL servers")]
  public sealed class MsSqlActionTranslatorTest : SqlActionTranslatorTest
  {
    protected override string GetConnectionUrl()
    {
      return "mssql2005://localhost/DO40-Tests";
    }

    protected override bool IsIncludedColumnsSupported { get { return true; } }

    protected override TypeInfo ConvertType(SqlValueType valueType)
    {
      var provider = new SqlConnectionProvider();
      using (var connection = provider.CreateConnection(Url) as SqlConnection) {
        connection.Open();
        var dataTypes = connection.Driver.ServerInfo.DataTypes;
        var nativeType = connection.Driver.Translator.Translate(valueType);

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

    protected override ProviderInfo CreateProviderInfo()
    {
      var providerInfo = new ProviderInfo();
      providerInfo.SupportsRealTimeSpan = false;
      providerInfo.SupportSequences = false;
      providerInfo.SupportKeyColumnSortOrder = true;
      providerInfo.SupportsIncludedColumns = true;
      providerInfo.SupportsForeignKeyConstraints = true;
      return providerInfo;
    }
  }
}