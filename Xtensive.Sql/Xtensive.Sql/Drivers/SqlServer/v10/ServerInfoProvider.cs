// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer.v10
{
  internal class ServerInfoProvider : v09.ServerInfoProvider
  {
    public override IndexInfo GetIndexInfo()
    {
      var result = base.GetIndexInfo();
      result.Features |= IndexFeatures.Filtered;
      return result;
    }

    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = base.GetDataTypesInfo();

      var common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing |
        DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      var index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering |
        DataTypeFeatures.FillFactor | DataTypeFeatures.KeyConstraint;

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index,
        new ValueRange<DateTime>(new DateTime(1, 1, 1), new DateTime(9999, 12,31)),
        "datetime2", "datetime", "date", "time", "smalldatetime");

      return types;
    }

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}